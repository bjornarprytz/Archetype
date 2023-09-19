grammar ActionBlock;

actionBlock:                '{' computedValueDeclaration? targetDeclaration? keywords '}';

computedValueDeclaration:   '[COMPUTED' computedValue* ']' ';';
targetDeclaration:          '<TARGETS' cardSelector '>' ';';
keywords:                   (effect | targetProvider)*;


targetProvider:             '<' keyword '>' operand* cardSelector ';';
effect:                     keyword target* operand* ';';

computedValue:              keyword operand* ';';
cardSelector:               cardFilters*;
target:                     '<' index '>';
computedValueRef:           '[' index ']';


cardFilters:                '(' filterList ')';
filterList:                 filter (',' filter)*;
filter:                     (filterKey ':' filterValue);
filterKey:                  keyword;
filterValue:                ANY ('|' ANY)*;

keyword:                    ANY;
index:                      NUMBER;
operand:                    NUMBER | STRING | computedValueRef;

STRING: '"' (~["\r\n])* '"';
NUMBER: DIGIT+;
ANY: WORD+;

fragment WORD: [a-zA-Z0-9_]+;
fragment DIGIT: [0-9];

WS: [ \t\r\n]+ -> skip; // skip whitespace characters


