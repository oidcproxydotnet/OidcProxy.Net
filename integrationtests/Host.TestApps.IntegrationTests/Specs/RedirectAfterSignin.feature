Feature: Redirecting after signing in

Scenario: Normal GET requests via browser
Given the OidcProxy is configured to disallow anonymous access
  And the user interacts with the site that implements the OidcProxy with a browser
 When the user navigates to <url>
 Then the user has been authenticated
  And the path in the browser equals <url>
  And access_tokens are forwarded to downstream APIs
Examples:
| url                   |
| /                     |
| /api/echo             |
| /api/echo?a=b&foo=bar |
| /api/echo?a=b#foo=bar |

Scenario: Defining whitelisted redirect-urls in sign-in url
Given the OidcProxy is configured to allow anonymous access
  And the user interacts with the site that implements the OidcProxy with a browser
  And the OidcProxy has included <url> in the whitelisted redirect urls
 When the user navigates to /.auth/login?landingpage=<url>
 Then the user has been authenticated
  And the path in the browser equals <url>
  And access_tokens are forwarded to downstream APIs
Examples:
| url                   |
| /                     |
| /api/echo             |