grammar ActionBlock;

actionBlock: '{' computedValueDeclaration? targetDeclaration? keywords '}';

computedValueDeclaration: '[COMPUTED' computedValue* ']' ';';
targetDeclaration: '<TARGETS' cardSelector '>' ';';
keywords: (effect | targetProvider)*;


targetProvider: '<' keyword '>' operand* cardSelector ';';
effect: keyword target* operand* ';';

computedValue: valueKey ':' gameStateValue | eventValue;
cardSelector: cardFilters*;
target: '<' index '>';

gameStateValue: cardFilters '#' aggregate;
eventValue: EVENT_SPAN  filterList '#' aggregate;


valueKey: ANY;
keyword: ANY;
index: NUMBER;
aggregate: 'count'; // TODO: Add more aggregates


cardFilters: '(' filterList ')';
filterList: filter (',' filter)*;
filter: (filterKey ':' filterValue) | (propertyFilter);
filterKey: keyword;
filterValue: ANY ('|' ANY)*;
propertyFilter: ('zone:' ZONE);

operand: NUMBER | STRING | computedValueRef;
computedValueRef: '[' valueKey ']';

ZONE: 'hand' | 'deck' | 'discard' | 'node' | 'exile' | 'any';
EVENT_SPAN: 'current' | 'turn' | 'game';

STRING: '"' (~["\r\n])* '"';
NUMBER: DIGIT+;
ANY: WORD+;

fragment WORD: [a-zA-Z0-9_]+;
fragment DIGIT: [0-9];

WS: [ \t\r\n]+ -> skip; // skip whitespace characters


