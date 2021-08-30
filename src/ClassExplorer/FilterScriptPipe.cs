using System;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace ClassExplorer
{
    internal sealed class FilterScriptPipe : IDisposable
    {
        private readonly SteppablePipeline _pipe;

        private bool _isDisposed;

        private bool _beginInvoked;

        private FilterScriptPipe(SteppablePipeline pipe)
        {
            _pipe = pipe;
        }

        public static FilterScriptPipe Create(ScriptBlock scriptBlock)
        {
            var sbAst = (ScriptBlockAst)scriptBlock.Ast;
            if (!(sbAst.BeginBlock is null && sbAst.EndBlock is null && sbAst.ProcessBlock is not null))
            {
                scriptBlock = ScriptBlockOps.ConvertToProcessBlock(scriptBlock);
            }

            return new FilterScriptPipe(
                ScriptBlock.Create("param(${+script block}) & ${+script block}")
                    .GetSteppablePipeline(CommandOrigin.Internal, new object[] { scriptBlock }));
        }

        public bool IsMatch(object? value)
        {
            if (!_beginInvoked)
            {
                _pipe.Begin(expectInput: true);
                _beginInvoked = true;
            }

            Array output;
            try
            {
                output = _pipe.Process(value);
            }
            catch
            {
                return false;
            }

            if (output is null)
            {
                return false;
            }

            if (output is object[] array && array is { Length: 1 })
            {
                if (array[0] is bool boolValue)
                {
                    return boolValue;
                }

                return LanguagePrimitives.ConvertTo<bool>(array[0]);
            }

            try
            {
                return LanguagePrimitives.ConvertTo<bool>(output);
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _pipe.Dispose();
            _isDisposed = true;
        }
    }
}
