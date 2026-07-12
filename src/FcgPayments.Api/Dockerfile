FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/FcgPayments.Api/FcgPayments.Api.csproj src/FcgPayments.Api/
COPY src/FcgPayments.Application/FcgPayments.Application.csproj src/FcgPayments.Application/
COPY src/FcgPayments.Domain/FcgPayments.Domain.csproj src/FcgPayments.Domain/
COPY src/FcgPayments.Infrastructure/FcgPayments.Infrastructure.csproj src/FcgPayments.Infrastructure/
COPY src/FcgPayments.IoC/FcgPayments.IoC.csproj src/FcgPayments.IoC/
COPY src/FcgPayments.ServiceDefaults/FcgPayments.ServiceDefaults.csproj src/FcgPayments.ServiceDefaults/
COPY src/FcgPayments.SharedKernel/FcgPayments.SharedKernel.csproj src/FcgPayments.SharedKernel/

RUN dotnet restore "src/FcgPayments.Api/FcgPayments.Api.csproj"

COPY src/ src/

WORKDIR /src/src/FcgPayments.Api
RUN dotnet publish "FcgPayments.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FcgPayments.Api.dll"]
