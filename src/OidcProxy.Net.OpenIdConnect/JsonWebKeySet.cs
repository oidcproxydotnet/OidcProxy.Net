using OidcProxy.Net.IdentityProviders;

namespace OidcProxy.Net.OpenIdConnect;

public class JsonWebKeySet : List<KeySet>
{
    internal static JsonWebKeySet Create(IdentityModel.Client.JsonWebKeySetResponse jwksResponse)
    {
        var keySets = jwksResponse.KeySet?.Keys
            .Select(x =>
            {
                var exponent = x.E.Base64UrlDecode();
                var modulus = x.N.Base64UrlDecode();
                var key = x.K.Base64UrlDecode();
                
                return new KeySet(exponent, modulus, key, x.Kid);
            })
            .ToArray();

        return new(keySets);
    }

    private JsonWebKeySet(IEnumerable<KeySet> keySets)
    {
        this.AddRange(keySets);
    }
}