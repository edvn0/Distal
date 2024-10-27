using Distal.Core.Models.Formats;

namespace Distal.Core.Tests.Utility;

public static class Extensions
{
    public static void ExecuteWithType<T>(this IParseableMeshFileFormat? output, Action<T> action) where T : class, new()
    {
        if (output is not T toPerformOn)
        {
            Assert.Fail("The output format of the parser does not match the requested type.");
            return;
        }

        action(toPerformOn);
    }
}