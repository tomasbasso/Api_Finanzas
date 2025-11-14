# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Api_Finanzas/Api_Finanzas.csproj", "Api_Finanzas/"]
RUN dotnet restore "Api_Finanzas/Api_Finanzas.csproj"

COPY . .
WORKDIR /src/Api_Finanzas
RUN dotnet publish "Api_Finanzas.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Render sets PORT automatically
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
EXPOSE ${PORT}

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Api_Finanzas.dll"]
