.PHONY: up down build run-api run-worker

up:
	docker-compose up -d --build

down:
	docker-compose down

build:
	dotnet build

run-api:
	dotnet run --project MicroNetflix.Api

run-worker:
	dotnet run --project MicroNetflix.Worker
