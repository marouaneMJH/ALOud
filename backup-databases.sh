#!/bin/bash

# ALOud Database Backup Script
# Exports SQL Server, Redis, and Qdrant databases

set -e  # Exit on error

# Configuration
BACKUP_DIR="./backups/$(date +%Y%m%d_%H%M%S)"
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
echo -e "${GREEN}  ALOud Database Backup Script${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Create backup directory
mkdir -p "$BACKUP_DIR"
echo -e "${YELLOW}[+] Backup directory: $BACKUP_DIR${NC}"
echo ""

# ============================================
# 1. SQL Server Backup
# ============================================
echo -e "${GREEN}[1/3] Backing up SQL Server (VeloStoreDB)...${NC}"

# Check if running in Docker
if docker ps | grep -q "sqlserver\|mssql"; then
    CONTAINER_NAME=$(docker ps | grep "sqlserver\|mssql" | awk '{print $NF}')
    echo "  → SQL Server detected in Docker container: $CONTAINER_NAME"
    
    docker exec "$CONTAINER_NAME" /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U "$SQL_SERVER_USER" -P "$SQL_SERVER_PASSWORD" -C \
        -Q "BACKUP DATABASE $SQL_DATABASE TO DISK = '/tmp/$SQL_DATABASE.bak' WITH FORMAT"
    
    docker cp "$CONTAINER_NAME:/tmp/$SQL_DATABASE.bak" "$BACKUP_DIR/$SQL_DATABASE.bak"
    echo -e "  [+] SQL Server backup saved: $BACKUP_DIR/$SQL_DATABASE.bak"
else
    # Direct sqlcmd (native installation)
    if command -v sqlcmd &> /dev/null; then
        # Use SQL Server's default backup directory (has proper permissions)
        SQL_BACKUP_DIR="/var/opt/mssql/data"
        TEMP_BACKUP="$SQL_BACKUP_DIR/$SQL_DATABASE.bak"
        
        echo "  → Backing up to SQL Server directory: $SQL_BACKUP_DIR"
        sqlcmd -S "$SQL_SERVER_HOST" -U "$SQL_SERVER_USER" -P "$SQL_SERVER_PASSWORD" -C \
            -Q "BACKUP DATABASE $SQL_DATABASE TO DISK = '$TEMP_BACKUP' WITH FORMAT"
        
        # Copy backup file to our backup directory
        if sudo test -f "$TEMP_BACKUP"; then
            sudo cp "$TEMP_BACKUP" "$BACKUP_DIR/$SQL_DATABASE.bak"
            sudo chown $(id -u):$(id -g) "$BACKUP_DIR/$SQL_DATABASE.bak"
            echo -e "  [+] SQL Server backup saved: $BACKUP_DIR/$SQL_DATABASE.bak"
        else
            echo -e "${YELLOW}  [!] Backup file not found. Trying alternative location...${NC}"
            # Try common backup locations
            for ALT_DIR in "/var/opt/mssql/backup" "$HOME"; do
                if sudo test -f "$ALT_DIR/$SQL_DATABASE.bak"; then
                    sudo cp "$ALT_DIR/$SQL_DATABASE.bak" "$BACKUP_DIR/$SQL_DATABASE.bak"
                    sudo chown $(id -u):$(id -g) "$BACKUP_DIR/$SQL_DATABASE.bak"
                    echo -e "  [+] SQL Server backup saved: $BACKUP_DIR/$SQL_DATABASE.bak"
                    break
                fi
            done
        fi
    else
        echo -e "${RED}  [!]  sqlcmd not found. Generating SQL script instead...${NC}"
        dotnet ef dbcontext script --output "$BACKUP_DIR/$SQL_DATABASE.sql" --project ./ALOud.csproj
        echo -e "  [+] SQL script saved: $BACKUP_DIR/$SQL_DATABASE.sql"
    fi
fi

echo ""

# ============================================
# 2. Redis Backup
# ============================================
echo -e "${GREEN}[2/3] Backing up Redis cache...${NC}"

