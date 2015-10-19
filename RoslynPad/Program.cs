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
            TestingMethodSymbolVisitor();
            Console.ReadKey();
        }

        public static void TestingSymbolVisitor()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
            class MyClass
            {
                class Nested
                {
                }
                void M()
                {
                }
            }");

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { mscorlib });

            var visitor = new NamedTypeVisitor();
            visitor.Visit(compilation.GlobalNamespace);
        }

        public static void TestingMethodSymbolVisitor()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
            class MyClass
            {
                class Nested
                {
                }
                void M()
                {
                }
            }");

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { mscorlib });

            var visitor = new MethodSymbolVisitor();
            visitor.Visit(compilation.GlobalNamespace);
        }
    }
}
