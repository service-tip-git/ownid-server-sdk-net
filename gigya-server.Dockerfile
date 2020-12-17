FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

EXPOSE 5002

# Copy csproj and restore as distinct layers
COPY ./OwnID ./OwnID
COPY ./OwnID.Extensibility ./OwnID.Extensibility
COPY ./OwnID.Web ./OwnID.Web
COPY ./OwnID.Web.Extensibility ./OwnID.Web.Extensibility
COPY ./OwnID.Web.Gigya ./OwnID.Web.Gigya
COPY ./OwnID.Server.Gigya ./OwnID.Server.Gigya
COPY ./OwnID.Redis ./OwnID.Redis

RUN dotnet publish ./OwnID.Server.Gigya/OwnID.Server.Gigya.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine
WORKDIR /app
COPY --from=build-env /app/out .


#ENV ALLOWEDHOSTS="localhost"
#ENV OWNID__WEB_APP_URL="https://app.ownid.com"
ENV OWNID__CALLBACK_URL="https://localhost:5002"
ENV OWNID__PUB_KEY="./keys/jwtRS256.key.pub"
ENV OWNID__PRIVATE_KEY="./keys/jwtRS256.key"
ENV OWNID__DID="did:ownid:local_151850889514"
ENV OWNID__NAME="Local Client"
ENV OWNID__DESCRIPTION="Local Client environment"
ENV GIGYA__SECRET="g157+kUR3kxvgIX4MneEWnVgBVzhQe4dXfoNe9ceSNA="
ENV GIGYA__API_KEY="3_s5-gLs4aLp5FXluP8HXs7_JN40XWNlbvYWVCCkbNCqlhW6Sm5Z4tXGGsHcSJYD3W"
ENV OWNID__CACHE_TYPE="web-cache"
ENV ELASTICCONFIGURATION__ENABLED="false"
ENV ASPNETCORE_ENVIRONMENT="prod"
#ENV SERVER_MODE="production"

ENTRYPOINT ["dotnet", "OwnID.Server.Gigya.dll"]

# docker build -t ownid-server-gigya:latest .
# docker run -it -p 5002:5002 ownid-server-gigya:latest