#!bin/sh

ENV=$1

#Deploy Netcore3 Server-Gigya
PKG_VERSION=`xmllint --xpath "string(//Project/PropertyGroup/AssemblyVersion)" ./OwnID.Server.Gigya/OwnID.Server.Gigya.csproj`
IMAGE_URI=$ARTIFACTORY_URL/$ENV/server/ownid-server-gigya_${PKG_VERSION-}:$TRAVIS_COMMIT

echo Docker push to $IMAGE_URI
docker tag ownid-server-gigya:latest $IMAGE_URI
docker push $IMAGE_URI

echo K8S cluster selection
aws eks --region us-east-2 update-kubeconfig --name ownid-eks

echo Apply latest manifest files
kubectl apply -f manifests/$ENV.yaml

echo Images URI update
kubectl -n=$ENV set image deployment/ownid-server-netcore3-demo-gigya-deployment ownid-server-netcore3-demo-gigya=$IMAGE_URI --record
kubectl -n=$ENV set image deployment/ownid-server-netcore3-demo-2-gigya-deployment ownid-server-netcore3-demo-2-gigya=$IMAGE_URI --record
kubectl -n=$ENV set image deployment/ownid-server-netcore3-demo-3-gigya-deployment ownid-server-netcore3-demo-3-gigya=$IMAGE_URI --record
kubectl -n=$ENV set image deployment/ownid-server-netcore3-demo-4-gigya-deployment ownid-server-netcore3-demo-4-gigya=$IMAGE_URI --record