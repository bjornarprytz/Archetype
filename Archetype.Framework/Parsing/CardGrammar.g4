grammar CardGrammar;

actionBlock: keywords;

keywords: (effect | targetProvider)*;

targetProvider: '<' keyword '>' operand* cardSelector ';';
effect: keyword target* operand* ';';

target: '<' index '>';

cardSelector: cardFilter*;
cardFilter: '(' filterList ')';
filterList: filter (',' filter)*;
filter: filterKey ':' filterValue;
filterKey: keyword;
filterValue: ANY ('|' ANY)*;

keyword: ANY;
operand: NUMBER | STRING;
index: NUMBER;

STRING: '"' (~["\r\n])* '"';
NUMBER: DIGIT+;
ANY: WORD+;

fragment WORD: [a-zA-Z0-9_]+;
fragment DIGIT: [0-9];

WS: [ \t\r\n]+ -> skip; // skip whitespace characters