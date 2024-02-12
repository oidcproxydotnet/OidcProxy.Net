using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Host.TestApps.OpenIdDict.IntegrationTests.Fixtures.OpenIddict;

public class DbContext : IdentityDbContext<ApplicationUser>
{
    public DbContext(DbContextOptions options)
        : base(options) { }
}