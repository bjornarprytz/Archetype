grammar BaseGrammar;

cardText:                   name static? effects? abilities?; 

static:                     staticKeyword+;
effects:                    actionBlock;
abilities:                  'ABILITIES' ability+;

ability:                    '{' name static? effects? '}';
actionBlock:                '{' costs? conditions? targets? computedValues? effect+ '}';

computedValues:             '[' (computedValueSpecs (',' )) ']';
computedValueSpecs:         computedValueKeyword;
targets:                    '<' (targetSpecs (',' targetSpecs)*) '>';
targetSpecs:                targetKeyword OPTIONAL?;
conditions:                 (conditionKeyword (',' conditionKeyword)*);
costs:                      (costKeyword (',' costKeyword)*);
effect:                     effectKeyword;

operand:                    any | targetRef | computedValueRef;

targetRef:                  '<' index '>' | SELF;
computedValueRef:           '[' index ']';

staticKeyword:                 /*STATIC_KEYWORD_LIST*/;
targetKeyword:                 /*TARGET_KEYWORD_LIST*/;
conditionKeyword:              /*CONDITION_KEYWORD_LIST*/;
computedValueKeyword:          /*COMPUTED_VALUE_KEYWORD_LIST*/;
costKeyword:                   /*COST_KEYWORD_LIST*/;
effectKeyword:                 /*EFFECT_KEYWORD_LIST*/;
name:                           STRING;
index:                          NUMBER;
any:                            STRING | NUMBER;

STRING: '"' (~["\r\n])* '"';
NUMBER: DIGIT+;
SELF: '~';
OPTIONAL: '?';

fragment DIGIT: [0-9];

WS: [ \t\r\n]+ -> skip; // skip whitespace characters


