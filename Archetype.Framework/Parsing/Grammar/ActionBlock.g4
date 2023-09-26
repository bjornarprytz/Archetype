grammar ActionBlock;

cardText:                   effects;

effects:                    '{' targets? computedValues? function* '}';

targets:                    '(TARGETS' ('<' filters '?'? '>')+ ')';
computedValues:             '(COMPUTE' function* ')';
function:                   '(' keyword targetRef* operand* ')';

operand:                    NUMBER | STRING | filters | computedValueRef;

targetRef:                  '<' index '>' | SELF;
computedValueRef:           '[' index ']';

filters:                    filterList;
filterList:                 filter ('&' filter)*;
filter:                     (filterKey ':' filterValue);
filterKey:                  keyword;
filterValue:                ANY ('|' ANY)*;

keyword:                    ANY;
index:                      NUMBER;

STRING: '"' (~["\r\n])* '"';
NUMBER: DIGIT+;
SELF: '~';
KEYWORD: WORD;
ANY: (WORD | NUMBER);

fragment WORD: ([a-zA-Z][a-zA-Z_]*);
fragment DIGIT: [0-9];

WS: [ \t\r\n]+ -> skip; // skip whitespace characters


