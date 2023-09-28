grammar ActionBlock;

// TODO: Might need something to differentiate cards and abilities
cardText:                   name static? effects? abilities?; 

name:                       '(NAME' STRING ')';
static:                     '(STATIC' keywordExpression+ ')';
effects:                    '(EFFECTS' actionBlock ')';
abilities:                  '(ABILITIES' ability+ ')';

ability:                    '{' abilityName actionBlock '}';
actionBlock:                '{' costs? conditions? targets? computedValues? keywordExpression+ '}';

computedValues:             '(COMPUTE' keywordExpression+ ')';
abilityName:                keyword;
targets:                    '(TARGETS' targetSpecs+ ')';
targetSpecs:                '<' filters OPTIONAL? '>';
conditions:                 '(CONDITIONS' keywordExpression+ ')';
costs:                      '(COSTS' keywordExpression+ ')';

keywordExpression:          '(' keyword targetRef* operand* ')';
operand:                    NUMBER | STRING | filter | computedValueRef;

targetRef:                  '<' index '>' | SELF;
computedValueRef:           '[' index ']';

filters:                    filterList;
filterList:                 filter ('&' filter)*;
filter:                     (filterKey ':' filterValue);
filterKey:                  keyword;
filterValue:                ANY ('|' ANY)*;

keyword:                    KEYWORD;
index:                      NUMBER;

STRING: '"' (~["\r\n])* '"';
NUMBER: DIGIT+;
SELF: '~';
OPTIONAL: '?';
KEYWORD: WORD;
ANY: (WORD | NUMBER);

fragment WORD: ([a-zA-Z][a-zA-Z_]*);
fragment DIGIT: [0-9];

WS: [ \t\r\n]+ -> skip; // skip whitespace characters


