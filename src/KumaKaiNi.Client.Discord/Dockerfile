FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
WORKDIR /app

RUN apk add --no-cache icu-libs

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_VERSION=0.0.0-alpha
ENV BUILD_VERSION=$BUILD_VERSION

WORKDIR /src
COPY ["KumaKaiNi.Client.Discord/KumaKaiNi.Client.Discord.csproj", "KumaKaiNi.Client.Discord/"]
RUN dotnet restore "KumaKaiNi.Client.Discord/KumaKaiNi.Client.Discord.csproj"
COPY . .
WORKDIR "/src/KumaKaiNi.Client.Discord"
RUN dotnet publish "KumaKaiNi.Client.Discord.csproj" -c Release -o /app/publish

FROM base AS final

ARG COMMIT_SHA=unknown
ENV COMMIT_SHA=$COMMIT_SHA

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KumaKaiNi.Client.Discord.dll"]