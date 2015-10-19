using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynPad.SymbolVisitorStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynPad
{
    class Program
    {
        static void Main(string[] args)
        {
            TestingSymbolVisitor();
            Console.ReadKey();
        }

        public static void TestingSymbolVisitor()
        {
            var visitor = new NamedTypeVisitor();

            var tree = CSharpSyntaxTree.ParseText(@"
            class C
            {
                void M()
                {
                }
            }");

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { mscorlib });


            visitor.Visit(compilation.GlobalNamespace);
        }
    }
}
