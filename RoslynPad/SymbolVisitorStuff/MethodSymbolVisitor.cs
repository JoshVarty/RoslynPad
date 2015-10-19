using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynPad.SymbolVisitorStuff
{
    public class MethodSymbolVisitor : SymbolVisitor
    {
        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach(var child in symbol.GetMembers())
            {
                child.Accept(this);
            }
        }
        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            foreach(var child in symbol.GetMembers())
            {
                child.Accept(this);
            }
        }

        public override void VisitMethod(IMethodSymbol symbol)
        {
            Console.WriteLine(symbol);
        }
    }
}
