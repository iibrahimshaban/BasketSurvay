# Survey Basket API

Survey Basket is an ASP.NET Core Web API for creating, publishing, answering, and analyzing surveys. It includes JWT authentication, email-confirmed accounts, role and permission-based authorization, SQL Server persistence, background notifications, health checks, rate limiting, and versioned OpenAPI documentation.

## Features

- User registration, email confirmation, login, refresh tokens, logout, and password reset
- JWT bearer authentication with ASP.NET Core Identity
- Role-based and permission-based authorization
- Built-in `Admin` and `Member` roles
- Poll creation, editing, publishing, scheduling, and deletion
- Questions with multiple selectable answers
- Member-only poll voting
- Poll results by response rows, day, and question
- User and role administration
- FluentValidation request validation
- Entity Framework Core with SQL Server and migrations
- Hybrid caching for question-related reads
- Hangfire background jobs and a protected dashboard
- Daily email notifications for newly active published polls
- Database, Hangfire, and mail-provider health checks
- Fixed-window and concurrency rate limiting
- API versioning through the `X-api-Version` request header
- Scalar/OpenAPI documentation in development
- Serilog request logging and centralized problem-details error responses

## Technology Stack

- .NET 9 / ASP.NET Core
- C# with nullable reference types and implicit usings enabled
- ASP.NET Core Identity
- JWT bearer authentication
- Entity Framework Core 9
- Microsoft SQL Server
- Hangfire with SQL Server storage
- Scalar and ASP.NET Core OpenAPI
- FluentValidation
- Mapster
- Serilog
- MailKit
- HybridCache

## Project Structure

```text
.
├── BasketSurvay.sln
└── BasketSurvay/
    ├── Abstractions/       Result types, pagination, permissions, and constants
    ├── Authentication/     JWT provider and custom permission authorization
    ├── Contracts/          Request and response models with validators
    ├── Controllers/        HTTP API endpoints
    ├── Entities/           Identity, poll, question, answer, and vote entities
    ├── Errors/             Typed application errors and exception handling
    ├── Extensions/         Application extension methods
    ├── Health/             Health checks for dependencies
    ├── Helpers/            Email and other application helpers
    ├── Mapping/            Mapster mapping configuration
    ├── Migrations/         Entity Framework Core migrations
    ├── OpenApiTransformers/ OpenAPI security and versioning configuration
    ├── Persistence/        DbContext and entity configurations
    ├── Services/           Authentication, poll, vote, user, role, and email services
    ├── Settings/           Strongly typed mail settings
    ├── Templates/          Email templates
    ├── Program.cs          Application startup and middleware pipeline
    └── DependancyInjection.cs
                            Service registration and infrastructure setup
```

## Prerequisites

Install the following before running the project:

- .NET 9 SDK
- SQL Server, LocalDB, or another SQL Server-compatible instance
- An SMTP provider for account confirmation, password reset, and poll notifications
- Optional: a REST client such as Postman or Bruno

The project currently targets `net9.0`. Check the installed SDK with:

```bash
dotnet --version
```

## Configuration

The application reads configuration from `appsettings.json`, environment-specific settings, user secrets, environment variables, and other standard ASP.NET Core providers.

Do not commit real passwords, signing keys, connection strings, or SMTP credentials. The repository's existing settings files contain environment-specific values that should be treated as exposed and rotated before deploying the application.

