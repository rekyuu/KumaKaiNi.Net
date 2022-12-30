FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["KumaKaiNi.Twitch/KumaKaiNi.Twitch.csproj", "KumaKaiNi.Twitch/"]
RUN dotnet restore "KumaKaiNi.Twitch/KumaKaiNi.Twitch.csproj"
COPY . .
WORKDIR "/src/KumaKaiNi.Twitch"
RUN dotnet publish "KumaKaiNi.Twitch.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KumaKaiNi.Twitch.dll"]