


up:
	docker-compose up -d

logs:
	docker-compose logs -f


restart:
	docker-compose down
	docker-compose up -d --build