grammar ActionBlock;

// TODO: Might need something to differentiate cards and abilities
cardText:                   static effects? abilities?; 
abilityText:                abilityName effects;

abilities:                  'abilities: ' abilityRef+;
effects:                    'effects:' actionBlock;

abilityRef:                 '{' abilityName static '}';
actionBlock:                '{' targets? computedValues? keywordExpression* '}';
computedValues:             '(COMPUTE' keywordExpression+ ')';

abilityName:                keyword;
static:                     keywordExpression*;
targets:                    '(TARGETS' targetSpecs+ ')';
targetSpecs:                '<' filters optional? '>';

keywordExpression:          '(' keyword targetRef* operand* ')';
operand:                    NUMBER | STRING | filter | computedValueRef;

targetRef:                  '<' index '>' | SELF;
computedValueRef:           '[' index ']';

filters:                    filterList;
filterList:                 filter ('&' filter)*;
filter:                     (filterKey ':' filterValue);
filterKey:                  keyword;
filterValue:                ANY ('|' ANY)*;
optional:                   '?';

keyword:                    KEYWORD;
index:                      NUMBER;

STRING: '"' (~["\r\n])* '"';
NUMBER: DIGIT+;
SELF: '~';
KEYWORD: WORD;
ANY: (WORD | NUMBER);

fragment WORD: ([a-zA-Z][a-zA-Z_]*);
fragment DIGIT: [0-9];

WS: [ \t\r\n]+ -> skip; // skip whitespace characters


