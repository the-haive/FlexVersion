grammar Output;     
 
start
	:  (expr)* ; 

expr 
	: expr expr 
	| VAR 
	| FUNC '(' commaexpr ')' 
	| text
	; 

commaexpr
	: WS? expr WS?
	| commaexpr ',' commaexpr
	;

text: TEXT+ ;
 
FUNC: '$' ID ; 
VAR : '<' ID '>' ;
fragment ID : [a-zA-Z] | [a-zA-Z][a-zA-Z0-9:]+ ; 
TEXT: .+?;
NL : [\r\n]+ -> skip ; 
WS: [ ]+ ;
