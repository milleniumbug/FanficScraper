FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FanficScraper/FanficScraper.csproj", "FanficScraper/"]
RUN dotnet restore "FanficScraper/FanficScraper.csproj"
COPY . .
WORKDIR "/src/FanficScraper"
RUN dotnet build "FanficScraper.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FanficScraper.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FanficScraper.dll"]
