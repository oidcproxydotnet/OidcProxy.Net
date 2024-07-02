namespace OidcProxy.Net.IdentityProviders;

public class KeySet (byte[] exponent, byte[] modulus, byte[] key, string kid)
{
    public byte[] Exponent { get; private set; } = exponent;
    
    public byte[] Modulus { get; private set; } = modulus;
    
    public byte[] Key { get; private set; } = modulus;

    public string Kid { get; private set; } = kid;
}