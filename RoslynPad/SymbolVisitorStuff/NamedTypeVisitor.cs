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
        int tabCount = 0;
        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            if (IsUnimportantSymbol(symbol))
                return;

            PrettyPrint(symbol);
            tabCount++;
            foreach(var childSymbol in symbol.GetMembers())
            {
                childSymbol.Accept(this);
            }
            tabCount--;
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            if (IsUnimportantSymbol(symbol))
                return;

            PrettyPrint(symbol);
            tabCount++;
            foreach (var childSymbol in symbol.GetTypeMembers())
            {
                childSymbol.Accept(this);
            }
            tabCount--;
        }

        private void PrettyPrint(ISymbol symbol)
        {
            string tabs = new string('\t', tabCount);
            Console.WriteLine(tabs + symbol.ToString());
        }

        private bool IsUnimportantSymbol(ISymbol symbol)
        {
            return symbol.Name.StartsWith("System")
             || symbol.Name.StartsWith("Microsoft")
             || symbol.Name.StartsWith("<")
             ;
        }
    }
}
