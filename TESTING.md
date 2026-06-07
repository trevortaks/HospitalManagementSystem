# Testing Guide

## Unit tests

- Inherit from `UnitTestBase` for common setup and mock helpers.
- Use `CreateMock<T>()` for general mocks, `CreateRepositoryMock<TEntity>()` for repository doubles, and `CreateSystemStatusServiceMock()` or `CreateNotificationServiceMock()` for service doubles.
- Keep unit tests focused on business logic and verify expected mock interactions with `VerifyAllMocks()`.
- See `HospitalMS.Tests/UnitTests/SampleServiceTests.cs` for a working example.

## Integration tests

- Inherit from `IntegrationTestBase` to get an isolated EF Core in-memory database per test.
- Override `SeedAsync(HospitalDbContext context)` when test data should exist before each test.
- Use `CreateContext()` to open additional contexts against the same in-memory database when you want to verify persisted state separately from the write operation.
- See `HospitalMS.Tests/IntegrationTests/SampleRepositoryTests.cs` for an example repository test.

## Test fixtures and helpers

- `TestFixtures.cs` contains reusable patient and user factories plus database option helpers.
- `MockFactories.cs` contains reusable repository and service mock builders.

## Running tests

```bash
dotnet test
```

## Running tests with coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

The test project already references `coverlet.collector`, so the coverage report is produced automatically when collection is enabled.
