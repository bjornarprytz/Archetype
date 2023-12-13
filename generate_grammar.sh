#!/bin/bash

gen_dir="Archetype.Grammar/generated"
grammar_dir="$gen_dir/grammar"
parser_dir="$gen_dir/parser"

generator="Archetype.GrammarGenerator"
base_grammar="$generator/template/BaseGrammar.template.g4"
target_assembly="$generator/bin/Debug/net8.0/Archetype.Prototype1.dll" # TODO: Change this to the correct path

# Clear previous generated files
if [ -d "$gen_dir" ]; then
    # Remove the directory and its contents
    rm -r "$gen_dir"
fi

dotnet build "$generator" || exit

# Generate Grammar
dotnet run --project "$generator" "$target_assembly" "$base_grammar" "$grammar_dir" || exit

# Remove the directory and its contents


# Run antlr4 with specific options
antlr4 "$grammar_dir"/*.g4 -Dlanguage=CSharp -o "$parser_dir"
