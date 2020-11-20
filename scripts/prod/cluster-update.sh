#!bin/sh
IMAGE_URI=$1

echo Update IMAGE in base kustomization.yaml
(cd manifests/base && kustomize edit set image server-gigya=$IMAGE_URI)

echo Deploying Demo
kustomize build manifests/prod/demo/ | kubectl apply -f -
echo

echo Deploying Pilot
kustomize build manifests/prod/pilot/ | kubectl apply -f -
echo

echo Deploying Nestle-Hipster
kustomize build manifests/prod/nestle-hipster/ | kubectl apply -f -
echo

echo Deploying Bayer
kustomize build manifests/prod/bayer/ | kubectl apply -f -
echo

echo Deploying UniversalID
kustomize build manifests/prod/universalid/ | kubectl apply -f -
echo

echo Deploying GigyaPOC-GigyaInsurance
kustomize build manifests/prod/gigyapoc-gigyainsurance/ | kubectl apply -f -
echo



