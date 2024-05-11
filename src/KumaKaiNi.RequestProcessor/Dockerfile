FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_VERSION=0.0.0-alpha
ENV BUILD_VERSION=$BUILD_VERSION

WORKDIR /src
COPY ["KumaKaiNi.RequestProcessor/KumaKaiNi.RequestProcessor.csproj", "KumaKaiNi.RequestProcessor/"]
RUN dotnet restore "KumaKaiNi.RequestProcessor/KumaKaiNi.RequestProcessor.csproj"
COPY . .
WORKDIR "/src/KumaKaiNi.RequestProcessor"
RUN dotnet publish "KumaKaiNi.RequestProcessor.csproj" -c Release -o /app/publish

FROM base AS final

ARG COMMIT_SHA=unknown
ENV COMMIT_SHA=$COMMIT_SHA

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KumaKaiNi.RequestProcessor.dll"]