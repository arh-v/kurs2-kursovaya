using BuildingStatePredictionApp.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingStatePredictionAppTests;

[TestFixture]
public class BlockNameGeneratorTests
{
    [TestCase(0, "A","B","C")]
    [TestCase(26, "AA","AB","AC")]
    [TestCase(2*26, "BA","BB","BC")]
    [TestCase(27*26, "AAA","AAB","AAC")]
    public void Test(int skipCount, params string[] expectedNames)
    {
        var namer = new BlockNameGenerator();

        while (skipCount-- > 0)
        {
            namer.NextName();
        }

        foreach (var expected in expectedNames)
        {
            Assert.That(namer.NextName(), Is.EqualTo(expected));
        }
    }
}