Create local secrets or environment variables for these settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SurveyBasket;Trusted_Connection=True;TrustServerCertificate=True",
    "HangfireConnection": "Server=localhost;Database=SurveyBasketJobs;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "use-a-long-random-secret-key",
    "Issuer": "SurveyBasketApp",
    "Audience": "SurveyBasketUsers",
    "ExpiryMinutes": 30
  },
  "MailSettings": {
    "Mail": "sender@example.com",
    "DisplayName": "Survey Basket",
    "Password": "smtp-password",
    "Host": "smtp.example.com",
    "Port": 587
  },
  "HangfireSettings": {
    "Username": "dashboard-user",
    "password": "dashboard-password"
  },
  "AllowedOrigins": ["http://localhost:3000"]
}
```

The application validates JWT and mail settings at startup. The required configuration keys are:

| Section             | Keys                                              | Purpose                                       |
| ------------------- | ------------------------------------------------- | --------------------------------------------- |
| `ConnectionStrings` | `DefaultConnection`, `HangfireConnection`         | Application and Hangfire SQL Server databases |
| `Jwt`               | `Key`, `Issuer`, `Audience`, `ExpiryMinutes`      | Access-token creation and validation          |
| `MailSettings`      | `Mail`, `DisplayName`, `Password`, `Host`, `Port` | SMTP email delivery and health checks         |
| `HangfireSettings`  | `Username`, `password`                            | Basic authentication for `/jobs`              |
| `AllowedOrigins`    | Array of origins                                  | CORS allow-list                               |

### User secrets

For local development, user secrets keep sensitive values outside the repository:

```bash
dotnet user-secrets init --project BasketSurvay/BasketSurvay.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<sql-server-connection-string>" --project BasketSurvay/BasketSurvay.csproj
dotnet user-secrets set "ConnectionStrings:HangfireConnection" "<hangfire-connection-string>" --project BasketSurvay/BasketSurvay.csproj
dotnet user-secrets set "Jwt:Key" "<long-random-signing-key>" --project BasketSurvay/BasketSurvay.csproj
```

Set the remaining mail and Hangfire values the same way, or provide them through the deployment secret manager.

## Getting Started

From the repository root:

```bash
dotnet restore BasketSurvay.sln
dotnet build BasketSurvay.sln
dotnet run --project BasketSurvay/BasketSurvay.csproj --launch-profile https
```

The development launch profiles use:

- HTTPS: `https://localhost:7095`
- HTTP: `http://localhost:5018`
- Scalar API reference: `https://localhost:7095/scalar/v1`

OpenAPI and Scalar are enabled only when `ASPNETCORE_ENVIRONMENT=Development`.

## Database Migrations

The project uses EF Core migrations stored under `BasketSurvay/Persistence/Migrations`.

Apply existing migrations:

```bash
dotnet ef database update \
  --project BasketSurvay/BasketSurvay.csproj \
  --startup-project BasketSurvay/BasketSurvay.csproj
```

Create a migration after changing the model:

```bash
dotnet ef migrations add DescribeYourChange \
  --project BasketSurvay/BasketSurvay.csproj \
  --startup-project BasketSurvay/BasketSurvay.csproj \
  --output-dir Persistence/Migrations
```

The database contains Identity tables plus polls, questions, answers, votes, and vote-answer relationships. Seed data includes the default roles and an administrative user; configure a secure administrative credential for each environment and do not reuse credentials from source code.

## Authentication and Authorization

### Authentication flow

1. Register through `POST /Auth/registration`.
2. Confirm the email through `GET /Auth/confirm-email`.
3. Log in through `POST /Auth` to receive an access token and refresh token.
4. Send the access token with protected requests:

```http
Authorization: Bearer <access-token>
```

5. Refresh or revoke the refresh token through the authentication endpoints when needed.

Email confirmation is required by the Identity configuration. Passwords must be at least eight characters long, and email addresses must be unique.

### Roles and permissions

The application defines these default roles:

- `Admin`: management access controlled by permissions
- `Member`: can access member voting endpoints

Permission-protected operations use these permission values:

| Area      | Permissions                                                  |
| --------- | ------------------------------------------------------------ |
| Polls     | `Polls:Read`, `Polls:Create`, `Polls:Update`, `Polls:Delete` |
| Questions | `Questions:Read`, `Questions:Create`, `Questions:Update`     |
| Users     | `User:Read`, `User:create`, `User:Update`                    |
| Roles     | `Roles:Read`, `Roles:create`, `Roles:Update`                 |
| Results   | `results:Read`                                               |

## API Overview

All routes below are relative to the application base URL. Unless noted otherwise, protected routes require a bearer token and the relevant permission.

