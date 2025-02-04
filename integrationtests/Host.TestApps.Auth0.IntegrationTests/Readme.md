# Integration test Auth0

OidcProxy.Net uses automated tests to make sure new releases don't break existing functionality. 
Part of the testing-suite is the integration-test with Auth0.

To be able to test integration with Auth0 properly, you need to create an Auth0 tenant (don't worry, it's free.)
In time, this codebase will include a script to provision it correctly.

To run the automated tests, navigate your terminal/CMD to `integrationtests/Host.TestApps.Auth0`.

Make sure to configure a test-application in Auth0 and configure the following user-secrets for the integration-test project:

```terminal
dotnet user-secrets set Auth0:ClientId "insert-clientid-here"
dotnet user-secrets set Auth0:ClientSecret "insert-clientsecret-here"
dotnet user-secrets set Auth0:Domain "insert-domain-here"
dotnet user-secrets set Auth0:Audience "insert-audience-here"
```

Also, create a user, have it authenticate with a username and a password. 
Because automated tests have trouble signing in with MFA and passkeys, be sure to disable authentication with passkeys, MFA, and so forth.

Next, navigate your terminal/CMD to `integrationtests/Host.TestApps.Auth0.IntegrationTests`.

Configure the following settings:

```terminal
dotnet user-secrets set Auth0:sub "insert-testuser-sub-here"
dotnet user-secrets set Auth0:username "insert-testuser-username-here" 
dotnet user-secrets set Auth0:password "insert-testuser-password-here"
```

Now, run the tests.