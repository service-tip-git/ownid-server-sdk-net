#!bin/sh

echo Adding K8S clusters
echo Prod A
aws eks --region us-east-1 update-kubeconfig --name ownid-production-cluster
echo Prod B
aws eks --region us-east-2 update-kubeconfig --name ownid-eks

#Deploy Netcore3 Server-Gigya
PKG_VERSION=`xmllint --xpath "string(//Project/PropertyGroup/AssemblyVersion)" ./OwnIdSdk.NetCore3.Server.Gigya/OwnIdSdk.NetCore3.Server.Gigya.csproj`
IMAGE_URI=$ARTIFACTORY_URL/prod/server/ownid-server-gigya_${PKG_VERSION-}:$TRAVIS_COMMIT

echo Docker push to $IMAGE_URI
docker tag ownid-server-gigya:latest $IMAGE_URI
docker push $IMAGE_URI

echo Adding K8S clusters...
aws eks --region us-east-1 update-kubeconfig --name ownid-production-cluster
aws eks --region us-east-2 update-kubeconfig --name ownid-eks
kubectl config get-contexts

echo Prod A Deployment
kubectl config use-context arn:aws:eks:us-east-1:571861302935:cluster/ownid-production-cluster
sh scripts/prod/k8s-update.sh $IMAGE_URI

echo Prod B Deployment
kubectl config use-context arn:aws:eks:us-east-2:571861302935:cluster/ownid-eks
sh scripts/prod/k8s-update.sh $IMAGE_URI
