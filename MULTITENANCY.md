# Multi-tenancy

## Tenant URL structure

Requests must include the tenant identifier as the first path segment:

- `/TENANT-GUID/Home/Index`
- `/TENANT-GUID/Patients/Details/123`

`TenantResolutionMiddleware` validates the GUID, loads the tenant, stores it in `HttpContext.Items["tenant"]`, and rewrites the remaining path so existing MVC routes continue to work.

## Adding new multi-tenant entities

1. Inherit the entity from `TenantEntity`.
2. Add a navigation on `Tenant` when the relationship should be traversable.
3. Configure the entity in `HospitalDbContext`.
4. Use tenant-scoped unique indexes, for example `{ TenantId, ExternalId }`.
5. Seed or persist the entity through a tenant-aware `HospitalDbContext` so `TenantId` is assigned automatically.

## Security considerations

- Tenant resolution rejects missing or unknown tenants with HTTP 400 before controller execution.
- `HospitalDbContext` applies `HasQueryFilter` to every `ITenantEntity`, preventing accidental cross-tenant reads.
- `SaveChanges` blocks cross-tenant updates and deletes, even when `IgnoreQueryFilters()` is used internally.
- Background jobs or seed scripts must either supply a tenant-aware provider or set `TenantId` explicitly.
