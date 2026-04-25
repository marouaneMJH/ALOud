#!/bin/bash

# ALOud Database Restore Script
# Restores SQL Server, Redis, and Qdrant databases from backup

set -e  # Exit on error

# Configuration
SQL_SERVER_HOST="localhost"
SQL_SERVER_USER="SA"
SQL_SERVER_PASSWORD="g849eYMeH6mmJGE2*8ku"
SQL_DATABASE="VeloStoreDB"
REDIS_HOST="localhost"
REDIS_PORT="6379"
QDRANT_HOST="localhost"
QDRANT_PORT="6333"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  ALOud Database Restore Script${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Check if backup directory is provided
if [ -z "$1" ]; then
    echo -e "${RED}Error: Please provide backup directory or archive file${NC}"
    echo "Usage: $0 <backup-directory-or-archive>"
    echo ""
    echo "Available backups:"
    ls -lhd backups/*/ 2>/dev/null || echo "  No backups found in ./backups/"
    ls -lh ALOud_backup_*.tar.gz 2>/dev/null || true
    exit 1
fi

BACKUP_SOURCE="$1"

# Extract archive if provided
if [[ "$BACKUP_SOURCE" == *.tar.gz ]]; then
    echo -e "${YELLOW}Extracting archive: $BACKUP_SOURCE${NC}"
    BACKUP_DIR="./restore_temp_$(date +%s)"
    mkdir -p "$BACKUP_DIR"
    tar -xzf "$BACKUP_SOURCE" -C "$BACKUP_DIR"
    echo -e "  [+] Archive extracted to: $BACKUP_DIR"
    echo ""
else
    BACKUP_DIR="$BACKUP_SOURCE"
fi

if [ ! -d "$BACKUP_DIR" ]; then
    echo -e "${RED}Error: Backup directory not found: $BACKUP_DIR${NC}"
    exit 1
fi

echo -e "${YELLOW}[+] Restoring from: $BACKUP_DIR${NC}"
echo ""

# ============================================
# 1. SQL Server Restore
# ============================================
echo -e "${GREEN}[1/3] Restoring SQL Server (VeloStoreDB)...${NC}"

if [ -f "$BACKUP_DIR/$SQL_DATABASE.bak" ]; then
    if docker ps | grep -q "sqlserver\|mssql"; then
        CONTAINER_NAME=$(docker ps | grep "sqlserver\|mssql" | awk '{print $NF}')
        echo "  → SQL Server detected in Docker container: $CONTAINER_NAME"
        
        # Copy backup file to container
        docker cp "$BACKUP_DIR/$SQL_DATABASE.bak" "$CONTAINER_NAME:/tmp/$SQL_DATABASE.bak"
        
        # Restore database
        docker exec "$CONTAINER_NAME" /opt/mssql-tools/bin/sqlcmd \
            -S localhost -U "$SQL_SERVER_USER" -P "$SQL_SERVER_PASSWORD" -C \
            -Q "RESTORE DATABASE $SQL_DATABASE FROM DISK = '/tmp/$SQL_DATABASE.bak' WITH REPLACE"
        
        echo -e "  [+] SQL Server database restored"
    elif command -v sqlcmd &> /dev/null; then
        # Use absolute path for SQL Server restore
        ABSOLUTE_BACKUP_PATH="$(cd "$(dirname "$BACKUP_DIR/$SQL_DATABASE.bak")" && pwd)/$(basename "$BACKUP_DIR/$SQL_DATABASE.bak")"
        sqlcmd -S "$SQL_SERVER_HOST" -U "$SQL_SERVER_USER" -P "$SQL_SERVER_PASSWORD" -C \
            -Q "RESTORE DATABASE $SQL_DATABASE FROM DISK = '$ABSOLUTE_BACKUP_PATH' WITH REPLACE"
        echo -e "  [+] SQL Server database restored"
    else
        echo -e "${RED}  [!]  sqlcmd not found. Cannot restore .bak file${NC}"
    fi
elif [ -f "$BACKUP_DIR/$SQL_DATABASE.sql" ]; then
    echo "  → Found SQL script backup"
    if command -v sqlcmd &> /dev/null; then
        # Use absolute path for SQL script
        ABSOLUTE_SQL_PATH="$(cd "$(dirname "$BACKUP_DIR/$SQL_DATABASE.sql")" && pwd)/$(basename "$BACKUP_DIR/$SQL_DATABASE.sql")"
        sqlcmd -S "$SQL_SERVER_HOST" -U "$SQL_SERVER_USER" -P "$SQL_SERVER_PASSWORD" -C \
            -i "$ABSOLUTE_SQL_PATH"
        echo -e "  [+] SQL Server database restored from script"
    else
        echo -e "${YELLOW} [=]  Run manually: sqlcmd -S $SQL_SERVER_HOST -U $SQL_SERVER_USER -P [password] -C -i $BACKUP_DIR/$SQL_DATABASE.sql${NC}"
    fi
else
    echo -e "${YELLOW}  [!]  SQL Server backup not found in $BACKUP_DIR${NC}"
fi

echo ""

# ============================================
# 2. Redis Restore
# ============================================
echo -e "${GREEN}[2/3] Restoring Redis cache...${NC}"

if [ -f "$BACKUP_DIR/redis_dump.rdb" ]; then
    if docker ps | grep -q "redis"; then
        REDIS_CONTAINER=$(docker ps | grep "redis" | awk '{print $NF}')
        echo "  → Redis detected in Docker container: $REDIS_CONTAINER"
        
        # Stop Redis, copy dump file, restart
        docker exec "$REDIS_CONTAINER" redis-cli SHUTDOWN NOSAVE || true
        sleep 2
        docker cp "$BACKUP_DIR/redis_dump.rdb" "$REDIS_CONTAINER:/data/dump.rdb"
        docker start "$REDIS_CONTAINER" || true
        sleep 2
        
        echo -e "  [+] Redis data restored"
    elif command -v redis-cli &> /dev/null; then
        # Stop Redis, copy dump file, restart
        redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" SHUTDOWN NOSAVE || true
        sleep 2
        cp "$BACKUP_DIR/redis_dump.rdb" /var/lib/redis/dump.rdb
        systemctl start redis || service redis start || true
        sleep 2
        
        echo -e "  [+] Redis data restored"
    else
        echo -e "${YELLOW}  [=]  Copy $BACKUP_DIR/redis_dump.rdb to Redis data directory manually${NC}"
    fi
else
    echo -e "${YELLOW}  [!]  Redis backup not found${NC}"
fi

echo ""

# ============================================
# 3. Qdrant Restore
# ============================================
echo -e "${GREEN}[3/3] Restoring Qdrant vector database...${NC}"

# Find snapshot files
SNAPSHOT_FILES=$(find "$BACKUP_DIR" -name "qdrant_*.snapshot" 2>/dev/null || true)

if [ -n "$SNAPSHOT_FILES" ]; then
    for SNAPSHOT_FILE in $SNAPSHOT_FILES; do
        FILENAME=$(basename "$SNAPSHOT_FILE")
        COLLECTION=$(echo "$FILENAME" | sed 's/qdrant_\(.*\)_.*/\1/')
        
        echo "  → Restoring collection: $COLLECTION"
        
        # Upload snapshot to Qdrant
        curl -X PUT "http://$QDRANT_HOST:$QDRANT_PORT/collections/$COLLECTION/snapshots/upload" \
            -H "Content-Type:application/octet-stream" \
            --data-binary "@$SNAPSHOT_FILE"
        
        echo -e "  [+] Collection '$COLLECTION' restored"
    done
elif [ -f "$BACKUP_DIR/qdrant_full_backup.tar.gz" ]; then
    echo "  → Restoring full Qdrant backup..."
    
    if docker ps | grep -q "qdrant"; then
        # Stop Qdrant, restore volume, restart
        docker stop qdrant
        docker run --rm \
            -v qdrant_data:/data \
            -v "$(pwd)/$BACKUP_DIR":/backup \
            alpine sh -c "cd /data && tar xzf /backup/qdrant_full_backup.tar.gz"
        docker start qdrant
        
        echo -e "  [+] Qdrant data fully restored"
    else
        echo -e "${YELLOW}  [=]  Extract $BACKUP_DIR/qdrant_full_backup.tar.gz to Qdrant data directory manually${NC}"
    fi
else
    echo -e "${YELLOW}  [!]  Qdrant backup not found${NC}"
fi

echo ""

# Cleanup temporary extraction directory
if [[ "$BACKUP_SOURCE" == *.tar.gz ]] && [ -d "$BACKUP_DIR" ]; then
    rm -rf "$BACKUP_DIR"
    echo -e "${GREEN}🧹 Cleaned up temporary extraction directory${NC}"
    echo ""
fi

# ============================================
# Summary
# ============================================
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  Restore Complete!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${YELLOW}[!]  Important: Restart your application to apply changes${NC}"
echo ""
echo -e "${GREEN}[+] All databases restored successfully!${NC}"
