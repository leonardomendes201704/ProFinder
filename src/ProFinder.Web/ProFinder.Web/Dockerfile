FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ProFinder.sln ./
COPY src/ProFinder.Domain/ProFinder.Domain/ProFinder.Domain.csproj src/ProFinder.Domain/ProFinder.Domain/
COPY src/ProFinder.Application/ProFinder.Application/ProFinder.Application.csproj src/ProFinder.Application/ProFinder.Application/
COPY src/ProFinder.Infrastructure/ProFinder.Infrastructure/ProFinder.Infrastructure.csproj src/ProFinder.Infrastructure/ProFinder.Infrastructure/
COPY src/ProFinder.Web/ProFinder.Web/ProFinder.Web.csproj src/ProFinder.Web/ProFinder.Web/

RUN dotnet restore src/ProFinder.Web/ProFinder.Web/ProFinder.Web.csproj

COPY . .
RUN dotnet publish src/ProFinder.Web/ProFinder.Web/ProFinder.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    PLAYWRIGHT_BROWSERS_PATH=/ms-playwright \
    CHROME_BIN=/usr/bin/chromium \
    CHROMEDRIVER_PATH=/usr/bin/chromedriver

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        python3 \
        python3-venv \
        python3-pip \
        curl \
        gnupg \
        unixodbc \
        chromium \
        chromium-driver \
        ca-certificates \
        fonts-liberation \
        libasound2 \
        libatk-bridge2.0-0 \
        libatk1.0-0 \
        libatspi2.0-0 \
        libdrm2 \
        libgbm1 \
        libgtk-3-0 \
        libnspr4 \
        libnss3 \
        libu2f-udev \
        libvulkan1 \
        libxcomposite1 \
        libxdamage1 \
        libxfixes3 \
        libxkbcommon0 \
        libxrandr2 \
        xdg-utils \
    && curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg \
    && echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/microsoft-prod.gpg] https://packages.microsoft.com/debian/12/prod bookworm main" > /etc/apt/sources.list.d/microsoft-prod.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y --no-install-recommends msodbcsql18 \
    && rm -rf /var/lib/apt/lists/*

RUN python3 -m venv /opt/crawler-venv
ENV PATH="/opt/crawler-venv/bin:${PATH}"

COPY crawler ./crawler
RUN pip install --no-cache-dir -r crawler/requirements.txt \
    && python -m playwright install chromium \
    && mkdir -p /app/crawler/exports

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ProFinder.Web.dll"]
