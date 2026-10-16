# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Azure Functions app (isolated worker, .NET 10, Functions v4) that reads `Person` documents from Azure Cosmos DB. Course project (Howest CTAI, Cloud & AI Services, week 4). There are no tests or linter configured.

## Commands

- Build: `dotnet build`
- Run locally: `func start` (needs Azure Functions Core Tools). Endpoints are served at `http://localhost:7071/api/...`.

## Architecture

Layered, wired together with DI in [Program.cs](Program.cs):

`PersonTrigger` (HTTP function) → `IPersonService` / `PersonService` → `IPersonRepository` / `PersonRepository` → `CosmosClient` (singleton, Gateway connection mode).

- `Program.cs` throws at startup if the `CosmosDbConnectionString` environment variable is missing. Locally it comes from `local.settings.json`, which is gitignored and must be created by hand.
- The repository reads from database `persondb`, container `persons`. Both must already exist in the Cosmos account.
- Application Insights / OpenTelemetry is only registered when `APPLICATIONINSIGHTS_CONNECTION_STRING` is set.
- Namespaces are deliberately split and bridged by [Usings.cs](Usings.cs) (`global using`): models and services are in `Howest.Functions.EX2`, the repository is in `MCT.Repositories`. New files in those folders rely on these global usings (e.g. `Newtonsoft.Json`, `Microsoft.Azure.Cosmos`) and don't import them.
- Models use Newtonsoft.Json `[JsonProperty]` attributes (not System.Text.Json) to map to the Cosmos document field names, e.g. `eMail` maps to `Email`.

## Conventions

- Whenever a new HTTP trigger is added to the project, always add a matching test request to `local.http` (create the file in the project root if it doesn't exist yet), using the same route and HTTP method as the function.
