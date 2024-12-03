FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_VERSION=0.0.0-alpha
ENV BUILD_VERSION=$BUILD_VERSION

WORKDIR /src
COPY ["KumaKaiNi.Client.Telegram/KumaKaiNi.Client.Telegram.csproj", "KumaKaiNi.Client.Telegram/"]
RUN dotnet restore "KumaKaiNi.Client.Telegram/KumaKaiNi.Client.Telegram.csproj"
COPY . .
WORKDIR "/src/KumaKaiNi.Client.Telegram"
RUN dotnet publish "KumaKaiNi.Client.Telegram.csproj" -c Release -o /app/publish

FROM base AS final

ARG COMMIT_SHA=unknown
ENV COMMIT_SHA=$COMMIT_SHA

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KumaKaiNi.Client.Telegram.dll"]