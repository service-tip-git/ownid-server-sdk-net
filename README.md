# OwnId server SDK for .NET 5

## Description
OwnID enables your customers to use their phone as a key to instantly login to your websites or apps. Passwords are finally gone.

This component is the server SDK that you can use to integrate with your Identity Management System. The project include integration to SAP CDC (Gigya). UI integration is using project [ownid-web-ui-sdk](https://github.com/SAP/ownid-web-ui-sdk). 

Evaluation is possible even without any back-end implementation. You can follow the developer-tutorial to set your environment. This is using OwnID back-end that already include the SAP CDC (Gigya) integration. Later on you can provide your SAP CDC (Gigya) credentials to set your production environment or you can take OwnID server SDK and implement the integration to your Identity Management System.

[![REUSE status](https://api.reuse.software/badge/github.com/SAP/ownid-server-sdk-net)](https://api.reuse.software/info/github.com/SAP/ownid-server-sdk-net)

## Documentation
OwnID Documentation you can find on our [Documentation](https://docs.ownid.com) page

## Prerequisites
The server SDK is developed using .NET 5.0. You can consume it in any supported OS.

## Configuration
You can find details in the [server online documentation] https://docs.ownid.com/server-sdk

## Running locally
Use 

```shell
dotnet run
```

or to set specific urls:  

```shell
dotnet run --urls http://0.0.0.0.:5002
``` 

