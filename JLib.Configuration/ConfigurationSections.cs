namespace JLib.Configuration;

public static class ConfigurationSections
{
    /// <summary>
    /// the key under which environments can be specified.
    /// this should only be overriden when the app launches to prevent enforcing the usage of the environment subgroups.
    /// </summary>
    public static string Environment { get; set; }= "Environment";
}