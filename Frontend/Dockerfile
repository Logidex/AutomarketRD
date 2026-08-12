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
