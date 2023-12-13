// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Archetype.Framework.Meta;
using Archetype.GrammarGenerator;

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

var grammarTemplate = File.ReadAllText(baseGrammar);
var targetGrammar = Path.Combine(targetGrammarDir, Path.GetFileName(baseGrammar).Replace(".template", ""));

var generatedGrammar =
    new KeywordAnalyzer(grammarTemplate).GenerateKeywordSyntax(GetAssemblies(assemblyPaths.Split(';')));

if (!Directory.Exists(targetGrammarDir))
{
    Directory.CreateDirectory(targetGrammarDir);
    Console.WriteLine($"Created directory: {targetGrammarDir}");
}

File.WriteAllText(targetGrammar, generatedGrammar);

return;

static Assembly[] GetAssemblies(IEnumerable<string> assemblyPaths)
{
    var assemblies = assemblyPaths.Select(Assembly.LoadFrom).ToList();
    var referencedAssemblies = assemblies.SelectMany(a => a.GetReferencedAssemblies()).ToList();
    
    assemblies.AddRange(referencedAssemblies.Select(Assembly.Load));
    
    return assemblies.ToArray();
}

