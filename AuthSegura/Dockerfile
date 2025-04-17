# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copiamos los archivos del proyecto y restauramos dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiamos el resto de los archivos y publicamos
COPY . ./
RUN dotnet publish -c Release -o out

# Imagen final
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copiamos el resultado de la build
COPY --from=build-env /app/out .

# Configuramos la URL de escucha
ENV ASPNETCORE_URLS=http://+:80

# Exponemos el puerto
EXPOSE 80

# Entrypoint
ENTRYPOINT ["dotnet", "AuthSegura.dll"]
