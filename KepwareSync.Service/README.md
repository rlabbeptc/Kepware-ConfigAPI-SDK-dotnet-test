# KepwareSync.Service

[![Build Status](https://github.com/BoBiene/Kepware-ConfigAPI-SDK-dotnet/actions/workflows/dotnet.yml/badge.svg)](https://github.com/BoBiene/Kepware-ConfigAPI-SDK-dotnet/actions)
[![Build Status](https://github.com/BoBiene/Kepware-ConfigAPI-SDK-dotnet/actions/workflows/docker-build-and-push.yml/badge.svg)](https://github.com/BoBiene/Kepware-ConfigAPI-SDK-dotnet/actions)

## Overview
The `KepwareSync.Service` is a CLI and service tool designed for synchronizing configuration data between Kepware servers and a local filesystem. It supports both one-way and two-way synchronization, making it ideal for real-time configuration management.

## Features
- **Command-Line Interface**: Execute synchronization tasks directly from the command line.
- **Service Mode**: Continuous monitoring and synchronization between Kepware servers and the local filesystem.
- **Support for Multiple Servers**: Synchronize with both primary and secondary Kepware servers.
- **Flexible Configuration**: Easily configurable using appsettings.json, environment variables, or CLI parameters.
- **HTTPS Support**: Secure connections with optional certificate validation.

## Usage

### CLI Commands
#### General Syntax
```bash
Kepware.SyncService [command] [options]
```

#### Commands
- **empty** (default): Run as an agent in the background, monitoring for changes and starting synchronization when detected
- **SyncToDisk**: Synchronize configuration data from Kepware servers to the local filesystem.
- **SyncFromDisk**: Apply configuration data from the local filesystem to Kepware servers.

#### Options
| Option                             | Description                                                                                  | Default Value          |
|------------------------------------|----------------------------------------------------------------------------------------------|------------------------|
| `--primary-kep-api-username`      | Primary Kepware API username (**required**).                                                 |                        |
| `--primary-kep-api-password`      | Primary Kepware API password (**required**).                                                 |                        |
| `--primary-kep-api-host`          | Primary Kepware API host URL (**required**).                                                 |                        |
| `--http-timeout`                  | HTTP timeout in seconds.                                                                     | `60`                  |
| `--secondary-kep-api`             | Secondary Kepware API configurations in `username:password@host` format.                    | `[]`                  |
| `--directory`                     | Directory for storing configuration files.                                                   | `ExportedYaml`        |
| `--persist-default-value`         | Persist default values during synchronization.                                               | `false`               |
| `--kep-sync-direction`            | Synchronization direction (`DiskToKepware`, `KepwareToDisk`, etc.).                          |                        |
| `--kep-sync-mode`                 | Synchronization mode (`OneWay` or `TwoWay`).                                                 |                        |
| `--kep-sync-throtteling`          | Throttling time in milliseconds after detecting a change.                                    |                        |
| `--version`                       | Show version information.                                                                    |                        |
| `-?, -h, --help`                  | Show help and usage information.                                                             |                        |

#### CLI examples
1. **Monitor and Synchronize bidrectional Kepware to Disk**
   ```bash
   Kepware.SyncService --primary-kep-api-username Administrator \
      --primary-kep-api-password InrayTkeDocker2024! \
      --primary-kep-api-host https://localhost:57512 \
      --directory ./ExportedYaml
   ```

2. **Synchronize Kepware to Disk:**
   ```bash
   Kepware.SyncService SyncToDisk --primary-kep-api-username Administrator \
      --primary-kep-api-password InrayTkeDocker2024! \
      --primary-kep-api-host https://localhost:57512 \
      --directory ./ExportedYaml
   ```

3. **Synchronize Disk to Kepware:**
   ```bash
   Kepware.SyncService SyncFromDisk --primary-kep-api-username Administrator \
      --primary-kep-api-password InrayTkeDocker2024! \
      --primary-kep-api-host https://localhost:57512 \
      --directory ./ExportedYaml
   ```

### Environment Variable Configuration
The service also supports environment variables for configuration using `AddEnvironmentVariables`. Example environment variables:

| Environment Variable               | Example Value               |
|------------------------------------|-----------------------------|
| `KEPWARE__PRIMARY__USERNAME`       | Administrator               |
| `KEPWARE__PRIMARY__PASSWORD`       | InrayTkeDocker2024!         |
| `KEPWARE__PRIMARY__HOST`           | https://localhost:57512     |
| `KEPWARE__DISABLECERTIFICATEVALIDATION` | true                  |
| `STORAGE__DIRECTORY`               | ExportedYaml                |

Set these variables in your system or Docker container to override appsettings.json.

### Sample Configuration File (`appsettings.json`)
```json
{
  "Kepware": {
    "DisableCertificateValidation": true,
    "Primary": {
      "Username": "Administrator",
      "Password": "InrayTkeDocker2024!",
      "Host": "https://localhost:57512"
    },
    "Secondary": [
      {
        "Username": "Administrator",
        "Password": "InrayTkeDocker2024!",
        "Host": "https://localhost:57513"
      }
    ]
  },
  "Storage": {
    "Directory": "ExportedYaml"
  }
}
```

## Recommendations
- Always back up your Kepware server configuration before performing any synchronization.
- For two-way synchronization, ensure there are no conflicting changes on both sides.
- Use `--persist-default-value` to retain default values in configurations if required.

## Licensing
This tool is provided "as is" under the MIT License. Refer to the [LICENSE](../LICENSE.txt) file for more information.

## Support
For issues or feature requests, please open a ticket in the GitHub repository.

