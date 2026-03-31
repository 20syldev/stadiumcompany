#!/bin/bash
# Daily backup script for StadiumCompany database (PostgreSQL)
# Usage: ./backup.sh
# Cron example (daily at 2:00 AM):
#   0 2 * * * /path/to/stadiumcompany/scripts/backup.sh >> /path/to/stadiumcompany/backups/backup.log 2>&1

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BACKUP_DIR="$SCRIPT_DIR/../backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/stadiumcompany_$TIMESTAMP.dump"

DB_HOST="${DB_HOST:-localhost}"
DB_NAME="${DB_NAME:-stadiumcompany}"
DB_USER="${DB_USER:-stadiumcompany}"
DB_PASSWORD="${DB_PASSWORD:-stadiumcompany}"

mkdir -p "$BACKUP_DIR"

export PGPASSWORD="$DB_PASSWORD"

echo "[$(date)] Starting backup..."

pg_dump -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" -F c -f "$BACKUP_FILE"

if pg_restore --list "$BACKUP_FILE" > /dev/null 2>&1; then
    echo "[$(date)] [OK] Backup verified: $(basename "$BACKUP_FILE")"
else
    echo "[$(date)] [ERROR] Backup verification failed!"
    rm -f "$BACKUP_FILE"
    exit 1
fi

# Clean up backups older than 30 days
DELETED=$(find "$BACKUP_DIR" -name "stadiumcompany_*.dump" -mtime +30 -print -delete | wc -l)
if [ "$DELETED" -gt 0 ]; then
    echo "[$(date)] Cleaned up $DELETED old backup(s)."
fi

echo "[$(date)] Backup complete."