### Authentication

| Method | Route                             | Description                                |
| ------ | --------------------------------- | ------------------------------------------ |
| `POST` | `/Auth`                           | Log in and issue access and refresh tokens |
| `POST` | `/Auth/registration`              | Register a user                            |
| `GET`  | `/Auth/confirm-email`             | Confirm a user's email address             |
| `POST` | `/Auth/resend-confirmation-email` | Resend the confirmation email              |
| `POST` | `/Auth/refresh`                   | Exchange a refresh token for new tokens    |
| `POST` | `/Auth/revoke-refresh-token`      | Revoke a refresh token                     |
| `POST` | `/Auth/forget-password`           | Request a password-reset code              |
| `POST` | `/Auth/reset-password`            | Reset a password                           |

Example login request:

```http
POST /Auth
Content-Type: application/json

{
  "email": "member@example.com",
  "password": "your-password"
}
```

### Current account

| Method | Route                 | Description                              |
| ------ | --------------------- | ---------------------------------------- |
| `GET`  | `/me`                 | Get the authenticated user's profile     |
| `PUT`  | `/me/info`            | Update the authenticated user's profile  |
| `PUT`  | `/me/change-password` | Change the authenticated user's password |

### Polls

| Method   | Route                            | Description                             |
| -------- | -------------------------------- | --------------------------------------- |
| `GET`    | `/api/Polls/all`                 | List all polls                          |
| `GET`    | `/api/Polls/current`             | Get the current poll using API v1 or v2 |
| `GET`    | `/api/Polls/{id}`                | Get a poll by ID                        |
| `POST`   | `/api/Polls`                     | Create a poll                           |
| `PUT`    | `/api/Polls/{id}`                | Update a poll                           |
| `DELETE` | `/api/Polls/{id}`                | Delete a poll                           |
| `PUT`    | `/api/Polls/{id}/toggle-publish` | Publish or unpublish a poll             |

Poll creation and update payload:

```json
{
  "title": "Preferred lunch options",
  "summary": "Choose the option you prefer.",
  "startsAt": "2026-09-20",
  "endsAt": "2026-09-27"
}
```

`/api/Polls/current` has two implementations:

```http
GET /api/Polls/current
X-api-Version: 1
```

```http
GET /api/Polls/current
X-api-Version: 2
```

When the header is omitted, the API assumes the default version. The controller currently marks v1 as deprecated and exposes v2.

### Questions and answers

| Method | Route                                              | Description                                      |
| ------ | -------------------------------------------------- | ------------------------------------------------ |
| `GET`  | `/api/Polls/{pollId}/Questions/all`                | List questions for a poll with filtering support |
| `GET`  | `/api/Polls/{pollId}/Questions/{id}`               | Get one question                                 |
| `POST` | `/api/Polls/{pollId}/Questions`                    | Add a question and its answers                   |
| `PUT`  | `/api/Polls/{pollId}/Questions/{id}`               | Update a question and its answers                |
| `PUT`  | `/api/Polls/{pollId}/Questions/{id}/toggle-status` | Activate or deactivate a question                |

Example question payload:

```json
{
  "content": "Which option do you prefer?",
  "answers": ["Option A", "Option B", "Option C"]
}
```

### Voting

Voting is restricted to authenticated users in the `Member` role and is protected by a concurrency limiter.

| Method | Route                      | Description                                    |
| ------ | -------------------------- | ---------------------------------------------- |
| `GET`  | `/api/Polls/{pollId}/Vote` | Get the active questions and available answers |
| `POST` | `/api/Polls/{pollId}/Vote` | Submit the member's vote                       |

Example vote payload:

```json
{
  "answers": [
    {
      "questionId": 1,
      "answerId": 3
    },
    {
      "questionId": 2,
      "answerId": 5
    }
  ]
}
```

### Results

All results endpoints require the results-read permission.

