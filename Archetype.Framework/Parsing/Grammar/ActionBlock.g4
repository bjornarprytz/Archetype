grammar ActionBlock;

cardText:                   static effects? abilities?;

static:                     keywordExpression*;
abilities:                  'abilities: ' actionBlock+;
effects:                    'effects:' actionBlock;

actionBlock:                '{' targets? computedValues? keywordExpression* '}';
targets:                    '(TARGETS' ('<' filters '?'? '>')+ ')';
computedValues:             '(COMPUTE' keywordExpression* ')';
keywordExpression:          '(' keyword targetRef* operand* ')';

operand:                    NUMBER | STRING | filter | computedValueRef;

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


