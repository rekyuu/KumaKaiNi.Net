FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["KumaKaiNi.Telegram/KumaKaiNi.Telegram.csproj", "KumaKaiNi.Telegram/"]
RUN dotnet restore "KumaKaiNi.Telegram/KumaKaiNi.Telegram.csproj"
COPY . .
WORKDIR "/src/KumaKaiNi.Telegram"
RUN dotnet publish "KumaKaiNi.Telegram.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KumaKaiNi.Telegram.dll"]