| Method | Route                                            | Description                 |
| ------ | ------------------------------------------------ | --------------------------- |
| `GET`  | `/api/Polls/{pollId}/Results/row-data`           | Get poll response data      |
| `GET`  | `/api/Polls/{pollId}/Results/votes-per-day`      | Get vote totals by day      |
| `GET`  | `/api/Polls/{pollId}/Results/votes-per-question` | Get vote totals by question |

### Users and roles

| Method | Route                           | Description                                     |
| ------ | ------------------------------- | ----------------------------------------------- |
| `GET`  | `/api/Users`                    | List all users                                  |
| `GET`  | `/api/Users/members`            | List member users                               |
| `GET`  | `/api/Users/{id}`               | Get a user                                      |
| `POST` | `/api/Users`                    | Create a user                                   |
| `PUT`  | `/api/Users/{id}`               | Update a user                                   |
| `PUT`  | `/api/Users/{id}/toggle-status` | Enable or disable a user                        |
| `PUT`  | `/api/Users/{id}/unlock`        | Unlock a user                                   |
| `GET`  | `/api/Roles`                    | List roles, optionally including disabled roles |
| `GET`  | `/api/Roles/{id}`               | Get role details                                |
| `POST` | `/api/Roles`                    | Create a role                                   |
| `PUT`  | `/api/Roles/{id}`               | Update a role                                   |
| `PUT`  | `/api/Roles/{id}/toggle-status` | Enable or disable a role                        |

## Background Jobs and Notifications

Hangfire uses the `HangfireConnection` SQL Server database and starts a background processing server with the application.

A recurring job named `SendNewPollNotification` runs daily. It finds published polls starting on the current UTC date and emails users in the `Member` role using the configured SMTP provider.

The Hangfire dashboard is available at:

```text
/jobs
```

The dashboard is protected by basic authentication using `HangfireSettings:Username` and `HangfireSettings:password`. Do not expose it publicly without HTTPS and strong credentials.

## Health Checks

The application exposes three health-check routes:

| Route              | Checks                                  |
| ------------------ | --------------------------------------- |
| `/health`          | All registered checks                   |
| `/health-api`      | Hangfire and mail-provider checks       |
| `/health-Database` | SQL Server and Hangfire database checks |

Responses use the Health Checks UI JSON format. These endpoints are suitable for local diagnostics and deployment probes, but should be protected or restricted at the network layer when operational details must remain private.

## Rate Limiting

The API defines these policies:

- `IpLimit`: three requests per IP in a ten-second fixed window
- `UserLimit`: three requests per authenticated user in a ten-second fixed window
- `concurrency`: ten concurrent requests with a queue of five; voting uses this policy

Rejected requests receive HTTP `429 Too Many Requests`.

## Error Handling and Validation

Requests are validated with FluentValidation. Application failures are converted to problem-details responses through the global exception handler and the result/error abstractions in `Abstractions/` and `Errors/`.

Successful responses use normal HTTP semantics, including `201 Created` for newly created resources, `204 No Content` for successful updates and deletes where appropriate, and `429 Too Many Requests` for rate-limit rejection.

## Development Notes

- API documentation is available through Scalar only in the Development environment.
- API versioning uses the `X-api-Version` header and currently defaults to version 1 when unspecified.
- CORS origins are configured through `AllowedOrigins` in development settings.
- Audit fields are populated during `SaveChangesAsync` for entities derived from `AuditableEntity`.
- SQL Server is required for both the application data store and Hangfire storage.
- There is currently no test project in the solution; add automated tests before relying on the API in production.

## Security Checklist

Before deploying:

- Replace and rotate every credential currently stored in local settings or source control.
- Generate a strong, unique JWT signing key for each environment.
- Store secrets in user secrets, environment variables, or a managed secret store.
- Use HTTPS and secure database connections.
- Restrict CORS to known frontend origins.
- Protect or disable the Hangfire dashboard outside trusted networks.
- Review seeded administrative access and change the initial password.
- Restrict health-check visibility if it reveals infrastructure status.
- Configure an SMTP provider that supports TLS and monitor mail health-check failures.

## License

No license file is currently included in the repository. Add a license before distributing the project publicly.
