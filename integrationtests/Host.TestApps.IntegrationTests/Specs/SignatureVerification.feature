Feature: Signature validation

Scenario: Valid Token RS256
Given the user interacts with the site that implements the OidcProxy with a browser
  And the Oidc Server signs the JWT with RS256
  And the user has authenticated (navigated to /.auth/login)
  When the user navigates to /.auth/me
  Then the payload of the the ID_TOKEN is visible

Scenario: Valid Token HS256
Given the user interacts with the site that implements the OidcProxy with a browser
  And the Oidc Server signs the JWT with HS256
  And the OidcProxy is configured to use a symmetric key to validate the token signature
  And the user has authenticated (navigated to /.auth/login)
  When the user navigates to /.auth/me
  Then the payload of the the ID_TOKEN is visible
  
Scenario: Tampered access_token RS256
Given the user interacts with the site that implements the OidcProxy with a browser
  And the Oidc Server signs the JWT with RS256
  And the Proxy receives a token that has been tampered with
  When the user has authenticated (navigated to /.auth/login)
  Then the endpoint responds with a 401 unauthorized

Scenario: Tampered access_token HS256
Given the user interacts with the site that implements the OidcProxy with a browser
  And the Oidc Server signs the JWT with HS256
  And the OidcProxy is configured to use a symmetric key to validate the token signature
  And the Proxy receives a token that has been tampered with
  When the user has authenticated (navigated to /.auth/login)
  Then the endpoint responds with a 401 unauthorized
