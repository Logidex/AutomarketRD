# Baseline de carga — AutoMarketRD staging

Primera medición de referencia sobre el VPS de Oracle (misma clase de
hardware que se prevé para producción). Ejecutada con k6 v0.54.0 desde el
propio servidor contra la API directamente (`http://localhost:8081`),
sin pasar por el túnel de Cloudflare, con
`RATE_LIMIT_GLOBAL=20000` para no tropezar con el límite anti-abuso.

- **Fecha**: 2026-08-22
- **Commit bajo prueba**: `ac446a6` (staging)
- **Hardware**: VM Oracle gratuita, x86_64, ~1 GB RAM
- **Escenario**: rampa 0→20→50 VUs sostenidos 2 min → 0 (5m30s totales)
- **Carga**: mezcla de `GET /api/anuncios/buscar` (páginas 1-3,
  tamanoPagina=12) y `GET /api/planes`

## Resultados

| Métrica | Valor | Threshold | Estado |
|---|---|---|---|
| Requests totales | 36 390 | — | — |
| Fallos HTTP | **0.00 %** | < 1 % | ✅ |
| Checks pasados | 100 % (54 585) | — | ✅ |
| Throughput sostenido | 55 iter/s (~110 req/s) | — | — |
| `buscar` p95 | **785 ms** | < 1500 ms | ✅ |
| `planes` p95 | **496 ms** | < 800 ms | ✅ |
| Duración media global | 315 ms (mediana 296 ms) | — | — |
| Máximo observado | 3.09 s (pico de rampa) | — | — |

## Lecturas

- Con 50 usuarios simultáneos navegando el catálogo, el sistema responde
  **por debajo del segundo en el p95** y sin un solo error: sobra margen
  para un lanzamiento temprano.
- El techo real está más arriba de los 50 VUs probados; la próxima ronda
  debería escalar a 100–150 VUs para encontrar el punto de degradación.
- El p95 de ~800 ms en búsqueda viene dado por PostgreSQL en una VM con
  CPU compartida; índices ya existen. Si al crecer el catálogo sube,
  considerar paginación cursor-based y caché de búsquedas frecuentes.
- Cada imagen servida por `/api/archivos/{clave}` cuesta una consulta a
  BD (`ExisteFotoAsync`). Con el catálogo actual es despreciable; si el
  tráfico de imágenes crece, agregar IMemoryCache de 5 min sobre esa
  verificación.

## Cómo reproducir

```bash
# 1. Elevar el límite temporalmente (en el VM)
cd ~/automarket-staging/Backend
RATE_LIMIT_GLOBAL=20000 docker compose -f docker-compose.staging.yml \
  --env-file ../.env.staging up -d --force-recreate api

# 2. Correr la prueba
~/bin/k6 run -e BASE=http://localhost:8081 load/k6-busqueda.js

# 3. Restaurar el límite normal
docker compose -f docker-compose.staging.yml --env-file ../.env.staging \
  up -d --force-recreate api
```

> Recordar que el límite global anti-abuso (300 req/min/IP) sigue activo
> en operación normal; solo se eleva durante mediciones.
