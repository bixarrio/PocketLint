using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;
using System.Collections.Immutable;
using System.Linq;

namespace PocketLint.SourceGenerators
{
    [Generator]
    public class ComponentRegistryGenerator : IIncrementalGenerator
    {
        #region Public Methods
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var scriptTypes = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => IsScriptSubclass(node),
                    transform: (ctx, _) => GetScriptType(ctx))
                .Where(t => t != null)
                .Collect();

            context.RegisterSourceOutput(scriptTypes, GenerateComponentRegistrySource);
        }
        #endregion

        #region Private Methods
        private static bool IsScriptSubclass(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax classDecl &&
                   classDecl.BaseList?.Types.Any(t => t.Type.ToString().Contains("Script")) == true;
        }

        private static INamedTypeSymbol GetScriptType(GeneratorSyntaxContext ctx)
        {
            var classDecl = (ClassDeclarationSyntax)ctx.Node;
            var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDecl);
            if (symbol?.BaseType?.ToString() == "PocketLint.Core.Components.Script")
            {
                return symbol;
            }
            return null;
        }

        private static void GenerateComponentRegistrySource(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> scriptTypes)
        {
            if (scriptTypes.IsEmpty)
            {
                return;
            }

            var template = Template.Parse(CodeTemplates.ComponentRegisterGeneratedTemplate);
            context.AddSource("ComponentRegistryGenerated.cs", template.Render(new { names = scriptTypes.Select(t => t.ToString()) }));
        }

        #endregion
    }
}