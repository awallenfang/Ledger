#!/bin/bash
set -euo pipefail

BACKUP_DIR="./data/backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
DB_DIR="./data/db"
BACKUP_NAME="db_backup_$TIMESTAMP"
COMPOSE_FILE="${1:-compose.yaml}"

mkdir -p "$BACKUP_DIR"

echo "[*] Backing up database files..."
tar czf "$BACKUP_DIR/$BACKUP_NAME.tar.gz" -C "$(dirname "$DB_DIR")" "$(basename "$DB_DIR")"

echo "[*] Creating SQL dump (if container is running)..."
if docker compose -f "$COMPOSE_FILE" ps --format json db 2>/dev/null | grep -q '"State":"running"'; then
    docker compose -f "$COMPOSE_FILE" exec -T db pg_dumpall -U ledger > "$BACKUP_DIR/${BACKUP_NAME}.sql" 2>/dev/null && \
        echo "[*] SQL dump saved: $BACKUP_DIR/${BACKUP_NAME}.sql" || \
        echo "[!] SQL dump failed (container may not be healthy) — file backup still created."
else
    echo "[!] DB container not running — skipping SQL dump."
fi

echo "[*] Backup complete: $BACKUP_DIR/$BACKUP_NAME.tar.gz"
echo "[*] To restore file backup: tar xzf $BACKUP_DIR/$BACKUP_NAME.tar.gz -C ."
echo "[*] To restore SQL dump (requires running DB): cat $BACKUP_DIR/${BACKUP_NAME}.sql | docker compose exec -T db psql -U ledger"
