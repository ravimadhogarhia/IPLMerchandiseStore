
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ["IPLMerchandise.Api/IPLMerchandise.Api.csproj", "IPLMerchandise.Api/"]
COPY ["IPLMerchandise.Application/IPLMerchandise.Application.csproj", "IPLMerchandise.Application/"]
COPY ["IPLMerchandise.Domain/IPLMerchandise.Domain.csproj", "IPLMerchandise.Domain/"]
COPY ["IPLMerchandise.Infrastructure/IPLMerchandise.Infrastructure.csproj", "IPLMerchandise.Infrastructure/"]

RUN dotnet restore "IPLMerchandise.Api/IPLMerchandise.Api.csproj"

COPY . .

WORKDIR /src/IPLMerchandise.Api

RUN dotnet publish "IPLMerchandise.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "IPLMerchandise.Api.dll"]
