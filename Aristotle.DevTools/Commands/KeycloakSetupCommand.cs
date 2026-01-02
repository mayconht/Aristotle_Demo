using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Aristotle.DevTools.Commands;
// WARNING: This script is intended for development purposes only.
// DONT TRUST IT FOR PRODUCTION USE!
// THIS PROJECT IS FOR EDUCATIONAL PURPOSES ONLY!

//MOBILE CLIENT NOT ON THIS REPO, shall not be made public
public static class KeycloakSetupCommand
{
    private const string KeycloakUrl = "http://localhost:8080";
    private const string AdminUser = "admin";
    private const string AdminPassword = "admin";
    private const string ConstRealmName = "userservice";
    private const string ClientId = "userservice-api";
    private const string MobileClientId = "userservice-mobile";

    public static async Task<int> ExecuteAsync()
    {
        Console.WriteLine("=== Keycloak Setup Automation ===\n");

        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(KeycloakUrl);

        try
        {
            // 1. Get admin token
            Console.WriteLine("1. Obtaining admin token...");
            var adminToken = await GetAdminTokenAsync(httpClient, AdminUser, AdminPassword);
            if (string.IsNullOrEmpty(adminToken))
            {
                Console.WriteLine("ERROR: Failed to obtain admin token");
                return 1;
            }
            Console.WriteLine("Admin token obtained\n");

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

            // 2. Create realm
            Console.WriteLine("2. Creating realm 'userservice'...");
            var realmCreated = await CreateRealmAsync(httpClient, ConstRealmName);
            Console.WriteLine(realmCreated ? "Realm created" : "Realm already exists");
            Console.WriteLine();

            // 3. Create client
            Console.WriteLine("3. Creating OAuth2 client 'userservice-api'...");
            var clientCreated = await CreateClientAsync(httpClient, ConstRealmName, ClientId);
            Console.WriteLine(clientCreated ? "Client created" : "Client already exists");
            Console.WriteLine();

            // 4. Configure client settings
            Console.WriteLine("4. Configuring client settings...");
            await ConfigureClientAsync(httpClient, ConstRealmName, ClientId);
            Console.WriteLine("Client configured\n");

            // 5. Create realm roles
            Console.WriteLine("5. Creating realm roles...");
            await CreateRoleAsync(httpClient, ConstRealmName, "Admins");
            await CreateRoleAsync(httpClient, ConstRealmName, "Managers");
            await CreateRoleAsync(httpClient, ConstRealmName, "Users");
            Console.WriteLine("Roles created\n");

            // 6. Create groups
            Console.WriteLine("6. Creating groups...");
            await CreateGroupAsync(httpClient, ConstRealmName, "admin-group");
            await CreateGroupAsync(httpClient, ConstRealmName, "manager-group");
            await CreateGroupAsync(httpClient, ConstRealmName, "user-group");
            Console.WriteLine("Groups created\n");

            // 7. Assign roles to groups
            Console.WriteLine("7. Assigning roles to groups...");
            await AssignRoleToGroupAsync(httpClient, ConstRealmName, "admin-group", "Admins");
            await AssignRoleToGroupAsync(httpClient, ConstRealmName, "manager-group", "Managers");
            await AssignRoleToGroupAsync(httpClient, ConstRealmName, "user-group", "Users");
            Console.WriteLine("Roles assigned to groups\n");

            // 8. Create test users
            Console.WriteLine("8. Creating test users...");
            var users = new[]
            {
                new UserInfo("admin", "admin123", "admin@userservice.com", "admin-group"),
                new UserInfo("manager", "manager123", "manager@userservice.com", "manager-group"),
                new UserInfo("user", "user123", "user@userservice.com", "user-group")
            };

            foreach (var userInfo in users)
            {
                await CreateUserWithGroupAsync(httpClient, ConstRealmName, userInfo);
            }
            Console.WriteLine("Test users created\n");

            // 9. Add protocol mappers
            Console.WriteLine("9. Configuring protocol mappers...");
            await AddProtocolMappersAsync(httpClient, ConstRealmName, ClientId);
            Console.WriteLine("Protocol mappers configured\n");

            // 10. Get and regenerate client secret
            Console.WriteLine("10. Generating client secret...");
            var clientSecret = await RegenerateClientSecretAsync(httpClient, ConstRealmName, ClientId);

            // 11. Create mobile public client
            Console.WriteLine("11. Creating mobile public client 'userservice-mobile'...");
            var mobileClientCreated = await CreateMobileClientAsync(httpClient, ConstRealmName, MobileClientId);
            Console.WriteLine(mobileClientCreated ? "Mobile client created" : "Mobile client already exists");
            Console.WriteLine();

            // 12. Configure mobile client settings
            Console.WriteLine("12. Configuring mobile client settings...");
            await ConfigureMobileClientAsync(httpClient, ConstRealmName, MobileClientId);
            Console.WriteLine("Mobile client configured\n");

            // 13. Add protocol mappers to mobile client
            Console.WriteLine("13. Configuring mobile client protocol mappers...");
            await AddProtocolMappersAsync(httpClient, ConstRealmName, MobileClientId);
            Console.WriteLine("Mobile client protocol mappers configured\n");

            // 14. Assign Admin role to API service account
            Console.WriteLine("14. Assigning Admin role to API service account...");
            await AssignRoleToServiceAccountAsync(httpClient, ConstRealmName, ClientId, "Admins");
            Console.WriteLine("Admin role assigned to service account\n");

            Console.WriteLine("\n=== SETUP COMPLETED SUCCESSFULLY ===");
            Console.WriteLine($"\nAPI Client Secret (confidential): {clientSecret}");
            Console.WriteLine("\nAdd this to your .env file:");
            Console.WriteLine($"KEYCLOAK_CLIENT_SECRET={clientSecret}");
            Console.WriteLine("\n--- Mobile App Configuration (Public Client with PKCE) ---");
            Console.WriteLine($"Client ID: {MobileClientId}");
            Console.WriteLine("Client Type: Public (No secret required)");
            Console.WriteLine("PKCE: Required (S256)");
            Console.WriteLine("Redirect URIs:");
            Console.WriteLine("  - myapp://callback");
            Console.WriteLine("  - com.userservice.mobile://callback");
            Console.WriteLine("  - iomrider://callback (IOMRider app)");
            Console.WriteLine("  - http://localhost:19006/--/* (Expo development)");
            Console.WriteLine("  - exp://localhost:19000/--/* (Expo Go)");
            Console.WriteLine("  - exp://192.168.1.157:8082/--/callback (Expo network)");
            Console.WriteLine("  - exp://*:8082/--/callback (Expo wildcard)");
            Console.WriteLine("\nTest users:");
            Console.WriteLine("  admin / admin123 (Admins role)");
            Console.WriteLine("  manager / manager123 (Managers role)");
            Console.WriteLine("  user / user123 (Users role)");

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return 1;
        }
    }

