using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

internal partial class SourceCodeBuilder
{
    /// <summary>Creates a DTO class.
    ///
    /// A Build...-method takes whatever Roslyn-ish is needed
    /// and distills the needed data for creating the result
    /// with as little as Roslyn knowledge as possible.
    /// So avoid passing <see cref="SourceProductionContext"/>, <see cref="ClassDeclarationSyntax"/> along.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="syntax"></param>
    /// <returns></returns>
    public ( string source, string namespaceName, string recordName) BuildDtoClass(
        SourceProductionContext _,
        ClassDeclarationSyntax syntax)
    {
        var @namespace = GetNameSpace(syntax);
        var members = GetProperties(syntax);
        var usePrivateConstructor = GetUsePrivateConstructor(syntax.AttributeLists);

        var @class = CreateDtoClass(RecordOrClassInfo.Create(
            syntax.Identifier.Text,
            members.Select(PropertyInfo.Create),
            usePrivateConstructor));

        var namespaceDeclaration =
            SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(@namespace.Name.ToString()))
                .AddMembers(@class);

        var unit = SyntaxFactory.CompilationUnit()
            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
            .AddMembers(namespaceDeclaration);

        return (unit.NormalizeWhitespace().ToFullString(), @namespace.Name.ToString(), syntax.Identifier.ToString());
    }

    private ClassDeclarationSyntax CreateDtoClass(RecordOrClassInfo recordOrClassInfo)
    {
        var constructorInfo = ConstructorInfo.Create(recordOrClassInfo.Name, recordOrClassInfo.Properties, recordOrClassInfo.IsPrivateConstructor);
        var constructor = CreateDtoConstructor(constructorInfo);

        var factoryMethod = CreateDtoFactoryMethod(constructorInfo);

        return SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(recordOrClassInfo.Name))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddMembers(constructor, factoryMethod);
    }

    /// <summary>Creates a DTO record.
    ///
    /// A Build...-method takes whatever Roslyn-ish is needed
    /// and distills the needed data for creating the result
    /// with as little as Roslyn knowledge as possible.
    /// So avoid passing <see cref="SourceProductionContext"/>, <see cref="ClassDeclarationSyntax"/> along.
    ///
    /// </summary>
    /// <param name="_"></param>
    /// <param name="syntax"></param>
    /// <returns></returns>
    public (string source, string namespaceName, string recordName) BuildDtoRecord(
        SourceProductionContext _,
        RecordDeclarationSyntax syntax)
    {
        var @namespace = GetNameSpace(syntax);
        var members = GetProperties(syntax);
        var usePrivateConstructor = GetUsePrivateConstructor(syntax.AttributeLists);

        var record = CreateDtoRecord(RecordOrClassInfo.Create(
            syntax.Identifier.Text,
            members.Select(PropertyInfo.Create),
            usePrivateConstructor));

        var namespaceDeclaration =
            SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(@namespace.Name.ToString()))
                .AddMembers(record);

        var unit = SyntaxFactory.CompilationUnit()
            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
            .AddMembers(namespaceDeclaration);

        return (unit.NormalizeWhitespace().ToFullString(), @namespace.Name.ToString(), syntax.Identifier.ToString());
    }

        /// <summary>Create a constructor taking a list of parameters
    /// and updating properties of the same name.
    /// </summary>
    /// <param name="constructorInfo"></param>
    /// <returns></returns>
    private static ConstructorDeclarationSyntax CreateDtoConstructor(ConstructorInfo constructorInfo)
    {
        var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(constructorInfo.IsPrivateConstructor
            ? SyntaxKind.PrivateKeyword
            : SyntaxKind.PublicKeyword));

        var parameters = CreateParameterList(constructorInfo.Properties);

        var body = SyntaxFactory.Block(constructorInfo.Properties.Select(propertyInfo =>
            // Assignment. E.g.: this.MyProperty = MyProperty;
            SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName("this." + propertyInfo.Name),
                SyntaxFactory.IdentifierName(propertyInfo.Name)))
        ));

        var ret = SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(constructorInfo.Name))
            .WithModifiers(modifiers)
            .WithParameterList(parameters)
            .WithBody(body);
        return ret;
    }

    private static MethodDeclarationSyntax CreateDtoFactoryMethod(ConstructorInfo constructorInfo)
    {
        var modifiers = SyntaxFactory.TokenList(
            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
            SyntaxFactory.Token(SyntaxKind.StaticKeyword));

        var parameters = CreateParameterList(constructorInfo.Properties);
        var arguments = CreateArgumentList(constructorInfo.Properties);

        var body = SyntaxFactory.Block(
            // Return. E.g.: return new MyDto(a,b,c);
            SyntaxFactory.ReturnStatement(
                SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.ParseTypeName(constructorInfo.Name)
                ).WithArgumentList(arguments)
            ));

        var ret = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(constructorInfo.Name),
                "Create")
            .WithModifiers(modifiers)
            .WithParameterList(parameters)
            .WithBody(body);
        return ret;
    }

    private static RecordDeclarationSyntax CreateDtoRecord(RecordOrClassInfo recordOrClassInfo)
    {
        var propertiesAsString = string.Join(",", recordOrClassInfo.Properties.Select(p => p.Text));

        var constructorInfo = ConstructorInfo.Create(recordOrClassInfo.Name, recordOrClassInfo.Properties, recordOrClassInfo.IsPrivateConstructor);

        var constructor = CreateDtoConstructor(constructorInfo);

        var factoryMethod = CreateDtoFactoryMethod(constructorInfo);

        var res = SyntaxFactory.RecordDeclaration(
                SyntaxKind.RecordDeclaration,
                SyntaxFactory.Token(SyntaxKind.RecordKeyword),
                SyntaxFactory.Identifier(recordOrClassInfo.Name))
            .WithModifiers(SyntaxTokenList.Create(
                SyntaxFactory.Token(SyntaxKind.PartialKeyword)
            ))
            .WithLeadingTrivia(CreateSingleLineComment($"Properties: {propertiesAsString}"))
            .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken))
            .AddMembers(constructor, factoryMethod);
        return res;
    }
}