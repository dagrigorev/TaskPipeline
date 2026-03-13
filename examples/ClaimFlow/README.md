# ClaimFlow

ClaimFlow is a compact ASP.NET Core MVC sample that demonstrates how to use the `TaskPipeline` NuGet package in a realistic business workflow.

## Scenario

The app processes insurance claims and shows why a strongly typed orchestration library is useful when a workflow has:

- ordered steps;
- business conditions;
- parallel branches;
- explicit merge logic;
- cancellation support;
- partial failure handling;
- execution auditing.

## Stack

- ASP.NET Core MVC (.NET 8)
- SQLite via EF Core
- TaskPipeline from NuGet
- Bootstrap-based UI
- xUnit tests

## What the sample demonstrates

- sequential pipeline nodes;
- `AddConditional` for true/false business branches;
- `AddFork` with named parallel branches;
- `DelegateMergeStrategy` for aggregation after fork completion;
- `ContinueOnError` failure mode;
- persisted execution snapshots;
- a simple dashboard for claims and execution details.

## Project layout

```text
src/
  ClaimFlow.Web/
tests/
  ClaimFlow.Web.Tests/
```

## Run

```bash
dotnet restore
dotnet build
dotnet run --project src/ClaimFlow.Web
```

Then open the local URL printed by ASP.NET Core.

## Notes

The sample uses mock integrations for anti-fraud, repair quotes, policy lookup, payment pre-checks, and notifications so the application can run locally with SQLite only.
