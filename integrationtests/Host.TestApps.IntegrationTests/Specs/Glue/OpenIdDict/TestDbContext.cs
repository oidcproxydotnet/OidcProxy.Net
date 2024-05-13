using Host.TestApps.IntegrationTests.Fixtures.OpenIddict;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Host.TestApps.IntegrationTests.Specs.Glue.OpenIdDict;

public class TestDbContext : IdentityDbContext<ApplicationUser>
{
    public TestDbContext(DbContextOptions options)
        : base(options) { }
}