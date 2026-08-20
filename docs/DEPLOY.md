# Despliegue a Producción — AutoMarketRD

Guía única para desplegar el sistema en un servidor propio (VPS) con Docker.
Complementa a `docs/PRODUCCION.md` (qué falta) y `docs/verificar-paypal.md`
(credenciales y verificación de pagos).

## Arquitectura

```
Internet
   │  HTTPS (TLS)
   ▼
[TLS: Cloudflare | nginx host | Caddy]  ← termina el HTTPS
   │  80
   ▼
frontend (nginx, puerto 80) ── estáticos del build de Vite
   │  /api/*  (reverse proxy)
   ▼
api (.NET 10, puerto 8080 interno) ── PostgreSQL (db)
   │                                        │
   ├── Cloudflare R2 (fotos/logos, URLs firmadas)      db-backup (cron 03:00, retención 14 días)
   ├── SMTP Gmail (correos)
   └── PayPal Live (pagos + webhook)
```

Los 4 servicios viven en `Backend/docker-compose.prod.yml`: `db`, `api`,
`frontend` y `db-backup`. **Solo el frontend expone el puerto 80**; la API y la
base de datos no se publican al exterior.

## 0. Requisitos previos

- Servidor con Docker Engine + Compose v2 (Ubuntu 22.04 o similar) y ~2 GB RAM.
- Dominio apuntando al IP del servidor (registro A).
- Credenciales reales de `docs/PRODUCCION.md` §B.1:
  BD, JWT, SMTP (Gmail app password), R2, **PayPal Live** (client id/secret +
  webhook id, ver `docs/verificar-paypal.md`) y admin inicial.
- Bucket de Cloudflare R2 creado y un API token R2 con permiso de objeto (lectura
  + escritura para subir; el acceso al público se hace por URLs firmadas).

## 1. Preparar el entorno en el servidor

```bash
git clone https://github.com/Logidex/AutomarketRD.git
cd AutomarketRD/Backend
```

Copiar la plantilla y rellenar con las credenciales reales:

```bash
cp .env.example .env.prod
nano .env.prod   # o usa tu editor / secrets manager
```

Campos obligatorios para el primer arranque (ver `.env.prod`):

- `POSTGRES_PASSWORD_PROD` / `CONNECTION_STRING_PROD` (secreto fuerte).
- `JWT_SECRET_PROD` (mínimo 32+ bytes, único).
- `SMTP_USER`/`SMTP_PASSWORD` (app password de Gmail) y `SMTP_SENDER_EMAIL`.
- `AWS_ACCESS_KEY`/`AWS_SECRET_KEY` (token R2), `AWS_BUCKET_NAME`,
  `AWS_SERVICE_URL=https://<ACCOUNT_ID>.r2.cloudflarestorage.com`.
- `PAYPAL_MODE=Live`, `PAYPAL_URL_BASE=https://api-m.paypal.com`,
  `PAYPAL_CLIENT_ID`, `PAYPAL_CLIENT_SECRET`, `PAYPAL_WEBHOOK_ID`.
- `CORS_ALLOWED_ORIGINS_PROD=https://tudominio.com,https://www.tudominio.com`.
- `APP_FRONTEND_URL=https://tudominio.com`.
- `ADMIN_EMAIL_PROD`/`ADMIN_PASSWORD_PROD` (admin inicial del seeder).

> **Seguridad**: `.env.prod` nunca se commitea (está en `.gitignore`).

## 2. Primer arranque (migraciones + seeder)

En el primer arranque se aplican las migraciones y se crean los planes y el
admin. Para eso, en `.env.prod`:

```dotenv
MIGRATE_ON_STARTUP=true
SEEDER_ENABLED=true
```

> En Producción estos flags están a `false` por defecto. Actívalos **solo la
> primera vez**. En desarrollos posteriores se aplican migraciones como paso del
> deploy (`dotnet ef database update` o `MIGRATE_ON_STARTUP=true` puntual).

## 3. Levantar el stack

```bash
docker compose -f docker-compose.prod.yml --env-file .env.prod up -d --build
```

Verificar:

```bash
# Estado de los contenedores
docker compose -f docker-compose.prod.yml --env-file .env.prod ps

# Health checks internos
docker compose -f docker-compose.prod.yml --env-file .env.prod exec api \
  wget -qO- http://localhost:8080/health/ready
docker compose -f docker-compose.prod.yml --env-file .env.prod exec api \
  wget -qO- http://localhost:8080/health
```

Si todo quedó sano, **vuelve a poner `MIGRATE_ON_STARTUP=false` y
`SEEDER_ENABLED=false`** y reinicia:

