using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DescribableEnumAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DescribableEnumAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor MissingDescriptionRule =
            new DiagnosticDescriptor(
                id: "DEA001",
                title: "Enum member missing Description attribute",
                messageFormat: "Enum member '{0}' in a [DescribableEnum] must have a [Description] attribute",
                category: "Design",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor DescribableEnumIncompleteRule =
            new DiagnosticDescriptor(
                id: "DEA002",
                title: "Enum marked with [DescribableEnum] but not all members have [Description]",
                messageFormat: "Enum '{0}' is marked with [DescribableEnum] but not all members have [Description] attributes",
                category: "Design",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor NonDescribableEnumUsageRule =
            new DiagnosticDescriptor(
                id: "DEA003",
                title: "Non-describable enum used in WithVersion<T>()",
                messageFormat: "Enum '{0}' used as generic parameter in WithVersion<T> must have [DescribableEnum]",
                category: "Design",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(MissingDescriptionRule, DescribableEnumIncompleteRule, NonDescribableEnumUsageRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // Analyze all enum types to validate describable enums
            context.RegisterSymbolAction(AnalyzeEnum, SymbolKind.NamedType);

            // Analyze method calls for WithVersion<T>()
            context.RegisterSyntaxNodeAction(AnalyzeGenericMethodUsage, Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
        }

        private void AnalyzeEnum(SymbolAnalysisContext context)
        {
            var enumSymbol = (INamedTypeSymbol)context.Symbol;
            if (enumSymbol.TypeKind != TypeKind.Enum)
                return;

            var hasDescribable = enumSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "DescribableEnumAttribute");
            if (!hasDescribable)
                return; // Only check enums marked with [DescribableEnum]

            var members = enumSymbol.GetMembers().OfType<IFieldSymbol>().Where(f => f.HasConstantValue).ToList();
            bool allHaveDescription = true;

            foreach (var member in members)
            {
                var hasDescription = member.GetAttributes().Any(a => a.AttributeClass?.Name == "DescriptionAttribute");
                if (!hasDescription)
                {
                    allHaveDescription = false;
                    context.ReportDiagnostic(Diagnostic.Create(MissingDescriptionRule, member.Locations.FirstOrDefault(), member.Name));
                }
            }

            if (!allHaveDescription)
            {
                context.ReportDiagnostic(Diagnostic.Create(DescribableEnumIncompleteRule, enumSymbol.Locations.FirstOrDefault(), enumSymbol.Name));
            }
        }

        private void AnalyzeGenericMethodUsage(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;

            if (methodSymbol == null || !methodSymbol.IsGenericMethod)
                return;

            if (methodSymbol.Name != "WithVersion")
                return;

            var typeArg = methodSymbol.TypeArguments.FirstOrDefault();
            if (typeArg == null || typeArg.TypeKind != TypeKind.Enum)
                return;

            var hasDescribable = typeArg.GetAttributes().Any(a => a.AttributeClass?.Name == "DescribableEnumAttribute");
            if (!hasDescribable)
            {
                context.ReportDiagnostic(Diagnostic.Create(NonDescribableEnumUsageRule, invocation.GetLocation(), typeArg.Name));
            }
        }
    }
}
