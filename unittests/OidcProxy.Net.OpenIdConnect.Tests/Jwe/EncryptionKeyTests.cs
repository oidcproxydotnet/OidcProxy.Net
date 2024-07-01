using System.Security.Authentication;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect.Jwe;

namespace OidcProxy.Net.OpenIdConnect.Tests.Jwe;

public class EncryptionKeyTests
{
    private const string AccessToken = "eyJhbGciOiJBMjU2S1ciLCJlbmMiOiJBMjU2Q0JDLUhTNTEyIiwidHlwIjoiYXQrand0IiwiY3R5" +
                                       "IjoiSldUIn0" +
                                       
                                       ".Q7ilXykDweLWbuetfSwLfq8yQF5eM5rD2YHFCb2ThC90tIAWfKPTkzIl3JMnAN8_YuF3FeFH1GI" +
                                       "yWWVurvdQYob-LLhar5DW" +
                                       ".Tksg8YDRC_DDd3odvCYl6g" +
                                       ".cH-1du2h7EFsZkFk-gGkNAfP0LOt_ErjQbHlHBCaBZf63uN26NkBUMsTWvtAOBt07WIT-i5Jllp2" +
                                       "iVCS657qrf0aGmubePqVitEi3bJMiBxSW5KPciKZ-9u1Ka0M1BXSr4y1Kkwf5LaGLKwQL03ckHfPW" +
                                       "HzxiM3RajvveqTcAEZcKn3rgH7XDVMBuULwXQV3Cqq62_bEZ0gxB0wlBqC5OREBNf9Z2hUxxrqrxL" +
                                       "qA-6rZTDEf6KTbSmxdWmPlwWAxcS2hGDNvmtnMJp-JFmMpI5A76Fa3yE_cW3Q3m-agGpyy5dwzOsX" +
                                       "8n4keTo2Esb_4iMZMeOHL-L36kg-Hmn9ObQmae6aVdUB0_fVh7XdfWPoftvfd_D2ndoIoX0vu4EnQ" +
                                       "6nxi-U3JE6tiOfM3CW3W9g8S3nAjoSYi38UWD1jlVHSd5hZczPmxSICee2xrELGJxuyxZOqjjEd7j" +
                                       "_TJsIthpTtFZah9SIbM2dftqFm9Wa2lQMaELa_AejplzIrPPsHIXJeghWOx4bgCtsHxj8F15Oa5Pb" +
                                       "TEJadxwR-p2m0qIkag5fV_QTAxNuLDqnUE1wOQuBH3-8GbKxwmds6CY2OW1fCBorc_DgJwq_DwHEY" +
                                       "IdKMF8SlLPwXQUKuf1VaiwtUJ23-7UiLaSQby2dJKYHqrDFXx37gXEV3dAyuEab_dluMH5XlFEKuO" +
                                       "pHZTESvRB-EcjPUUYZxN-XeyRN3cgJehy_S03ZIUbDqCdGMS9Ny_IzP0l4YnAjeSONgEqwi3v9vHY" +
                                       "s8d8v8sSJliyapc1OYhNuTsGNaJvALBu4XcrYoPqQy5Wo0UZBpRvhnoEznbQ1s6d3UXVKf8BTJJy-" +
                                       "Kvjm6NMAB9ajJLB9uPNAzOI9s29-c_5kNSzzRLStYyddqrh3si0XeV1K7359ayHRq2keOoMt-OlHl" +
                                       "B5jZviykP2uB1-rdqPlYy7qpVWUIF5Hez-8lhm9JHciVXPdyZ-WwOqyZ91RRB91Y_KE8KOg3K2Jfs" +
                                       "SL24MnvVLpdMfS4gcqMZfZbjpMg8ROjbsi9_KtLeJNQZnuXTOx85EFaiAeXqvDVJOJOU_z5yReRXA" +
                                       "Semd41biuAZL5lBKFGktge_YcYLx6ciK8hkCC3U-vMbHbzPXB0RCzD1e3aUm5e_zNuNX4KubwLw-a" +
                                       "SXxHVDBqEJavuR_fT4KTedlLvSJXw-TWZU3D1GzobmZmr8foGq4mam4RawJN6MhChy1v1eVMY_4rv" +
                                       "4qxWqUioBScI3XFlo-Yj98Wy-T8sbUhJLQEQdDIB8vphk8Zp301FAMvnU16OieI5DPFPrJDMa9nkR" +
                                       "bmOPppsuT7Hke8ngc8pQtQ5j6wH_RO1F1gNunIZmd2Od1hV_1TWLDpHhl9UKcihZ68GltAgIDr1cp" +
                                       "kwlyiuBoViDgkU2YsuGgyFb.BmMbXVr9e-W97GkG_P1Oqk0GXSpq3A3UycdhaMVOoAk";

    private readonly ITokenParser _sut;
    
    public EncryptionKeyTests()
    {
        var bytes = Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=");
        var key = new SymmetricSecurityKey(bytes);
        
        _sut = new JweParser(new ProxyOptions(), new EncryptionKey(key));
    }
    
    [Fact]
    public void ItShouldDecryptToken()
    {
        var actual = _sut.ParseJwtPayload(AccessToken);

        actual.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void WhenParsingFails_ShouldThrowAuthenticationException()
    {
        var actual = () => _sut.ParseJwtPayload(string.Empty);

        actual.Should().Throw<AuthenticationException>();
    }
}