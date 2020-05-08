#!bin/sh

REPOSITORY_URI=$1
IMAGE_TAG=$2
ENV=$3

PKG_VERSION=`xmllint --xpath "string(//Project/PropertyGroup/Version)" ./OwnIdSdk.NetCore3.Web/OwnIdSdk.NetCore3.Web.csproj`
S3PATH=s3://ownid-sdks-bucket/$ENV/server-sdks/dotnetcore3
FOLDER=$PKG_VERSION"_"$TRAVIS_BUILD_NUMBER

echo Path: $S3PATH/$FOLDER

# Publish Netcore3 library
dotnet build  --configuration Release --version-suffix $FOLDER

aws s3 cp ./OwnIdSdk.NetCore3.Web/bin/Release/netcoreapp3.1 $S3PATH/latest --recursive
aws s3 cp ./OwnIdSdk.NetCore3.Web/bin/Release/netcoreapp3.1 $S3PATH/$FOLDER --recursive

#Deploy Netcore3 Server-Gigya
echo Pushing image $REPOSITORY_URI:$IMAGE_TAG to registry
docker tag ownid-server-netcore3-gigya:latest $REPOSITORY_URI:$IMAGE_TAG
docker push $REPOSITORY_URI:$IMAGE_TAG

echo Updating image in Cluster deployment
kubectl apply -f manifests/$ENV.yaml
kubectl -n=$ENV set image deployment/ownid-server-netcore3-gigya-deployment ownid-server-netcore3-gigya=$REPOSITORY_URI:$IMAGE_TAG --record
kubectl -n=$ENV set image deployment/ownid-server-netcore3-gigya-deployment ownid-server-netcore3-gigya-2=$REPOSITORY_URI:$IMAGE_TAG --record
