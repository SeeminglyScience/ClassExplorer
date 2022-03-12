using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Runtime.CompilerServices;

namespace ClassExplorer
{
    public sealed class RangeExpression
    {
        private readonly int _start;

        private readonly int _end;

        public RangeExpression(int value)
        {
            _start = value;
            _end = value;
        }

        public RangeExpression(int start, int end)
        {
            _start = start;
            _end = end;
        }

        public static RangeExpression Parse(string expression)
        {
            if (TryParse(expression, out RangeExpression? range))
            {
                return range;
            }

            ThrowInvalidFormat();
            return null!;

            [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
            static void ThrowInvalidFormat()
            {
                throw new PSArgumentException(
                    SR.RangeExpressionInvalidFormat,
                    nameof(expression));
            }
        }

        public static bool TryParse(string expression, [NotNullWhen(true)] out RangeExpression? range)
        {
            ReadOnlySpan<char> span = expression.AsSpan();
            int dotPosition = span.IndexOf('.');
            if (dotPosition is -1)
            {
                if (!Poly.TryParseInt32(span, out int singleResult))
                {
                    goto fail;
                }

                range = new(singleResult);
                return true;
            }

            if (dotPosition is 0)
            {
                if (expression.Length is < 3)
                {
                    goto fail;
                }

                ReadOnlySpan<char> remaining = span[2..];
                if (!Poly.TryParseInt32(remaining, out int result))
                {
                    goto fail;
                }

                range = new(0, result);
                return true;
            }

            ReadOnlySpan<char> start = span[..dotPosition];
            if (!Poly.TryParseInt32(start, out int startResult))
            {
                goto fail;
            }

            if (dotPosition == span.Length - 1)
            {
                goto fail;
            }

            if (span[dotPosition + 1] != '.')
            {
                goto fail;
            }

            if (dotPosition + 2 == span.Length)
            {
                range = new(startResult, -1);
                return true;
            }

            ReadOnlySpan<char> end = span[(dotPosition + 2)..];
            if (!Poly.TryParseInt32(end, out int endResult))
            {
                goto fail;
            }

            range = new(startResult, endResult);
            return true;

fail:
            range = null;
            return false;
        }

        public bool IsInRange(int value)
        {
            if (_start == _end)
            {
                return value == _start;
            }

            if (_end is -1)
            {
                return value >= _start;
            }

            return value >= _start && value <= _end;
        }
    }
}
