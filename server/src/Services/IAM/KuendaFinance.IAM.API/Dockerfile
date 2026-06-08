FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy projects and restore
COPY ["src/Shared/KuendaFinance.Shared/KuendaFinance.Shared.csproj", "src/Shared/KuendaFinance.Shared/"]
COPY ["src/Services/IAM/KuendaFinance.IAM.Domain/KuendaFinance.IAM.Domain.csproj", "src/Services/IAM/KuendaFinance.IAM.Domain/"]
COPY ["src/Services/IAM/KuendaFinance.IAM.Application/KuendaFinance.IAM.Application.csproj", "src/Services/IAM/KuendaFinance.IAM.Application/"]
COPY ["src/Services/IAM/KuendaFinance.IAM.Infrastructure/KuendaFinance.IAM.Infrastructure.csproj", "src/Services/IAM/KuendaFinance.IAM.Infrastructure/"]
COPY ["src/Services/IAM/KuendaFinance.IAM.API/KuendaFinance.IAM.API.csproj", "src/Services/IAM/KuendaFinance.IAM.API/"]

RUN dotnet restore "src/Services/IAM/KuendaFinance.IAM.API/KuendaFinance.IAM.API.csproj"

# Copy all source
COPY . .
WORKDIR "/src/src/Services/IAM/KuendaFinance.IAM.API"
RUN dotnet build "KuendaFinance.IAM.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "KuendaFinance.IAM.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KuendaFinance.IAM.API.dll"]
