using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace BetterTacticalDevicesHandling.Tests;

internal static class GameAssembly
{
    private const string AssemblyName = "Assembly-CSharp";
    private const string FileHollowed = "hollowed.dll";

    // Game types live in Assembly-CSharp, which the build references as hollowed.dll; the runtime looks it up by
    // its assembly name, so point that name at the copied file.
    [ModuleInitializer]
    internal static void Register()
    {
        AssemblyLoadContext.Default.Resolving += (context, name) =>
        {
            if (name.Name != AssemblyName)
            {
                return null;
            }

            string path = Path.Combine(Path.GetDirectoryName(typeof(GameAssembly).Assembly.Location)!, FileHollowed);
            return File.Exists(path) ? context.LoadFromAssemblyPath(path) : null;
        };
    }
}
