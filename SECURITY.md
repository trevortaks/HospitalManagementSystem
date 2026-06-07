# Security Infrastructure

## JWT configuration

Configure JWT settings in `appsettings.json` or environment-specific configuration under `JwtSettings`:

```json
"JwtSettings": {
  "Secret": "ChangeThisJwtSecretInProduction1234567890",
  "Issuer": "HospitalMS.API",
  "Audience": "HospitalMS.Clients",
  "ExpiryMinutes": 60
}
```

- Use a secret with at least 32 characters.
- Keep the secret in secure configuration providers for non-development environments.
- Register the infrastructure with `builder.Services.AddHospitalSecurity(builder.Configuration);`
- Enable the middleware pipeline with `app.UseHospitalSecurity();`

## RBAC usage

- Roles are defined in `HospitalMS.Common/Constants/UserRoles.cs`.
- Permissions are defined and mapped in `HospitalMS.Common/Constants/Permissions.cs`.
- Decorate controllers or actions with `[RoleBasedAuth(UserRoles.Admin, UserRoles.Doctor)]`.
- Permission claims are emitted into JWT tokens automatically from the assigned roles.
- Authorization policies are registered for each permission and role.

## Audit logging integration

- `AuditLogger` captures user ID, IP address, request context, and custom details.
- `AuditInterceptor` inspects EF Core tracked entity changes during `SaveChanges` and `SaveChangesAsync`.
- Integrate the interceptor on a DbContext registration with:

```csharp
builder.Services.AddDbContext<HospitalDbContext>((serviceProvider, options) =>
{
    options.AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());
});
```

## Security middleware

- `SecurityHeadersMiddleware` adds HSTS, `X-Frame-Options`, `X-Content-Type-Options`, CSP, `Permissions-Policy`, and referrer policy headers.
- CORS origins are configured under `Security:AllowedOrigins`.
- Set production origins explicitly instead of allowing broad origin lists.

## Best practices

- Rotate JWT secrets regularly.
- Keep token lifetimes short and renew them through authenticated refresh flows.
- Always hash passwords with `PasswordHasher`; never store plaintext passwords.
- Apply the least-privilege role required for each user account.
- Review audit logs for sensitive operations such as billing, HR, and admin actions.