```bash
docker compose -f docker-compose.prod.yml --env-file .env.prod up -d
```

## 4. HTTPS (obligatorio para producción)

La API exige `Request.IsHttps` para emitir HSTS, y PayPal exige URLs `https`.
Elige una opción:

### Opción A — Cloudflare (la más simple)
- Activa el proxy naranja del DNS (termina TLS en Cloudflare).
- En SSL/TLS usa **Full (strict)**.
- Nota: nginx interno responde por HTTP en el puerto 80; Cloudflare
  `X-Forwarded-Proto=https` se propaga a la API (la usa `SecurityHeadersMiddleware`
  para HSTS).

### Opción B — nginx host o Caddy en el servidor
- Caddy (auto-TLS): instala Caddy y reversa a `http://127.0.0.1` (puerto 80 del
  frontend). Caddy emite Let's Encrypt solo.
- nginx host: config con certificados + `proxy_pass http://127.0.0.1` y cabeceras
  `X-Forwarded-Proto $scheme`.

### Cabeceras recomendadas en el TLS externo
```nginx
add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
add_header X-Content-Type-Options "nosniff" always;
```

> **CSP** (opcional): si se agrega, permitir `'self'` y `style-src 'unsafe-inline'`
> (React usa estilos inline). Ejemplo seguro de partida:
> `Content-Security-Policy: default-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https://*.r2.cloudflarestorage.com; connect-src 'self'; frame-ancestors 'none'`
> El contenedor `frontend` ya sirve estáticos con cache inmutable (`/assets/`) y
> `no-cache` para `index.html`.

## 5. Webhook de PayPal

El webhook debe apuntar a la URL pública:
`https://tudominio.com/api/pagos/webhook` (el nginx lo reenvía a la API).

- Crea el webhook en developer.paypal.com (app Live) y copia su ID a
  `PAYPAL_WEBHOOK_ID`.
- Detalle y verificación del flujo completo: `docs/verificar-paypal.md`.

## 6. Backups (automáticos)

El contenedor `db-backup` hace un `pg_dump` gzip diario a las 03:00 (hora del
servidor) con retención de 14 días (configurable):

```dotenv
BACKUP_RETENTION_DAYS=14
CRON_SCHEDULE="0 3 * * *"
```

Restaurar un backup:

```bash
# Listar backups
docker compose -f docker-compose.prod.yml --env-file .env.prod exec db-backup ls -lh /backups

# Restaurar el más reciente
ls -t Backend/*.sql.gz >/dev/null 2>&1   # (los backups viven en el volumen db_backups)
BACKUP=$(docker compose -f docker-compose.prod.yml --env-file .env.prod exec -T db-backup sh -c "ls -t /backups/*.sql.gz | head -1")
docker compose -f docker-compose.prod.yml --env-file .env.prod exec db-backup sh -c \
  "gunzip -c $BACKUP" | docker compose -f docker-compose.prod.yml --env-file .env.prod exec -T db psql -U ${POSTGRES_USER} -d ${POSTGRES_DB}
```

> Respaldos fuera del servidor (ej. copiar el volumen `db_backups` a un storage
> externo) es recomendable para tolerar pérdida del VPS.

## 7. Actualizar a una nueva versión

```bash
git pull origin main
docker compose -f docker-compose.prod.yml --env-file .env.prod build
docker compose -f docker-compose.prod.yml --env-file .env.prod up -d
```

Si la versión incluye migraciones, ejecútalas una vez:
`MIGRATE_ON_STARTUP=true` en `.env.prod` → `up -d` → verificar → volver a `false`.

Rollback y releases por tags: ver el playbook en `docs/PRODUCCION.md` §C.5.

## 8. Monitoreo y logs

- Health checks: `/health` (general) y `/health/ready` (dependencias) desde el
  contenedor `frontend`/balanceador.
- Logs:
  ```bash
  docker compose -f docker-compose.prod.yml --env-file .env.prod logs -f api
  ```
  La API loguea a Serilog (consola/archivo); los errores quedan con contexto
  (`Path`, `Method`, excepción completa) y **sin** datos sensibles ni respuestas
  crudas de terceros (endurecido en Fases 1 y 3).

## 9. Checklist final

- [ ] `https://tudominio.com` sirve el frontend con HTTPS.
- [ ] `https://tudominio.com/api/pagos/webhook` responde (webhook PayPal verificado).
- [ ] `/health` y `/health/ready` en verde.
- [ ] Login admin funciona; Swagger/Scalar da 404 en prod.
- [ ] Un pago real (monto pequeño) se confirma y el webhook llega.
- [ ] Un reembolso deja el pago en `Reembolsado`.
- [ ] El backup diario genera archivos en `db_backups`.