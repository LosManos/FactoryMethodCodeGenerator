using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// https://gist.github.com/cmendible/9b8c7d7598f1ab0bc7ab5d24b2622622

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

    public IImmutableList<(IsOfType isOfType, TypeDeclarationSyntax @type, IEnumerable<AttributeSyntax>
        attribs)> TypesWithAttribute(params string[] attributeNames)
    {
        return _types.Where(t => t.attribs.Any(a => attributeNames.Contains(a.Name.ToString())))
            .ToImmutableList();
    }

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