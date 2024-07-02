using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using OidcProxy.Net.Cryptography;
using OidcProxy.Net.Jwt;

namespace OidcProxy.Net.OpenIdConnect.Tests.Jwe;

public class SslCertificateTests
{
    private const string AccessToken =
        "eyJhbGciOiJSU0EtT0FFUCIsImVuYyI6IkEyNTZDQkMtSFM1MTIiLCJraWQiOiJDRjc4OUQ3NDgxNkYwNUUwODA4MzU2QTk0MUNENDNDNzl" +
        "GNEUzRUQ4IiwidHlwIjoiYXQrand0IiwiY3R5IjoiSldUIn0" +
        
        ".u0ZP2C-mNbi8arpu5Az5XykinRHCktKgmZQJ1ApR4iMbce82dpM3xOfhxb6IodLa6YwTJSBToScT3ipVZptsvyoiAdmBaCF0DAuCku5SH-" +
        "LshYhjRQyucRWwmryaVF88fieNdcNQMLxyQkuF1AHi9WbRXPj7oqV_bMtonCNdMEqLdWk-u18QI7YOOBq0P8_pCoVMCd8B8xVKPoRMNrxG2" +
        "OvXExj5BCe8XAprmvq3wJ8Dp-d7XLH-9148dx72Q23CimruCpGW59G340MZTlD4Hf3c1G2ByBit3g9H-Uy9gFit4rUYDr2pm00JDllAA1kn" +
        "nquFIDI-a1pbeNCbQz1XIg" +
        
        ".qiUV4wuOTeDvLc04sxtMcg." +
        
        "OaGpswztgMWEeSRq7f7LDHJIW8VYbMlZrMMGIlg475Haarq5gVzaFaF1DVKYXlwQy-SrgnLSvuSogTTEnnZ5Yg0T8hBGSk7BUW-5lYjWW27" +
        "LX2CReAKpDsaSDI5YIGJglnT17rYEsEcY2mUF8bd02tVtCYSqGRqZSFmaeq3KUPeROcKF0-JeZAFDiBnCXitveLcm0_sutBalYPXnFpvftK" +
        "62uTinhrm0GgTCIi-Dmuc_Cp5Js_yyeh1AzYeUi115kZsE1YdQkcFQDuXSylvNv4737n3-0Vd2jxSXXKDtNAJIJ9kjmbM5kUP69lbepItW0" +
        "X3qzdWaxG68Ffkx_sWrwzEEBUw12kJe86Vd7JUm1Rry-yjGPLD353dp2KAyw1RbB7_qI26x73BPtqrOzo3Dk59g5U7cZmgLXMt_d-Pcq52E" +
        "vwCEOSNEQ03JQTizlO6lrVbNsBekH_9Y2hfyEn5GUYkxaxijADpiKQcPYTV8H1eHvUcXjl16ZSonb9c0ZHC51ZAcG7Jypp-Fx6Fzggp0WqN" +
        "JjcboceqqUzlb8UI9SZZs2LGT51Ld7td5TEURqganPVRTyztGgTEvLvd42bvojuvUzhH1iNTjaN4uQRfzg9axfgc6oGzazwwTk9Suay18Fo" +
        "1EWYExsThqS4wqEqR0IDxTFIF1V9KeXvezBYcjEBnfO0ANBly8JhFOcgqgsWfPVW7GT2C0oKwUbWdhytLr7Hrud_aKup1kYM7LW9AliPD4x" +
        "5MW6KTnctD5Bn79Y-VXJe70qknklSOmORO2y21BMIE6Ithg_rLRpRp1suhMWno3t0Tgd537QtAdHC_CK070KtheY-bUtCezZjS74QHu5w3v" +
        "JNYE9RLd_93spdaURJfL8JWTpMx4RyHgK-czCwnfcmiBqzy4ec3Nm1AJmVxt2RdyHUcPbM_kHtH2nIuZ6_1FYDFoeAwQNLX4LSZ-g2R4trJ" +
        "me1rqYe7C_6p1_O0jQN1H5ieMFade5SBOpFw1RjQM89wPreo2gHzTd3hF3JOfOcfRRxzE0iJ7iybJ05U3znA59JAnOM-u7TuOFgR610gDhZ" +
        "A9bPjKQidKlguezluRQftOhBSTb12hMiisS59AG9u_jFIzwpC5wI1lRE5GhKL7vK6YZrHKVMNOJRaXHy6n2MDsS3DyFrlt7aBeBcbsl9gj4" +
        "Mh-d_5SHPRqiPu7wL4ntiVoOy99r1AJb28m82D3uYPPlZMxlnl2R_5icQ825f_uAQQ-4tntOMCxP1QdVTDXENDCDjFGfNpvP3bk3v8XYBlU" +
        "FZouTMSe1zbv2UTBf4d1Pod-sprcXcPQv3t1puJdebVFPlsSI4sjKRXjP55RiYsbOEpXEOKkmEMObAzFN6MRBHeqdDK-eGBvRXiVXJuVi05" +
        "b1nJLr1wCiZgDukmq.dhhO6WTgTz-ayv3oUs33_0H9a9DQl4qv2oELAoenJVw";

    private string CertPath = $"{Guid.NewGuid()}.pem";
    private string PrivateKeyPath = $"{Guid.NewGuid()}.pem";
    private string PublicKeyPath = $"{Guid.NewGuid()}.pem";
    
    public SslCertificateTests()
    {
        File.WriteAllText(CertPath, Files.Cert);
        File.WriteAllText(PrivateKeyPath, Files.PrivateKey);
    }
    
    ~SslCertificateTests() 
    {
        File.Delete(CertPath);
        File.Delete(PrivateKeyPath);
        File.Delete(PublicKeyPath);
    }

    [Fact]
    public void ItShouldDecryptToken()
    {
        var cert = X509Certificate2.CreateFromPemFile(CertPath, PrivateKeyPath);
        var sut = new JweParser(new SslCertificate(cert));

        var actual = sut.ParseJwtPayload(AccessToken);

        actual.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void WhenCertificateWithoutPrivateKey_ItShouldThrowNotSupportedException()
    {
        var cert = new X509Certificate2();
        var sut = new JweParser(new SslCertificate(cert));

        var actual = () => sut.ParseJwtPayload(AccessToken);

        actual.Should().Throw<AuthenticationException>();
    }
}