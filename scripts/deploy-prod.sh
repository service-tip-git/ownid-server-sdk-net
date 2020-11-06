#!bin/sh

ENV=$1

#Deploy Netcore3 Server-Gigya
PKG_VERSION=`xmllint --xpath "string(//Project/PropertyGroup/AssemblyVersion)" ./OwnIdSdk.NetCore3.Server.Gigya/OwnIdSdk.NetCore3.Server.Gigya.csproj`
IMAGE_URI=$ARTIFACTORY_URL/$ENV/server/ownid-server-gigya_${PKG_VERSION-}:$TRAVIS_COMMIT

echo Docker push to $IMAGE_URI
docker tag ownid-server-gigya:latest $IMAGE_URI
docker push $IMAGE_URI

echo Prod A Deployment

echo K8S cluster selection
aws eks --region us-east-2 update-kubeconfig --name ownid-eks

echo Updating objects in Cluster 
kubectl apply -f manifests/$ENV.yaml

echo Updating images in Cluster deployments
kubectl -n=$ENV set image deployment/nestle-hipster-deployment nestle-hipster=$IMAGE_URI --record
kubectl -n=$ENV set image deployment/bayer-deployment bayer=$IMAGE_URI --record
kubectl -n=$ENV set image deployment/universalid-deployment universalid=$IMAGE_URI --record

echo Prod B Deployment

echo K8S cluster selection
aws eks --region us-east-1 update-kubeconfig --name ownid-production-cluster
kubectl config use-context arn:aws:eks:us-east-2:571861302935:cluster/ownid-eks

echo Updating objects in Cluster 
kubectl apply -f manifests/$ENV.yaml

echo Updating images in Cluster deployments
kubectl -n=$ENV set image deployment/nestle-hipster-deployment nestle-hipster=$IMAGE_URI --record
kubectl -n=$ENV set image deployment/bayer-deployment bayer=$IMAGE_URI --record
kubectl -n=$ENV set image deployment/universalid-deployment universalid=$IMAGE_URI --record
