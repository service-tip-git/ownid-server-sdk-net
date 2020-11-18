#!bin/sh
IMAGE_URI=$1

echo Deploying Demo
kubectl apply -f manifests/prod/demo.yaml
kubectl -n=prod set image deployment/ownid-demo-server-deployment ownid-demo-server=$IMAGE_URI --record
echo

echo Deploying Pilot
kubectl apply -f manifests/prod/pilot.yaml
kubectl -n=prod set image deployment/ownid-pilot-server-deployment ownid-pilot-server=$IMAGE_URI --record
echo

echo Deploying Nestle-Hipster
kubectl apply -f manifests/prod/nestle-hipster.yaml
kubectl -n=prod set image deployment/nestle-hipster-deployment nestle-hipster=$IMAGE_URI --record
echo

echo Deploying Bayer
kubectl apply -f manifests/prod/bayer.yaml
kubectl -n=prod set image deployment/bayer-deployment bayer=$IMAGE_URI --record
echo

echo Deploying UniversalID
kubectl apply -f manifests/prod/universalid.yaml
kubectl -n=prod set image deployment/universalid-deployment universalid=$IMAGE_URI --record
echo

echo Deploying GigyaPOC-GigyaInsurance
kubectl apply -f manifests/prod/gigyapoc-gigyainsurance.yaml
kubectl -n=prod set image deployment/gigyapoc-gigyainsurance-deployment gigyapoc-gigyainsurance=$IMAGE_URI --record
echo



