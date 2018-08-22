using System;
using Antlr4.Runtime;

namespace IntelliSearch.FlexVersion
{
    public class OutputErrorListener : BaseErrorListener
    {
        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new ArgumentException($"{e}: line {line}/column {charPositionInLine} {msg}");
        }
    }
}