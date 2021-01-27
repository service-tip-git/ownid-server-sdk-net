#!/bin/bash

ENV=$1

#Deploy Netcore3 Server-Gigya
PKG_VERSION=$(xmllint --xpath "string(//Project/PropertyGroup/AssemblyVersion)" ./OwnID.Server.Gigya/OwnID.Server.Gigya.csproj)
IMAGE_URI=$DOCKER_URL/$ENV/server/ownid-server-gigya:${PKG_VERSION-}

echo Docker push to $IMAGE_URI
docker tag ownid-server-gigya:latest $IMAGE_URI
docker push $IMAGE_URI

echo K8S cluster selection
aws eks --region us-east-2 update-kubeconfig --name ownid-eks

echo Update IMAGE in base kustomization.yaml
(cd manifests/base && kustomize edit set image server-gigya=$IMAGE_URI)

echo Applications update

if [ "$ENV" == "dev" ]; then
  apps=(demo demo2 multilevel1 multilevel2)
else
  apps=(demo demo2 demo3 demo4 dor)
fi


for app in "${apps[@]}"; do
  echo Deploying $app
  kustomize build manifests/$ENV/$app/ | kubectl apply -f -
  echo
done