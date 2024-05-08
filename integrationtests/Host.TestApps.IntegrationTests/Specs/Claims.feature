Feature: Claims

Scenario: Plain JWT
Given the user interacts with the site that implements the OidcProxy with a browser
  And the user has authenticated (navigated to /.auth/login)
 When the user navigates to /.auth/me
 Then the payload of the the ID_TOKEN is visible

Scenario: JWE with symmetric key
Given the OidcProxy is configured to decrypt JWEs with this symmetric key: DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=
  And the Oidc Server encrypts JWEs with this symmetric key: DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=
  And the user interacts with the site that implements the OidcProxy with a browser
  And the user has authenticated (navigated to /.auth/login)
 When the user navigates to /.auth/me
 Then the payload of the the ID_TOKEN is visible
 
Scenario: JWE with certificate
Given the user interacts with the site that implements the OidcProxy with a browser
  And the OidcProxy is configured to decrypt JWEs with this certificate: cert.pem, key.pem
  And the Oidc server is configured to encrypt JWEs with this certificate: cert.pem, key.pem
  And the user has authenticated (navigated to /.auth/login)
 When the user navigates to /.auth/me
 Then the payload of the the ID_TOKEN is visible

Scenario: Claims transformation
Given the user interacts with the site that implements the OidcProxy with a browser
  And the user of OidcProxy has implemented claims transformation
  And the user has authenticated (navigated to /.auth/login)
 When the user navigates to /.auth/me
 Then the /me endpoint shows the claims returned by the claims transformation only