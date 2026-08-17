# AutoMarketRDSpn

AutoMarketRDSpn es una plataforma de compraventa de vehículos pensada para conectar compradores, vendedores y dealers en un mismo lugar. El proyecto está organizado como una solución full stack: un backend en .NET 10 con PostgreSQL y Docker, y un frontend en React + Vite + TypeScript + Tailwind.

Este README está escrito para que cualquier nuevo desarrollador pueda entender el proyecto rápidamente: qué hace, cómo está dividido, cómo correrlo en local y qué partes están listas hoy.

---

## Objetivo del proyecto

La idea de AutoMarketRDSpn es permitir que un usuario pueda:

- Registrarse e iniciar sesión.
- Publicar anuncios de vehículos.
- Ver anuncios publicados por otros usuarios.
- Guardar anuncios como favoritos.
- Contactar vendedores a través de leads.
- Gestionar un perfil de dealer.
- Realizar flujos de pago relacionados con servicios o publicaciones, según la lógica del proyecto.

El objetivo principal es construir un marketplace automotriz con una base técnica limpia, escalable y fácil de mantener.

---

## Características principales

- Autenticación con JWT.
- Roles de usuario y dealer.
- CRUD de anuncios de vehículos.
- Sistema de favoritos.
- Gestión de leads para contacto entre usuarios y vendedores.
- Integración con PayPal para pagos.
- Arquitectura backend separada por capas.
- Frontend moderno con React, TypeScript y Tailwind.
- Uso de Docker para desarrollo y despliegue local.

---

## Estructura general

```txt
AutoMarketRDSpn/
├── Backend/
│   ├── AutoMarket.API/
│   ├── AutoMarket.Application/
│   ├── AutoMarket.Core/
│   ├── AutoMarket.Infrastructure/
│   ├── docker-compose.dev.yml
│   ├── docker-compose.staging.yml
│   ├── docker-compose.prod.yml
│   ├── .env.dev
│   ├── .env.staging
│   ├── .env.prod
│   ├── .env.staging.example
│   └── .env.example
├── Frontend/
│   ├── src/
│   ├── public/
│   ├── package.json
│   └── vite.config.ts
├── .gitignore
└── README.md
```

### Qué significa cada parte

