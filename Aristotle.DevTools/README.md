# Aristotle DevTools

Development only utilities CLI for the Aristotle Study platform.

again

DEVELOPMENT ONLY 

Mobile project will not be made public.

## Overview

This tool provides automated setup and testing commands for local development environment, including Keycloak configuration and authentication testing.

## Prerequisites

- .NET 8.0 SDK
- Docker and Docker Compose (for Keycloak and PostgreSQL)
- Running Keycloak instance on `http://localhost:8080`
- Running PostgreSQL instance on `localhost:5432`

## Installation

```bash
cd Scripts/Aristotle.DevTools
dotnet build
```

## Usage

### Show Help

```bash
dotnet run -- --help
```

### Keycloak Commands

#### Setup Keycloak

Automatically configures a complete Keycloak environment with:
- Realm creation (`userservice`)
- **Two OAuth2 clients:**
  - `userservice-api`: Confidential client for backend API (with client secret)
  - `userservice-mobile`: Public client for mobile apps (PKCE-enabled, no secret)
- Roles (Admins, Managers, Users)
- Groups (admin-group, manager-group, user-group)
- Test users with passwords
- Protocol mappers for JWT claims

```bash
dotnet run -- keycloak setup
```

**Output:**
- Generates and displays API client secret
- Configures mobile client for PKCE authentication
- Creates test users:
  - `admin / admin123` (Admins role)
  - `manager / manager123` (Managers role)
  - `user / user123` (Users role)

**Important:** Copy the generated `KEYCLOAK_CLIENT_SECRET` to your `.env` file.

#### Mobile App Configuration

The setup creates a **public client** (`userservice-mobile`) specifically for mobile applications:

**Key Features:**
- Public client (no client secret required)
- PKCE required with SHA-256 challenge method
- Authorization Code flow enabled
- Resource Owner Password Credentials flow disabled (security best practice)
- Mobile-friendly redirect URIs

**Redirect URIs configured:**
- `myapp://callback` - Custom URL scheme
- `com.userservice.mobile://callback` - Package-based redirect
- `http://localhost:19006/--/*` - Expo development
- `exp://localhost:19000/--/*` - Expo Go

**Mobile App Usage Example:**

```typescript
// React Native / Expo example
import * as AuthSession from 'expo-auth-session';

const discovery = {
  authorizationEndpoint: 'http://localhost:8080/realms/userservice/protocol/openid-connect/auth',
  tokenEndpoint: 'http://localhost:8080/realms/userservice/protocol/openid-connect/token',
};

const [request, response, promptAsync] = AuthSession.useAuthRequest(
  {
    clientId: 'userservice-mobile',
    scopes: ['openid', 'profile', 'email'],
    redirectUri: AuthSession.makeRedirectUri({
      scheme: 'myapp'
    }),
    usePKCE: true, // PKCE is required
  },
  discovery
);
```

**Why Public Client for Mobile?**
- Mobile apps cannot securely store secrets (any secret in the app binary can be extracted)
- PKCE (Proof Key for Code Exchange) provides security without requiring secrets
- Follows OAuth 2.0 best practices for native applications (RFC 8252)

#### Test Keycloak Authentication

Validates Keycloak configuration by:
- Authenticating with test users
- Parsing and displaying JWT tokens
- Validating claims (sub, email, roles, groups)

```bash
dotnet run -- keycloak test
```

## Quick Start Workflow

1. **Start infrastructure:**
   ```bash
   docker-compose up -d postgres keycloak
   ```

2. **Setup Keycloak:**
   ```bash
   cd Scripts/Aristotle.DevTools
   dotnet run -- keycloak setup
   ```

3. **Copy the generated client secret to `.env`:**
   ```bash
   KEYCLOAK_CLIENT_SECRET=<generated-secret>
   ```

4. **Test authentication:**
   ```bash
   dotnet run -- keycloak test
   ```

5. **Start UserService:**
   ```bash
   cd ../../USR/UserService
   dotnet run
   ```

## Architecture

```
Aristotle.DevTools/
├── Program.cs                     # CLI entry point with System.CommandLine
├── Commands/
│   ├── KeycloakSetupCommand.cs   # Keycloak setup automation
│   └── KeycloakTestCommand.cs    # Keycloak authentication testing
└── README.md
```

## Development

### Adding New Commands

1. Create a new command class in `Commands/` folder
2. Implement a `public static async Task<int> ExecuteAsync()` method
3. Register the command in `Program.cs`

Example:
```csharp
// Commands/NewCommand.cs
namespace Aristotle.DevTools.Commands;

public static class NewCommand
{
    public static async Task<int> ExecuteAsync()
    {
        Console.WriteLine("Executing new command...");
        return 0; // Success
    }
}

// Program.cs
var newCmd = new Command("new", "Description of new command");
newCmd.SetHandler(async () =>
{
    var result = await NewCommand.ExecuteAsync();
    Environment.ExitCode = result;
});
rootCommand.AddCommand(newCmd);
```

## Troubleshooting

### Keycloak not accessible
Ensure Keycloak is running and accessible:
```bash
curl http://localhost:8080/health
```

### Authentication fails
- Verify the client secret matches between DevTools output and `.env` file
- Check that test users were created successfully
- Ensure Keycloak realm is `userservice` and client is `userservice-api`

### Database connection issues
Verify PostgreSQL is running:
```bash
docker ps | grep postgres
```

## License

Part of the Aristotle Study platform.
