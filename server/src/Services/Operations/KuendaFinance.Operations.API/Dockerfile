FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy projects and restore
COPY ["src/Shared/KuendaFinance.Shared/KuendaFinance.Shared.csproj", "src/Shared/KuendaFinance.Shared/"]
COPY ["src/Services/Operations/KuendaFinance.Operations.Domain/KuendaFinance.Operations.Domain.csproj", "src/Services/Operations/KuendaFinance.Operations.Domain/"]
COPY ["src/Services/Operations/KuendaFinance.Operations.Application/KuendaFinance.Operations.Application.csproj", "src/Services/Operations/KuendaFinance.Operations.Application/"]
COPY ["src/Services/Operations/KuendaFinance.Operations.Infrastructure/KuendaFinance.Operations.Infrastructure.csproj", "src/Services/Operations/KuendaFinance.Operations.Infrastructure/"]
COPY ["src/Services/Operations/KuendaFinance.Operations.API/KuendaFinance.Operations.API.csproj", "src/Services/Operations/KuendaFinance.Operations.API/"]

RUN dotnet restore "src/Services/Operations/KuendaFinance.Operations.API/KuendaFinance.Operations.API.csproj"

# Copy all source
COPY . .
WORKDIR "/src/src/Services/Operations/KuendaFinance.Operations.API"
RUN dotnet build "KuendaFinance.Operations.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "KuendaFinance.Operations.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KuendaFinance.Operations.API.dll"]
