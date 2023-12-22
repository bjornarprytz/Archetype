grammar BaseGrammar;

cardText:                   name static* actionBlock? abilities?; 

abilities:                  'ABILITIES' ability+;

ability:                    '{' name actionBlock '}';
actionBlock:                '{' targets? computedValues? cost* condition* effect+ '}';

computedValues:             '[' (computedValue (',' computedValue)*) ']';
targets:                    '<' (targetSpecs (',' targetSpecs)*) '>';
targetSpecs:                target OPTIONAL?;

targetRef:                  '<' index '>';
computedValueRef:           '[' index ']';

static:                     /*STATIC_KEYWORD_LIST*/;
target:                     /*TARGET_KEYWORD_LIST*/;
condition:                  /*CONDITION_KEYWORD_LIST*/;
computedValue:              /*COMPUTED_VALUE_KEYWORD_LIST*/;
cost:                       /*COST_KEYWORD_LIST*/;
effect:                     /*EFFECT_KEYWORD_LIST*/;

name:                       STRING;
index:                      NUMBER;
opString:                   STRING;
opNumber:                   NUMBER | computedValueRef;
opAtom:                     targetRef | SELF;

STRING: '"' (~["\r\n])* '"';
NUMBER: DIGIT+;
SELF: '~';
OPTIONAL: '?';

fragment DIGIT: [0-9];

WS: [ \t\r\n]+ -> skip; // skip whitespace characters