    private static async Task<string?> GetAdminTokenAsync(HttpClient client, string username, string password)
    {
        var tokenRequest = new Dictionary<string, string>
        {
            ["username"] = username,
            ["password"] = password,
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli"
        };

        var response = await client.PostAsync("/realms/master/protocol/openid-connect/token",
            new FormUrlEncodedContent(tokenRequest));

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Token error: {error}");
            return null;
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return tokenResponse?.AccessToken;
    }

    private static async Task<bool> CreateRealmAsync(HttpClient client, string realmName)
    {
        var realmPayload = new
        {
            realm = realmName,
            enabled = true,
            registrationAllowed = false,
            loginWithEmailAllowed = true,
            duplicateEmailsAllowed = false,
            resetPasswordAllowed = true,
            editUsernameAllowed = false,
            bruteForceProtected = true,
            accessTokenLifespan = 300,
            ssoSessionIdleTimeout = 1800,
            ssoSessionMaxLifespan = 36000
        };

        var response = await client.PostAsJsonAsync("/admin/realms", realmPayload);
        return response.IsSuccessStatusCode;
    }

    private static async Task<bool> CreateClientAsync(HttpClient client, string realmName, string clientId)
    {
        var clientPayload = new
        {
            clientId,
            enabled = true,
            publicClient = false,
            directAccessGrantsEnabled = true,
            serviceAccountsEnabled = true,
            standardFlowEnabled = true,
            redirectUris = new[] { "http://localhost:3000/*", "http://localhost:3000/swagger/oauth2-redirect.html" },
            webOrigins = new[] { "http://localhost:3000", "+" }
        };

        var response = await client.PostAsJsonAsync($"/admin/realms/{realmName}/clients", clientPayload);
        return response.IsSuccessStatusCode;
    }

