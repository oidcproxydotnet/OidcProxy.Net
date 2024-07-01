Feature: Signature validation

Scenario Outline: Valid Token
Given the user interacts with the site that implements the OidcProxy with a browser
  And the Oidc Server signs the JWT with <algorithm>
  And the user has authenticated (navigated to /.auth/login)
  When the user navigates to /.auth/me
  Then the payload of the the ID_TOKEN is visible
Examples:
| algorithm |
| SHA256    |

Scenario Outline: Man-In-The-Middle Attack
Given the user interacts with the site that implements the OidcProxy with a browser
  And the Oidc Server signs the JWT with <algorithm>
  And the Proxy receives a token that has been tampered with
  When the user has authenticated (navigated to /.auth/login)
  Then the endpoint responds with a 401 unauthorized
Examples:
| algorithm |
| SHA256    |