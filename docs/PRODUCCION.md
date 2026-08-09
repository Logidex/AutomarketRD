# Checklist de Producción — AutoMarketRD

Este documento es la *guía única* para saber **qué falta para salir a producción**.
Cuando termines una tarea, marca su casilla con `[x]`. La intención es que quede claro
qué bloquea un lanzamiento y qué es solo recomendable.

---

## A. Funciones pendientes (producto)

> Estas son funcionalidades que aún no existen o están a medias. Bloquean "tener un
> marketplace completo", pero no bloquean al panel interno del dealer.

- [ ] **Home / vitrina pública** — catálogo de vehículos visible para visitantes
      (hoy el frontend público es placeholder; la API de búsqueda ya existe).
- [ ] **Detalle de vehículo para compradores** — ficha pública con fotos, equipo,
      descripción y botón "Contactar" (crea el lead).
- [ ] **Página pública del vendedor (`/vendedor/:id`)** — perfil público con anuncios.
- [ ] **Frontend de comprador funcional** — login/registro del comprador, favoritos,
      historial, creación de leads desde la ficha.
- [ ] **Panel de administración (frontend)** — el backend admin existe (`AdminController`),
      la UI administrativa no.
- [ ] **Flujo de pago verificado** — el flujo funciona en local (retorno `confirmar-pago`),
      falta probarlo contra una cuenta PayPal real.
- [ ] **Correos operativos** — confirmaciones de registro, lead nuevo y recordatorio de
      renovación llegando realmente a la bandeja (depende del SMTP real, sección B).
- [ ] **Automatización E2E (recomendado)** — Playwright que recorra registrar → publicar →
      comprar plan; así el CI valida la UX, no solo el build.

---

## B. Requisitos de operación y claves (obligatorio para lanzar)

> Ninguno de estos es "código", pero sin ellos la API no sirve de verdad en producción.
> Varios de estos se prueban solo contra servicios reales.

### B.1 Credenciales reales (hoy son placeholders en `.env`/user-secrets)
- [ ] **Base de datos**: cambiar `POSTGRES_PASSWORD` y la `CONNECTION_STRING` por un secreto fuerte.
- [ ] **JWT**: secreto largo y rotado (min 32+ chars, no el de ejemplo).
- [ ] **SMTP**: host/puerto/usuario/password reales.
- [ ] **AWS S3**: access key, secret key, bucket y región; bucket privado con URLs firmadas para las fotos.
- [ ] **PayPal**: pasar de `sandbox` a **producción** (`PAYPAL_URL_BASE=https://api-m.paypal.com`),
      `CLIENT_ID`/`CLIENT_SECRET` reales, y configurar el **webhook** (B.2).
- [ ] **Admin inicial**: email/contraseña reales y `ADMIN_ROTATE_PASSWORD=false`.
- [ ] **CORS**: permitir solo el dominio real (`https://tudominio.com`), no `*`.

### B.2 El entpo de red
- [ ] **Dominio + HTTPS** (proxy inverso / load balancer, ej. Nginx, Caddy, Cloudflare, un VPS).
- [ ] **URLs de retorno/cancelación PayPal** apuntando al dominio HTTPS real
      (`PAYPAL_RETURN_URL`/`PAYPAL_CANCEL_URL`).
- [ ] **Webhook de PayPal operativo**: necesita URL pública (https://tudominio.com/api/pagos/webhook)
      y el `PAYPAL_WEBHOOK_ID` verificado.
- [ ] **Base de datos** en servicio gestionado o VPS con backup (sección B.4).

### B.3 Despliegue de la API
- [ ] **Migraciones**: `dotnet ef database update` como paso del deploy (no auto-migrate en prod).
- [ ] **Seeder desactivado**: confirmar que en `Release` no se crea/rotada datos ni el admin (gate por
      `Development`/`Seeder:Enabled`).
- [ ] **Swagger/Scalar cerrado**: verificado en producción (response 404).
- [ ] **Publicación optimizada**: `dotnet publish -c Release` con runtime-publish (self-contained o framework).
- [ ] **Secretos no versionados**: config vía entorno/Secrets Manager, nunca en el repo.

### B.4 Operación día a día
- [ ] **Backups automáticos** de la base de datos (diarios + retención).
- [ ] **Monitoreo/logs**: recoger logs con correlación (Serilog claro o OpenTelemetry/Application Insights)
      y alertas de errores.
- [ ] **Health checks conectados**: `/health` y `/health/ready` expuestos al balanceador/reverse proxy.
- [ ] **Medición de `Frontend`**: build estático servido por CDN o reverse proxy con cache.

---

## C. Endurecimiento (recomendable antes del lanzamiento)

> Son mejoras que bajan el riesgo. Pueden ir después del lanzamiento, pero mejor antes.

- [ ] **Rate limiting** en login, registro y creación de leads (evitar abuso/spam).
- [ ] **Cabeceras de seguridad** (HSTS, CSP, X-Content-Type-Options) en la respuesta HTTP.
- [ ] **Revisión de rutas públicas** para que ninguna fuga información de borradores o leads.
- [ ] **Pruebas de carga** básica (leads y búsqueda) para conocer el techo del servidor.
- [ ] **Playbook de rollback** documentado (restaurar backup + release anterior).

---

## Estado actual (al crear este documento)

- [x] Panel dealer: crear/editar anuncios, fotos S3, publicación con cupo y 5 fotos,
      cambio de estado validado, leads y vistas solo en publicado.
- [x] Suscripciones y pagos PayPal (link + `confirmar-pago` idempotente + webhook como respaldo).
- [x] Downgrade de plan validando inventario activo.
- [x] Runtime endurecido: Swagger/Scalar y Seeder solo en `Development`; sin `AddAuthorization` duplicado.
- [x] Backend: **197 / 197 tests** (incluye controllers y servicios del dealer).
- [x] Frontend: build + lint limpios.
- [x] **CI** verde en GitHub Actions (push/PR a `master`): build + tests backend y build+lint frontend.

---

## ¿Cuándo está "listo para producción"?

Si marcas todas las casillas de **A** (funciones pendientes) y **B** (operación y claves),
y dejas **C** tan cubierta como puedas, el proyecto se considera lista.

Prioridad de ejecución sugerida: primero **A** (funciones de producto para que tenga sentido
tener, luego **B** (con el deploy real de un solo golpe) y al final **C** mientras tanto
puedes ir cerrando ítems de **C** en paralelo.