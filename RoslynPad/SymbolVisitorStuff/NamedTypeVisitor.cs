using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynPad.SymbolVisitorStuff
{
    public class NamedTypeVisitor : SymbolVisitor
    {
        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            Console.WriteLine(symbol.ToString());
            foreach(var childSymbol in symbol.GetMembers())
            {
                childSymbol.Accept(this);
            }
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            Console.WriteLine(symbol.ToString());
            foreach (var childSymbol in symbol.GetTypeMembers())
            {
                childSymbol.Accept(this);
            }
        }
    }
}
