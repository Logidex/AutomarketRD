#!/bin/sh
# =========================================
# Watchdog de salud AutoMarketRD
# Sondea el health check de la API y envía un correo por Gmail SMTP
# cuando falla N veces consecutivas y otro al recuperarse.
#
# Configuración (archivo fuente con asignaciones sh, permisos 600):
#   SMTP_HOST, SMTP_PORT, SMTP_USER, SMTP_PASS, SMTP_FROM, ALERT_TO
# Programación sugerida (crontab del usuario):
#   */2 * * * * /ruta/a/ops/watchdog-health.sh >> /var/log/automarket-watchdog.log 2>&1
# =========================================
set -u

URL="${WATCHDOG_URL:-http://localhost:8081/health/ready}"
STATE_FILE="${WATCHDOG_STATE:-/var/tmp/automarket-watchdog.state}"
FALLOS_PARA_ALERTA="${WATCHDOG_FALLOS:-2}"
ENV_FILE="${WATCHDOG_ENV_FILE:-/etc/automarket-watchdog.env}"
TIMEOUT_SEG="${WATCHDOG_TIMEOUT:-15}"

if [ ! -f "$ENV_FILE" ]; then
    echo "[watchdog] ERROR: no existe $ENV_FILE"
    exit 1
fi
# shellcheck disable=SC1090
. "$ENV_FILE"

if [ -z "${SMTP_USER:-}" ] || [ -z "${SMTP_PASS:-}" ]; then
    echo "[watchdog] ERROR: faltan SMTP_USER/SMTP_PASS en $ENV_FILE"
    exit 1
fi

SMTP_FROM="${SMTP_FROM:-${SMTP_USER}}"
ALERT_TO="${ALERT_TO:-${SMTP_USER}}"

fails_anteriores=$(cat "$STATE_FILE" 2>/dev/null || echo 0)

codigo=$(curl -s -o /dev/null -w "%{http_code}" --max-time "$TIMEOUT_SEG" "$URL" 2>/dev/null)
if [ -z "$codigo" ]; then codigo=000; fi

if [ "$codigo" = "200" ]; then
    if [ "$fails_anteriores" -ge "$FALLOS_PARA_ALERTA" ]; then
        echo "[watchdog] $(date -Iseconds) RECUPERADO (antes $fails_anteriores fallos)"
        printf 'Asunto: [AutoMarket] RECUPERADO - %s\n\nLa API volvio a responder 200 en %s.\nHora: %s\n' \
            "$URL" "$URL" "$(date -Iseconds)" \
        | curl -s --max-time 30 \
            --url "smtp://${SMTP_HOST}:${SMTP_PORT}" \
            --ssl-reqd \
            --mail-from "${SMTP_FROM}" \
            --mail-rcpt "${ALERT_TO}" \
            --user "${SMTP_USER}:${SMTP_PASS}" \
            -T - && echo "[watchdog] Correo de recuperacion enviado a ${ALERT_TO}"
    fi
    echo 0 > "$STATE_FILE"
    exit 0
fi

fails=$((fails_anteriores + 1))
echo "$fails" > "$STATE_FILE"
echo "[watchdog] $(date -Iseconds) Fallo (HTTP $codigo), consecutivos: $fails"

if [ "$fails" -eq "$FALLOS_PARA_ALERTA" ]; then
    printf 'Asunto: [AutoMarket] CAIDA DETECTADA - HTTP %s\n\nLa API no responde correctamente.\nURL: %s\nCodigo HTTP: %s\nFallos consecutivos: %s\nHora: %s\n' \
        "$codigo" "$URL" "$codigo" "$fails" "$(date -Iseconds)" \
    | curl -s --max-time 30 \
        --url "smtp://${SMTP_HOST}:${SMTP_PORT}" \
        --ssl-reqd \
        --mail-from "${SMTP_FROM}" \
        --mail-rcpt "${ALERT_TO}" \
        --user "${SMTP_USER}:${SMTP_PASS}" \
        -T - && echo "[watchdog] Alerta enviada a ${ALERT_TO}"
fi
