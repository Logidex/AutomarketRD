# =========================================
# ETAPA 1: BUILD (Node + Vite)
# =========================================
FROM node:22-alpine AS build
WORKDIR /app

# Instalar dependencias (usa package-lock.json para builds reproducibles)
COPY package.json package-lock.json ./
RUN npm ci

# Copiar código fuente y compilar
COPY . .

# En producción el frontend se sirve desde nginx en el mismo origen,
# así que la API se invoca por ruta relativa /api (nginx hace el proxy).
ARG VITE_API_URL=/api
ENV VITE_API_URL=$VITE_API_URL

# Mantenimiento de pagos: deshabilitado por defecto; el stack E2E lo activa.
ARG VITE_PAGOS_HABILITADOS=false
ENV VITE_PAGOS_HABILITADOS=$VITE_PAGOS_HABILITADOS

# Anuncios: desactivados por defecto (evita ads involuntarios en e2e/builds
# sin configurar). docker-compose.staging.yml los activa con sus ARGs. El valor
# de ENV (proceso) tiene prioridad sobre los .env de Vite.
ARG VITE_ADSENSE_ENABLED=false
ENV VITE_ADSENSE_ENABLED=$VITE_ADSENSE_ENABLED
ARG VITE_ADSENSE_CLIENT=
ENV VITE_ADSENSE_CLIENT=$VITE_ADSENSE_CLIENT

RUN npm run build

# =========================================
# ETAPA 2: SERVIDOR ESTÁTICO (nginx)
# =========================================
FROM nginx:1.27-alpine AS final

# Configuración de sitio: estáticos + reverse proxy a la API
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copiar el build de Vite
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD wget -q -O /dev/null http://127.0.0.1/ || exit 1
