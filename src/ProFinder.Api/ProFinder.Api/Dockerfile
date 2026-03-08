FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ProFinder.sln ./
COPY src/ProFinder.Domain/ProFinder.Domain/ProFinder.Domain.csproj src/ProFinder.Domain/ProFinder.Domain/
COPY src/ProFinder.Application/ProFinder.Application/ProFinder.Application.csproj src/ProFinder.Application/ProFinder.Application/
COPY src/ProFinder.Infrastructure/ProFinder.Infrastructure/ProFinder.Infrastructure.csproj src/ProFinder.Infrastructure/ProFinder.Infrastructure/
COPY src/ProFinder.Api/ProFinder.Api/ProFinder.Api.csproj src/ProFinder.Api/ProFinder.Api/

RUN dotnet restore src/ProFinder.Api/ProFinder.Api/ProFinder.Api.csproj

COPY . .
RUN dotnet publish src/ProFinder.Api/ProFinder.Api/ProFinder.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ProFinder.Api.dll"]
