FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["src/KumaKaiNi.Twitch/KumaKaiNi.Twitch.csproj", "KumaKaiNi.Twitch/"]
RUN dotnet restore "src/KumaKaiNi.Twitch/KumaKaiNi.Twitch.csproj"
COPY . .
WORKDIR "/src/KumaKaiNi.Twitch"
RUN dotnet build "KumaKaiNi.Twitch.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KumaKaiNi.Twitch.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KumaKaiNi.Twitch.dll"]
