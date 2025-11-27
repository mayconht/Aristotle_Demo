using System.CommandLine;
using Aristotle.DevTools.Commands;

var rootCommand = new RootCommand("Aristotle DevTools - Development utilities for Aristotle Study platform");

// Add keycloak command
var keycloakCommand = new Command("keycloak", "Keycloak configuration and management");
var setupCommand = new Command("setup", "Setup Keycloak realm, client, roles, groups and test users");
var testCommand = new Command("test", "Test Keycloak authentication and token validation");

keycloakCommand.AddCommand(setupCommand);
keycloakCommand.AddCommand(testCommand);
rootCommand.AddCommand(keycloakCommand);

// Setup command handler
setupCommand.SetHandler(async () =>
{
    var result = await KeycloakSetupCommand.ExecuteAsync();
    Environment.ExitCode = result;
});

// Test command handler
testCommand.SetHandler(async () =>
{
    var result = await KeycloakTestCommand.ExecuteAsync();
    Environment.ExitCode = result;
});

return await rootCommand.InvokeAsync(args);
