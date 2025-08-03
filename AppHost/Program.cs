using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject("webapi", "../WebAPI/HospitalManagementSystem.WebAPI.csproj");
var web = builder.AddProject("webapp", "../WebApp/HospitalManagementSystem.WebApp.csproj")
                 .WithReference(api);

builder.Build().Run();

