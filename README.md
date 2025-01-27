# Kepware Configuration API SDK for .NET

## Overview
The Kepware Configuration API SDK for .NET provides tools and libraries to interact with the Kepware REST API, enabling configuration management for Kepware servers. This repository includes examples and utilities to streamline development, including a service for continuous synchronization and an API client library.

## Features
- **API Client Library**: Simplify interaction with the Kepware Configuration API.
- **Service for Synchronization**:
  - Bi-directional synchronization between Kepware servers and local filesystems.
  - Support for one-way and two-way synchronization modes.
- **Sample Application**: Demonstrates API usage with real-world examples.
- **HTTPS Support**: Certificate validation and secure connections.

## Projects
This repository contains the following projects:

### 1. `KepwareSync.Service`
A service application for synchronizing configurations between Kepware servers and the local filesystem. It supports monitoring and synchronization in real time.

[Readme for KepwareSync.Service](./KepwareSync.Service/README.md)

### 2. `Kepware.Api`
A .NET library providing an easy-to-use client for interacting with the Kepware Configuration API. Includes functionality for managing channels, devices, tags, and more.

[Readme for Kepware.Api](./Kepware.Api/README.md)

### 3. `Kepware.Api.Sample`
A sample console application demonstrating how to use `Kepware.Api` to interact with the Kepware Configuration API. Includes examples for creating channels, devices, and tags.

[Readme for Kepware.Api.Sample](./Kepware.Api.Sample/README.md)

## Installation
To install the SDK:

1. Clone this repository:
   ```bash
   git clone https://github.com/your-org/Kepware-ConfigAPI-SDK-dotnet.git
   ```
2. Build the solution using Visual Studio or the .NET CLI:
   ```bash
   dotnet build
   ```
3. Add the `Kepware.Api` project or its compiled DLL as a reference in your .NET project.

## Documentation
Detailed documentation is available for each project:
- **API Library**: Usage examples and method details can be found in the [Kepware.Api Readme](./Kepware.Api/README.md).
- **Synchronization Service**: Configuration and usage instructions are outlined in the [KepwareSync.Service Readme](./KepwareSync.Service/README.md).
- **Sample Application**: Example code is provided in the [Kepware.Api.Sample Readme](./Kepware.Api.Sample/README.md).

## Contribution Guidelines
We welcome contributions to this repository. Please follow these guidelines:

### Commit Message Convention
We adhere to the [Conventional Commits](https://www.conventionalcommits.org/) specification:

**Format:** `<type>(<scope>): <short summary>`

**Examples:**
- `feat(api): add support for new configuration objects`
- `fix(sync): resolve issue with file monitoring`
- `docs(service): update usage instructions`

**Types:**
- `feat`: New features.
- `fix`: Bug fixes.
- `docs`: Documentation updates.
- `style`: Code style changes.
- `refactor`: Code restructuring without functional changes.
- `test`: Adding or updating tests.
- `chore`: Maintenance tasks, e.g., dependency updates.

## Licensing
This SDK is provided "as is" under the MIT License. See the [LICENSE](./LICENSE.txt) file for details.

## Support
For issues or feature requests, please open a ticket in the GitHub repository.

---
Ready to dive in? Check out the project-specific Readmes for detailed information on how to get started!

