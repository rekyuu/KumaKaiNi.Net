FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/KumaKaiNi.Telegram/KumaKaiNi.Telegram.csproj", "src/KumaKaiNi.Telegram/"]
RUN dotnet restore "src/KumaKaiNi.Telegram/KumaKaiNi.Telegram.csproj"
COPY . .
WORKDIR "/src/src/KumaKaiNi.Telegram"
RUN dotnet build "KumaKaiNi.Telegram.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KumaKaiNi.Telegram.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KumaKaiNi.Telegram.dll"]