using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            NamespaceDeclaration(ParseName(@namespace.Name.ToString()))
                .AddMembers(record);

        var unit = CompilationUnit()
            .AddUsings(UsingDirective(ParseName("System")))
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
        var methodModifiers = TokenList(
            Token(SyntaxKind.PublicKeyword),
            Token(SyntaxKind.StaticKeyword));
        var name = syntax.Identifier.Text;

        var attributes = syntax.AttributeLists.GetMapAttributes()
            .Select(a => (name: a.Name, sourceType: a.ArgumentList?.Arguments.First()));

        var methods = attributes.Select((attrib, index) =>
                MethodDeclaration(
                    ParseTypeName(GetTargetTypeName(attrib.name)),
                    GetSourceTypeName(attrib.name) + "_To_" + GetTargetTypeName(attrib.name))
                    .WithModifiers(methodModifiers)
                    .WithParameterList(CreateParameters(attrib.name))
                    .WithBody(Block(CreateBody(attributes)))
                )
            .ToArray();

        var method = MethodDeclaration(
            ParseTypeName("void"),
            "MyMethodName");

        var recordModifiers = TokenList(
            Token(SyntaxKind.PublicKeyword),
            Token(SyntaxKind.AbstractKeyword),
            Token(SyntaxKind.PartialKeyword)
        );

        var res = RecordDeclaration(
                SyntaxKind.RecordDeclaration,
                Token(SyntaxKind.RecordKeyword),
                Identifier(name))
            .WithModifiers(recordModifiers)
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken))
            .AddMembers(methods);

        return res;

        ParameterListSyntax CreateParameters(NameSyntax ns)
        {
            var parameters = ParameterList()
                .AddParameters(
                    CreateParameter(
                        PropertyInfo.Create(
                            "source",
                            IdentifierName(Identifier(GetSourceTypeName(ns))),
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
            var createCall =
                InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName( GetTargetTypeName(attributeData.Single().name)),
                            IdentifierName("Create")))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList<ArgumentSyntax>(
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        Literal(42))))));

            var @var = VariableDeclaration(
                IdentifierName(
                    Identifier(
                        TriviaList(),
                        SyntaxKind.VarKeyword,
                        "var",
                        "var",
                        TriviaList())));

            // var result = CopyTarget.Create(...
            var assignment = ExpressionStatement(AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                IdentifierName("var result"),
                createCall
            ));

            var body = Block(
                assignment
            );

            // var body = SyntaxFactory.Block(
            //     // Return. E.g.: return new MyDto(a,b,c);
            //     SyntaxFactory.ReturnStatement(
            //         SyntaxFactory.ObjectCreationExpression(
            //             SyntaxFactory.ParseTypeName(constructorInfo.Name)
            //         ).WithArgumentList(arguments)
            //     ));
            return body.AddStatements(ParseStatement($"return result;"));
        }
    }
}