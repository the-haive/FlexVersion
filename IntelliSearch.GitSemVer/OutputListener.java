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
}