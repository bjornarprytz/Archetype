#!/bin/bash

generator_program="Archetype.GrammarGenerator"
base_grammar="Archetype.GrammarGenerator/static/BaseGrammar.g4.template"

dotnet build "$generator_program"
target_assembly="$generator_program/bin/Debug/net8.0/Archetype.Prototype1.dll"

generated_directory="Archetype.Prototype1/Parsing/Generated"
grammar_directory="Archetype.Prototype1/Parsing/Grammar"

# Generate Grammar
dotnet run --project "$generator_program" "$target_assembly" "$base_grammar" "$grammar_directory"

# Remove the directory and its contents
rm -r "$generated_directory"

# Run antlr4 with specific options
antlr4 "$grammar_directory"/*.g4 -Dlanguage=CSharp -o "$generated_directory"
