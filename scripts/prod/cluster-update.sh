#!bin/bash

apps=( demo pilot nestle-hipster bayer universalid gigyapoc-gigyainsurance )

for app in "${apps[@]}"
do
    echo Deploying $app
	kustomize build manifests/prod/$app/ | kubectl apply -f -
    echo
done

