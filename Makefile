# ALOud Project Makefile

.PHONY: test coverage coverage-report help dev run build backup restore all sonar-analyse sonar-analyse-with-coverage coverage-install coverage-check

ifneq (,$(wildcard .env.sonar))
include .env.sonar
endif

export SONAR_TOKEN SONAR_PROJECT_KEY SONAR_HOST_URL

SONAR_HOST_URL ?= http://localhost:9000/
SONAR_PROJECT_KEY ?= ALOud
COVERAGE_REPORT ?= coverage.xml
COVERAGE_REPORT_DIRECTORY ?= .

help:
	@echo "	make help: to see make options"
	@echo "	make test: to run all tests"
	@echo "	make coverage: to run tests with coverage"
	@echo "	make coverage-report: to generate HTML coverage report"
	@echo "	make dev: to start development environment"
	@echo "	make run: to start production environment"
	@echo "	make build: to build the application"
	@echo "	make sonar-analyse: run SonarQube analysis without coverage"
	@echo "	make sonar-analyse-with-coverage: run SonarQube analysis WITH test coverage"
	@echo "	make coverage-install: install dotnet-coverage tool"
	@echo "	make backup: to backup all databases"
	@echo "	make restore: to restore databases from backup"
	@echo "	make start-services: to start all database services"
	@echo "	make stop-services: to stop all database services"

test:
	dotnet build ALOud.sln
	dotnet test ALOud.sln --logger "console;verbosity=normal"

coverage:
	@echo "Building solution..."
	dotnet build ALOud.sln -c Release
	@echo "Running tests with coverage..."
	@if [ -d "Tests" ] && [ -n "$$(find Tests -name '*.cs' -type f 2>/dev/null)" ]; then \
		dotnet test ALOud.sln --no-build -c Release \
			--collect:"XPlat Code Coverage" \
			--results-directory ./coverage-results \
			--logger "console;verbosity=normal"; \
	else \
		echo "No test files found in Tests/, skipping"; \
	fi

coverage-report:
	dotnet tool install -g dotnet-reportgenerator-globaltool 2>/dev/null || true
	reportgenerator \
		-reports:"coverage-results/**/coverage.cobertura.xml" \
		-targetdir:"coverage-html" \
		-reporttypes:Html
	@echo "Done. Open coverage-html/index.html in your browser."

dev:
	@echo "Running the application with development mode ..."
	@dotnet watch --project ALOud.csproj run

run:
	@echo "Running the application..."
	@dotnet run --project ALOud.csproj

build:
	@dotnet build ALOud.sln

start-sonar-server:
	@docker start sonarqube

sonar-analyse:
	@if [ -z "$$SONAR_TOKEN" ]; then \
		echo "Error: SONAR_TOKEN is not set."; \
		echo "Run: export SONAR_TOKEN='<your-sonar-token>'"; \
		exit 1; \
	fi
	@echo "Starting SonarQube analysis for project: $(SONAR_PROJECT_KEY)"
	@dotnet-sonarscanner begin \
		/k:"$(SONAR_PROJECT_KEY)" \
		/d:sonar.host.url="$(SONAR_HOST_URL)" \
		/d:sonar.token="$$SONAR_TOKEN" \
		/d:sonar.dotnet.solution="ALOud.sln"
	@echo "Building project..."
	@dotnet build ALOud.sln
	@echo "Finalizing SonarQube analysis..."
	@dotnet-sonarscanner end /d:sonar.token="$$SONAR_TOKEN"

coverage-install:
	@echo "Installing dotnet-coverage global tool..."
	@dotnet tool install --global dotnet-coverage
	@echo "dotnet-coverage installed successfully"

sonar-analyse-with-coverage:
	@if [ -z "$$SONAR_TOKEN" ]; then \
		echo "SONAR_TOKEN is not set"; \
		exit 1; \
	fi

	@echo "Starting Sonar analysis with coverage..."

	@set -e; \
	TEST_EXIT_CODE=0; \
	mkdir -p coverage-reports; \
	rm -rf coverage-results; \
	\
	dotnet-sonarscanner begin \
		/k:"$(SONAR_PROJECT_KEY)" \
		/d:sonar.host.url="$(SONAR_HOST_URL)" \
		/d:sonar.token="$(SONAR_TOKEN)" \
		/d:sonar.dotnet.solution="ALOud.sln" \
		/d:sonar.projectBaseDir="$(PWD)" \
		/d:sonar.cs.cobertura.reportPaths="$(PWD)/coverage-reports/Cobertura.xml" \
		/d:sonar.exclusions="**/bin/**,**/obj/**,**/Migrations/**,**/.sonarqube/**,**/TestResults/**,**/raw-data/**" \
		/d:sonar.test.inclusions="**/Tests/**/*.cs" \
		/d:sonar.coverage.exclusions="**/Tests/**,**/Migrations/**,**/Program.cs"; \
	\
	echo "Building solution..."; \
	dotnet build ALOud.sln --no-incremental; \
	\
	echo "Running tests with coverage..."; \
	if [ -d "Tests" ] && [ -n "$$(find Tests -name '*.cs' -type f 2>/dev/null)" ]; then \
		dotnet test ALOud.sln --no-build \
			--collect:"XPlat Code Coverage" \
			--results-directory ./coverage-results \
			-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura \
			|| TEST_EXIT_CODE=$$?; \
	else \
		echo "No test projects found, skipping test coverage"; \
	fi; \
	\
	echo "Merging coverage reports..."; \
	reportgenerator \
		-reports:"coverage-results/**/coverage.cobertura.xml" \
		-targetdir:"coverage-reports" \
		-reporttypes:Cobertura; \
	\
	echo "Ending Sonar analysis..."; \
	dotnet-sonarscanner end /d:sonar.token="$(SONAR_TOKEN)"; \
	\
	if [ "$$TEST_EXIT_CODE" -ne 0 ]; then \
		echo "Tests failed but coverage uploaded"; \
		exit $$TEST_EXIT_CODE; \
	fi

	@echo "Sonar analysis completed successfully"
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
