// Generated from Output.g4 by ANTLR 4.7.1
import org.antlr.v4.runtime.tree.ParseTreeVisitor;

/**
 * This interface defines a complete generic visitor for a parse tree produced
 * by {@link OutputParser}.
 *
 * @param <T> The return type of the visit operation. Use {@link Void} for
 * operations with no return type.
 */
public interface OutputVisitor<T> extends ParseTreeVisitor<T> {
	/**
	 * Visit a parse tree produced by {@link OutputParser#start}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitStart(OutputParser.StartContext ctx);
	/**
	 * Visit a parse tree produced by {@link OutputParser#expr}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitExpr(OutputParser.ExprContext ctx);
	/**
	 * Visit a parse tree produced by {@link OutputParser#commaexpr}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitCommaexpr(OutputParser.CommaexprContext ctx);
	/**
	 * Visit a parse tree produced by {@link OutputParser#text}.
	 * @param ctx the parse tree
	 * @return the visitor result
	 */
	T visitText(OutputParser.TextContext ctx);
}