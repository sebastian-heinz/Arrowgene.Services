using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;

namespace Arrowgene.Services.Scripting
{
    /// <summary>
    /// Used to create instances.
    /// </summary>
    public class ScriptTask
    {
        private readonly ScriptContext _context;
        private List<Diagnostic> _diagnostics;
        private readonly string _name;
        private string _code;
        private object[] _args;

        public List<Diagnostic> Diagnostics => _diagnostics;
        public string Name => _name;

        /// <summary>
        /// Creates a new <see cref="ScriptTask"/> instance.
        /// </summary>
        /// <param name="taskName">Name of the task for identification</param>
        /// <param name="code">Code to create an instance from</param>
        /// <param name="context">Dependencies of the code</param>
        /// <param name="args">Constructor arguments to pass</param>
        public ScriptTask(string taskName, string code, ScriptContext context, object[] args)
        {
            _name = taskName;
            _context = context;
            _diagnostics = new List<Diagnostic>();
            _code = code;
            _args = args;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateInstance<T>()
        {
            return CreateInstance<T>(_args);
        }

        /// <summary>
        /// Creates a new instance with constructor parameters.
        /// </summary>
        /// <param name="args">Constructor arguments to pass</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateInstance<T>(params object[] args)
        {
            return CreateInstance<T>(_code, _context, ref _diagnostics, args);
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="context"></param>
        /// <param name="diagnostics"></param>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateInstance<T>(string code, ScriptContext context, ref List<Diagnostic> diagnostics,
            params object[] args)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            NamespaceDeclarationSyntax ns = GetNamespace(tree);

            tree = RemoveNamespace(tree);
            tree = FixUsingDirectives(tree, context, ns.Name.ToString());

            Script script = CreateScript(tree, context);
            script.Compile();
            MemoryStream stream = new MemoryStream();
            EmitResult emitResult = script.GetCompilation().Emit(stream);
            if (emitResult.Success)
            {
                Assembly assembly = Assembly.Load(stream.ToArray());
                Type[] types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (typeof(T).IsAssignableFrom(type))
                    {
                        T instance = (T) Activator.CreateInstance(type, args);
                        return instance;
                    }
                }
            }
            else
            {
                if (diagnostics != null)
                {
                    diagnostics.Clear();
                    foreach (Diagnostic diagnostic in emitResult.Diagnostics)
                    {
                        diagnostics.Add(diagnostic);
                    }
                }
            }

            return default(T);
        }

        public static SyntaxTree RemoveNamespace(SyntaxTree tree)
        {
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            NamespaceDeclarationSyntax namespaceNode = GetNamespace(tree);

            if (namespaceNode != null)
            {
                root = SyntaxFactory.CompilationUnit(
                    root.Externs,
                    root.Usings,
                    new SyntaxList<AttributeListSyntax>(),
                    namespaceNode.Members
                );
            }

            return root.SyntaxTree;
        }

        public static SyntaxTree FixUsingDirectives(SyntaxTree tree, ScriptContext context, string nameSpace)
        {
            List<string> usingStatements = new List<string>();
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            Compilation compilation = GetCompilation(tree, context);
            SemanticModel semanticModel = compilation.GetSemanticModel(tree);
            foreach (SyntaxNode node in root.DescendantNodes())
            {
                if (node is IdentifierNameSyntax)
                {
                    if (ModelExtensions.GetSymbolInfo(semanticModel, node).Symbol == null)
                    {
                        IdentifierNameSyntax identifier = (IdentifierNameSyntax) node;
                        string value = identifier.Identifier.ValueText;
                        if (context.TypeResolver.ContainsKey(value))
                        {
                            List<string> namespaces = context.TypeResolver[value];
                            string foundNameSpace = null;
                            if (namespaces.Count == 1)
                            {
                                foundNameSpace = namespaces[0];
                            }
                            else if (namespaces.Count > 1)
                            {
                                foundNameSpace = FindNamespace(nameSpace, namespaces);
                            }

                            if (!string.IsNullOrEmpty(foundNameSpace))
                            {
                                if (!usingStatements.Contains(foundNameSpace))
                                {
                                    usingStatements.Add(foundNameSpace);
                                }
                            }
                        }
                    }
                }
            }

            if (usingStatements.Count > 0)
            {
                root = AddUsingDirectives(root, usingStatements);
            }

            return root.SyntaxTree;
        }

        public static Script CreateScript(SyntaxTree tree, ScriptContext context)
        {
            ScriptOptions scriptOptions = ScriptOptions.Default.WithReferences(context.Assemblies);
            Script script = CSharpScript.Create(tree.ToString(), scriptOptions);
            return script;
        }

        public static Compilation GetCompilation(SyntaxTree tree, ScriptContext context)
        {
            string assemblyName = string.Format("{0}_{1}", "ScriptEngineResult", Guid.NewGuid());
            Compilation compilation = CSharpCompilation.Create(assemblyName)
                .WithReferences(context.MetadataReferences)
                .AddSyntaxTrees(tree);
            return compilation;
        }

        public static CompilationUnitSyntax AddUsingDirectives(CompilationUnitSyntax root, List<string> usingStatements)
        {
            if (usingStatements != null)
            {
                List<string> usingStatementsCopy = new List<string>(usingStatements);
                for (int i = 0; i < usingStatements.Count; i++)
                {
                    foreach (UsingDirectiveSyntax usingSyntax in root.Usings)
                    {
                        if (usingStatements[i].ToLower() == usingSyntax.Name.ToFullString().ToLower())
                        {
                            usingStatementsCopy.Remove(usingStatements[i]);
                        }
                    }
                }

                foreach (string usingStatement in usingStatementsCopy)
                {
                    root = root.AddUsings(
                        SyntaxFactory
                            .UsingDirective(SyntaxFactory.IdentifierName(usingStatement))
                            .NormalizeWhitespace()
                            .WithTrailingTrivia(SyntaxFactory.Whitespace("\n"))
                    );
                }
            }

            return root;
        }

        public static NamespaceDeclarationSyntax GetNamespace(SyntaxTree tree)
        {
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            NamespaceDeclarationSyntax namespaceNode = null;
            foreach (SyntaxNode child in root.ChildNodes())
            {
                if (child is NamespaceDeclarationSyntax)
                {
                    namespaceNode = (NamespaceDeclarationSyntax) child;
                    break;
                }
            }

            return namespaceNode;
        }

        public static string FindNamespace(string nameSpace, List<string> canidates)
        {
            foreach (string canidate in canidates)
            {
                if (nameSpace.Contains(canidate))
                {
                    return canidate;
                }
            }

            return null;
        }
    }
}