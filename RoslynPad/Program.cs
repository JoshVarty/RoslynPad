using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using RoslynPad.SymbolVisitorStuff;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoslynPad
{
    class Program
    {
        static void Main(string[] args)
        {
            FindFieldSymbols();
        }

        private static void ChangeTree()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
            class MyClass
            {
                void MyMethod()
                {
                }
            }");

            //We navigate these trees by getting the root, and then
            //searching up and down the tree for the nodes we're interested in.
            var root = tree.GetRoot();
            var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

            //Let's create a new method with a different name
            var newIdentifier = SyntaxFactory.Identifier("MyNewMethodWithADifferentName");
            //NOTE: We're creating a new tree, not changing the old one!
            var newMethod = method.WithIdentifier(newIdentifier);
            Console.WriteLine(newMethod);
        }

        private static void FindFieldSymbols()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
            class MyClass
            {
                int firstVariable, secondVariable;
                string thirdVariable;
            }");

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { mscorlib });

            //Get the semantic model
            //You can also get it from Documents
            var model = compilation.GetSemanticModel(tree);

            var fields = tree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>();
            var declarations = fields.Select(n => n.Declaration.Type);
            foreach (var type in declarations)
            {
                var typeSymbol = model.GetSymbolInfo(type).Symbol as INamedTypeSymbol;
                var fullName = typeSymbol.ToString();
                //Some types like int are special:
                var specialType = typeSymbol.SpecialType;
            }

            var declaredVariables = fields.SelectMany(n => n.Declaration.Variables);
            foreach (var variable in declaredVariables)
            {
                var symbol = model.GetDeclaredSymbol(variable);
                var symbolFullName = symbol.ToString();
            }

        }

        private static void FindField()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
            class MyClass
            {
                int firstVariable, secondVariable;
                string thirdVariable;
            }");

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { mscorlib });

            var fields = tree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>();

            //Get a particular variable in a field
            var second = fields.SelectMany(n => n.Declaration.Variables).Where(n => n.Identifier.ValueText == "secondVariable").Single();
            //Get the type of both of the first two fields.
            var type = fields.First().Declaration.Type;
            //Get the third field
            var third = fields.SelectMany(n => n.Declaration.Variables).Where(n => n.Identifier.ValueText == "thirdVariable").Single();


        }

        private static readonly Lazy<Type> DependentTypeFinder = new Lazy<Type>(() => typeof(SymbolFinder).Assembly.GetType("Microsoft.CodeAnalysis.FindSymbols.DependentTypeFinder"));
        private static readonly Lazy<Func<INamedTypeSymbol, Solution, IImmutableSet<Project>, CancellationToken, Task<IEnumerable<INamedTypeSymbol>>>> FindDerivedClassesAsync
            = new Lazy<Func<INamedTypeSymbol, Solution, IImmutableSet<Project>, CancellationToken, Task<IEnumerable<INamedTypeSymbol>>>>(() => (Func<INamedTypeSymbol, Solution, IImmutableSet<Project>, CancellationToken, Task<IEnumerable<INamedTypeSymbol>>>)Delegate.CreateDelegate(typeof(Func<INamedTypeSymbol, Solution, IImmutableSet<Project>, CancellationToken, Task<IEnumerable<INamedTypeSymbol>>>), DependentTypeFinder.Value.GetMethod("FindDerivedClassesAsync", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)));

        private static void TestingFindDerivedTypes()
        {
            var slnPath = @"C:\Users\JoshVarty\Documents\Visual Studio 2015\Projects\DerivedFromXUnitFact\DerivedFromXUnitFact.sln";
            var ws = MSBuildWorkspace.Create();
            var solution = ws.OpenSolutionAsync(slnPath).Result;

            var props = ws.Properties;
            foreach(var prop in props)
            {

            }

            var proj = solution.Projects.Single();
            var compilation = proj.GetCompilationAsync().Result;
            var factType = compilation.GetTypeByMetadataName("Xunit.FactAttribute");

            var x = FindDerivedClassesAsync.Value(factType, solution, null, CancellationToken.None).Result;

            foreach(var y in x)
            {

            }

        }

        private static void TestingAdHocWorkspace()
        {
            Workspace ws = null;
            var currentSolution = ws.CurrentSolution;

            foreach (var projectId in currentSolution.ProjectIds)
            {
                var project = currentSolution.GetProject(projectId);
                foreach (var documentId in project.DocumentIds)
                {
                    Document doc = project.GetDocument(documentId);
                    var root = doc.GetSyntaxRootAsync().Result;

                    //Rewrite your root here
                    var rewrittenRoot = root;

                    doc = doc.WithSyntaxRoot(root);
                    //Persist your changes to the current project
                    project = doc.Project;
                }
                //Persist the project changes to the current solution
                currentSolution = project.Solution;
            }

            //Now you have your rewritten solution. You can emit the projects to disk one by one if you'd like.
        }

        public static void InvestigatingDebuggerBug()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
using System;
using Xunit;

namespace CodeConnect.TestSolution.TestFrameworkComparison
{
    [CollectionDefinition(""SharedEndToEndCollection"")]
    public class EndToEndCollection : ICollectionFixture<SharedCrossStoreFixture>
        {
            public static void Main()
            {
            }

        }
        public class SharedCrossStoreFixture
        {
        }

        
        public abstract class EndToEndTest<TTestStore, TFixture> : IDisposable
        {
            protected EndToEndTest(TFixture fixture)
            {
            }

            [Fact]
            public virtual void Can_save_changes_and_query()
            {
            }

            public void Dispose()
            {
            }
        }

        [Collection(""SharedEndToEndCollection"")]
        public class SharedInMemoryEndToEndTest : EndToEndTest<string, SharedCrossStoreFixture>
        {
            public SharedInMemoryEndToEndTest(SharedCrossStoreFixture fixture) : base(fixture)
            {
            }
        }

        [Collection(""SharedEndToEndCollection"")]
        public class SharedSqlServerEndToEndTest : EndToEndTest<string, SharedCrossStoreFixture>
        {
            public SharedSqlServerEndToEndTest(SharedCrossStoreFixture fixture) : base(fixture)
            {
            }
        }
    }");
            MetadataReference[] references = new MetadataReference[]
            {
                //Xunit needs this PCL stuff.
                MetadataReference.CreateFromFile(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5\Profile\Profile7\System.Runtime.dll"),
                MetadataReference.CreateFromFile(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5\Profile\Profile7\System.Diagnostics.Debug.dll"),
                MetadataReference.CreateFromFile(typeof(System.Collections.Immutable.ImmutableArray).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Xunit.FactAttribute).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: references);

            var errs = compilation.GetDiagnostics().Where(n => n.Severity == DiagnosticSeverity.Error);
            Debug.Assert(errs.Count() == 0);

            var model = compilation.GetSemanticModel(tree);
            var test = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var classSymbol = model.GetDeclaredSymbol(test);

            var attributeData = classSymbol.GetAttributes().First();

            //Cannot peek into arguments
            var arguments = attributeData.ConstructorArguments;




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
