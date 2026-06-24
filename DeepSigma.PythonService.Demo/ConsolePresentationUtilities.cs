using System;
using System.Collections.Generic;
using System.Text;

namespace DeepSigma.PythonService.Demo;

internal static class ConsolePresentationUtilities
{
    private const string seperator = "///////////////////////////////////////////";
    internal static void Print(string text)
    {
        Console.WriteLine();
        Console.WriteLine(seperator);
        Console.WriteLine(text);
        Console.WriteLine(seperator);
    }
        
}

