using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

partial class SourceCodeBuilder
{
    public (string source, string namespaceName, string recordName) BuildMapRecord(
        SourceProductionContext spc,
        RecordDeclarationSyntax syntax)
    {
        var @namespace = GetNameSpace(syntax);

        var record = CreateMapRecord(spc, syntax);

        var namespaceDeclaration =
            SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(@namespace.Name.ToString()))
                .AddMembers(record);

        var unit = SyntaxFactory.CompilationUnit()
            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
            .AddMembers(namespaceDeclaration);

//        var sourceCode = $"public record RemoveMe;";

        return (unit.NormalizeWhitespace().ToFullString(), @namespace.Name.ToString(), syntax.Identifier.ToString());
    }

    /// <summary>Creates the record containing the mapping methods.
    /// Crude example:
    ///     public abstract partial record Mapping
    ///     {
    ///         public static CopyTarget CopySource_To_CopyTarget(CopySource source)
    ///         {
    ///             return CopyTarget.Create(source.Id, source.Name);
    ///         }
    ///     }
    /// </summary>
    private static RecordDeclarationSyntax CreateMapRecord(
        SourceProductionContext spc,
        RecordDeclarationSyntax syntax)
    {
        var methodModifiers = SyntaxFactory.TokenList(
            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
            SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        var name = syntax.Identifier.Text;

        var attributes = syntax.AttributeLists.GetMapAttributes()
            .Select(a => (name: a.Name, sourceType: a.ArgumentList?.Arguments.First()));

        var methods = attributes.Select((attrib, index) =>
                SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.ParseTypeName(GetTargetTypeName(attrib.name)),
                    GetSourceTypeName(attrib.name) + "_To_" + GetTargetTypeName(attrib.name))
                    .WithModifiers(methodModifiers)
                    .WithParameterList(CreateParameters(attrib.name))
                    .WithBody(SyntaxFactory.Block(CreateBody(attributes)))
                )
            .ToArray();

        var method = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.ParseTypeName("void"),
            "MyMethodName");

        var recordModifiers = SyntaxFactory.TokenList(
            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
            SyntaxFactory.Token(SyntaxKind.AbstractKeyword),
            SyntaxFactory.Token(SyntaxKind.PartialKeyword)
        );

        var res = SyntaxFactory.RecordDeclaration(
                SyntaxKind.RecordDeclaration,
                SyntaxFactory.Token(SyntaxKind.RecordKeyword),
                SyntaxFactory.Identifier(name))
            .WithModifiers(recordModifiers)
            .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken))
            .AddMembers(methods);

        return res;

        ParameterListSyntax CreateParameters(NameSyntax ns)
        {
            var parameters = SyntaxFactory.ParameterList()
                .AddParameters(
                    CreateParameter(
                        PropertyInfo.Create(
                            "source",
                            SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(GetSourceTypeName(ns))),
                            string.Empty)));
            return parameters;
        }

        string GetSourceTypeName(NameSyntax ns)
        {
            var gns = (ns as GenericNameSyntax);
            var arg = gns?.TypeArgumentList.Arguments[0];
            var ins = (arg as IdentifierNameSyntax)?.Identifier.Text;
            return ins ?? throw new Exception($"Error in {nameof(GetSourceTypeName)}.");
        }

        string GetTargetTypeName(NameSyntax ns)
        {
            var gns = (ns as GenericNameSyntax);
            var arg = gns?.TypeArgumentList.Arguments[1];
            var ins = (arg as IdentifierNameSyntax)?.Identifier.Text;
            return ins ?? throw new Exception($"Error in {nameof(GetSourceTypeName)}.");
        }

        // Creates a body like
        // {
        //     return CopyTarget.Create(source.Id, source.Name);
        // }
        StatementSyntax CreateBody(IEnumerable<(NameSyntax name, AttributeArgumentSyntax? sourceType)> attributeData)
        {
            // var result = CopyTarget.Create(...
            var assignment = SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName("var result"),
                SyntaxFactory.IdentifierName(GetTargetTypeName(attributeData.Single().name) + ".Create(" + "1" + ")")
            ));

            var body = SyntaxFactory.Block(
                assignment
            );

            // var body = SyntaxFactory.Block(
            //     // Return. E.g.: return new MyDto(a,b,c);
            //     SyntaxFactory.ReturnStatement(
            //         SyntaxFactory.ObjectCreationExpression(
            //             SyntaxFactory.ParseTypeName(constructorInfo.Name)
            //         ).WithArgumentList(arguments)
            //     ));
            return body.AddStatements(SyntaxFactory.ParseStatement($"return result;"));
        }
    }
}