    private static async Task ConfigureClientAsync(HttpClient client, string realmName, string clientId)
    {
        var clientUuid = await GetClientUuidAsync(client, realmName, clientId);
        if (string.IsNullOrEmpty(clientUuid)) return;

        var updatePayload = new
        {
            authorizationServicesEnabled = false,
            implicitFlowEnabled = false,
            attributes = new Dictionary<string, string>
            {
                ["pkce.code.challenge.method"] = "S256"
            }
        };

        await client.PutAsJsonAsync($"/admin/realms/{realmName}/clients/{clientUuid}", updatePayload);
    }

    private static async Task<bool> CreateMobileClientAsync(HttpClient client, string internalRealmName, string internalClientId)
    {
        var clientPayload = new
        {
            clientId = internalClientId,
            name = "Mobile App Client",
            description = "Public client for mobile applications using PKCE",
            enabled = true,
            publicClient = true,  // Public client - no secret needed
            directAccessGrantsEnabled = false,  // Disable Resource Owner Password Credentials flow
            serviceAccountsEnabled = false,  // Not applicable for public clients
            standardFlowEnabled = true,  // Enable Authorization Code flow
            implicitFlowEnabled = false,  // Disable implicit flow (not secure)
            bearerOnly = false,
            consentRequired = false,
            redirectUris = new[]
            {
                "myapp://callback",  // Custom URL scheme for mobile app
                "com.userservice.mobile://callback",  // Package-based redirect
                "iomrider://callback",  // IOMRider custom scheme
                "http://localhost:19006/--/*",  // Expo development
                "exp://localhost:19000/--/*",  // Expo Go
                "exp://192.168.1.157:8082/--/callback",  // Expo network callback
                "exp://*:8082/--/callback"  // Expo wildcard for any IP
            },
            webOrigins = new[] { "+" },  // Allow all origins (CORS)
            attributes = new Dictionary<string, string>
            {
                ["pkce.code.challenge.method"] = "S256",  // Require PKCE with SHA-256
                ["post.logout.redirect.uris"] = "myapp://logout||com.userservice.mobile://logout"
            }
        };

        var response = await client.PostAsJsonAsync($"/admin/realms/{internalRealmName}/clients", clientPayload);
        return response.IsSuccessStatusCode;
    }

    private static async Task ConfigureMobileClientAsync(HttpClient client, string internalRealmName, string internalClientId)
    {
        var clientUuid = await GetClientUuidAsync(client, internalRealmName, internalClientId);
        if (string.IsNullOrEmpty(clientUuid)) return;

        // Get current configuration
        var getResponse = await client.GetAsync($"/admin/realms/{internalRealmName}/clients/{clientUuid}");
        if (!getResponse.IsSuccessStatusCode) return;

        var clientConfig = await getResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        if (clientConfig == null) return;

        // Update with mobile-specific settings
        clientConfig["publicClient"] = true;
        clientConfig["standardFlowEnabled"] = true;
        clientConfig["directAccessGrantsEnabled"] = false;
        clientConfig["implicitFlowEnabled"] = false;
        clientConfig["serviceAccountsEnabled"] = false;

        // Ensure PKCE is required
        if (clientConfig.ContainsKey("attributes"))
        {
            var attrs = clientConfig["attributes"] as Dictionary<string, string> ?? new Dictionary<string, string>();
            attrs["pkce.code.challenge.method"] = "S256";
            clientConfig["attributes"] = attrs;
        }
        else
        {
            clientConfig["attributes"] = new Dictionary<string, string>
            {
                ["pkce.code.challenge.method"] = "S256"
            };
        }

        await client.PutAsJsonAsync($"/admin/realms/{internalRealmName}/clients/{clientUuid}", clientConfig);
    }

