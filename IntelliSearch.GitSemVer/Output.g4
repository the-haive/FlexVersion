grammar Output;		

start:	(expr)* ;
expr : expr expr
	| VAR
	| FUNC '(' expr ')'
	| STRING
	;

FUNC: '$' STRING ;
VAR : '<' STRING '>';
STRING : [a-zA-Z0-9:]+ ;
WS : [ \t\r\n]+ -> skip ;