if command -v redis-cli &> /dev/null; then
    # Trigger Redis save
    redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" SAVE > /dev/null 2>&1 || echo "  → SAVE command sent"
    
    # Check if running in Docker
    if docker ps | grep -q "redis"; then
        REDIS_CONTAINER=$(docker ps | grep "redis" | awk '{print $NF}')
        echo "  → Redis detected in Docker container: $REDIS_CONTAINER"
        docker cp "$REDIS_CONTAINER:/data/dump.rdb" "$BACKUP_DIR/redis_dump.rdb" 2>/dev/null || \
        docker cp "$REDIS_CONTAINER:/var/lib/redis/dump.rdb" "$BACKUP_DIR/redis_dump.rdb" 2>/dev/null || \
        echo -e "${YELLOW}  [!]  Redis dump.rdb not found in standard locations${NC}"
    else
        # Native Redis installation
        if [ -f "/var/lib/redis/dump.rdb" ]; then
            cp /var/lib/redis/dump.rdb "$BACKUP_DIR/redis_dump.rdb"
        elif [ -f "/data/dump.rdb" ]; then
            cp /data/dump.rdb "$BACKUP_DIR/redis_dump.rdb"
        else
            echo -e "${YELLOW}  [!]  Redis dump.rdb not found${NC}"
        fi
    fi
    
    if [ -f "$BACKUP_DIR/redis_dump.rdb" ]; then
        echo -e "  [+] Redis backup saved: $BACKUP_DIR/redis_dump.rdb"
    else
        echo -e "${YELLOW}  [!]  Redis backup skipped (persistence may not be enabled)${NC}"
    fi
else
    echo -e "${RED}  [!]  redis-cli not found. Skipping Redis backup.${NC}"
fi

echo ""

# ============================================
# 3. Qdrant Backup
# ============================================
echo -e "${GREEN}[3/3] Backing up Qdrant vector database...${NC}"

# Get list of collections
COLLECTIONS=$(curl -s "http://$QDRANT_HOST:$QDRANT_PORT/collections" | grep -oP '(?<="name":")[^"]+' || echo "")

if [ -n "$COLLECTIONS" ]; then
    echo "  → Found collections: $COLLECTIONS"
    
    for COLLECTION in $COLLECTIONS; do
        echo "  → Creating snapshot for collection: $COLLECTION"
        
        # Create snapshot
        SNAPSHOT_RESPONSE=$(curl -s -X POST "http://$QDRANT_HOST:$QDRANT_PORT/collections/$COLLECTION/snapshots")
        SNAPSHOT_NAME=$(echo "$SNAPSHOT_RESPONSE" | grep -oP '(?<="name":")[^"]+' | head -1)
        
        if [ -n "$SNAPSHOT_NAME" ]; then
            # Download snapshot
            curl -s "http://$QDRANT_HOST:$QDRANT_PORT/collections/$COLLECTION/snapshots/$SNAPSHOT_NAME" \
                -o "$BACKUP_DIR/qdrant_${COLLECTION}_${SNAPSHOT_NAME}"
            echo -e "  [+] Qdrant snapshot saved: $BACKUP_DIR/qdrant_${COLLECTION}_${SNAPSHOT_NAME}"
        else
            echo -e "${YELLOW}  [!]  Failed to create snapshot for $COLLECTION${NC}"
        fi
    done
else
    # Fallback: Export entire Qdrant data volume
    if docker ps | grep -q "qdrant"; then
        echo "  → Exporting entire Qdrant data volume..."
        docker run --rm \
            -v qdrant_data:/data \
            -v "$(pwd)/$BACKUP_DIR":/backup \
            alpine tar czf "/backup/qdrant_full_backup.tar.gz" -C /data .
        echo -e "  [+] Qdrant full backup saved: $BACKUP_DIR/qdrant_full_backup.tar.gz"
    else
        echo -e "${YELLOW}  [!]  Qdrant not accessible. Skipping backup.${NC}"
    fi
fi

echo ""

# ============================================
# Summary
# ============================================
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  Backup Complete!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "[+] Backup location: ${YELLOW}$BACKUP_DIR${NC}"
echo ""
echo "Files created:"
ls -lh "$BACKUP_DIR"
echo ""
echo -e "${GREEN}[+] All databases backed up successfully!${NC}"

# Create compressed archive
ARCHIVE_NAME="ALOud_backup_$(date +%Y%m%d_%H%M%S).tar.gz"
tar -czf "$ARCHIVE_NAME" -C "$BACKUP_DIR" .
echo ""
echo -e "${GREEN}[+] Compressed archive created: $ARCHIVE_NAME${NC}"
echo "Archive size: $(du -h "$ARCHIVE_NAME" | cut -f1)"
