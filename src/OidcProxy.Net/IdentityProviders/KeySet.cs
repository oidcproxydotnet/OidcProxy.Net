namespace OidcProxy.Net.IdentityProviders;

public class KeySet (byte[] exponent, byte[] modulus, string Kid)
{
    public byte[] Exponent { get; private set; } = exponent;
    
    public byte[] Modulus { get; private set; } = modulus;

    public string Kid { get; private set; } = Kid;
}