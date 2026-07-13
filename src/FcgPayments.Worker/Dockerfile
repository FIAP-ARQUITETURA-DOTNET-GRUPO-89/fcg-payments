# syntax=docker/dockerfile:1.7

########################################
# Build
########################################
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Arquivos compartilhados da solução
COPY global.json .
COPY Directory.Build.props .
COPY Directory.Packages.props .

# Copia apenas os arquivos de projeto (melhora o cache)
COPY src/FcgPayments.Worker/FcgPayments.Worker.csproj src/FcgPayments.Worker/
COPY src/FcgPayments.Application/FcgPayments.Application.csproj src/FcgPayments.Application/
COPY src/FcgPayments.Domain/FcgPayments.Domain.csproj src/FcgPayments.Domain/
COPY src/FcgPayments.Infrastructure/FcgPayments.Infrastructure.csproj src/FcgPayments.Infrastructure/
COPY src/FcgPayments.IoC/FcgPayments.IoC.csproj src/FcgPayments.IoC/
COPY src/FcgPayments.SharedKernel/FcgPayments.SharedKernel.csproj src/FcgPayments.SharedKernel/
COPY src/FcgPayments.ServiceDefaults/FcgPayments.ServiceDefaults.csproj src/FcgPayments.ServiceDefaults/

# Restore
RUN dotnet restore src/FcgPayments.Worker/FcgPayments.Worker.csproj

# Copia todo o restante do código
COPY . .

# Publish
RUN dotnet publish src/FcgPayments.Worker/FcgPayments.Worker.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

########################################
# Runtime
########################################
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Executa como usuário não-root
USER $APP_UID

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FcgPayments.Worker.dll"]
