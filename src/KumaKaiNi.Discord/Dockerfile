FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/KumaKaiNi.Discord/KumaKaiNi.Discord.csproj", "src/KumaKaiNi.Discord/"]
RUN dotnet restore "src/KumaKaiNi.Discord/KumaKaiNi.Discord.csproj"
COPY . .
WORKDIR "/src/src/KumaKaiNi.Discord"
RUN dotnet build "KumaKaiNi.Discord.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KumaKaiNi.Discord.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KumaKaiNi.Discord.dll"]