using System;
using System.Reflection;

namespace ClassExplorer.Signatures
{
    internal sealed class RefSignature : TypeSignature
    {
        internal RefSignature(RefKind kind, ITypeSignature element)
        {
            Poly.Assert(element is not null);
            Kind = kind;
            Element = element;
        }

        internal RefKind Kind { get; }

        internal ITypeSignature Element { get; }

        private static bool DoesRefKindMatch(RefKind kind, ParameterInfo parameter)
        {
            if (!parameter.ParameterType.IsByRef)
            {
                return false;
            }

            if (kind is RefKind.AnyRef)
            {
                return true;
            }

            if (parameter.IsOut)
            {
                return kind is RefKind.Out;
            }

            if (parameter.IsDefined("System.Runtime.CompilerServices.IsReadOnlyAttribute"))
            {
                return kind is RefKind.In;
            }

            if (parameter.Position is -1)
            {
                if (parameter.IsDefined("System.Runtime.CompilerServices.IsReadOnlyAttribute"))
                {
                    return kind is RefKind.In;
                }

                if (kind is RefKind.In)
                {
                    return false;
                }
            }

            return kind is RefKind.Ref;
        }

        private static bool DoesRefKindMatch(RefKind kind, Type type)
        {
            if (!type.IsByRef)
            {
                return false;
            }

            if (kind is not RefKind.Ref or RefKind.AnyRef)
            {
                return false;
            }

            return true;
        }

        public override bool IsMatch(ParameterInfo parameter)
        {
            if (!DoesRefKindMatch(Kind, parameter))
            {
                return false;
            }

            Type elementType = parameter.ParameterType.GetElementType()!;
            return Element.IsMatch(elementType);
        }

        public override bool IsMatch(Type type)
        {
            if (!DoesRefKindMatch(Kind, type))
            {
                return false;
            }

            Type elementType = type.GetElementType()!;
            return Element.IsMatch(elementType);
        }
    }
}
