# Reservant API

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

Example `appsettings.Production.json`:

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
  }
}
```
