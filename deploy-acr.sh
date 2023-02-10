#!/bin/bash
echo "what should the version be?" 
read VERSION
echo "What is the name of the container registry?"
read CONTAINER_REGISTRY

echo "Pushing image version $VERSION to $CONTAINER_REGISTRY"
az acr login --name $CONTAINER_REGISTRY

docker build -t eesfrontendcontainer:$VERSION .

docker tag eesfrontendcontainer:$VERSION $CONTAINER_REGISTRY.azurecr.io/eesfrontendcontainer:$VERSION
docker push $CONTAINER_REGISTRY.azurecr.io/eesfrontendcontainer:$VERSION
# 