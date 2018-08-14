// Generated from Output.g4 by ANTLR 4.7.1
import org.antlr.v4.runtime.tree.ParseTreeListener;

/**
 * This interface defines a complete listener for a parse tree produced by
 * {@link OutputParser}.
 */
public interface OutputListener extends ParseTreeListener {
	/**
	 * Enter a parse tree produced by {@link OutputParser#start}.
	 * @param ctx the parse tree
	 */
	void enterStart(OutputParser.StartContext ctx);
	/**
	 * Exit a parse tree produced by {@link OutputParser#start}.
	 * @param ctx the parse tree
	 */
	void exitStart(OutputParser.StartContext ctx);
	/**
	 * Enter a parse tree produced by {@link OutputParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterExpr(OutputParser.ExprContext ctx);
	/**
	 * Exit a parse tree produced by {@link OutputParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitExpr(OutputParser.ExprContext ctx);
	/**
	 * Enter a parse tree produced by {@link OutputParser#variable}.
	 * @param ctx the parse tree
	 */
	void enterVariable(OutputParser.VariableContext ctx);
	/**
	 * Exit a parse tree produced by {@link OutputParser#variable}.
	 * @param ctx the parse tree
	 */
	void exitVariable(OutputParser.VariableContext ctx);
	/**
	 * Enter a parse tree produced by {@link OutputParser#function}.
	 * @param ctx the parse tree
	 */
	void enterFunction(OutputParser.FunctionContext ctx);
	/**
	 * Exit a parse tree produced by {@link OutputParser#function}.
	 * @param ctx the parse tree
	 */
	void exitFunction(OutputParser.FunctionContext ctx);
	/**
	 * Enter a parse tree produced by {@link OutputParser#commaexpr}.
	 * @param ctx the parse tree
	 */
	void enterCommaexpr(OutputParser.CommaexprContext ctx);
	/**
	 * Exit a parse tree produced by {@link OutputParser#commaexpr}.
	 * @param ctx the parse tree
	 */
	void exitCommaexpr(OutputParser.CommaexprContext ctx);
	/**
	 * Enter a parse tree produced by {@link OutputParser#text}.
	 * @param ctx the parse tree
	 */
	void enterText(OutputParser.TextContext ctx);
	/**
	 * Exit a parse tree produced by {@link OutputParser#text}.
	 * @param ctx the parse tree
	 */
	void exitText(OutputParser.TextContext ctx);
}