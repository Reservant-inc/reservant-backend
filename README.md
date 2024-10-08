# Reservant API

![Build Status](https://github.com/Reservant-inc/reservant-backend/actions/workflows/build-and-test.yml/badge.svg)
![Code QL Status](https://github.com/Reservant-inc/reservant-backend/actions/workflows/github-code-scanning/codeql/badge.svg)

Backend for the app in the form of a web API.

## Running Locally

Add the following to .NET User Secrets for the project `Api`:

```json
{
  "ConnectionStrings": {
    "Default": "{MS SQL Server connection string}"
  }
}
```

Run the project:

```shell
cd Api
dotnet run
```

## Deploying

Building the Docker image:

```shell
docker build -t reservant-api -f Api/Dockerfile .
```

You will have to provide production configuration by mounting a configuration
file to `/api/appsettings.Production.json`. Example contents:

![](appsettings.Production.EXAMPLE.json)
```json
{
  "ConnectionStrings": {
    "Default": "{MS SQL Server Connection String}"
  },
  "JwtOptions": {
    "Issuer": "{JWT Issuer}",
    "Audience": "{JWT Audience}",
    "LifetimeHours": 0,
    "Key": "{JWT Secret Key}"
  },
  "FileUploads": {
    "ServeUrlBase": "{https://example.com/uploads}"
  },
  "Firebase": {
    "CredentialsPath": "{Path to the Firebase Service Account Credentials}"
  }
}
```
