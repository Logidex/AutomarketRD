# Checklist de Producción — AutoMarketRD

Este documento es la *guía única* para saber **qué falta para salir a producción**.
Cuando termines una tarea, marca su casilla con `[x]`. La intención es que quede claro
qué bloquea un lanzamiento y qué es solo recomendable.
Para el *cómo* desplegar, ver `docs/DEPLOY.md`; para el pago PayPal, `docs/verificar-paypal.md`.

---

## A. Funciones pendientes (producto)

> Todo el frontend de producto ya existe y está migrado a TanStack Query. Esta sección
> queda para lo que aún no existe o depende de servicios reales.

- [x] **Home / vitrina pública** — catálogo con filtros y búsqueda.
- [x] **Detalle de vehículo para compradores** — ficha con fotos, equipo, descripción
      y "Contactar" (crea el lead).
- [x] **Página pública del vendedor (`/vendedor/:id`)** — perfil público con anuncios.
- [x] **Frontend de comprador funcional** — login/registro, favoritos, historial,
      leads desde la ficha, comparador.
- [x] **Panel de administración (frontend)** — usuarios, anuncios, planes y pagos.
- [x] **Reembolsos de pagos PayPal (Feature 5)** — capture id, `ReembolsarAsync`
      contra `/v2/payments/captures/{id}/refund`, estado `Reembolsado`, página
      admin `/admin/pagos`. Verificado en staging (SQL de la migración + endpoint).
- [x] **Precios en RD$/USD (Feature 2)** — moneda por anuncio (DOP/USD) filtrable,
      mostrada con `formatearPrecio` en toda la vitrina. Migración aplicada en staging.
- [x] **Flujo de pago verificado contra PayPal real** — funciona en local
      (`confirmar-pago` idempotente + webhook); confirmado en staging con
      credenciales sandbox reales (env `Staging`). Falta probar contra una cuenta
      **live** de PayPal antes de lanzar a producción.
- [ ] **Correos operativos llegando a la bandeja** — depende del SMTP real (sección B).
- [ ] **Automatización E2E (recomendado)** — Playwright que recorra registrar → publicar →
      comprar plan; así el CI valida la UX, no solo el build y el lint.

---

## B. Requisitos de operación y claves (obligatorio para lanzar)

> Ninguno de estos es "código", pero sin ellos la API no sirve de verdad en producción.

### B.1 Credenciales reales (hoy son placeholders en `.env`/user-secrets)
- [ ] **Base de datos**: cambiar `POSTGRES_PASSWORD` y la `CONNECTION_STRING` por un secreto fuerte.
- [ ] **JWT**: secreto largo y rotado (min 32+ chars, no el de ejemplo).
- [ ] **SMTP**: host/puerto/usuario/password reales.
- [ ] **AWS S3**: access key, secret key, bucket y región; bucket privado con URLs firmadas.
- [ ] **PayPal**: pasar de `sandbox` a **producción** (`PAYPAL_URL_BASE=https://api-m.paypal.com`),
      `CLIENT_ID`/`CLIENT_SECRET` reales, y configurar el **webhook** (B.2).
- [ ] **Admin inicial**: email/contraseña reales y `ADMIN_ROTATE_PASSWORD=false`.
- [ ] **CORS**: permitir solo el dominio real (`https://tudominio.com`), no `*`.

### B.2 El entorno de red
- [ ] **Dominio + HTTPS** (proxy inverso / load balancer, ej. Nginx, Caddy, Cloudflare, un VPS).
- [ ] **URLs de retorno/cancelación PayPal** apuntando al dominio HTTPS real.
- [ ] **Webhook de PayPal operativo**: URL pública (`https://tudominio.com/api/pagos/webhook`)
      y `PAYPAL_WEBHOOK_ID` verificado.
- [ ] **Base de datos** en servicio gestionado o VPS con backup (B.4).

### B.3 Despliegue de la API
- [ ] **Migraciones**: `dotnet ef database update` como paso del deploy (no auto-migrate en prod).
- [ ] **Seeder desactivado**: confirmar que en `Release` no se crea/rota datos ni el admin
      (gate por `Development`/`Seeder:Enabled`).
- [ ] **Swagger/Scalar cerrado**: verificado en producción (response 404).
- [ ] **Publicación optimizada**: `dotnet publish -c Release` (self-contained o framework).
- [ ] **Secretos no versionados**: config vía entorno/Secrets Manager, nunca en el repo.
- [ ] **Deploy automatizado con rollback por tags**: elegir destino (VPS con Docker,
      PaaS, registro de imágenes) y conectar el workflow de release a la tag `v*`.

### B.4 Operación día a día
- [x] **Backups automáticos** de la base de datos: diarios 03:00, retención 14 días
      en volumen local **+ copia offsite a S3/R2** (`db-backups/<entorno>/`),
      tanto en staging como en producción.
- [ ] **Monitoreo/logs**: logs con correlación (Serilog claro o OpenTelemetry) y alertas.
- [ ] **Health checks conectados**: `/health` y `/health/ready` al balanceador/reverse proxy.
- [ ] **Medición de `Frontend`**: build estático servido por CDN o reverse proxy con cache.

