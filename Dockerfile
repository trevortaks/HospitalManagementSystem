FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG APP_PROJECT=HospitalMS.Web/HospitalMS.Web.csproj
ARG APP_DLL=HospitalMS.Web.dll

WORKDIR /src

COPY HospitalManagementSystem.sln ./
COPY HospitalMS.API/HospitalMS.API.csproj HospitalMS.API/
COPY HospitalMS.AppHost/HospitalMS.AppHost.csproj HospitalMS.AppHost/
COPY HospitalMS.Business/HospitalMS.Business.csproj HospitalMS.Business/
COPY HospitalMS.Common/HospitalMS.Common.csproj HospitalMS.Common/
COPY HospitalMS.Data/HospitalMS.Data.csproj HospitalMS.Data/
COPY HospitalMS.ServiceDefaults/HospitalMS.ServiceDefaults.csproj HospitalMS.ServiceDefaults/
COPY HospitalMS.Tests/HospitalMS.Tests.csproj HospitalMS.Tests/
COPY HospitalMS.Web/HospitalMS.Web.csproj HospitalMS.Web/

RUN dotnet restore HospitalManagementSystem.sln

COPY . .

RUN dotnet build "${APP_PROJECT}" -c Release --no-restore \
    && dotnet publish "${APP_PROJECT}" -c Release --no-build -o /app/publish /p:UseAppHost=false \
    && test -f "/app/publish/${APP_DLL}"

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ARG APP_DLL=HospitalMS.Web.dll

WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:5001 \
    ASPNETCORE_ENVIRONMENT=Production \
    APP_DLL=${APP_DLL}

EXPOSE 5001 5002

ENTRYPOINT ["sh", "-c", "dotnet ${APP_DLL}"]
