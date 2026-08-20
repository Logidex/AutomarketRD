# Verificación del flujo PayPal — Live (Fase 5)

Runbook para verificar localmente el flujo completo de pago contra **PayPal Live**
antes del lanzamiento. La API ya valida la coherencia `PayPal:Mode`/`PayPal:UrlBase`
en todos los entornos (fail-fast): si `Mode=Live` pero `UrlBase` apunta a Sandbox,
la API no arranca.

## 1. Crear la app Live en developer.paypal.com

Esto lo haces tú (requiere cuenta PayPal de negocio):

1. [developer.paypal.com](https://developer.paypal.com) → **Apps & Credentials**.
2. Alterna el toggle a **Live** y crea una app (o usa la existente).
3. Copia **Client ID** y **Secret** → van a `PAYPAL_CLIENT_ID` / `PAYPAL_CLIENT_SECRET`.
4. Crea el **Webhook** (Apps & Credentials → Webhooks → Add Webhook):
   - URL: `https://tudominio.com/api/pagos/webhook`
   - Eventos: al menos `PAYMENT.CAPTURE.COMPLETED` y `PAYMENT.CAPTURE.DENIED`.
   - Copia el **Webhook ID** → `PAYPAL_WEBHOOK_ID`.
5. Nota: PayPal exige URLs `https` en las apps Live (también para
   `PAYPAL_RETURN_URL`/`PAYPAL_CANCEL_URL`). Para probar en local puedes usar un
   túnel (`ngrok http 8080` o `cloudflared tunnel`) y apuntar las URLs al túnel.

## 2. Configuración local (modo Live)

En `Backend/.env.dev` (o variables de entorno del proceso):

```dotenv
PAYPAL_MODE=Live
PAYPAL_URL_BASE=https://api-m.paypal.com
PAYPAL_CLIENT_ID=TU_CLIENT_ID_PROD
PAYPAL_CLIENT_SECRET=TU_CLIENT_SECRET_PROD
PAYPAL_RETURN_URL=https://TU_DOMINIO_O_TUNEL/pago-exitoso
PAYPAL_CANCEL_URL=https://TU_DOMINIO_O_TUNEL/pago-cancelado
PAYPAL_WEBHOOK_ID=TU_WEBHOOK_ID_PROD
PAGO_TASA_CAMBIO_RD_USD=0.017
```

La API **no arrancará** si:
- `PAYPAL_MODE=Live` pero `PAYPAL_URL_BASE` no es `https://api-m.paypal.com`.
- `PAYPAL_MODE=Sandbox` pero `PAYPAL_URL_BASE` no es `https://api-m.sandbox.paypal.com`.
- En `Production` además exige `Mode=Live` + URL Live.

Para un **pago mínimo** (evitar gastar de más): crea un plan con precio pequeño o
usa `PAGO_TASA_CAMBIO_RD_USD` para reducir el monto en USD.

## 3. Flujo a verificar (local)

Levanta la API (`dotnet run` en `Backend/AutoMarket.API`) y el frontend
(`npm run dev` en `Frontend`). Con un dealer logueado:

1. **Crear orden**: `POST /api/pagos/generar-link`
   `{ "nombrePlan": "Pro", "ciclo": "Mensual" }` → devuelve `{ url, monto, moneda }`.
   - El monto USD debe coincidir con `precioPlanRD * tasa` redondeado a 2 decimales.
2. **Aprobar en PayPal**: abrir `url`, loguearse con la cuenta **Live** y pagar.
   - PayPal redirige a `pago-exitoso?token=<ORDER_ID>` (retorna `PAYPAL_RETURN_URL`).
3. **Confirmar**: el frontend llama `POST /api/pagos/confirmar-pago` con
   `{ "orderId": "<ORDER_ID>" }` (idempotente).
   - Debe quedar `exito=true`; la suscripción del dealer pasa al plan comprado.
4. **Webhook** (requiere URL pública): con ngrok/cloudflared, revisa en el dashboard
   de PayPal que el evento llegue a `https://tudominio.com/api/pagos/webhook` y que
   la firma se valide (`verification_status=SUCCESS` en logs).
   - Si el webhook no llega (local sin túnel), `confirmar-pago` cubre la activación.
5. **Idempotencia**: repetir `confirmar-pago` con el mismo `ORDER_ID` → `yaProcesado=true`,
   no crea pago duplicado.
6. **Reembolso** (admin): en `/admin/pagos`, reembolsar la suscripción creada.
   - `POST /api/admin/pagos/{id}/reembolsar` → PayPal `POST /v2/payments/captures/{id}/refund`.
   - El pago pasa a estado `Reembolsado` y la suscripción se revierte.

## 4. Qué revisar en los logs

- `Link de PayPal generado correctamente ...` (generar-link).
- `Pago PayPal confirmado y suscripción activada ...` (confirmar-pago).
- `Webhook PayPal validado correctamente ...` y `Resultado de captura PayPal ...`.
- Errores de reembolso: `PayPal rechazó el reembolso (HTTP <status>)` — **sin**
  cuerpo crudo de PayPal (endurecido en Fase 3).

## 5. Comprobaciones automáticas

- `dotnet test` incluye `PayPalServiceTests` (23 tests): creación de orden, captura,
  webhook, reembolso y **caché del OAuth token** (una sola llamada a `/v1/oauth2/token`
  por sesión). Ejecutar:
  ```
  cd Backend
  dotnet test AutoMarketRD.sln --filter "FullyQualifiedName~PayPal"
  ```

## Notas de seguridad

- El token OAuth se cachea en memoria con margen de 60 s y renovación bajo lock.
- No se exponen las respuestas crudas de PayPal al cliente.
- Nunca commitees `PAYPAL_CLIENT_SECRET` ni el `Webhook ID` (usar `.env.*`/secrets).