FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/KumaKaiNi.Twitch/KumaKaiNi.Twitch.csproj", "src/KumaKaiNi.Twitch/"]
RUN dotnet restore "src/KumaKaiNi.Twitch/KumaKaiNi.Twitch.csproj"
COPY . .
WORKDIR "/src/src/KumaKaiNi.Twitch"
RUN dotnet build "KumaKaiNi.Twitch.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KumaKaiNi.Twitch.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KumaKaiNi.Twitch.dll"]