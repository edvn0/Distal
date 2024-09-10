namespace Distal.Core.Options;

public interface IExternalAPIOption
{
    public string Address { get; init; }
    public int Timeout { get; }
}

public class Random : IExternalAPIOption
{
    public string Address { get; init; } = string.Empty;
    public int Timeout { get; set; } = 15;
}

public class ExternalAPIOptions
{
    public const string Identifier = "ExternalAPI";

    public required Random Random { get; set; }
}
