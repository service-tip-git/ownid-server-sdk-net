#!bin/bash
IMAGE_URI=$1

echo Update IMAGE in base kustomization.yaml
(cd manifests/base && kustomize edit set image server-gigya=$IMAGE_URI)
echo

apps=( demo pilot nestle-hipster bayer universalid gigyapoc-gigyainsurance )

for app in "${apps[@]}"
do
    echo Deploying $app
	kustomize build manifests/prod/$app/ | kubectl apply -f -
    echo
done

