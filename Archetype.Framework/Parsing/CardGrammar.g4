grammar CardGrammar;

actionBlock: effect*;

effect: keyword targetSelector* operand* ';';

keyword: ANY;

operand: NUMBER | STRING;

targetSelector: ZONE ('(' filterList ')')?;

filterList: filter (',' filter)*;

filter: filterKey ':' filterValue;

filterKey: ANY;

filterValue: ANY ('|' ANY)*;

ZONE: 'hand' | 'target';
NUMBER: DIGIT+;
STRING: '"' WORD '"';
ANY: WORD;

fragment WORD : [a-zA-Z0-9_]+;
fragment DIGIT: [0-9];

WS: [ \t\r\n]+ -> skip; // skip whitespace characters