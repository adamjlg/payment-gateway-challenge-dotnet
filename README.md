## Build
From the repository root:

- Build all projects in the solution
  dotnet build

- Build just the API project
  dotnet build src/PaymentGateway.Api/PaymentGateway.Api.csproj

- Build just the test project
  dotnet build test/PaymentGateway.Api.Tests/PaymentGateway.Api.Tests.csproj

## Run the API
From the repository root:

- Run directly (development settings)
  dotnet run --project src/PaymentGateway.Api/PaymentGateway.Api.csproj

The API will start and expose Swagger (in development) at:
- https://localhost:7092/swagger
- http://localhost:5067/swagger

Example requests:
- POST https://localhost:7092/api/payments
- POST http://localhost:5067/api/payments

## Run Tests
From the repository root:

- Run all tests
  dotnet test

- Run tests for a specific project
  dotnet test test/PaymentGateway.Api.Tests/PaymentGateway.Api.Tests.csproj

## Bank Simulator (optional)
The API uses an HTTP client to talk to a bank simulator running at http://localhost:8080.
You can run the simulator with Docker Compose:

- Start simulator
  docker compose up -d

- Stop simulator
  docker compose down

## Environment
- Configuration files: src/PaymentGateway.Api/appsettings.json and appsettings.Development.json


# Instructions for candidates

This is the .NET version of the Payment Gateway challenge. If you haven't already read this [README.md](https://github.com/cko-recruitment/) on the details of this exercise, please do so now. 

## Template structure
```
src/
    PaymentGateway.Api - a skeleton ASP.NET Core Web API
test/
    PaymentGateway.Api.Tests - an empty xUnit test project
imposters/ - contains the bank simulator configuration. Don't change this

.editorconfig - don't change this. It ensures a consistent set of rules for submissions when reformatting code
docker-compose.yml - configures the bank simulator
PaymentGateway.sln
```

Feel free to change the structure of the solution, use a different test library etc.
