# KepwareSync

## Overview
KepwareSync is a powerful tool designed to manage and synchronize the configuration of Kepware servers via the Kepware REST API. This tool offers both command-line and service/agent functionality to enable flexible and efficient synchronization processes between Kepware servers and local filesystems.

### Key Features
- **Command-line "One-shot" Synchronization**:
  - Synchronize Kepware server configuration to the local filesystem.
  - Apply changes from the local filesystem to the Kepware server.

- **Service/Agent Mode**:
  - Bi-directional synchronization between Kepware servers and local filesystems.
  - Continuous monitoring of both the Kepware server and local filesystem for changes.
  - Automatic detection and synchronization of changes.

### Local Filesystem Structure
The Kepware configuration is exported and managed in a hierarchical directory structure:
```
<Channels>/<Device>/<TagGroup>/<SubTagGroup>/...
```
Example:
```
ExportedYaml/
  project.yaml
  Kanal1/
    Gerät1/
      device.yaml
      tags.csv
    Gerät2/
      device.yaml
      tags.csv
```

## Usage
### Command-Line Interface
#### General
```bash
Kepware.SyncService [command] [options]
```
#### Available Commands
- `SyncToDisk` - Synchronize data from Kepware to the local filesystem.
- `SyncFromDisk` - Synchronize data from the local filesystem to Kepware.

#### Options
- `--kep-api-username <username>`: Kepware REST API username.
- `--kep-api-password <password>`: Kepware REST API password.
- `--kep-api-host <host>`: Kepware REST API host URL.
- `--directory <path>`: Path to the local storage directory.
- `--persist-default-value`: Persist default values during synchronization.
- `--kep-sync-direction <direction>`: Primary synchronization direction (DiskToKepware or KepwareToDisk).
- `--kep-sync-mode <mode>`: Synchronization mode (OneWay or TwoWay).
- `--kep-sync-throtteling <ms>`: Throttling time in milliseconds before starting synchronization after detecting a change.

#### Examples
1. Synchronize Kepware server configuration to the local filesystem:
   ```bash
   Kepware.SyncService SyncToDisk --kep-api-username admin --kep-api-password password --kep-api-host http://localhost:57412 --directory ./ExportedYaml
   ```
2. Synchronize local filesystem configuration to the Kepware server:
   ```bash
   Kepware.SyncService SyncFromDisk --kep-api-username admin --kep-api-password password --kep-api-host http://localhost:57412 --directory ./ExportedYaml
   ```

### Service/Agent Mode
To enable continuous bi-directional synchronization, start KepwareSync as a service/agent:
```bash
Kepware.SyncService --kep-api-username admin --kep-api-password password --kep-api-host http://localhost:57412 --directory ./ExportedYaml --kep-sync-mode TwoWay
```
In service mode, changes in both the Kepware server and local filesystem are monitored and synchronized automatically.

## Configuration
The `appsettings.json` file allows you to configure logging and Kepware REST API credentials:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  },
  "KepApi": {
    "Username": "",
    "Password": "",
    "Host": ""
  }
}
```

## Important Notes
### Disclaimer
**Backup Before Syncing**: Always create a backup of your Kepware server configuration before using KepwareSync. Depending on the synchronization direction specified during initialization, existing configurations may be overwritten.

- **Default Behavior**: If not specified, the default synchronization direction in service mode is `Kepware -> Disk`. This means local changes will be overwritten during the initial full sync.

### Recommendations
- Ensure the local directory structure matches the expected hierarchy for proper synchronization.
- Use `--persist-default-value` to retain default values if required.

## Licensing
This tool is provided "as is". Use at your own risk. The authors are not liable for any data loss resulting from improper usage.

## Support
For issues or feature requests, please contact the developer or refer to the documentation.

## Contribution Guidelines

### Commit Message Convention
We follow the [Conventional Commits](https://www.conventionalcommits.org/) standard to ensure clear and consistent commit messages.

**Format:**
<type>(<scope>): <short summary>

**Examples:**
- `feat(sync): add bi-directional synchronization for Kepware servers`
- `fix(api): resolve authentication issue with Kepware REST API`
- `docs(filesystem): update documentation for local directory structure`
- `refactor(service): optimize change detection logic`
- `test(cli): add tests for command-line synchronization`

**Types:**
- `feat`: Introduces a new feature (e.g., new sync modes or API support)
- `fix`: Fixes a bug (e.g., REST API issues, file parsing errors)
- `docs`: Documentation changes (e.g., README or usage guides)
- `style`: Code style/formatting changes (no logic, e.g., renaming variables)
- `refactor`: Code restructuring (no functional changes, e.g., optimizing sync logic)
- `test`: Adds or modifies tests (e.g., CLI or service behavior)
- `chore`: Maintenance tasks (e.g., dependency updates or CI improvements)

For more details, see the [Conventional Commits Specification](https://www.conventionalcommits.org/).
