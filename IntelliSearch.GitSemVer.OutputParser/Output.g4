grammar Output;     
 
start
	:  (expr)* ; 

expr 
	: expr expr 
	| variable
	| function
	| text
	;

variable
	: VAR
	;

function
	: FUNC '(' commaexpr ')' 
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
