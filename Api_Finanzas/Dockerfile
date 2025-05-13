# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copiar el archivo de solución
COPY ["Api_Finanzas.sln", "./"]

# Copiar los proyectos (ajustá los nombres si están en subcarpetas)
COPY ["Api_Finanzas/Api_Finanzas.csproj", "Api_Finanzas/"]
RUN dotnet restore "./Api_Finanzas.csproj"

# Copiar el resto de los archivos
COPY . .

WORKDIR "/src/Api_Finanzas"
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Api_Finanzas.dll"]