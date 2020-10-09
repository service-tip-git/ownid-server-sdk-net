#!bin/sh
ENV=$1

S3PATH=s3://passwordless.$ENV.ownid.com
aws s3 cp $S3PATH ./OwnIdSdk.NetCore3.Server.Gigya/wwwroot --recursive

dotnet restore
# - dotnet test /p:CollectCoverage=true /p:Threshold=20
docker build -t ownid-server-netcore3-gigya:latest . --network=host