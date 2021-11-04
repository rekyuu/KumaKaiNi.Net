FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["src/KumaKaiNi.Telegram/KumaKaiNi.Telegram.csproj", "KumaKaiNi.Telegram/"]
RUN dotnet restore "src/KumaKaiNi.Telegram/KumaKaiNi.Telegram.csproj"
COPY . .
WORKDIR "/src/KumaKaiNi.Telegram"
RUN dotnet build "KumaKaiNi.Telegram.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KumaKaiNi.Telegram.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KumaKaiNi.Telegram.dll"]
