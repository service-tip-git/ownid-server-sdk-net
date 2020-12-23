#!/bin/bash

ENV=$1

#Deploy OwnID.Server.WebApp
PKG_VERSION=$(xmllint --xpath "string(//Project/PropertyGroup/AssemblyVersion)" ./OwnID.Server.WebApp/OwnID.Server.WebApp.csproj)
echo PKG_VERSION=${PKG_VERSION-}
IMAGE_URI=$DOCKER_URL/$ENV/server/ownid-server-webapp:${PKG_VERSION-}

echo Docker push to $IMAGE_URI
docker tag ownid-server-webapp:latest $IMAGE_URI
docker push $IMAGE_URI

echo K8S cluster selection
aws eks --region us-east-2 update-kubeconfig --name ownid-eks

echo Update IMAGE in base kustomization.yaml
(cd manifests_webapp/base && kustomize edit set image server-webapp=$IMAGE_URI)

echo Deploying webapp
kustomize build manifests_webapp/$ENV/ | kubectl apply -f -
echo

#example
# kustomize build manifests_webapp/dev/ > manifests_webapp/result.yaml