    private static async Task CreateRoleAsync(HttpClient client, string internalRealmName, string roleName)
    {
        var rolePayload = new { name = roleName, description = $"{roleName} role" };
        // var response = await client.PostAsJsonAsync($"/admin/realms/{internalRealmName}/roles", rolePayload);
        await client.PostAsJsonAsync($"/admin/realms/{internalRealmName}/roles", rolePayload);
        // return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict;
    }

    private static async Task CreateGroupAsync(HttpClient client, string internalRealmName, string groupName)
    {
        var groupPayload = new { name = groupName };
await client.PostAsJsonAsync($"/admin/realms/{internalRealmName}/groups", groupPayload);
        // var response = await client.PostAsJsonAsync($"/admin/realms/{internalRealmName}/groups", groupPayload);

        // return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict;
    }

    private static async Task AssignRoleToGroupAsync(HttpClient client, string internalRealmName, string groupName, string roleName)
    {
        var groupId = await GetGroupIdAsync(client, internalRealmName, groupName);
        var role = await GetRoleAsync(client, internalRealmName, roleName);

        if (string.IsNullOrEmpty(groupId) || role == null) return;

        await client.PostAsJsonAsync($"/admin/realms/{internalRealmName}/groups/{groupId}/role-mappings/realm", new[] { role });
    }

    private static async Task AssignRoleToServiceAccountAsync(HttpClient client, string realmName, string clientIdParam, string roleName)
    {
        // Get the client UUID
        var clientUuid = await GetClientUuidAsync(client, realmName, clientIdParam);
        if (string.IsNullOrEmpty(clientUuid)) return;

        // Get the service account user ID
        var serviceAccountResponse = await client.GetAsync($"/admin/realms/{realmName}/clients/{clientUuid}/service-account-user");
        if (!serviceAccountResponse.IsSuccessStatusCode) return;

        var serviceAccount = await serviceAccountResponse.Content.ReadFromJsonAsync<User>();
        if (serviceAccount == null) return;

        // Get the role
        var role = await GetRoleAsync(client, realmName, roleName);
        if (role == null) return;

        // Assign role to service account
        await client.PostAsJsonAsync($"/admin/realms/{realmName}/users/{serviceAccount.Id}/role-mappings/realm", new[] { role });
    }

    private static async Task CreateUserWithGroupAsync(HttpClient client, string internalRealmName, UserInfo userInfo)
    {
        var userPayload = new
        {
            username = userInfo.Username,
            enabled = true,
            email = userInfo.Email,
            emailVerified = true,
            firstName = userInfo.Username,
            lastName = "User",
            requiredActions = Array.Empty<string>(),
            credentials = new[] { new { type = "password", value = userInfo.Password, temporary = false } }
        };

        var response = await client.PostAsJsonAsync($"/admin/realms/{internalRealmName}/users", userPayload);
        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Conflict)
        {
            Console.WriteLine($"  Failed to create user '{userInfo.Username}'");
            // return false;
        }

