Feature: Token forwarding

Scenario: Unauthenticated users
Given the user interacts with the site that implements the OidcProxy with a browser
 When the user invokes a downstream API
 Then the downstream API does not receive an AUTHORIZATION header

Scenario: Authenticated users
Given the user interacts with the site that implements the OidcProxy with a browser
  And the user has authenticated (navigated to /.auth/login)
 When the user invokes a downstream API
 Then the downstream API receives an AUTHORIZATION header
 
Scenario: Signing out
Given the user interacts with the site that implements the OidcProxy with a browser
  And the user has authenticated (navigated to /.auth/login)
  And the user has signed out (navigated to /.auth/end-session)
 When the user invokes a downstream API
 Then the downstream API does not receive an AUTHORIZATION header


