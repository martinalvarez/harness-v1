using NetArchTest.Rules;
using Xunit;

namespace Gates.Architecture.Tests;

/// <summary>
/// Architecture gates derived 1:1 from /specs/constitution.md and
/// /specs/verification.md (ARCH-01..ARCH-05). Inspects compiled IL via
/// NetArchTest, not source, so it works against whatever /src produces.
/// </summary>
public sealed class ArchitectureGateTests
{
    [Fact]
    public void ARCH01_Api_DoesNotDependOn_EntityFrameworkCore()
    {
        var result = Types.InAssembly(AssemblyLoader.Load("Api"))
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void ARCH02_Api_DoesNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(AssemblyLoader.Load("Api"))
            .ShouldNot()
            .HaveDependencyOn("Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void ARCH03_Domain_DoesNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(AssemblyLoader.Load("Domain"))
            .ShouldNot()
            .HaveDependencyOn("Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void ARCH04_Domain_DoesNotDependOn_Frameworks()
    {
        var result = Types.InAssembly(AssemblyLoader.Load("Domain"))
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void ARCH05_ControllerTypes_NameEndsWithController()
    {
        var result = Types.InAssembly(AssemblyLoader.Load("Api"))
            .That()
            .ResideInNamespaceContaining("Controllers")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Controller")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result) =>
        result.IsSuccessful
            ? string.Empty
            : "Failing types: " + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>());
}
