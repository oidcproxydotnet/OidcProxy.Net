using OidcProxy.Net.Cryptography;

namespace OidcProxy.Net.OpenIdConnect;

internal class JsonWebKeySet : List<KeySet>
{
    internal static JsonWebKeySet Create(Duende.IdentityModel.Client.JsonWebKeySetResponse jwksResponse)
    {
        var keySets = jwksResponse.KeySet?.Keys
            .Select(x =>
            {
                var exponent = x.E.Base64UrlDecode();
                var modulus = x.N.Base64UrlDecode();
                
                return new KeySet(exponent, modulus, x.Kid);
            })
            .ToArray();

        return new(keySets);
    }

    private JsonWebKeySet(IEnumerable<KeySet> keySets)
    {
        this.AddRange(keySets);
    }
}