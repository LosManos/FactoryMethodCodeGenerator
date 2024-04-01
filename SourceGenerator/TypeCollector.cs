using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

public class TypeCollector : ISyntaxReceiver
{
    public enum IsOfType{
        IsClass = 1,
        IsRecord,
    }

    // Classes and Records
    public IImmutableList<(IsOfType isOfType, TypeDeclarationSyntax @type, IEnumerable<AttributeSyntax>
        attribs)> Types =>
        _types.ToImmutableList();

    private List<(IsOfType isOfType, TypeDeclarationSyntax @type, IEnumerable<AttributeSyntax> attribs)> _types { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclaration)
        {
            _types.Add((IsOfType.IsClass, classDeclaration, classDeclaration.AttributeLists.SelectMany(a=>a.Attributes)));
        } else if (syntaxNode is RecordDeclarationSyntax recordDeclarationSyntax)
        {
            _types.Add((IsOfType.IsRecord, recordDeclarationSyntax, recordDeclarationSyntax.AttributeLists.SelectMany(a => a.Attributes)));
        }
    }
}