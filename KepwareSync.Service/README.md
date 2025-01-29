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
      --primary-kep-api-password StrongAdminPassword2025! \
      --primary-kep-api-host https://localhost:57512 \
      --directory ./ExportedYaml
   ```

2. **Synchronize Kepware to Disk:**
   ```bash
   Kepware.SyncService SyncToDisk --primary-kep-api-username Administrator \
      --primary-kep-api-password StrongAdminPassword2025! \
      --primary-kep-api-host https://localhost:57512 \
      --directory ./ExportedYaml
   ```

3. **Synchronize Disk to Kepware:**
   ```bash
   Kepware.SyncService SyncFromDisk --primary-kep-api-username Administrator \
      --primary-kep-api-password StrongAdminPassword2025! \
      --primary-kep-api-host https://localhost:57512 \
      --directory ./ExportedYaml
   ```

### Environment Variable Configuration
The service also supports environment variables for configuration using `AddEnvironmentVariables`. Example environment variables:

| Environment Variable               | Example Value               |
|------------------------------------|-----------------------------|
| `KEPWARE__PRIMARY__USERNAME`       | Administrator               |
| `KEPWARE__PRIMARY__PASSWORD`       | StrongAdminPassword2025!         |
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
      "Password": "StrongAdminPassword2025!",
      "Host": "https://localhost:57512"
    },
    "Secondary": [
      {
        "Username": "Administrator",
        "Password": "StrongAdminPassword2025!",
        "Host": "https://localhost:57513"
      }
    ]
  },
  "Storage": {
    "Directory": "ExportedYaml"
  }
}
```

### Docker Deployment

To run `KepwareSync.Service` in a Docker container, use the following instructions. Ensure that you have Docker installed and running on your system.

#### Run the Container with Volume Mounts
If you want to access the persisted yaml files, you can mount the `STORAGE__DIRECTORY` as shown.

```bash
docker run -d \
  --name kepware-sync-service \
  --restart always \
  -v ./config:/app/config \
  -e STORAGE__DIRECTORY=/app/config \
  -e KEPWARE__PRIMARY__USERNAME=Administrator \
  -e KEPWARE__PRIMARY__PASSWORD=StrongAdminPassword2025! \
  -e KEPWARE__PRIMARY__HOST=https://localhost:57512 \
  ghcr.io/bobiene/kepware-sync-service:latest
```

This command:
- Runs the container in detached mode (`-d`).
- Assigns a name to the container (`--name kepware-sync-service`).
- Restarts the container automatically on failure or system reboot (`--restart always`).
- Mounts the local `config` directory to `/app/config` inside the container (`-v $(pwd)/config:/app/config`).
- Uses environment variables to configure the primary Kepware connection.

#### Docker Compose Setup

If you want to automate Kepware deployment with a Git-based configuration, you can use Git-Sync as a sidecar alongside *KepwareSync.Service*. This setup ensures that configuration changes in your Git repository are automatically synchronized with Kepware, enabling a scalable and GitOps-driven deployment

For a more structured deployment, use `docker-compose.yml`:

```yaml
version: '3.8'

services:
  kepware:
    image: inrayhub.azurecr.io/kepware_edge:1.9-beta
    pull_policy: always
    networks:
      - tke_network
    ports:
      - "57513:57513"
      - "49330:49330"
    environment:
      - EDGEADMINPW=StrongAdminPassword2025!
      - LICENSE_SERVER_ADDRESS=192.168.189.81
      - TZ=UTC
    volumes:
      - ./data/config:/opt/tkedge/v1/.config
    restart: unless-stopped

  kepware-sync:
    image: ghcr.io/bobiene/kepware-sync-service:latest
    pull_policy: always
    depends_on:
      - kepware
    networks:
      - tke_network
    volumes:
      - config_volume:/app/config
    environment:
      - KEPWARE__PRIMARY__USERNAME=Administrator
      - KEPWARE__PRIMARY__PASSWORD=StrongAdminPassword2025!
      - KEPWARE__PRIMARY__HOST=https://kepware:57513
      - STORAGE__DIRECTORY=/app/config
    restart: unless-stopped

  git-sync:
    image: k8s.gcr.io/git-sync:v3.4.1
    pull_policy: always
    depends_on:
      - kepware-sync
    networks:
      - tke_network
    environment:
      - GIT_SYNC_REPO=https://your-repo.git
      - GIT_SYNC_BRANCH=main
      - GIT_SYNC_WAIT=10
    volumes:
      - config_volume:/repo
    restart: unless-stopped

volumes:
  config_volume:

networks:
  tke_network:
    driver: bridge
```

## Recommendations
- Always back up your Kepware server configuration before performing any synchronization.
- For two-way synchronization, ensure there are no conflicting changes on both sides.
- Use `--persist-default-value` to retain default values in configurations if required.

## Licensing
This tool is provided "as is" under the MIT License. Refer to the [LICENSE](../LICENSE.txt) file for more information.

## Support
For issues or feature requests, please open a ticket in the GitHub repository.

