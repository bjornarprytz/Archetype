#!/bin/bash

gen_dir="Archetype.Grammar/generated"
grammar_dir="$gen_dir/grammar"
parser_dir="$gen_dir/parser"

generator="Archetype.GrammarGenerator"
base_grammar="$generator/template/BaseGrammar.template.g4"
target_assembly="$generator/bin/Debug/net8.0/Archetype.Prototype1.dll" # TODO: Change this to the correct path


dotnet build "$generator" || exit


# Generate Grammar
dotnet run --project "$generator" "$target_assembly" "$base_grammar" "$grammar_dir" || exit



# Run antlr4 with specific options
antlr4 "$grammar_dir"/*.g4 -Dlanguage=CSharp -o "$parser_dir"
