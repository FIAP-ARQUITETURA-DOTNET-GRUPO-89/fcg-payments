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
COPY src/FcgUsers.Api/FcgUsers.Api.csproj src/FcgUsers.Api/
COPY src/FcgUsers.Application/FcgUsers.Application.csproj src/FcgUsers.Application/
COPY src/FcgUsers.Domain/FcgUsers.Domain.csproj src/FcgUsers.Domain/
COPY src/FcgUsers.Infrastructure/FcgUsers.Infrastructure.csproj src/FcgUsers.Infrastructure/
COPY src/FcgUsers.IoC/FcgUsers.IoC.csproj src/FcgUsers.IoC/
COPY src/FcgUsers.ServiceDefaults/FcgUsers.ServiceDefaults.csproj src/FcgUsers.ServiceDefaults/
COPY src/FcgUsers.SharedKernel/FcgUsers.SharedKernel.csproj src/FcgUsers.SharedKernel/

# Restore
RUN dotnet restore src/FcgUsers.Api/FcgUsers.Api.csproj

# Copia todo o restante do código
COPY . .

# Publish
RUN dotnet publish src/FcgUsers.Api/FcgUsers.Api.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

########################################
# Runtime
########################################
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

RUN apt-get update && apt-get install -y \
    libgssapi-krb5-2 \
    krb5-locales \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

USER $APP_UID

EXPOSE 7071

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FcgUsers.Api.dll"]
