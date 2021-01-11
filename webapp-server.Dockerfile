FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

EXPOSE 5002

# Copy csproj and restore as distinct layers
COPY ./OwnID.Server.WebApp ./OwnID.Server.WebApp
COPY ./OwnID.Extensibility ./OwnID.Extensibility

RUN dotnet publish ./OwnID.Server.WebApp/OwnID.Server.WebApp.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine
WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "OwnID.Server.WebApp.dll"]