#!bin/sh
IMAGE_URI=$1

echo Deploying Pilot
kubectl apply -f manifests/prod/pilot.yaml
kubectl -n=prod set image deployment/ownid-pilot-server-deployment ownid-pilot-server=$IMAGE_URI --record
