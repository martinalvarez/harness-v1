using System.Reflection;
using Xunit;

namespace Gates.Architecture.Tests;

/// <summary>
/// Loads compiled assemblies from /src by project name (e.g. "Api",
/// "Domain"). Deliberately reflection-based, not a ProjectReference:
/// NetArchTest inspects compiled IL, and this project must still build
/// when /src is empty. A missing assembly fails the test with a clear
/// message instead of a build error.
/// </summary>
internal static class AssemblyLoader
{
    private static readonly string SrcRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src"));

    public static Assembly Load(string projectName)
    {
        var projectDir = Path.Combine(SrcRoot, projectName);
        if (!Directory.Exists(projectDir))
        {
            Assert.Fail(
                $"ARCH gate: '{projectName}' project not found at '{projectDir}'. " +
                "Implement /src per specs before this gate can pass.");
        }

        var dll = Directory.EnumerateFiles(projectDir, $"{projectName}.dll", SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();

        if (dll is null)
        {
            Assert.Fail(
                $"ARCH gate: no build output for '{projectName}' under '{projectDir}'. " +
                "Run `dotnet build` on /src before this gate can pass.");
        }

        return Assembly.LoadFrom(dll!);
    }
}
