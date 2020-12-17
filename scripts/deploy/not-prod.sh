#!/bin/bash

ENV=$1

bash ./scripts/deploy/webapp.sh $ENV
bash ./scripts/deploy/gigya.sh $ENV
