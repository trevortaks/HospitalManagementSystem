# Hospital Management System

HospitalMS is a .NET solution with an Aspire AppHost for local orchestration, an MVC web front end, an API, shared service defaults, and a PostgreSQL dependency.

## Local development

### Aspire dashboard

Run the local stack with:

```bash
dotnet run --project HospitalMS.AppHost
```

Default local endpoints:

- Aspire dashboard: http://localhost:18888
- Web: http://localhost:5001
- API: http://localhost:5002
- PostgreSQL: localhost:5432

The dashboard surfaces resource state, health checks, environment variables, logs, traces, and metrics for all locally orchestrated services.

For dashboard navigation, metrics interpretation, startup output examples, and troubleshooting, see [ASPIRE_DASHBOARD.md](ASPIRE_DASHBOARD.md).

## Additional docs

- [DOCKER.md](DOCKER.md)
- [TESTING.md](TESTING.md)
- [SECURITY.md](SECURITY.md)
