FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

EXPOSE 5002

# Copy csproj and restore as distinct layers
COPY ./OwnIdSdk.NetCore3 ./OwnIdSdk.NetCore3
COPY ./OwnIdSdk.NetCore3.Extensibility ./OwnIdSdk.NetCore3.Extensibility
COPY ./OwnIdSdk.NetCore3.Web ./OwnIdSdk.NetCore3.Web
COPY ./OwnIdSdk.NetCore3.Web.Extensibility ./OwnIdSdk.NetCore3.Web.Extensibility
COPY ./OwnIdSdk.NetCore3.Web.Gigya ./OwnIdSdk.NetCore3.Web.Gigya
COPY ./OwnIdSdk.NetCore3.Server.Gigya ./OwnIdSdk.NetCore3.Server.Gigya
COPY ./OwnIdSdk.NetCore3.Redis ./OwnIdSdk.NetCore3.Redis

RUN dotnet restore ./OwnIdSdk.NetCore3.Server.Gigya/OwnIdSdk.NetCore3.Server.Gigya.csproj
RUN dotnet publish ./OwnIdSdk.NetCore3.Server.Gigya/OwnIdSdk.NetCore3.Server.Gigya.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
WORKDIR /app
COPY --from=build-env /app/out .


#ENV ALLOWEDHOSTS="localhost"
ENV OWNID__WEB_APP_URL="https://app.ownid.com"
ENV OWNID__CALLBACK_URL="https://localhost:5002"
ENV OWNID__PUB_KEY="./keys/jwtRS256.key.pub"
ENV OWNID__PRIVATE_KEY="./keys/jwtRS256.key"
ENV OWNID__DID="did:ownid:local_151850889514"
ENV OWNID__NAME="Local Client NetCore3"
ENV OWNID__DESCRIPTION="Local Client NetCore3 environment"
ENV GIGYA__SECRET="g157+kUR3kxvgIX4MneEWnVgBVzhQe4dXfoNe9ceSNA="
ENV GIGYA__API_KEY="3_s5-gLs4aLp5FXluP8HXs7_JN40XWNlbvYWVCCkbNCqlhW6Sm5Z4tXGGsHcSJYD3W"
ENV OWNID__CACHE_TYPE="web-cache"
ENV ELASTICCONFIGURATION__ENABLED="false"
ENV ASPNETCORE_ENVIRONMENT="prod"

ENTRYPOINT ["dotnet", "OwnIdSdk.NetCore3.Server.Gigya.dll"]

# docker build -t ownid-server-gigya:latest .
# docker run -it -p 5002:5002 ownid-server-gigya:latest