
help:
	@echo "	make dev: to start development environment"
	@echo "	run: to start production  environment"


dev:
	dotnet watch run

run:
	dotnet run

all: help