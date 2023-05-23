using System.Security.Claims;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Test;
using Newtonsoft.Json;

namespace TestIdentityServer.ModuleInitializers;

public static class TestUsers
{
    
    private static readonly object Address = new
    {
        street_address = "One Hacker Way",
        locality = "Heidelberg",
        postal_code = 69118,
        country = "Germany"
    };
    
    public static List<TestUser> Users =>
        new()
        {
            new TestUser
            {
                SubjectId = "test-user-1",
                Username = "palpatine",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "Sheev Palpatine"),
                    new Claim(JwtClaimTypes.GivenName, "Sheev"),
                    new Claim(JwtClaimTypes.FamilyName, "Palpatine"),
                    new Claim(JwtClaimTypes.Email, "senatorpalpatine@rechtspraak.nl"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "http://darkside.com"),
                    new Claim(JwtClaimTypes.Address, JsonConvert.SerializeObject(Address), IdentityServerConstants.ClaimValueTypes.Json)
                }
            },
            new TestUser
            {
                SubjectId = "test-user-2",
                Username = "yoda",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "Master Yoda"),
                    new Claim(JwtClaimTypes.GivenName, "Master"),
                    new Claim(JwtClaimTypes.FamilyName, "Yoda"),
                    new Claim(JwtClaimTypes.Email, "masteryoda@rechtspraak.nl"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "http://jedimindtricks.com"),
                    new Claim(JwtClaimTypes.Address, JsonConvert.SerializeObject(Address), IdentityServerConstants.ClaimValueTypes.Json)
                }
            },
            new TestUser
            {
                SubjectId = "test-user-3",
                Username = "hansolo",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "Han Solo"),
                    new Claim(JwtClaimTypes.GivenName, "Han"),
                    new Claim(JwtClaimTypes.FamilyName, "Solo"),
                    new Claim(JwtClaimTypes.Email, "hansolo@rechtspraak.nl"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "http://milleniumfalcon.com"),
                    new Claim(JwtClaimTypes.Address, JsonConvert.SerializeObject(Address), IdentityServerConstants.ClaimValueTypes.Json)
                }
            },
            new TestUser
            {
                SubjectId = "test-user-4",
                Username = "luke",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "Luke Skywalker"),
                    new Claim(JwtClaimTypes.GivenName, "Luke"),
                    new Claim(JwtClaimTypes.FamilyName, "Skywalker"),
                    new Claim(JwtClaimTypes.Email, "lukeskywalker@rechtspraak.nl"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "http://tatooine-is-boring.com"),
                    new Claim(JwtClaimTypes.Address, JsonConvert.SerializeObject(Address), IdentityServerConstants.ClaimValueTypes.Json)
                }
            },
            new TestUser
            {
                SubjectId = "test-user-5",
                Username = "chewie",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "Chewbaka Nobodyknows"),
                    new Claim(JwtClaimTypes.GivenName, "Chewbaka"),
                    new Claim(JwtClaimTypes.FamilyName, "?"),
                    new Claim(JwtClaimTypes.Email, "chewbaka@rechtspraak.nl"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "http://wookieworld.com"),
                    new Claim(JwtClaimTypes.Address, JsonConvert.SerializeObject(Address), IdentityServerConstants.ClaimValueTypes.Json)
                }
            },
            new TestUser
            {
                SubjectId = "test-user-6",
                Username = "obiwan",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "Obiwan Kenobi"),
                    new Claim(JwtClaimTypes.GivenName, "Obiwan"),
                    new Claim(JwtClaimTypes.FamilyName, "Kenobi"),
                    new Claim(JwtClaimTypes.Email, "obikenobi@rechtspraak.nl"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "http://jedimindtricks.com"),
                    new Claim(JwtClaimTypes.Address, JsonConvert.SerializeObject(Address), IdentityServerConstants.ClaimValueTypes.Json)
                }
            },
            new TestUser
            {
                SubjectId = "test-user-7",
                Username = "bobafett",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "Boba Fett"),
                    new Claim(JwtClaimTypes.GivenName, "Boba"),
                    new Claim(JwtClaimTypes.FamilyName, "Fett"),
                    new Claim(JwtClaimTypes.Email, "bobafett@rechtspraak.nl"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "http://thisistheway.com"),
                    new Claim(JwtClaimTypes.Address, JsonConvert.SerializeObject(Address), IdentityServerConstants.ClaimValueTypes.Json)
                }
            },
            new TestUser
            {
                SubjectId = "test-user-8",
                Username = "lando",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "Lando Calrissian"),
                    new Claim(JwtClaimTypes.GivenName, "Lando"),
                    new Claim(JwtClaimTypes.FamilyName, "Calrissian"),
                    new Claim(JwtClaimTypes.Email, "landocalrissian@rechtspraak.nl"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "http://google.com"),
                    new Claim(JwtClaimTypes.Address, JsonConvert.SerializeObject(Address), IdentityServerConstants.ClaimValueTypes.Json)
                }
            },
            new TestUser
            {
                SubjectId = "test-user-9",
                Username = "r2d2",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "R2D2"),
                    new Claim(JwtClaimTypes.GivenName, "R2"),
                    new Claim(JwtClaimTypes.FamilyName, "D2"),
                    new Claim(JwtClaimTypes.Email, "r2d2@rechtspraak.nl"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "http://bitsandbytes.com"),
                    new Claim(JwtClaimTypes.Address, JsonConvert.SerializeObject(Address), IdentityServerConstants.ClaimValueTypes.Json)
                }
            }
        };
}