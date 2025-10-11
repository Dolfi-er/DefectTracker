#!/bin/bash

DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/backups"
LOG_FILE="$BACKUP_DIR/backup.log"

log() {
    echo "$(date +'%Y-%m-%d %H:%M:%S') - $1" >> $LOG_FILE
}

mkdir -p $BACKUP_DIR

BACKUP_FILE="$BACKUP_DIR/backup_$DATE.sql"

log "Начало бэкапа базы данных"

export PGPASSWORD=$DB_PASSWORD
pg_dump -h $DB_HOST -U $DB_USER -d $DB_NAME > $BACKUP_FILE

if [ $? -eq 0 ]; then
    log "Бэкап успешно создан: $BACKUP_FILE"
    
    gzip $BACKUP_FILE
    log "Бэкап сжат: $BACKUP_FILE.gz"
    
    cd $BACKUP_DIR && ls -t backup_*.sql.gz | tail -n +8 | xargs rm -f
    log "Удалены старые бэкапы (оставлены последние 7)"
else
    log "Ошибка при создании бэкапа"
    exit 1
fi

log "Бэкап завершен"