grammar Output;

start
  : expr* EOF
  ;

expr
 : function
 //| expr ( MULT | DIV ) expr
 //| expr ( PLUS | MINUS ) expr
 | variable
 | text
 ;

function
 : FUNC '(' params ')';

variable
 : VAR;

params
 : expr+ ( ',' expr+ )*
 ;

text
 : OTHER+
 ;

FUNC      : '$' ID;
VAR       : '<' ID '>';
OPEN_PAR  : '(';
CLOSE_PAR : ')';
COMMA     : ',';
OTHER     : . ;

fragment ID : [_a-zA-Z] [_a-zA-Z0-9:]* ;
