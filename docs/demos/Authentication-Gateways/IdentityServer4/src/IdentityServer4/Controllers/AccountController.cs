using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using TestIdentityServer.Models.Account;
using TestIdentityServer.ModuleInitializers;

namespace TestIdentityServer.Controllers;

public class AccountController : Controller
{
    private readonly TestUserStore _users = new(TestUsers.Users);
    
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;

    public AccountController(IIdentityServerInteractionService interaction,
        IEventService events)
    {
        _interaction = interaction;
        _events = events;
    }
    
    [HttpGet]
    public async Task<IActionResult> Login(string returnUrl)
    {
        await _interaction.GetAuthorizationContextAsync(returnUrl);
        
        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }
    
    [HttpPost]
    public async Task<IActionResult> Login(LoginInputModel model, string username)
    {
        var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

        var user = _users.FindByUsername(username);
        await _events.RaiseAsync(new UserLoginSuccessEvent(user.Username, 
            user.SubjectId, 
            user.Username, 
            clientId: context?.Client.ClientId));

        var issuer = new IdentityServerUser(user.SubjectId)
        {
            DisplayName = user.Username
        };

        await HttpContext.SignInAsync(issuer, null);

        if (context != null || Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }
        
        throw new Exception("invalid return URL");
    }
    
    [HttpGet]
    public async Task<IActionResult> Logout(string logoutId)
    {
        await _interaction.GetLogoutContextAsync(logoutId);
        
        return View(new LogOutInputModel
        {
            LogoutId = logoutId
        });
    }

    
    /// <summary>
    /// Handle logout page postback
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Logout(LogOutInputModel model)
    {
        var logout = await _interaction.GetLogoutContextAsync(model.LogoutId);

        if (User?.Identity.IsAuthenticated == true)
        {
            // delete local authentication cookie
            await HttpContext.SignOutAsync();

            // raise the logout event
            await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
        }
        
        return Redirect(logout?.PostLogoutRedirectUri);
    }
}