- **Backend/**: contiene toda la API y la lógica del servidor.
- **Frontend/**: contiene la interfaz visual que usará el usuario final.
- **README.md**: guía principal del proyecto.
- **.gitignore**: archivos que no deben subirse al repositorio.

---

## Arquitectura del backend

El backend está dividido en capas para mantener el código organizado y fácil de crecer.

### AutoMarket.API

Es la capa de entrada del sistema. Aquí viven los controladores, configuración de la app, middleware y el punto de arranque de la API.

### AutoMarket.Application

Contiene la lógica de aplicación. Aquí van casos de uso, servicios de aplicación, validaciones y reglas que orquestan procesos.

### AutoMarket.Core

Contiene las entidades principales del dominio y conceptos centrales del negocio.

### AutoMarket.Infrastructure

Se encarga de la persistencia, acceso a datos, configuración de Entity Framework, repositorios y otras integraciones técnicas.

Esta separación ayuda a mantener la base limpia y preparada para crecer sin mezclar demasiadas responsabilidades.

---

## Tecnologías usadas

### Backend

- .NET 10
- Entity Framework Core 10
- PostgreSQL 16
- JWT en cookie HttpOnly
- Docker y Docker Compose
- Serilog (consola + archivo)
- Scalar para documentación de API
- xUnit + 339 tests de backend

### Frontend

- React 19
- Vite
- TypeScript
- Tailwind CSS
- React Router 7
- Axios / React Query
- Vitest (25 tests)

---

## Requisitos previos

Antes de correr el proyecto, asegúrate de tener instalado lo siguiente:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Node.js 20+](https://nodejs.org/)
- [Git](https://git-scm.com/)

---

## Cómo correr el proyecto

## Backend

### 1. Configurar variables de entorno

Dentro de `Backend/`, crea un archivo `.env.dev` basado en `.env.example`.

Ejemplo:

```bash
# Copiar plantilla
cp .env.example .env.dev
```

Luego ajusta las credenciales, secretos y valores necesarios para tu entorno local.

### 2. Levantar el backend en desarrollo

```bash
cd Backend
docker compose -f docker-compose.dev.yml --env-file .env.dev up -d
```

### 3. Ver logs

```bash
docker compose -f docker-compose.dev.yml --env-file .env.dev logs -f api
```

### 4. Detener el backend

```bash
docker compose -f docker-compose.dev.yml --env-file .env.dev down
```

### 5. Endpoints locales del backend

- API: `http://localhost:8080`
- Health check: `http://localhost:8080/health/ready`
- Scalar: `http://localhost:8080/scalar`

### 6. Base de datos en desarrollo

- Host: `localhost`
- Puerto: `5432`
- Usuario: `postgres`
- Base de datos: `AutoMarketDB`
- Password: la que definas en `.env.dev`

### 7. Levantar el backend en modo producción local

Si quieres probar el entorno de producción en tu máquina:

```bash
cd Backend
docker compose -f docker-compose.prod.yml --env-file .env.prod up -d
```

Para detenerlo:

```bash
docker compose -f docker-compose.prod.yml --env-file .env.prod down
```

### 8. Levantar el backend en modo staging

El entorno de staging replica la configuración de producción pero con credenciales
de PayPal Sandbox y su propia base de datos. Es el entorno donde se prueban los
flujos de pago antes de salir a producción.

```bash
cd Backend
# Copiar la plantilla y editar las credenciales (todas son obligatorias)
cp .env.staging.example .env.staging

# Levantar (el api fallara al iniciar si falta cualquier variable requerida)
docker compose -f docker-compose.staging.yml --env-file .env.staging up -d
```

Ver logs:

```bash
docker compose -f docker-compose.staging.yml --env-file .env.staging logs -f api
```

Detener:

```bash
docker compose -f docker-compose.staging.yml --env-file .env.staging down
```

URLs locales de staging:

- Frontend: `http://localhost:5174`
- API: `http://localhost:8081`
- Health check: `http://localhost:8081/health/ready`

> **Importante**: el `docker-compose.staging.yml` exige todas las variables de
> entorno (usa `${VAR:?}` sin defaults). Completa `.env.staging` con credenciales
> reales de PayPal Sandbox (https://developer.paypal.com/dashboard/applications/sandbox),
> SMTP, S3/AWS y el usuario admin. Sin ellas el contenedor `api` no arranca.

### 9. Aplicar migraciones manualmente

Las migraciones suelen aplicarse automáticamente al iniciar, pero si necesitas ejecutarlas manualmente:

```bash
cd Backend
dotnet ef database update --project AutoMarket.Infrastructure --startup-project AutoMarket.API
```

### 10. Build y pruebas

```bash
cd Backend
dotnet build
dotnet test
```

---

## Frontend

### 1. Instalar dependencias

```bash
cd Frontend
npm install
```

### 2. Ejecutar en desarrollo

```bash
npm run dev
```

### 3. URL local

- Frontend: `http://localhost:5173`

### 4. Qué contiene hoy el frontend

El frontend está completo en sus flujos principales:

- Home con búsqueda (marca, modelo, tipo, rango de precio) y vitrina de destacados.
- Directorio de vehículos con filtros y paginación.
- Directorio de agencias (`/agencias`) con filtros por plan y verificadas.
- Comparador de vehículos y detalle de anuncio con leads.
- Registro, login (cookie HttpOnly), recuperación de contraseña y confirmación de correo.
- Paneles por rol: Vendedor, Dealer (dashboard, suscripciones y pagos PayPal) y Admin.
- Tema claro/oscuro.

### 5. Tests del frontend

```bash
cd Frontend
npm test        # Vitest (25 tests)
npm run lint    # ESLint
npm run build   # tsc + vite build
```

---

## Funcionalidades de la API

### Autenticación

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/auth/registrar` | Registrar nuevo usuario |
| POST | `/api/auth/login` | Iniciar sesión (cookie JWT HttpOnly) |
| POST | `/api/auth/logout` | Cerrar sesión |
| POST | `/api/auth/recuperar-password` | Solicitar código de recuperación |
| POST | `/api/auth/restablecer-password` | Restablecer contraseña con código |
| POST | `/api/auth/confirmar-correo` | Confirmar correo con token del enlace |
| POST | `/api/auth/reenviar-confirmacion` | Reenviar enlace de confirmación |

### Anuncios

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/anuncios` | Listar anuncios | No |
| GET | `/api/anuncios/{id}` | Obtener un anuncio por ID | No |
| GET | `/api/anuncios/buscar` | Búsqueda con filtros y paginación | No |
| GET | `/api/anuncios/destacados` | Anuncios destacados de la portada | No |
| POST | `/api/anuncios` | Crear un nuevo anuncio | Sí (Dealer/Vendedor) |
| PUT | `/api/anuncios/{id}` | Actualizar un anuncio | Sí (Dealer/Vendedor) |
| DELETE | `/api/anuncios/{id}` | Eliminar un anuncio | Sí (Dealer/Vendedor) |
| PATCH | `/api/anuncios/{id}/publicar` | Publicar un anuncio | Sí (Dealer/Vendedor) |
| PATCH | `/api/anuncios/{id}/estado` | Cambiar estado | Sí (Dealer/Vendedor) |
| POST | `/api/anuncios/{id}/imagenes` | Subir imágenes (PNG/JPEG, máx 10) | Sí (Dealer/Vendedor) |
| DELETE | `/api/anuncios/{id}/imagenes` | Eliminar imágenes | Sí (Dealer/Vendedor) |
| PUT | `/api/anuncios/{id}/foto-principal` | Establecer foto principal | Sí (Dealer/Vendedor) |
| PATCH | `/api/anuncios/{id}/destacar` | Destacar anuncio (requiere cupo del plan) | Sí (Dealer) |
| PATCH | `/api/anuncios/{id}/quitar-destacado` | Quitar destacado | Sí (Dealer) |
| POST | `/api/anuncios/{id}/registrar-vista` | Registrar vista del anuncio | No |

### Dealers y Agencias

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/dealers` | Listar agencias (búsqueda, verificadas, plan) | No |
| GET | `/api/dealers/{id}` | Obtener perfil público de un dealer | No |
| PUT | `/api/dealers/me` | Actualizar mi perfil de agencia | Sí (Dealer) |

### Planes y Suscripciones

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/planes` | Catálogo público de planes | No |
| POST | `/api/pagos/generar-link` | Generar link de pago con PayPal | Sí |
| POST | `/api/pagos/confirmar-pago` | Confirmar pago al regresar de PayPal | Sí |
| POST | `/api/pagos/webhook` | Webhook verificado de PayPal | No |

### Favoritos, Leads, Tickets, Contacto y Comparador

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET/POST/DELETE | `/api/favoritos` | Gestionar favoritos | Sí |
| POST | `/api/leads` | Crear lead para contactar un vendedor | No |
| GET | `/api/leads/mis-leads` | Leads como dealer | Sí (Dealer) |
| GET | `/api/leads/mis-contactos` | Contactos de mis anuncios | Sí (Dealer) |
| POST/GET | `/api/tickets` | Soporte: abrir y listar tickets | Sí (Dealer/Vendedor) |
| POST | `/api/tickets/{id}/mensajes` | Responder ticket | Sí (Dealer/Vendedor) |
| POST | `/api/contacto` | Formulario de contacto | No |
| GET | `/api/comparador` | Comparación de vehículos | No |
| GET | `/api/admin/*` | Panel de administración | Sí (Admin) |

> El catálogo completo de endpoints y sus DTOs está documentado con XML en los
> controladores y en la UI de Scalar en entorno de desarrollo (`/scalar`).

---

## Convenciones del proyecto

Estas reglas ayudan a que el código se mantenga entendible para cualquier persona que entre al proyecto.

### Backend

- Mantener las entidades y reglas del negocio en sus capas correctas.
- No mezclar acceso a datos con lógica de presentación.
- Usar DTOs para comunicación entre capas.
- Validar entradas antes de procesarlas.
- Mantener nombres claros y consistentes.

### Frontend

- Usar nombres descriptivos para componentes y archivos.
- Colocar páginas completas en `pages/`.
- Colocar piezas reutilizables en `components/`.
- Centralizar llamadas HTTP en `services/`.
- Mantener tipos en `types/`.
- Evitar lógica compleja directamente dentro del JSX cuando pueda moverse a funciones o hooks.

---

## Estado actual

### Backend

- API completa con arquitectura por capas (.NET 10 / EF Core 10).
- Autenticación JWT en cookie HttpOnly + BCrypt + rate limiting.
- Anuncios, destacados por plan, favoritos, leads, comparador y tickets de soporte.
- Suscripciones con PayPal (webhook verificado, idempotente y con validación de monto).
- Almacenamiento de imágenes en S3 privado con URLs firmadas.
- Docker para desarrollo, staging y producción + script de backup de la BD.
- 339 tests automatizados en CI.

### Frontend

- Flujos principales completos (catálogo, búsqueda, agencias, comparador, auth, paneles por rol).
- Tema claro/oscuro y responsive.
- 25 tests con Vitest en CI.

## Roadmap sugerido

- HTTPS/TLS y proveedor de hosting para producción.
- Reescrituras de imágenes y optimización de assets estáticos.
- Tests e2e.
- Refresh tokens o expiración de sesión deslizante.
- Documentación de operaciones (`docs/DEPLOY.md`).

---

## Contribuir

Si vas a trabajar en el proyecto:

1. Crea una rama desde `develop`.
2. Haz cambios pequeños y claros.
3. Usa commits descriptivos.
4. Abre un Pull Request hacia `develop`.

Ejemplo de rama:

```bash
git checkout -b feature/nueva-pantalla
```

---

## Seguridad

- Nunca subir `.env.dev`, `.env.staging` ni `.env.prod` al repositorio.
- Usar `.env.example` y `.env.staging.example` como plantillas.
- Guardar secrets en GitHub Secrets si se usa CI/CD.
- No exponer credenciales reales en el README.

---

## Licencia

© 2025 Erick Lopez. Todos los derechos reservados.

Este proyecto es privado. No se permite el uso, reproducción, distribución o modificación sin autorización expresa del autor.

---

## Autor

Erick Lopez

---

## Nota final

Este README está pensado para servir como guía de entrada al proyecto. La idea es que un nuevo desarrollador pueda entender en pocos minutos qué hace AutoMarketRDSpn, cómo levantarlo y dónde empezar a trabajar.
