.PHONY:  help dev run all
help:
	@echo "	make help: to see make options"
	@echo "	make dev: to start development environment"
	@echo "	run: to start production  environment"

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



all: help