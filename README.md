# AutoMarketRD

Plataforma de compraventa de vehículos con roles de usuario y dealer, autenticación JWT, pagos con PayPal y más.

## 📂 Estructura del Proyecto
AutoMarketRDspn/
├── backend/ # API .NET 8 + PostgreSQL
│ ├── AutoMarket.API/
│ ├── AutoMarket.Application/
│ ├── AutoMarket.Core/
│ ├── AutoMarket.Infrastructure/
│ ├── docker-compose.dev.yml
│ ├── docker-compose.prod.yml
│ ├── .env.dev
│ ├── .env.prod
│ └── .env.example # Template de variables
├── frontend/ # React + Vite + TypeScript (próximamente)
├── .gitignore
└── README.md

## 🛠️ Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Node.js 18+](https://nodejs.org/) (para el frontend)
- [Git](https://git-scm.com/)

## 🚀 Desarrollo Local

### Backend

#### 1. Configurar variables de entorno

En `backend/`, crea `.env.dev` basado en `.env.example`:

```bash
# Copia y edita backend/.env.example a backend/.env.dev
# Asegúrate de cambiar contraseñas y secrets
```

#### 2. Levantar entorno de desarrollo

```bash
cd backend

# Levantar solo desarrollo
docker compose -f docker-compose.dev.yml --env-file .env.dev up -d

# Ver logs en tiempo real
docker compose -f docker-compose.dev.yml --env-file .env.dev logs -f api

# Detener
docker compose -f docker-compose.dev.yml --env-file .env.dev down
```

**Endpoints:**
- API: `http://localhost:8080`
- Health check: `http://localhost:8080/health/ready`
- Swagger/Scalar: `http://localhost:8080/scalar`

**Base de datos (solo dev):**
- Host: `localhost`
- Puerto: `5432`
- Usuario: `postgres`
- Password: (ver `.env.dev`)
- Base de datos: `AutoMarketDB`

#### 3. Levantar entorno de producción (local)

```bash
cd backend

# Detener desarrollo si está corriendo
docker compose -f docker-compose.dev.yml --env-file .env.dev down

# Levantar producción
docker compose -f docker-compose.prod.yml --env-file .env.prod up -d

# Ver logs
docker compose -f docker-compose.prod.yml --env-file .env.prod logs -f api

# Detener
docker compose -f docker-compose.prod.yml --env-file .env.prod down
```

**Endpoints:**
- API: `http://localhost` (puerto 80)
- Health check: `http://localhost/health/ready`

**Nota:** En producción, el puerto 5432 de la BD **no está expuesto** por seguridad.

#### 4. Cambiar entre entornos

```bash
cd backend

# De dev a prod
docker compose -f docker-compose.dev.yml --env-file .env.dev down
docker compose -f docker-compose.prod.yml --env-file .env.prod up -d

# De prod a dev
docker compose -f docker-compose.prod.yml --env-file .env.prod down
docker compose -f docker-compose.dev.yml --env-file .env.dev up -d
```

#### 5. Aplicar migraciones (si es necesario)

Las migraciones se aplican automáticamente al iniciar, pero si necesitas hacerlo manualmente:

```bash
cd backend
dotnet ef database update --project AutoMarket.Infrastructure --startup-project AutoMarket.API
```

#### 6. Build y tests

```bash
cd backend

# Build
dotnet build

# Tests (si los tienes)
dotnet test
```

### Frontend (Próximamente)

```bash
cd frontend
npm install
npm run dev
```

**URL:** `http://localhost:5173`

## 📡 Endpoints de la API

### Autenticación

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/auth/registro` | Registrar nuevo usuario |
| POST | `/api/auth/login` | Login y obtener JWT |

### Anuncios

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/anuncios` | Obtener todos los anuncios | No |
| GET | `/api/anuncios/{id}` | Obtener anuncio por ID | No |
| POST | `/api/anuncios` | Crear nuevo anuncio | Sí |
| PUT | `/api/anuncios/{id}` | Actualizar anuncio | Sí |
| DELETE | `/api/anuncios/{id}` | Eliminar anuncio | Sí |
| PATCH | `/api/anuncios/{id}/publicar` | Publicar anuncio | Sí |

### Dealers

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/dealers/{id}` | Obtener perfil público de dealer | No |
| PUT | `/api/dealers/me` | Actualizar mi perfil | Sí (Dealer) |

### Favoritos

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/favoritos` | Obtener mis favoritos | Sí |
| POST | `/api/favoritos/anuncio/{id}` | Agregar a favoritos | Sí |
| DELETE | `/api/favoritos/anuncio/{id}` | Quitar de favoritos | Sí |

### Leads

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| POST | `/api/leads` | Crear lead (contactar vendedor) | No |
| GET | `/api/leads/anuncio/{anuncioId}` | Ver leads de un anuncio | Sí |
| GET | `/api/leads/mis-leads` | Ver mis leads (dealer) | Sí (Dealer) |

### Pagos

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| POST | `/api/pagos/generar-link` | Generar link de pago PayPal | Sí |
| POST | `/api/pagos/webhook` | Webhook de PayPal | No |

## 🔧 Tecnologías

### Backend

- .NET 8
- Entity Framework Core 8
- PostgreSQL 16
- JWT Authentication
- Docker + Docker Compose
- AutoMapper
- FluentValidation
- Scalar (API docs)

### Frontend (Próximamente)

- React 18
- Vite
- TypeScript
- Tailwind CSS
- React Router
- Axios / React Query

## 📝 Contribuir

1. Crear rama desde `develop`: `git checkout -b feature/nombre-feature`
2. Hacer cambios y commits descriptivos
3. Crear Pull Request a `develop`

## 🔒 Seguridad

- **NUNCA** commitear `.env.dev` o `.env.prod` al repo
- Usar `.env.example` como template
- Los secrets van en GitHub Secrets para CI/CD

## 📄 Licencia

© 2025 Erick Pérez. Todos los derechos reservados.

Este es un proyecto privado. No se permite el uso, reproducción,
distribución o modificación sin autorización expresa del autor.

## 👤 Autor

Erick - [[Tu GitHub](https://github.com/Logidex)/[LinkedIn](https://www.linkedin.com/in/erick-hipolito-lopez-genao-8172b9271/)]

---

**¿Problemas?** Revisa los logs con `docker compose logs -f api` o abre un issue.
