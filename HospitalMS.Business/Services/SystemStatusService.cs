using HospitalMS.Common.Constants;

namespace HospitalMS.Business.Services;

public interface ISystemStatusService
{
    string GetStatus();
}

public sealed class SystemStatusService : ISystemStatusService
{
    public string GetStatus() => $"{ApplicationInfo.ApplicationName} business layer ready.";
}
