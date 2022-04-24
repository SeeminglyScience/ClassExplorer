using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace ClassExplorer
{
    internal static class ScriptBlockOps
    {
        public static ScriptBlock ConvertToProcessBlock(ScriptBlock scriptBlock)
        {
            ScriptBlockAst sbAst = (ScriptBlockAst)scriptBlock.Ast;
            ScriptBlockAst newSbAst = new(
                scriptBlock.Ast.Extent,
                paramBlock: null,
                beginBlock: null,
                processBlock: new NamedBlockAst(
                    sbAst.EndBlock.Extent,
                    TokenKind.Process,
                    new StatementBlockAst(
                        sbAst.EndBlock.Extent,
                        sbAst.EndBlock.Statements.Select(s => s.Copy()).Cast<StatementAst>(),
                        Enumerable.Empty<TrapStatementAst>()),
                    unnamed: false),
                endBlock: null,
                dynamicParamBlock: null);

            return newSbAst.GetScriptBlock();
        }
    }
}
