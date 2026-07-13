# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY global.json .
COPY Directory.Build.props .
COPY Directory.Packages.props .

COPY src/FcgPayments.Api/FcgPayments.Api.csproj src/FcgPayments.Api/
COPY src/FcgPayments.Application/FcgPayments.Application.csproj src/FcgPayments.Application/
COPY src/FcgPayments.Domain/FcgPayments.Domain.csproj src/FcgPayments.Domain/
COPY src/FcgPayments.Infrastructure/FcgPayments.Infrastructure.csproj src/FcgPayments.Infrastructure/
COPY src/FcgPayments.IoC/FcgPayments.IoC.csproj src/FcgPayments.IoC/
COPY src/FcgPayments.SharedKernel/FcgPayments.SharedKernel.csproj src/FcgPayments.SharedKernel/
COPY src/FcgPayments.ServiceDefaults/FcgPayments.ServiceDefaults.csproj src/FcgPayments.ServiceDefaults/

RUN dotnet restore src/FcgPayments.Api/FcgPayments.Api.csproj

COPY . .

RUN dotnet publish src/FcgPayments.Api/FcgPayments.Api.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

USER $APP_UID

EXPOSE 7074

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet","FcgPayments.Api.dll"]
