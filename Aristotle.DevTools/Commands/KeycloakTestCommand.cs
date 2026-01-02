namespace Aristotle.DevTools.Commands;

using System.Net.Http.Json;
using System.Text.Json;
// WARNING: This script is intended for development purposes only.
// DONT TRUST IT FOR PRODUCTION USE!
// THIS PROJECT IS FOR EDUCATIONAL PURPOSES ONLY!
public static class KeycloakTestCommand
{
    private const string KeycloakUrl = "http://localhost:8080";
    private const string Realm = "userservice";
    private const string ClientId = "userservice-api";
    private const string ClientSecret = "dev-client-secret-12345";

    public static async Task<int> ExecuteAsync()
    {
        Console.WriteLine("=== KEYCLOAK VALIDATION TEST ===\n");

        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(KeycloakUrl);

        Console.WriteLine("1. Testing authentication with user 'admin'...");
        var tokenResponse = await GetTokenAsync(httpClient, Realm, ClientId, ClientSecret, "admin", "admin123");

        if (tokenResponse == null)
        {
            Console.WriteLine("FAILURE: Could not obtain token");
            return 1;
        }

        Console.WriteLine($"OK: Token obtained successfully");
        Console.WriteLine($"Token Type: {tokenResponse.TokenType}");
        Console.WriteLine($"Expires In: {tokenResponse.ExpiresIn} seconds");
        Console.WriteLine($"Access Token (first 50 chars): {tokenResponse.AccessToken[..50]}...\n");

        Console.WriteLine("2. Decoding and validating claims...");
        var claims = ParseToken(tokenResponse.AccessToken);

        if (claims == null)
        {
            Console.WriteLine("FAILURE: Could not decode token");
            return 1;
        }

        var claimsValue = claims.Value;

        Console.WriteLine("Claims found:");
        Console.WriteLine($"  sub: {claimsValue.GetProperty("sub").GetString()}");
        Console.WriteLine($"  email: {claimsValue.GetProperty("email").GetString()}");
        Console.WriteLine($"  preferred_username: {claimsValue.GetProperty("preferred_username").GetString()}");

        if (claimsValue.TryGetProperty("realm_access", out var realmAccess))
        {
            if (realmAccess.TryGetProperty("roles", out var roles))
            {
                Console.WriteLine("  realm_access.roles:");
                foreach (var role in roles.EnumerateArray())
                {
                    Console.WriteLine($"    - {role.GetString()}");
                }
            }
        }

        if (claimsValue.TryGetProperty("groups", out var groups))
        {
            Console.WriteLine("  groups:");
            foreach (var group in groups.EnumerateArray())
            {
                Console.WriteLine($"    - {group.GetString()}");
            }
        }

        Console.WriteLine("\n3. Testing Manager and User users...");
        var managerToken = await GetTokenAsync(httpClient, Realm, ClientId, ClientSecret, "manager", "manager123");
        var userToken = await GetTokenAsync(httpClient, Realm, ClientId, ClientSecret, "user", "user123");

        if (managerToken != null && userToken != null)
        {
            Console.WriteLine("OK: All users authenticated successfully");
        }

        Console.WriteLine("\n=== VALIDATION COMPLETED SUCCESSFULLY ===");
        return 0;
    }

    private static async Task<TokenResponse?> GetTokenAsync(HttpClient client, string realmName, string clientId, string clientSecret, string username, string password)
    {
        try
        {
            var tokenEndpoint = $"/realms/{realmName}/protocol/openid-connect/token";

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["username"] = username,
                ["password"] = password,
                ["scope"] = "openid profile email roles"
            });

            var response = await client.PostAsync(tokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error obtaining token: {error}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TokenResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            return null;
        }
    }

    private static JsonElement? ParseToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            var payload = parts[1];
            var paddedPayload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var decodedBytes = Convert.FromBase64String(paddedPayload);
            var decodedPayload = System.Text.Encoding.UTF8.GetString(decodedBytes);

            var jsonDoc = JsonDocument.Parse(decodedPayload);
            return jsonDoc.RootElement;
        }
        catch
        {
            return null;
        }
    }
}

record TokenResponse(
    [property: System.Text.Json.Serialization.JsonPropertyName("access_token")] string AccessToken,
    [property: System.Text.Json.Serialization.JsonPropertyName("expires_in")] int ExpiresIn,
    [property: System.Text.Json.Serialization.JsonPropertyName("token_type")] string TokenType,
    [property: System.Text.Json.Serialization.JsonPropertyName("refresh_token")] string? RefreshToken
);
