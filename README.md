# AutoMarketRDSpn

AutoMarketRDSpn es una plataforma de compraventa de vehículos pensada para conectar compradores, vendedores y dealers en un mismo lugar. El proyecto está organizado como una solución full stack: un backend en .NET 8 con PostgreSQL y Docker, y un frontend en React + Vite + TypeScript + Tailwind.

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
│   ├── docker-compose.prod.yml
│   ├── .env.dev
│   ├── .env.prod
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

- .NET 8
- Entity Framework Core 8
- PostgreSQL 16
- JWT Authentication
- Docker y Docker Compose
- AutoMapper
- FluentValidation
- Scalar para documentación de API

### Frontend

- React 18
- Vite
- TypeScript
- Tailwind CSS
- React Router
- Axios / React Query

---

## Requisitos previos

Antes de correr el proyecto, asegúrate de tener instalado lo siguiente:

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Node.js 18+](https://nodejs.org/)
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

### 8. Aplicar migraciones manualmente

Las migraciones suelen aplicarse automáticamente al iniciar, pero si necesitas ejecutarlas manualmente:

```bash
cd Backend
dotnet ef database update --project AutoMarket.Infrastructure --startup-project AutoMarket.API
```

### 9. Build y pruebas

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

El frontend está en una etapa inicial, pero ya tiene la base necesaria para crecer de forma ordenada:

- Estructura inicial en React.
- Configuración con Vite.
- Tipado con TypeScript.
- Estilos base con Tailwind.
- Carpetas preparadas para componentes, páginas, servicios, tipos, hooks y utilidades.

### 5. Cómo debe crecer el frontend

La idea es que la interfaz se vaya construyendo por partes:

- Páginas principales en `pages/`.
- Componentes reutilizables en `components/`.
- Llamadas a API en `services/`.
- Tipos en `types/`.
- Lógica reutilizable en `hooks/` y `utils/`.

---

## Funcionalidades de la API

### Autenticación

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/auth/registro` | Registrar nuevo usuario |
| POST | `/api/auth/login` | Iniciar sesión y obtener JWT |

### Anuncios

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/anuncios` | Obtener todos los anuncios | No |
| GET | `/api/anuncios/{id}` | Obtener un anuncio por ID | No |
| POST | `/api/anuncios` | Crear un nuevo anuncio | Sí |
| PUT | `/api/anuncios/{id}` | Actualizar un anuncio | Sí |
| DELETE | `/api/anuncios/{id}` | Eliminar un anuncio | Sí |
| PATCH | `/api/anuncios/{id}/publicar` | Publicar un anuncio | Sí |

### Dealers

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/dealers/{id}` | Obtener perfil público de un dealer | No |
| PUT | `/api/dealers/me` | Actualizar mi perfil | Sí (Dealer) |

### Favoritos

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/favoritos` | Obtener mis favoritos | Sí |
| POST | `/api/favoritos/anuncio/{id}` | Agregar un anuncio a favoritos | Sí |
| DELETE | `/api/favoritos/anuncio/{id}` | Quitar un anuncio de favoritos | Sí |

### Leads

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| POST | `/api/leads` | Crear lead para contactar un vendedor | No |
| GET | `/api/leads/anuncio/{anuncioId}` | Ver leads de un anuncio | Sí |
| GET | `/api/leads/mis-leads` | Ver mis leads como dealer | Sí (Dealer) |

### Pagos

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| POST | `/api/pagos/generar-link` | Generar link de pago con PayPal | Sí |
| POST | `/api/pagos/webhook` | Recibir webhook de PayPal | No |

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

- API base construida.
- Estructura por capas definida.
- Autenticación JWT implementada.
- Gestión de anuncios disponible.
- Favoritos, leads y pagos contemplados.
- Docker configurado para desarrollo y producción.

### Frontend

- Base creada.
- Organización inicial clara.
- Preparado para crecer en pantallas y componentes.
- Aún en fase temprana de implementación.

---

## Roadmap sugerido

Lo siguiente que conviene construir es:

- Pantalla de inicio con navegación clara.
- Login y registro en el frontend.
- Listado de anuncios.
- Detalle de vehículo.
- Formularios para crear y editar anuncios.
- Vista de favoritos.
- Perfil de dealer.
- Consumo real de la API desde el frontend.
- Manejo de loading, error y estados vacíos.

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

- Nunca subir `.env.dev` ni `.env.prod` al repositorio.
- Usar `.env.example` como plantilla.
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
