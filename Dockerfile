FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["WeatherAPI.sln", "./"]
COPY ["src/WeatherAPI.Api/WeatherAPI.Api.csproj", "src/WeatherAPI.Api/"]
COPY ["src/WeatherAPI.Application/WeatherAPI.Application.csproj", "src/WeatherAPI.Application/"]
COPY ["src/WeatherAPI.Domain/WeatherAPI.Domain.csproj", "src/WeatherAPI.Domain/"]
COPY ["src/WeatherAPI.Infrastructure/WeatherAPI.Infrastructure.csproj", "src/WeatherAPI.Infrastructure/"]

RUN dotnet restore "WeatherAPI.sln"

COPY . .
RUN dotnet publish "src/WeatherAPI.Api/WeatherAPI.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WeatherAPI.Api.dll"]
