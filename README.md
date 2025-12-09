# Welcome to the BaSyx .NET SDK

[![Build and push Docker image](https://github.com/fluid40/basyx-dotnet/actions/workflows/aasx-sm-server-docker-image.yml/badge.svg)](https://github.com/fluid40/basyx-dotnet/actions/workflows/aasx-sm-server-docker-image.yml)

This is forked from Fluid40 Dotnet SDK

It implements the offical AAS Part 1: Metamodel v3 as well as the Part 2: API v1

The entire .NET SDK is structured in 5 separate folder:
- basyx-dotnet-sdk: Contains all the core libraries to build everything from scratch
- basyx-dotnet-components: Built on top of the core libraries providing more high-level components as well as various client and server libraries
- basyx-dotnet-applications: Off-the-shelf components ready to be used and deployed in any scenario (Cloud, On Premises, Embedded Systems, RaspberryPi, Docker, etc.)
- basyx-dotnet-examples: Solution with a couple of example projects showing how things work
- basyx-dotnet-test: Unit and integration tests of the SDK

# Setup BaSyx

## NuGet Packages
All tagged/released packages are available as NuGet packages on nuget.org and can be installed via NuGet Package Manager within Visual Studio.

## Build NuGet Packages on your own
In order to build your own NuGet packages and use them in your project with the newest commits on the main-branch clone or download this repository, then execute **Setup_BaSyx.bat** and **Build_BaSyx.bat** afterwards. 
Make sure Visual Studio is closed the first time you run these scripts. The first script will add a new folder **basyx-dotnet-nuget-packages** and add this folder as NuGet package source to the system. The second script will build the NuGet packages and put them into the mentioned folder. (To build the NuGet packages in Visual Studio don't forget to change the Solution Configuration to *Release*)

# New implementations since fork
## Fixes
- Fixed Swagger UI for Repos (Ignore PostAASXPackages)
- Fixed a bug where the `Submodels -> Parent` property pointed to the wrong object after the AAS was updated (AssetAdministrationShellServiceProvider)
- Fixed a bug where the `SubmodelElements -> Parent` property pointed to the wrong object after the Submodel was updated (SubmodelServiceProvider)
- Rework Submodel reference handling in ASS. The References are no the primary source and will be updated if submodel objects added / removed from AAS (ElementContainer)
- Submodel update behavior corrected so that the `IdShort` property can be changed by the input parameter (SubmodelServiceProvider)
- Change http method from delete to get for endpoint `/shells/{aasIdentifier}/$reference` (AssetAdministrationShellRepositoryController)
- Fixed put '/submodels/{submodelIdentifier}' endpoint to replace submodel instead of update the submodel (SubmodelRepositoryController)
- Fixed Submodel element serialization to write `valueType` property tp use XSD data type definition (SubmodelElementConverter)
- The processing of values for Boolean properties has been corrected when these are provided as strings via the REST API. The return format for Boolean values via the REST API has been improved (upper- / lower-case).

## Features
- Add Submodel Registry HTTP Server (SubmodelRegistryHttpServer)

## Testing
- Fixed AdminShellClientServerTests
- Fixed AdminShellRepoClientServerTests
- Fixed SubmodelClientServerTests
- Fixed SubmodelRepoClientServerTests

## Server Applications
- Build new server with AAS Repository server, AAS Registry sever, Submodel Repository server and Submodel Registry server linked by reverse proxy (BaSyx.AASX.SM.Server.Http.App)
- Build new server variation as combination of AAS Repository and Submodel Repository as single server (BaSyx.Repo.Server.Http.App)


## Infrastructure
- Add Docker support for BaSyx.AASX.SM.Server.Http.App
- Add Docker support for BaSyx.Repo.Server.Http.App
- Add a GitHub workflow to create images of Docker-supported applications and transfer them to Docker Hub (https://hub.docker.com/search?q=engineeringmethodsag)
- Add Nunit tests to GitHub workflow

# Known Issues
- Missing (currently not possible) serialization/deserialization of the `OnMethodCalled` property in the `Operation` class. Therefore, this property cannot be set via Rest API endpoints or is destroyed when using Rest API endpoints (e.g. update endpoints) if it has already been set.
- To add a submodel reference to an AAS object in the data model, a complete submodel object must be added to the 'submodels' property of the ASS
- In AAS and Submodel, empty list properties (e.g. display name and description) are returned as empty lists rather than as the 'null' value. This can cause problems with some converters. The reason for this is the default initialization of list properties.