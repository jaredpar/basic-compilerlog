﻿using Basic.CompilerLog.Util;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Basic.CompilerLog.UnitTests;

[Collection(CompilerLogCollection.Name)]
public sealed class SolutionReaderTests : TestBase
{
    public CompilerLogFixture Fixture { get; }

    public SolutionReaderTests(ITestOutputHelper testOutputHelper, CompilerLogFixture fixture)
        : base(testOutputHelper, nameof(SolutionReader))
    {
        Fixture = fixture;
    }

    [Fact]
    public async Task DocumentsGeneratedDefaultHost()
    {
        var host = new BasicAnalyzerHostOptions(BasicAnalyzerKind.Default);
        using var reader = SolutionReader.Create(Fixture.ConsoleComplogPath.Value, host);
        var workspace = new AdhocWorkspace();
        var solution = workspace.AddSolution(reader.ReadSolutionInfo());
        var project = solution.Projects.Single();
        Assert.NotEmpty(project.AnalyzerReferences);
        var docs = project.Documents.ToList();
        var generatedDocs = (await project.GetSourceGeneratedDocumentsAsync()).ToList();
        Assert.Null(docs.FirstOrDefault(x => x.Name == "RegexGenerator.g.cs"));
        Assert.Single(generatedDocs);
        Assert.NotNull(generatedDocs.First(x => x.Name == "RegexGenerator.g.cs"));
    }

    [Fact]
    public async Task DocumentsGeneratedNoneHost()
    {
        var host = new BasicAnalyzerHostOptions(BasicAnalyzerKind.None);
        using var reader = SolutionReader.Create(Fixture.ConsoleComplogPath.Value, host);
        var workspace = new AdhocWorkspace();
        var solution = workspace.AddSolution(reader.ReadSolutionInfo());
        var project = solution.Projects.Single();
        Assert.Empty(project.AnalyzerReferences);
        var docs = project.Documents.ToList();
        var generatedDocs = (await project.GetSourceGeneratedDocumentsAsync()).ToList();
        Assert.Equal(5, docs.Count);
        Assert.Equal("RegexGenerator.g.cs", docs.Last().Name);
        Assert.Empty(generatedDocs);
    }
}
