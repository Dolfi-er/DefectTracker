FROM postgres:15

RUN apt-get update && apt-get install -y cron && \
    rm -rf /var/lib/apt/lists/*

RUN mkdir -p /scripts

COPY backup.sh /scripts/backup.sh
RUN chmod +x /scripts/backup.sh

RUN mkdir -p /backups

RUN echo "0 2 * * * /scripts/backup.sh >> /backups/cron.log 2>&1" > /etc/cron.d/backup-cron

RUN chmod 0644 /etc/cron.d/backup-cron

RUN crontab /etc/cron.d/backup-cron

CMD ["cron", "-f"]