FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["KumaKaiNi.Discord/KumaKaiNi.Discord.csproj", "KumaKaiNi.Discord/"]
RUN dotnet restore "KumaKaiNi.Discord/KumaKaiNi.Discord.csproj"
COPY . .
WORKDIR "/src/KumaKaiNi.Discord"
RUN dotnet publish "KumaKaiNi.Discord.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KumaKaiNi.Discord.dll"]