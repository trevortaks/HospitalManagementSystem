# Aspire Dashboard

## Accessing the dashboard

Start the orchestrated local environment from the repository root:

```bash
dotnet run --project HospitalMS.AppHost
```

The AppHost starts the Aspire dashboard for local development at:

- Dashboard: http://localhost:18888
- Web service: http://localhost:5001
- API service: http://localhost:5002
- PostgreSQL: localhost:5432

If you override `ASPNETCORE_URLS` before starting the AppHost, the dashboard uses your custom URL instead.

## What the dashboard shows

The dashboard gives a single place to monitor and orchestrate the HospitalMS local stack:

- **Resources**: `hospitalms-web`, `hospitalms-api`, `postgres`, and the `hospitalms` database
- **Status**: running, starting, stopped, and failed states for each resource
- **Endpoints**: the web and API HTTP endpoints plus PostgreSQL connection information
- **Logs**: live console logs from all resources with built-in filtering and search
- **Traces**: distributed traces emitted through OpenTelemetry OTLP export
- **Metrics**: ASP.NET Core request metrics, HTTP client metrics, and runtime metrics flowing into Aspire
- **Health**: `/health` readiness results for the web and API services
- **Environment**: injected environment variables, including selected values mirrored from each service's `appsettings*.json`

## Navigation tips

- Use the **Resources** view to verify startup order, dependencies, URLs, and environment variables.
- Open a specific resource to inspect **logs**, **traces**, **metrics**, and **health checks**.
- Use **Console Logs** filtering to narrow by service name, severity, or message text.
- Use the **Traces** view to confirm cross-service activity and latency.
- Use the **Metrics** view to compare request activity and runtime behavior over time.

## Interpreting the metrics

- **ASP.NET Core metrics** help confirm request volume and request duration for the web and API projects.
- **HTTP client metrics** appear when one service calls another through service discovery-enabled clients.
- **Runtime metrics** help explain CPU and memory pressure by showing managed runtime activity such as GC and thread pool behavior.
- **Resource usage** in the resource list helps confirm whether a service is under stress or idle.

## Expected startup output

When `dotnet run --project HospitalMS.AppHost` starts successfully, expect console output in this shape:

```text
info: Aspire.Hosting.DistributedApplication[0]
      Aspire Dashboard available at: http://localhost:18888
info: Aspire.Hosting.DistributedApplication[0]
      Resource 'postgres' started
info: Aspire.Hosting.DistributedApplication[0]
      Resource 'hospitalms-api' started
info: Aspire.Hosting.DistributedApplication[0]
      Resource 'hospitalms-web' started
```

Exact wording can vary by SDK version, but the dashboard URL and per-resource startup events should be present.

## Expected dashboard state

After a healthy startup, expect to see:

- `postgres` and `hospitalms` marked healthy and running
- `hospitalms-api` marked running with an HTTP endpoint on port `5002` and a passing `/health` check
- `hospitalms-web` marked running with an HTTP endpoint on port `5001`, a passing `/health` check, and a dependency on `hospitalms-api`
- live logs and OpenTelemetry traces visible within a few seconds of traffic hitting the services

## Troubleshooting

- **Dashboard does not open**: confirm port `18888` is free or set a different `ASPNETCORE_URLS` value before starting the AppHost.
- **No traces or metrics**: verify the AppHost is starting the services and that the projects were launched through `HospitalMS.AppHost`, not separately.
- **No health status**: confirm the resource shows `/health` and that the web or API process is still running.
- **PostgreSQL does not start**: make sure Docker is running locally because Aspire provisions PostgreSQL as a container resource.
- **Logs are empty**: reload the resource view and confirm the target project is still in `Running` state.
