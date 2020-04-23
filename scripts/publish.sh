#!bin/sh

ENV=$1

PKG_VERSION=`xpath ./OwnIdSdk.NetCore3.Web/OwnIdSdk.NetCore3.Web.csproj //Project/PropertyGroup/Version/text\(\)`
S3PATH=s3://ownid-sdks-bucket/$ENV/server-sdks/dotnetcore3
FOLDER=$PKG_VERSION"_"$TRAVIS_BUILD_NUMBER

echo Path: $S3PATH/$FOLDER

dotnet build  --configuration Release --version-suffix ci-build-1 

aws s3 cp ./OwnIdSdk.NetCore3.Web/bin/Release/netcoreapp3.1 $S3PATH/latest --recursive
aws s3 cp ./OwnIdSdk.NetCore3.Web/bin/Release/netcoreapp3.1 $S3PATH/$FOLDER --recursive

