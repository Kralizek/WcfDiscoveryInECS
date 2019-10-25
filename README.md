# WCF Discovery in AWS ECS

This repository contains a simple WCF service to be deployed in a AWS ECS Windows cluster. The service will announce itself to a service registry so that clients can discover it.

## DiscoverableService

This project contains a console application using TopShelf to create a Windows Service.

The project uses [EMG.Utilities.ServiceModel](https://www.nuget.org/packages/EMG.Utilities.ServiceModel/) to streamline the creation and setup of the WCF service. 

The service is meant to be hosted in a Docker container.

You can create the docker container by executing the following command

```powershell
docker build -t discoverable-wcf-service .\DiscoverableService\
```

You can run the container locally by executing the following command

```powershell
docker run --rm -e Environment=Development -p 10000:10000 discoverable-wcf-service
```

## DirectClient

This project contains a console application that connects directly to the service.

## DiscoveringClient

This project contains a console application that connects to the service via WCF Discovery.