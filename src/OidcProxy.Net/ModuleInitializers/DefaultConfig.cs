namespace OidcProxy.Net.ModuleInitializers;

internal class DefaultAppSettingsSection : IAppSettingsSection
{
    public bool Validate(out IEnumerable<string> errors)
    {
        errors = Array.Empty<string>();
        return true;
    }

    public void Apply(ProxyOptions options)
    {
        // i.l.e.
    }
}