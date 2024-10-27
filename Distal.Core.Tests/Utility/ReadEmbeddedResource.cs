using System.Reflection;

namespace Distal.Core.Tests.Utility;

public static class EmbeddedResources
{
    public static Stream GetStream(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fullResourceName = $"Distal.Core.Tests.TestData.Meshes.{resourceName}";
        var stream = assembly.GetManifestResourceStream(fullResourceName) ?? throw new FileNotFoundException($"Embedded resource '{fullResourceName}' not found.");
        return stream;
    }
}
