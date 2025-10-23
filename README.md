# .NET Release File Generator

[![CI](https://github.com/canonical/dotnet-release-file-generator/actions/workflows/generate-releases-file.yaml/badge.svg)](https://github.com/canonical/dotnet-release-file-generator/actions/workflows/generate-releases-file.yaml)

A tool to generate release metadata files (`release.json`) for .NET packages across all supported Ubuntu versions. It queries the Launchpad API to collect all released .NET versions and outputs structured JSON manifests for downstream automation and packaging.

## Features

- Queries Launchpad API for .NET package versions
- Generates `release.json` files for each supported Ubuntu release
- Supports automation via GitHub Actions
- Outputs are suitable for use in packaging, CI/CD, and reporting

## Project Structure

- `src/Flamenco.Packaging.Dpkg/` — Dpkg and changelog parsing utilities
- `src/Flamenco.Shared/` — Shared utilities and types
- `src/ReleasesFileGenerator.Console/` — Main CLI tool and manifest generation logic
- `src/ReleasesFileGenerator.Launchpad/` — Launchpad API client
- `src/ReleasesFileGenerator.Types/` — Core types and versioning logic
- `test/` — Unit tests

## Usage

### Prerequisites
- .NET 8 SDK

```bash
sudo apt install dotnet8
```

### Build

```bash
dotnet publish src/ReleasesFileGenerator.Console/ReleasesFileGenerator.Console.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained \
  --output ./dist \
  /p:PublishSingleFile=true
```

### Run

```bash
./dist/ReleasesFileGenerator.Console <ubuntu-release> <output-directory>
```

- `<ubuntu-release>`: Ubuntu release codename (e.g., `jammy`, `noble`)
- `<output-directory>`: Directory to write the generated JSON metadata

### Example

```bash
./dist/ReleasesFileGenerator.Console jammy ./out
```

## GitHub Actions

This repository includes a workflow to automatically build and update release files for all supported Ubuntu versions. See [generate-releases-file.yaml](.github/workflows/generate-releases-file.yaml).

## Output

The tool generates a JSON metadata for each Ubuntu release, which lists all available .NET versions and their details.

The metadata is available publicly under their own release Git branches, in this repository (e.g. `release/jammy` and `release/noble`)

## Contributing

Contributions are welcome! Please open issues or pull requests on [GitHub](https://github.com/canonical/dotnet-release-file-generator).

## License

This project is licensed under the [GPL-3.0 License](LICENSE).
