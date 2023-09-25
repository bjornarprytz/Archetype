grammar ActionBlock;

effects:                '{' function* '}';

function:                   '(' keyword targetRef* operand* ')';

operand:                    NUMBER | STRING | filters | computedValueRef;

targetRef:                  '<' index '>' | SELF;
computedValueRef:           '[' index ']';

filters:                 filterList;
filterList:                 filter ('&' filter)*;
filter:                     (filterKey ':' filterValue);
filterKey:                  ANY;
filterValue:                ANY ('|' ANY)*;

keyword:                    ANY;
index:                      NUMBER;

STRING: '"' (~["\r\n])* '"';
NUMBER: DIGIT+;
SELF: '~';
ANY: WORD+;

fragment WORD: [a-zA-Z0-9_]+;
fragment DIGIT: [0-9];

WS: [ \t\r\n]+ -> skip; // skip whitespace characters


