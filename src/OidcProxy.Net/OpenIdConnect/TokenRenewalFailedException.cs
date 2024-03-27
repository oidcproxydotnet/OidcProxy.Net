namespace OidcProxy.Net.OpenIdConnect;

public class TokenRenewalFailedException(string errorMessage) : ApplicationException(errorMessage);