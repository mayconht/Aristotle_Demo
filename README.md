# Aristotle Study - User Management API

<div align="center">

[![Build, Unit & Integration Tests](https://github.com/mayconht/Aristotle_Study/actions/workflows/ci-pipeline.yml/badge.svg)](https://github.com/mayconht/Aristotle_Study/actions/workflows/ci-pipeline.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=mayconht_Aristotle_Demo&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=mayconht_Aristotle_Demo)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=mayconht_Aristotle_Demo&metric=bugs)](https://sonarcloud.io/summary/new_code?id=mayconht_Aristotle_Demo)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=mayconht_Aristotle_Demo&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=mayconht_Aristotle_Demo)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=mayconht_Aristotle_Demo&metric=coverage)](https://sonarcloud.io/summary/new_code?id=mayconht_Aristotle_Demo)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=mayconht_Aristotle_Demo&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=mayconht_Aristotle_Demo)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=mayconht_Aristotle_Demo&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=mayconht_Aristotle_Demo)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=mayconht_Aristotle_Demo&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=mayconht_Aristotle_Demo)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=mayconht_Aristotle_Demo&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=mayconht_Aristotle_Demo)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=mayconht_Aristotle_Demo&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=mayconht_Aristotle_Demo)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=mayconht_Aristotle_Demo&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=mayconht_Aristotle_Demo)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=mayconht_Aristotle_Demo&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=mayconht_Aristotle_Demo)

</div>

A simple ASP.NET Core Web API for learning Entity Framework Core with Clean Architecture principles.
Keep in mind that this is a simple project for educational purposes and many improvements can be made.

## Technologies Used

- **.NET 8.0** - Target framework
- **ASP.NET Core Web API** - Web framework
- **Entity Framework Core 9.0** - ORM with SQLite and PostgreSQL providers
- **PostgreSQL** - Alternative database (configurable)
- **AutoMapper** - Object-to-object mapping
- **DotNetEnv** - Environment variable loader
- **Swagger/OpenAPI** - API documentation (Swashbuckle.AspNetCore)
- **xUnit v3** - Unit testing framework
- **Coverlet** - Code coverage analysis
- **SonarCloud** - Code quality and security analysis
- **Docker** - Containerization
- **Keycloak** - OAuth2/OIDC identity provider for authentication
- **JWT Bearer** - Token-based authentication

## Architecture

The project follows Clean Architecture principles with the following layers:

- **Domain** - Entities, interfaces, and domain exceptions
- **Application** - Services and application-specific logic
- **Infrastructure** - Data access, repositories, and external concerns
- **Controllers** - API endpoints and HTTP concerns

## Features

- User CRUD operations
- OAuth2/OIDC authentication with Keycloak
- JWT Bearer token validation
- JIT (Just-In-Time) user provisioning
- Email uniqueness validation
- Input validation
- Global exception handling
- Logging with structured logging
- Unit tests with high coverage
- API documentation with Swagger OAuth2 integration
- Docker support with full stack deployment

## Domain Entities

### User

- **Id**: Guid (Primary Key)
- **KeycloakUserId**: string (Required, unique, maps to remote IdP user id)
- **Name**: string (Required, max 130 chars)
- **Email**: string (Required, max 200 chars, unique)
- **DateOfBirth**: DateTime? (Optional)
- **CreatedAt**: DateTime (User creation timestamp)
- **LastLoginAt**: DateTime? (Last login timestamp)

## API Endpoints

All endpoints require authentication via JWT Bearer token obtained from Keycloak.

**Note**: User creation is managed through Keycloak login. Users are automatically provisioned in the local database upon first login (JIT provisioning).

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [JetBrains Rider](https://www.jetbrains.com/rider/) (recommended)
  or [Visual Studio](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (required for Keycloak setup)

## Authentication and Authorization

This project uses **Keycloak** as the identity provider (IdP) with OAuth2/OIDC for authentication.

### Key Features

- **Keycloak as Single Source of Truth**: User accounts are managed in Keycloak
- **JWT Bearer Authentication**: API validates tokens issued by Keycloak
- **JIT Provisioning**: Users are automatically created in local database on first login
- **Swagger OAuth2 Integration**: Test authenticated endpoints directly from Swagger UI

### Setup Keycloak (Automated with DevTools)

**Quick Start - Automated Setup:**

```bash
# 1. Copy environment template
cp .env.example .env

# 2. Start infrastructure
docker-compose up -d postgres keycloak

# 3. Run automated Keycloak setup
cd Scripts/Aristotle.DevTools
dotnet run -- keycloak setup

# 4. Copy the generated client secret to .env
# KEYCLOAK_CLIENT_SECRET=<generated-secret>
```

This automated setup creates:
- Realm: `userservice`
- Client: `userservice-api` (with PKCE enabled)
- Roles: Admins, Managers, Users
- Groups: admin-group, manager-group, user-group
- Test users: admin/admin123, manager/manager123, user/user123
- Protocol mappers for roles and groups claims

**Test the setup:**
```bash
dotnet run -- keycloak test
```

See [DevTools README](Scripts/Aristotle.DevTools/README.md) for more details.

## Running the Project

### Option 1: Docker Compose (Recommended)

Runs complete stack (Keycloak + API):

```bash
cp .env.example .env
docker compose up -d
```

Access:
- **Keycloak**: http://localhost:8080
- **API**: http://localhost:3000
- **Swagger**: http://localhost:3000/swagger

### Option 2: Local Development

Run API locally while using Keycloak from Docker:

```bash
docker compose up -d keycloak

cd USR/UserService
dotnet run
```

Access:
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger

### Testing Authenticated Endpoints

1. Open Swagger UI
2. Click **Authorize** button
3. Complete OAuth2 login flow with Keycloak
4. Make requests to protected endpoints

The application will automatically open Swagger UI in your default browser when running in development mode.

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run from solution root
dotnet test UserServiceTests/UserService.UnitTests.csproj
```

### Code Coverage

The project aims to maintain a code coverage of at least **80%** to ensure reliability and maintainability. We use Coverlet for measuring code coverage and SonarCloud for visualization.

**Current Status**: ~45-50% coverage with **104 unit tests** (all passing ✅)

To run tests with coverage and generate a coverage report:

```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory:"./TestResults"

# Generate HTML coverage report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" \
                 -targetdir:"./TestResults/Report" \
                 -reporttypes:"Html;TextSummary"

# View the report
open ./TestResults/Report/index.html  # macOS/Linux
start ./TestResults/Report/index.html  # Windows
```

The coverage report helps identify areas that need additional testing to meet our 80% coverage target.

### SonarCloud Integration

The project is integrated with SonarCloud for code quality analysis. To configure SonarCloud properly:

1. **Access SonarCloud Settings**:
    - Go to [SonarCloud](https://sonarcloud.io/) and log in
    - Select your project `mayconht_Aristotle_Demo`
    - Navigate to "Administration" > "Analysis Method"

2. **Configure Analysis Method**:
    - Disable "Automatic Analysis" if you're using CI-based analysis
    - Or vice versa (don't run both simultaneously)

3. **Required GitHub Secrets**:
    - `SONAR_TOKEN`: Your SonarCloud API token (from SonarCloud user settings)

### Local SonarQube Setup (Optional)

For local code quality analysis, you can run SonarQube using Docker Compose:

```bash
# Start SonarQube and PostgreSQL
docker-compose up -d

# Access SonarQube at http://localhost:9000
# Default credentials: admin/admin
```

### Hot Reload Feature

The application supports hot reload with `dotnet watch` which provides:

- **Automatic recompilation** when you save code changes
- **Live reload** without manual restart
- **Real-time feedback** for development

> **Tip**: You'll see hot reload messages in the terminal when files are changed and recompiled.

### Available Endpoints

The API provides user management functionality. Check the Swagger documentation for complete API details:

- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

**Note**: User creation is handled automatically through JIT provisioning on first login via Keycloak.

### Database

The project uses SQLite with Entity Framework Core:

- **Database file**: `users.db` (created automatically)
- **Migrations**: Located in `Migrations/` folder
- **Context**: `ApplicationDbContext` in Infrastructure layer

To create new migrations:

```bash
cd UserService
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

### Development Workflow

1. Make code changes in your IDE
2. Save the file (`Ctrl+S`)
3. Watch the terminal for hot reload confirmation (if using `dotnet watch`)
4. Test changes immediately in Swagger UI
5. Create unit tests
6. Run tests to make sure everything works
7. Repeat!

### Project Structure

```
Aristotle_Study/
├── Aristotle_Study.sln          # Solution file
├── coverlet.runsettings         # Code coverage settings
├── Dockerfile                   # Docker configuration
├── global.json                  # .NET SDK version
├── README.md                    # This file
├── UserService/                 # Main ASP.NET Core Web API project
│   ├── appsettings.Development.json
│   ├── appsettings.json
│   ├── Program.cs
│   ├── UserService.csproj
│   ├── Application/             # Application services and logic
│   │   ├── MappingProfile.cs
│   │   ├── UserValidator.cs
│   │   ├── DTOs/
│   │   ├── Exceptions/
│   │   └── Service/
│   ├── Controllers/             # API controllers
│   │   └── UserController.cs
│   ├── Domain/                  # Domain entities and interfaces
│   │   ├── Entities/
│   │   ├── Exceptions/
│   │   └── Interfaces/
│   ├── Infrastructure/          # Data access and external concerns
│   │   ├── ApplicationDbContext.cs
│   │   ├── Data/
│   │   ├── Exceptions/
│   │   └── Middleware/
│   ├── bin/
│   └── obj/
├── UserServiceTests/            # Unit tests for UserService
│   ├── UserService.UnitTests.csproj
│   ├── Application/
│   │   ├── MappingProfileTests.cs
│   │   ├── UserValidatorTests.cs
│   │   ├── Controllers/
│   │   ├── DTOs/
│   │   └── Service/
│   ├── Builders/
│   │   └── UserBuilder.cs
│   ├── Config/
│   │   └── TestConfig.cs
│   ├── Domain/
│   │   ├── Entities/
│   │   └── Exceptions/
│   ├── Infrastructure/
│   │   ├── ApplicationDbContextTests.cs
│   │   ├── Data/
│   │   └── Middleware/
│   ├── bin/
│   └── obj/
├── Tests/                       # Additional test project
│   ├── bin/
│   └── obj/
├── ISTQB/                       # ISTQB study materials
│   ├── StartHere.md
│   └── 1_Fundamentals-Of-Testing/
│       └── 1_Fundamentals.md
├── bruno/                       # API testing with Bruno
│   └── UserService API/
│       ├── bruno.json
│       ├── collection.bru
│       ├── environments/
│       └── User API Regression Test/
└── Aristotle_Project/           # Legacy project directory
    ├── bin/
    └── obj/
```

### Testing Strategy

The project includes unit tests covering:

- Controller endpoints with various scenarios
- Service layer business logic
- Domain entity validation
- Repository data access operations
- Exception handling
- Builder pattern for test data

Test frameworks used:

- xUnit for test execution
- Moq for mocking dependencies
- Bogus for generating fake test data
- Verify for snapshot testing

### API Testing with Bruno

The project includes API testing using Bruno, a CLI tool for testing APIs. The test collection is located in the `bruno/UserService API/` directory. To execute the tests,
follow these steps:

1. **Install Bruno CLI**:
   ```bash
   npm install -g @usebruno/cli
   ```

2. **Run the Tests**:
   ```bash
   bruno run bruno/UserService\ API/collection.bru
   ```

3. **View Reports**:
    - Test results will be generated in the specified output directory if configured.

Bruno is also integrated into the CI/CD pipeline to ensure API functionality during automated builds.
To understand more about this outstanding tool, visit the [Bruno Documentation](https://docs.usebruno.com/).

You can also download the Bruno extension for [VS Code](https://marketplace.visualstudio.com/items?itemName=usebruno.bruno) or [JetBrains IDEs](https://plugins.jetbrains.com/plugin/20449-bruno).
or even download the [Bruno Desktop App](https://www.usebruno.com/download).

### Contributing

This is an educational project, but contributions are welcome! Areas for improvement include:

- Increase test coverage from 45% to 80%
- Fix .NET compiler warnings
- Address SonarQube security hotspots
- Add integration tests
- Implement additional validators
- Improve error handling
- Add more comprehensive logging

### Troubleshooting

**Port Issues**: If default ports are in use, modify the URLs in `Properties/launchSettings.json`

**Database Issues**: Delete `users.db` file to reset the database (it will be recreated automatically)

**Build Issues**: Run `dotnet clean` followed by `dotnet restore` and `dotnet build`

---

*This project serves as a learning exercise comparing C#/.NET development patterns with Java/Spring Boot, focusing on
Clean Architecture principles and modern development practices.*
