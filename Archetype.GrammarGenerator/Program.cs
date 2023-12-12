// See https://aka.ms/new-console-template for more information



using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class KeywordAttribute : Attribute
{
    public string Keyword { get; }

    public KeywordAttribute(string keyword)
    {
        Keyword = keyword;
    }
}

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: Program.exe assemblyPath1 assemblyPath2 ...");
            return;
        }

        var assemblyPaths = args.ToList();

        var keywords = GetKeywords(assemblyPaths);
        var antlrKeywordsString = "(" + string.Join("|", keywords) + ")";
        Console.WriteLine(antlrKeywordsString);
    }

    static string[] GetKeywords(List<string> assemblyPaths)
    {
        var keywordClasses = new List<Type>();

        foreach (var assemblyPath in assemblyPaths)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);

            var classesWithKeywordAttribute = assembly.GetTypes()
                .Where(type => type.GetCustomAttribute<KeywordAttribute>() != null);

            keywordClasses.AddRange(classesWithKeywordAttribute);
        }

        return keywordClasses
            .Select(type => type.GetCustomAttribute<KeywordAttribute>().Keyword)
            .ToArray();
    }
}
