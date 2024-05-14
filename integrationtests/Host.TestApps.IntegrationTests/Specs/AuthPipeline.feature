Feature: ASP.NET Core Authorization pipeline

Scenario: Authorized users requesting a resource protected by an [Authorize] attribute
Given the user interacts with the site that implements the OidcProxy with a browser
  And the user has authenticated (navigated to /.auth/login)
 When a resource is requested that requires authorization
 Then the endpoint produces a 200 OK

 Scenario: Unauthenticated users accessing an endpoint with an [Authorize] attribute
 Given the user interacts with the site that implements the OidcProxy with a browser
  When a resource is requested that requires authorization
  Then the endpoint responds with a 401 unauthorized
  
 Scenario: Unauthenticated users interact with [Policy] attribute
 Given the user interacts with the site that implements the OidcProxy with a browser
   And the OidcProxy has been configured to use an ASP.NET Core Policy
   And the user has authenticated (navigated to /.auth/login)
  When a resource is requested that requires authorization
  Then the ASP.NET Core can use ACCESS_TOKEN claims only
 
