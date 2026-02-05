## Build
From the repository root:

- Build all projects in the solution:
  dotnet build

- Build just the API project:
  dotnet build src/PaymentGateway.Api/PaymentGateway.Api.csproj

- Build just the test project:
  dotnet build test/PaymentGateway.Api.Tests/PaymentGateway.Api.Tests.csproj

## Run the API
From the repository root:

- Run directly (development settings):
  dotnet run --project src/PaymentGateway.Api/PaymentGateway.Api.csproj

The API will start and expose Swagger (in development) at:
- https://localhost:7092/swagger
- http://localhost:5067/swagger

Example requests:
- POST https://localhost:7092/api/payments
- POST http://localhost:5067/api/payments

## Run Tests
From the repository root:

- Run all tests:
  dotnet test

- Run tests for a specific project:
  dotnet test test/PaymentGateway.Api.Tests/PaymentGateway.Api.Tests.csproj

## Bank Simulator (optional)
The API uses an HTTP client to talk to a bank simulator running at http://localhost:8080.
You can run the simulator with Docker Compose:

- Start simulator:
  docker compose up -d

- Stop simulator:
  docker compose down

## Environment
- Configuration files: src/PaymentGateway.Api/appsettings.json and appsettings.Development.json

## Design Considerations
- The solution is designed to be clear, modular, and maintainable. Classes have a single responsibility. The validator handles input validation, service implements business logic and bank interaction, repository stores payments in memory, and the controller exposes the API. This separation makes the code easier to test and extend.

- All input validation occurs before any bank interaction. This prevents invalid requests from reaching the bank, reducing unnecessary calls.

- The IBankClient interface abstracts the bank interaction allowing the real simulator or a fake bank to be swapped easily in tests. This supports both unit and integration testing without requiring a live bank connection.

- Payments are stored in an in memory repository for simplicity.

- Bank responses are mapped to clear payment statuses. Any unexpected errors or invalid responses result in a rejected status to ensure predictable behavior.

- Unit Tests: Validate individual rules, such as card number format, expiry dates, CVV, and currency. The service is also tested with a fake bank client to ensure correct behavior without calling the real bank.

- Integration Tests: Test end-to-end API flows using the in-memory repository and the bank simulator. This ensures the system behaves as expected in realistic scenarios.

- Full card numbers are never returned in API responses. Only the last four digits are exposed for security.

- The design could be refactored for scalability in the future.

- Some basic logs are presented to the user on input validation fail via api response

- While the current implementation supports only a single bank and basic validation, the architecture allows extension to multiple banks or retry mechanisms without major refactoring.

## Assumptions
All invalid requests are rejected before contacting the bank, and only a single bank (the simulator running on localhost) is supported. No retries, fallback logic, or multi-bank support is implemented. Assume its sufficient for the transaction status to be rejected if an unexpected error occurs during the transaction. The solution does not include idempotency handling or structured logging/observability infrastructure. Testing checks core operations but does not test edge cases. Assume its sufficient to be open to extension, but not fully implement this.

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
