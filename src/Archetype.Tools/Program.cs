using System.Reflection;

if (args.Length == 0 || args[0] is "--help" or "-h")
{
    Console.WriteLine("Usage: dotnet archetype <command> [options]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  install-skills    Copy Archetype Claude Code skills into .claude/skills/");
    Console.WriteLine();
    Console.WriteLine("Options for install-skills:");
    Console.WriteLine("  --global      Install to ~/.claude/skills/ instead of ./.claude/skills/");
    Console.WriteLine("  --overwrite   Replace existing skill files");
    return 0;
}

if (args[0] == "install-skills")
{
    var isGlobal = args.Contains("--global");
    var overwrite = args.Contains("--overwrite");

    var targetDir = isGlobal
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude", "skills")
        : Path.Combine(Directory.GetCurrentDirectory(), ".claude", "skills");

    var assembly = Assembly.GetExecutingAssembly();
    const string prefix = "skills/";

    var resources = assembly.GetManifestResourceNames()
        .Where(n => n.StartsWith(prefix))
        .ToList();

    if (resources.Count == 0)
    {
        Console.Error.WriteLine("No skills found in package. This is a bug — please report it.");
        return 1;
    }

    var installed = 0;
    var skipped = 0;

    foreach (var resourceName in resources)
    {
        var relativePath = resourceName[prefix.Length..];   // e.g. "archetype-cards/SKILL.md"
        var destPath = Path.Combine(targetDir, relativePath);

        if (!overwrite && File.Exists(destPath))
        {
            Console.WriteLine($"  skip   {relativePath}");
            skipped++;
            continue;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var dest = File.Create(destPath);
        stream.CopyTo(dest);

        Console.WriteLine($"  write  {relativePath}");
        installed++;
    }

    Console.WriteLine();
    Console.WriteLine($"Installed {installed} file(s) to {targetDir}");
    if (skipped > 0)
        Console.WriteLine($"Skipped {skipped} existing file(s) — use --overwrite to replace");

    return 0;
}

Console.Error.WriteLine($"Unknown command: {args[0]}");
Console.Error.WriteLine("Run 'dotnet archetype --help' for usage.");
return 1;
