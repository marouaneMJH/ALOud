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

start-services:
	@redis-start
	@mssql-start

stop-services:
	@redis-stop
	@mssql-stop


all: help