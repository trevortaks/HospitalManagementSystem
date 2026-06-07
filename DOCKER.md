# Docker Setup

## Environment setup

1. Copy the example environment file:

   ```bash
   cp .env.example .env
   ```

2. Update `.env` with secure values for:
   - `POSTGRES_USER`
   - `POSTGRES_PASSWORD`
   - `POSTGRES_DB`
   - `JWT_SECRET`
   - `CONNECTION_STRING`
   - `ASPNETCORE_ENVIRONMENT`

## Build images

These examples use Docker Compose v2 (`docker compose`). If your machine still uses the legacy binary, replace it with `docker-compose`.

```bash
docker compose build
```

## Run locally

```bash
docker compose up -d
```

Services:

- Web: http://localhost:5001
- API: http://localhost:5002
- PgAdmin: http://localhost:5050
- PostgreSQL: localhost:5432

## Common commands

```bash
docker compose down
docker compose logs -f
docker compose logs -f web
docker compose logs -f api
docker compose exec web sh
docker compose exec api sh
docker compose exec postgres sh
```

## Health checks

- PostgreSQL uses `pg_isready`.
- Web and API call `/health`.
- PgAdmin checks `/misc/ping`.

Inspect health states:

```bash
docker compose ps
```

## Troubleshooting

- If build fails, run `docker compose build --no-cache`.
- If containers do not start cleanly, inspect logs with `docker compose logs <service>`.
- If ports are busy, stop local processes or change the published ports in `docker-compose.yml`.
- If the app cannot reach PostgreSQL, verify `CONNECTION_STRING` points to host `postgres` inside Docker networking.
- If JWT authentication fails, confirm `JWT_SECRET` matches the expected configuration and is at least 32 characters.

## Production considerations

- Replace development secrets before deployment.
- Use a managed secret store instead of a plaintext `.env`.
- Persist PostgreSQL data with production-grade storage and backups.
- Add reverse proxy/TLS termination in front of the Web and API services.
- Review resource limits, logging, monitoring, and image-tag pinning before release.
