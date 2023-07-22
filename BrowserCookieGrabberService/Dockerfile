FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BrowserCookieGrabberService/BrowserCookieGrabberService.csproj", "BrowserCookieGrabberService/"]
RUN dotnet restore "BrowserCookieGrabberService/BrowserCookieGrabberService.csproj"
COPY . .
WORKDIR "/src/BrowserCookieGrabberService"
RUN dotnet build "BrowserCookieGrabberService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BrowserCookieGrabberService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BrowserCookieGrabberService.dll"]
