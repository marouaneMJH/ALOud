# Request to use 
# I want floral and Gourmand perfume for men good to ware in summer and hot office

.PHONY:  help dev run build backup restore all sonar-analyse start-sonar-server

ifneq (,$(wildcard .env.sonar))
include .env.sonar
endif

export SONAR_TOKEN SONAR_PROJECT_KEY SONAR_HOST_URL

SONAR_HOST_URL ?= http://localhost:9000/
SONAR_PROJECT_KEY ?= ALOud

help:
	@echo "	make help: to see make options"
	@echo "	make dev: to start development environment"
	@echo "	make run: to start production environment"
	@echo "	make build: to build the application"
	@echo "	make sonar-analyse: run SonarQube begin/build/end scan"
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

start-sonar-server:
	@docker start sonarqube

sonar-analyse:
	@if [ -z "$$SONAR_TOKEN" ]; then \
		echo "Error: SONAR_TOKEN is not set."; \
		echo "Run: export SONAR_TOKEN='<your-sonar-token>'"; \
		exit 1; \
	fi
	@echo "Starting SonarQube analysis for project: $(SONAR_PROJECT_KEY)"
	@dotnet sonarscanner begin /k:"$(SONAR_PROJECT_KEY)" /d:sonar.host.url="$(SONAR_HOST_URL)" /d:sonar.token="$$SONAR_TOKEN"
	@echo "Building project..."
	@dotnet build
	@echo "Finalizing SonarQube analysis..."
	@dotnet sonarscanner end /d:sonar.token="$$SONAR_TOKEN"

json-issues-sonar:
	@if [ -z "$$SONAR_TOKEN" ]; then \
		echo "Error: SONAR_TOKEN is not set."; \
		echo "Run: export SONAR_TOKEN='<your-sonar-token>'"; \
		exit 1; \
	fi
	@curl -u "$(SONAR_TOKEN)": "$(SONAR_HOST_URL)api/issues/search?componentKeys=$(SONAR_PROJECT_KEY)&issueStatuses=OPEN,CONFIRMED&ps=500" -o issues.json

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