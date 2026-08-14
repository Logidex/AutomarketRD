# Migración del token JWT: localStorage → cookie HttpOnly

Estado: **implementado** (frontend + backend + tests).

## Motivación

`react-doctor` detectaba `auth-token-in-web-storage` (2 warnings) porque el JWT se
guardaba en `localStorage["token"]`, accesible a cualquier script inyectado (XSS).

## Cambios

### Backend (AutoMarket.API)

- **Nuevo** `Helpers/AuthCookieHelper.cs`: establece/limpia la cookie
  `automarket_token` con `HttpOnly; Secure (no dev); SameSite=Lax; Path=/; Expires 2h`
  (mismo plazo que el token de `TokenService`).
- `Controllers/AuthController.cs`:
  - `Login`: ahora hace `Set-Cookie` con el JWT y el cuerpo solo devuelve
    `{ exito, mensaje, usuario }` (**el token ya no viaja en el body**).
  - Nuevo `POST /api/auth/logout`: elimina la cookie.
- `Controllers/UsuarioController.cs` (`ascender-rol`): rota la cookie con el nuevo
  token (nuevos claims de rol) y el cuerpo tampoco incluye token.
- `Program.cs` (CORS): en dev ya no usa `AllowAnyOrigin`; usa orígenes explícitos
  (`http://localhost:5173`, `http://127.0.0.1:5173`) con `AllowCredentials`.
  `AllowAnyOrigin + AllowCredentials` es inválido y rompería el envío de la cookie.

### Frontend (src)

- `services/api.ts`: `withCredentials: true`; se eliminó el interceptor que agregaba
  `Authorization: Bearer <token>` desde localStorage; el 401 limpia `user:v1`/`user`.
- `services/auth.service.ts`: se eliminaron `token`/`token_expira` de localStorage.
  `guardarSesion` solo guarda el usuario; `logout` limpia el usuario y llama a
  `/api/auth/logout` (fire-and-forget) para borrar la cookie;
  `isAuthenticated()` se basa en el usuario guardado; se eliminó `getToken()`.
- `types/auth.types.ts`: `AuthResponse` ya no tiene `token`.
- `pages/Login.tsx`: ya no exige `response.token`.
- `utils/jwt.util.ts`: `getUserIdFromToken()` lee `usuarioId` del usuario guardado
  (`user:v1`, fallback `user`) en vez de decodificar el token (inalcanzable desde JS).

## Seguridad

- **XSS**: el JWT no es legible por JS (`HttpOnly`).
- **CSRF**: `SameSite=Lax` bloquea el envío de la cookie en solicitudes cross-site
  (GET de navegación top-level exceptuadas). El SPA es del mismo origen. No se
  requieren headers antiforgery adicionales para este despliegue.
- **Robo**: el token sigue siendo válido hasta expirar (2h) aunque se cierre sesión
  (JWT sin revocación server-side). Comportamiento equivalente al anterior.

## Dev

- Iniciar el frontend con `npm run dev` (puerto 5173) y el API en `localhost:8080`.
  La cookie no lleva `Secure` en dev porque no hay HTTPS.
- En producción: `Cors:AllowedOrigins` (config) + `Secure` automático (no dev).

## Tests

- Backend: `AuthControllerTests` valida que login establezca cookie HttpOnly,
  que el body no tenga `Token`, y que `logout` borre la cookie.
- Frontend: `jwt.util.test.ts` cubre la lectura de `usuarioId` desde `user:v1`.

## Resultado react-doctor

De 9 issues → 7 (todos falsos positivos/decisiones documentados en
`docs/issues-react-doctor-pendientes.md`). `auth-token-in-web-storage` resuelto.