---

## C. Endurecimiento (recomendable antes del lanzamiento)

- [x] **Rate limiting** global por IP (300 req/min) + políticas específicas en
      login, recuperación y creación de leads/contacto (evitar abuso/spam).
- [x] **Cabeceras de seguridad** (HSTS, CSP, X-Content-Type-Options) en la respuesta HTTP.
- [x] **Revisión de rutas públicas** para que ninguna fuga información de borradores o leads.
- [ ] **Pruebas de carga** básica (leads y búsqueda) para conocer el techo del servidor.
- [x] **Playbook de rollback documentado** (sección C.5).

### C.5 Playbook de rollback por tags

Cada release se etiqueta con una tag semántica (`v1.2.3`) sobre `main`. Rollback =
re-deploy de una tag anterior. Pasos:

1. **Identificar el release a restaurar**: `git tag -l 'v*' --sort=-version:refname`
   (el anterior a la última tag que causó el incidente).
2. **Revert del código (si hace falta)**:
   - Hotfix: `git checkout -b hotfix/<descripcion> <tag-anterior>` → corregir → PR → merge a `main`.
   - Sin corrección: mantener la rama pero desplegar la imagen/artefacto de la tag anterior.
3. **Redeploy del artefacto anterior**:
   - **Docker**: `docker compose -f docker-compose.prod.yml pull` con la tag anterior
     (imágenes versionadas con la tag de git) y `docker compose up -d --force-recreate api`.
   - **Frontend estático**: restaurar el `dist` de la tag anterior (artefacto del CI de esa tag).
4. **Migraciones**: si el release fallido incluyó una migración de base de datos,
   **no** se revierte la base automáticamente. Evaluar con `dotnet ef migrations list`
   si la migración ya corrió y aplicar el fix en un hotfix (nunca eliminar migraciones aplicadas).
5. **Verificar**: `/health` y `/health/ready` OK; smoke test del flujo afectado
   (login, pago, carga de vitrina).
6. **Documentar**: fecha, tag anterior/fallida, causa, y acción correctiva en el repo
   (issue o nota del release).

---

## Estado actual (al día)

- [x] Frontend completo: vitrina, ficha, vendedor público, comprador (favoritos/historial/
      leads/comparador), panel dealer, panel vendedor y panel admin.
- [x] **Frontend 100 % migrado a TanStack Query** (ninguna página llama servicios directo).
- [x] **Code splitting**: rutas con `lazy()` + `<Suspense>`; chunk principal ~308 kB (antes ~711 kB).
- [x] **Logo optimizado**: `AutoMarketRD_Logo.svg` de 251 kB a ~70 kB (-72 %).
- [x] Suscripciones y pagos PayPal (link + `confirmar-pago` idempotente + webhook).
- [x] Backend: **340 tests** y compilación `net10.0`.
- [x] **CI** verde en GitHub Actions (push/PR a `main` y `develop`): .NET 10.0.x + Node 20,
      build+test backend y build+lint frontend.
- [x] **Tests frontend** (unidad de utilidades/hooks con Vitest) — 33 tests.
- [x] Flow de pago y reembolso PayPal verificados en staging (migraciones
      `AgregarMonedaAAnuncios` y `AgregarCaptureIdPayPalPagos` aplicadas; `/health` y
      `/health/ready` OK; login admin y `GET /api/admin/pagos` respondiendo con datos).

### Preparación para producción (Fases 1–6)

- [x] **Fase 1 — Logs y entorno**: sin datos sensibles en logs, `UseForwardedHeaders`
      consolidado, SMTP con placeholder.
- [x] **Fase 2 — Base de producción**: `docker-compose.prod.yml` (db, api, frontend,
      db-backup), `.env.prod`/`.env.example`, fail-fast de PayPal en producción.
- [x] **Fase 3 — Seguridad backend**: excepciones sin leaks (mensajes genéricos en prod
      para errores internos), GUID en logos, rate limiting por IP, cabeceras de seguridad
      (HSTS, X-Content-Type-Options, X-Frame-Options) en la API.
- [x] **Fase 4 — Frontend**: `VITE_API_URL=/api` para producción (mismo origen, proxy
      nginx), gitignore con case correcto, token JWT httpOnly verificado, rutas públicas
      sin fugas de borradores/leads.
- [x] **Fase 5 — PayPal Live**: caché del token OAuth, validación Mode/UrlBase en todos
      los entornos, runbook de verificación (`docs/verificar-paypal.md`).
- [x] **Fase 6 — Docs**: `docs/DEPLOY.md` (paso a paso del despliegue) y este checklist
      al día.

---

## ¿Cuándo está "listo para producción"?

Si marcas todas las casillas de **A** (funciones pendientes) y **B** (operación y claves),
y dejas **C** tan cubierta como puedas, el proyecto se considera listo.

Prioridad de ejecución sugerida: primero **A** (funciones de producto), luego **B**
(deploy real de un solo golpe) y al final **C** mientras tanto puedes ir cerrando ítems
de **C** en paralelo.