        var userId = await GetUserIdAsync(client, internalRealmName, userInfo.Username);
        var groupId = await GetGroupIdAsync(client, internalRealmName, userInfo.GroupName);

        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(groupId))
        {
            await client.PutAsync($"/admin/realms/{internalRealmName}/users/{userId}/groups/{groupId}", null);
            Console.WriteLine($"  User '{userInfo.Username}' created and assigned to '{userInfo.GroupName}'");
            // return true;
        }

        // return false;
    }

    private static async Task AddProtocolMappersAsync(HttpClient client, string internalRealmName, string internalClientId)
    {
        var clientUuid = await GetClientUuidAsync(client, internalRealmName, internalClientId);
        if (string.IsNullOrEmpty(clientUuid)) return;

        var roleMapper = new
        {
            name = "realm-roles-mapper",
            protocol = "openid-connect",
            protocolMapper = "oidc-usermodel-realm-role-mapper",
            config = new Dictionary<string, string>
            {
                ["claim.name"] = "roles",
                ["jsonType.label"] = "String",
                ["multivalued"] = "true",
                ["access.token.claim"] = "true",
                ["id.token.claim"] = "true",
                ["userinfo.token.claim"] = "true"
            }
        };

        var groupMapper = new
        {
            name = "groups-mapper",
            protocol = "openid-connect",
            protocolMapper = "oidc-group-membership-mapper",
            config = new Dictionary<string, string>
            {
                ["claim.name"] = "groups",
                ["full.path"] = "false",
                ["access.token.claim"] = "true",
                ["id.token.claim"] = "true",
                ["userinfo.token.claim"] = "true"
            }
        };

        await client.PostAsJsonAsync($"/admin/realms/{internalRealmName}/clients/{clientUuid}/protocol-mappers/models", roleMapper);
        await client.PostAsJsonAsync($"/admin/realms/{internalRealmName}/clients/{clientUuid}/protocol-mappers/models", groupMapper);
    }

    private static async Task<string?> RegenerateClientSecretAsync(HttpClient client, string internalRealmName, string internalClientId)
    {
        // Use a fixed secret for CI/CD consistency
        const string fixedSecret = "dev-client-secret-12345";

        var clientUuid = await GetClientUuidAsync(client, internalRealmName, internalClientId);
        if (string.IsNullOrEmpty(clientUuid)) return null;

        // Get current client configuration
        var getResponse = await client.GetAsync($"/admin/realms/{internalRealmName}/clients/{clientUuid}");
        if (!getResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to get client configuration");
            return null;
        }

        var clientConfig = await getResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        if (clientConfig == null) return null;

        // Update client with the fixed secret
        clientConfig["secret"] = fixedSecret;

        var updateResponse = await client.PutAsJsonAsync($"/admin/realms/{internalRealmName}/clients/{clientUuid}", clientConfig);

        if (!updateResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to set fixed client secret, using regenerate endpoint...");
            // Fallback to regenerate if setting specific secret fails
            var response = await client.PostAsync($"/admin/realms/{internalRealmName}/clients/{clientUuid}/client-secret", null);
            if (!response.IsSuccessStatusCode) return null;

            var secretResponse = await response.Content.ReadFromJsonAsync<ClientSecret>();
            return secretResponse?.Value;
        }

        return fixedSecret;
    }

    private static async Task<string?> GetClientUuidAsync(HttpClient client, string realm, string clientIdParam)
    {
        var response = await client.GetAsync($"/admin/realms/{realm}/clients?clientId={clientIdParam}");
        if (!response.IsSuccessStatusCode) return null;

        var clients = await response.Content.ReadFromJsonAsync<List<Client>>();
        return clients?.FirstOrDefault()?.Id;
    }

    private static async Task<string?> GetGroupIdAsync(HttpClient client, string realm, string groupName)
    {
        var response = await client.GetAsync($"/admin/realms/{realm}/groups");
        if (!response.IsSuccessStatusCode) return null;

        var groups = await response.Content.ReadFromJsonAsync<List<Group>>();
        return groups?.FirstOrDefault(g => g.Name == groupName)?.Id;
    }

    private static async Task<string?> GetUserIdAsync(HttpClient client, string realm, string username)
    {
        var response = await client.GetAsync($"/admin/realms/{realm}/users?username={username}");
        if (!response.IsSuccessStatusCode) return null;

        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        return users?.FirstOrDefault()?.Id;
    }

    private static async Task<Role?> GetRoleAsync(HttpClient client, string realm, string roleName)
    {
        var response = await client.GetAsync($"/admin/realms/{realm}/roles/{roleName}");
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<Role>();
    }

    private record TokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("token_type")] string? TokenType,
        [property: JsonPropertyName("expires_in")] int ExpiresIn
    );

    private record Client(string Id);
    private record ClientSecret(string Value);
    private record Group(string Id, string Name);
    private record User(string Id, string Username);
    private record Role(string Id, string Name);
    private record UserInfo(string Username, string Password, string Email, string GroupName);
}
