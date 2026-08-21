#!/bin/sh
# =========================================
# Script de backup de la base de datos PostgreSQL
# =========================================
set -e

DB_HOST="${POSTGRES_HOST:-db}"
DB_PORT="${POSTGRES_PORT:-5432}"
DB_NAME="${POSTGRES_DB:-AutoMarketDB}"
DB_USER="${POSTGRES_USER:-postgres}"
BACKUP_DIR="${BACKUP_DIR:-/backups}"
RETENTION_DAYS="${BACKUP_RETENTION_DAYS:-14}"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILE="${BACKUP_DIR}/${DB_NAME}_${TIMESTAMP}.sql.gz"

mkdir -p "${BACKUP_DIR}"

echo "[$(date -Iseconds)] Iniciando backup de ${DB_NAME}@${DB_HOST}:${DB_PORT} → ${BACKUP_FILE}"

PGPASSWORD="${POSTGRES_PASSWORD}" pg_dump \
    -h "${DB_HOST}" \
    -p "${DB_PORT}" \
    -U "${DB_USER}" \
    -d "${DB_NAME}" \
    --no-owner --no-privileges \
    | gzip -9 > "${BACKUP_FILE}"

echo "[$(date -Iseconds)] Backup completado: ${BACKUP_FILE} ($(du -h "${BACKUP_FILE}" | cut -f1))"

# =========================================
# Copia offsite opcional a S3/R2
# Se activa si BACKUP_S3_BUCKET está definido y aws-cli está instalada.
# Un fallo de subida NO invalida el backup local.
# =========================================
if [ -n "${BACKUP_S3_BUCKET}" ] && command -v aws >/dev/null 2>&1; then
    ENDPOINT_ARGS=""
    if [ -n "${AWS_S3_ENDPOINT}" ]; then
        ENDPOINT_ARGS="--endpoint-url ${AWS_S3_ENDPOINT}"
    fi

    PREFIX="${BACKUP_S3_PREFIX:-db-backups}"
    echo "[$(date -Iseconds)] Subiendo copia offsite a s3://${BACKUP_S3_BUCKET}/${PREFIX}/ ..."

    if AWS_PAGER="" aws s3 cp "${BACKUP_FILE}" \
        "s3://${BACKUP_S3_BUCKET}/${PREFIX}/$(basename "${BACKUP_FILE}")" \
        ${ENDPOINT_ARGS} >> "${BACKUP_DIR}/offsite.log" 2>&1; then
        echo "[$(date -Iseconds)] Copia offsite completada."
    else
        echo "[$(date -Iseconds)] AVISO: falló la subida offsite (detalle en ${BACKUP_DIR}/offsite.log); el backup local se conserva."
    fi
fi

echo "[$(date -Iseconds)] Limpiando backups anteriores a ${RETENTION_DAYS} días..."
find "${BACKUP_DIR}" -name "${DB_NAME}_*.sql.gz" -type f -mtime +${RETENTION_DAYS} -delete

echo "[$(date -Iseconds)] Backup finalizado. Backups restantes:"
ls -lh "${BACKUP_DIR}"/${DB_NAME}_*.sql.gz 2>/dev/null || echo "  (ninguno)"
