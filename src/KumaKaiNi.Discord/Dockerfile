FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["src/KumaKaiNi.Discord/KumaKaiNi.Discord.csproj", "KumaKaiNi.Discord/"]
RUN dotnet restore "src/KumaKaiNi.Discord/KumaKaiNi.Discord.csproj"
COPY . .
WORKDIR "/src/KumaKaiNi.Discord"
RUN dotnet build "KumaKaiNi.Discord.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KumaKaiNi.Discord.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KumaKaiNi.Discord.dll"]
