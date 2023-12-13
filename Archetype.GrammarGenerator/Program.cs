// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Archetype.Framework.Meta;

if (args.Length == 0)
{
    Console.WriteLine("Usage: Program.exe assemblyPath baseGrammar.template ...");
    return;
}

Console.WriteLine("Current directory: " + Directory.GetCurrentDirectory());

var assemblyPaths = args[0];
var baseGrammar = args[1];
var targetGrammarDir = args[2];


if (!File.Exists(baseGrammar))
{
    Console.WriteLine($"File not found: {baseGrammar}");
    return;
}

if (!File.Exists(assemblyPaths))
{
    Console.WriteLine($"File not found: {assemblyPaths}");
    return;
}

if (!baseGrammar.EndsWith(".template.g4"))
{
    Console.WriteLine($"File must end with .template.g4: {baseGrammar}");
    return;
}

var grammarFile = File.ReadAllText(baseGrammar);
var targetGrammar = Path.Combine(targetGrammarDir, Path.GetFileName(baseGrammar).Replace(".template", ""));

var keywords = GetKeywords(assemblyPaths.Split(';'));
var antlrKeywordsString = "(" + string.Join("|", keywords) + ")";

grammarFile = grammarFile.Replace("/*KEYWORD_LIST*/", antlrKeywordsString);

if (!Directory.Exists(targetGrammarDir))
{
    Directory.CreateDirectory(targetGrammarDir);
    Console.WriteLine($"Created directory: {targetGrammarDir}");
}

File.WriteAllText(targetGrammar, grammarFile);

return;


static string[] GetKeywords(IEnumerable<string> assemblyPaths)
{
    var keywordClasses = new List<Type>();
    var assemblies = assemblyPaths.Select(Assembly.LoadFrom).ToList();
    var referencedAssemblies = assemblies.SelectMany(a => a.GetReferencedAssemblies()).ToList();
    
    assemblies.AddRange(referencedAssemblies.Select(Assembly.Load));
    

    foreach (var classesWithKeywordAttribute in assemblies.Select(assembly => assembly.GetTypes()
                     .Where(type => type.GetCustomAttribute<KeywordAttribute>() != null)))
    {
        keywordClasses.AddRange(classesWithKeywordAttribute);
    }

    return keywordClasses
        .Where(type => !string.IsNullOrWhiteSpace(type.GetCustomAttribute<KeywordAttribute>()?.Keyword))
        .Select(type => $"'{type.GetCustomAttribute<KeywordAttribute>()!.Keyword}'")
        .ToArray();
}