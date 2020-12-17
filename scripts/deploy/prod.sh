#!/bin/bash

PKG_VERSION=`xmllint --xpath "string(//Project/PropertyGroup/AssemblyVersion)" ./OwnID.Server.Gigya/OwnID.Server.Gigya.csproj`
IMAGE_URI=$ARTIFACTORY_URL/prod/server/ownid-server-gigya_${PKG_VERSION-}:$TRAVIS_COMMIT

echo Docker push to $IMAGE_URI
docker tag ownid-server-gigya:latest $IMAGE_URI
docker push $IMAGE_URI

echo Update IMAGE in base kustomization.yaml
(cd manifests/base && kustomize edit set image server-gigya=$IMAGE_URI)
echo

echo Adding K8S clusters...
aws eks --region us-east-1 update-kubeconfig --name ownid-production-cluster
aws eks --region us-east-2 update-kubeconfig --name ownid-eks
echo

# Add new client to the list. It's name of folder in manifests/prod/
apps=( demo pilot nestle-hipster bayer universalid gigyapoc-gigyainsurance sap-commerce daimani )

echo Prod A Deployment
kubectl config use-context arn:aws:eks:us-east-1:571861302935:cluster/ownid-production-cluster
echo
for app in "${apps[@]}"
do
    echo Deploying $app
	kustomize build manifests/prod/$app/ | kubectl apply -f -
    echo
done
echo

echo Prod B Deployment
kubectl config use-context arn:aws:eks:us-east-2:571861302935:cluster/ownid-eks
echo
for app in "${apps[@]}"
do
    echo Deploying $app
	kustomize build manifests/prod/$app/ | kubectl apply -f -
    echo
done


#
# OwnID.Server.WebApp
#
PKG_VERSION=`xmllint --xpath "string(//Project/PropertyGroup/AssemblyVersion)" ./OwnID.Server.WebApp/OwnID.Server.WebApp.csproj`
IMAGE_URI=$ARTIFACTORY_URL/prod/server/ownid-server-webapp:${PKG_VERSION-}

echo Docker push to $IMAGE_URI
docker tag ownid-server-webapp:latest $IMAGE_URI
docker push $IMAGE_URI

echo Update IMAGE in base kustomization.yaml
(cd manifests_webapp/base && kustomize edit set image server-webapp=$IMAGE_URI)
echo

#echo Adding K8S clusters...
#aws eks --region us-east-1 update-kubeconfig --name ownid-production-cluster
#aws eks --region us-east-2 update-kubeconfig --name ownid-eks
#echo

echo Prod A Deployment - webapp
kubectl config use-context arn:aws:eks:us-east-1:571861302935:cluster/ownid-production-cluster
kustomize build manifests_webapp/prod/ | kubectl apply -f -
echo

echo Prod B Deployment - webapp
kubectl config use-context arn:aws:eks:us-east-2:571861302935:cluster/ownid-eks
kustomize build manifests_webapp/prod/ | kubectl apply -f -
echo