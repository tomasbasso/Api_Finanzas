FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["Api_Finanzas/Api_Finanzas.csproj", "Api_Finanzas/"]
RUN dotnet restore "Api_Finanzas/Api_Finanzas.csproj"

# Copy the remaining source and build
COPY . .
WORKDIR /src/Api_Finanzas
RUN dotnet publish "Api_Finanzas.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Render passes the port via the PORT env var
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
EXPOSE 10000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api_Finanzas.dll"]
