namespace OidcProxy.Net.ModuleInitializers;

public interface IAppSettingsSection
{
    /// <summary>
    /// Validates the user-provided values from the config section.
    /// </summary>
    /// <param name="errors">Yields a list of error messages</param>
    /// <returns>Returns a boolean value indicating the validity of values in the config file</returns>
    public bool Validate(out IEnumerable<string> errors);
    
    /// <summary>
    /// Transfers the values from this object onto the ProxyOptions object, allowing OidcProxy to use the values.
    /// </summary>
    /// <param name="options">An instance of ProxyOptions, the object used to bootstrap OidcProxy.Net.</param>
    public void Apply(ProxyOptions options);
}