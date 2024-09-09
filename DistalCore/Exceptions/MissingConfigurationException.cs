namespace DistalCore.Exceptions;

public class MissingConfigurationException(string message) : Exception($"Missing configuration for object '{message}'")
{
}
