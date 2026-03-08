# Request to use 
# I want floral and Gourmand perfume for men good to ware in summer and hot office

.PHONY:  help dev run build backup restore all
help:
	@echo "	make help: to see make options"
	@echo "	make dev: to start development environment"
	@echo "	make run: to start production environment"
	@echo "	make build: to build the application"
	@echo "	make backup: to backup all databases"
	@echo "	make restore: to restore databases from backup"
	@echo "	make start-services: to start all database services"
	@echo "	make stop-services: to stop all database services"

dev:
	@echo "Running the application with development mode ..."
	@dotnet watch run

run:
	@echo "Running the application..."
	@dotnet run

build:
	@dotnet build

start-services:
	@redis-start
	@mssql-start
	@docker start qdrant

stop-services:
	@redis-stop
	@mssql-stop
	@docker stop qdrant

qdrant-dashboard:
	@xdg-open http://localhost:6333/dashboard#/collections/perfumes

backup:
	@echo "Backing up all databases..."
	@./backup-databases.sh

restore:
	@if [ -z "$(BACKUP)" ]; then \
		echo "Error: Please specify backup path using BACKUP=<path>"; \
		echo "Example: make restore BACKUP=./backups/20260308_143000/"; \
		echo "         make restore BACKUP=ALOud_backup_20260308_143000.tar.gz"; \
		echo ""; \
		echo "Available backups:"; \
		ls -lhd backups/*/ 2>/dev/null || echo "  No backups found"; \
		ls -lh ALOud_backup_*.tar.gz 2>/dev/null || true; \
		exit 1; \
	fi
	@echo "Restoring databases from $(BACKUP)..."
	@./restore-databases.sh "$(BACKUP)"

all: help