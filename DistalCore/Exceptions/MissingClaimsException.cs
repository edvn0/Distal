namespace DistalCore.Exceptions;

public class MissingClaimsException(string message) : Exception($"Missing claim '{message}'.")
{
}
