using Xunit;
using Xunit.Abstractions;

namespace MarvinDateMcp.Tests;

public class GenerateDocumentationTests
{
    private readonly ITestOutputHelper _output;

    public GenerateDocumentationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task GenerateJsonResults_ForDocumentation()
    {
        var generator = new JsonResultsGenerator();
        var json = await generator.GenerateJsonResultsAsync();
        
        // Write to test output
        _output.WriteLine(json);
        
        // Also save to file
        var docsDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "docs");
        Directory.CreateDirectory(docsDir);
        
        var jsonFile = Path.Combine(docsDir, "test-results.json");
        await File.WriteAllTextAsync(jsonFile, json);
        
        _output.WriteLine($"\nJSON results saved to: {jsonFile}");
    }
}
