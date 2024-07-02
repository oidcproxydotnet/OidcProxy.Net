namespace OidcProxy.Net.Cryptography;

public class KeySet (byte[] exponent, byte[] modulus, string kid)
{
    public byte[] Exponent { get; private set; } = exponent;
    
    public byte[] Modulus { get; private set; } = modulus;
    
    public string Kid { get; private set; } = kid;
}