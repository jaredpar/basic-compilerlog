namespace Basic.CompilerLog.UnitTests;

using System.Runtime.InteropServices;
using Basic.CompilerLog.Util;
using Microsoft.CodeAnalysis;
using Xunit;

[Collection(CompilerLogCollection.Name)]
public sealed class InMemoryLoaderTests : TestBase
{
    public CompilerLogFixture Fixture { get; }

    public InMemoryLoaderTests(ITestOutputHelper testOutputHelper, ITestContextAccessor testContextAccessor, CompilerLogFixture fixture)
        : base(testOutputHelper, testContextAccessor, nameof(CompilerLogReaderTests))
    {
        Fixture = fixture;
    }

#if NET

    [Theory]
    [InlineData("AbstractTypesShouldNotHaveConstructorsAnalyzer", LanguageNames.CSharp, 1)]
    [InlineData("AbstractTypesShouldNotHaveConstructorsAnalyzer", null, 2)]
    [InlineData("NotARealName", null, 0)]
    public void AnalyzersForNetAnalyzers(string analyzerTypeName, string? language, int expectedCount)
    {
        using var reader = CompilerLogReader.Create(Fixture.Console.Value.CompilerLogPath, BasicAnalyzerKind.InMemory);
        var compilerCall = reader.ReadCompilerCall(0);
        var compilerData = reader.ReadCompilationData(compilerCall);
        var analyzerReference = compilerData.AnalyzerReferences.Single(x => x.Display == "Microsoft.CodeAnalysis.NetAnalyzers");
        var analyzers = language is null
            ? analyzerReference.GetAnalyzersForAllLanguages()
            : analyzerReference.GetAnalyzers(language);
        var specific = analyzers.Where(x => x.GetType().Name == analyzerTypeName);
        Assert.Equal(expectedCount, specific.Count());
    }

    [Theory]
    [InlineData("ComClassGenerator", LanguageNames.CSharp, 1)]
    [InlineData("ComClassGenerator", null, 1)]
    [InlineData("NotARealName", null, 0)]
    public void GeneratorsForCom(string analyzerTypeName, string? language, int expectedCount)
    {
        using var reader = CompilerLogReader.Create(Fixture.Console.Value.CompilerLogPath, BasicAnalyzerKind.InMemory);
        var compilerCall = reader.ReadCompilerCall(0);
        var compilerData = reader.ReadCompilationData(compilerCall);
        var analyzerReference = compilerData.AnalyzerReferences.Single(x => x.Display == "Microsoft.Interop.ComInterfaceGenerator");
        var generators = language is null
            ? analyzerReference.GetGeneratorsForAllLanguages()
            : analyzerReference.GetGenerators(language);
        var specific = generators.Where(x => TestUtil.GetGeneratorType(x).Name == analyzerTypeName);
        Assert.Equal(expectedCount, specific.Count());
    }
#endif
}
