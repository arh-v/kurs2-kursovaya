using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingStatePredictionApp.Models.Extensions;

public static class StreamReaderExtensions
{
    public static IEnumerable<string> ReadLines(this StreamReader reader)
    {
        while (reader.ReadLine() is { } line)
        {
            yield return line;
        }
    }
}
