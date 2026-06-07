using HospitalMS.Business.Services;
using HospitalMS.Common.Constants;

namespace HospitalMS.Tests.Architecture;

public class ProjectStructureTests
{
    [Fact]
    public void BusinessLayerPlaceholder_ReturnsReadyStatus()
    {
        var service = new SystemStatusService();

        Assert.Equal($"{ApplicationInfo.ApplicationName} business layer ready.", service.GetStatus());
    }

    [Fact]
    public void AppHostBuildOutput_ExistsAfterCompilation()
    {
        var repoRoot = FindRepositoryRoot();
        var buildOutputs = Directory.EnumerateFiles(
            Path.Combine(repoRoot, "HospitalMS.AppHost", "bin"),
            "HospitalMS.AppHost.dll",
            SearchOption.AllDirectories);

        Assert.NotEmpty(buildOutputs);
    }

    [Fact]
    public void AppHostProgram_ConfiguresPostgresAndServices()
    {
        var repoRoot = FindRepositoryRoot();
        var programFile = Path.Combine(repoRoot, "HospitalMS.AppHost", "Program.cs");
        var appHostProgram = File.ReadAllText(programFile);

        Assert.Contains("DistributedApplication.CreateBuilder(", appHostProgram);
        Assert.Contains("AddPostgres(", appHostProgram);
        Assert.Contains("AddDatabase(", appHostProgram);
        Assert.Contains("AddProject<Projects.HospitalMS_API>", appHostProgram);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "HospitalManagementSystem.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
