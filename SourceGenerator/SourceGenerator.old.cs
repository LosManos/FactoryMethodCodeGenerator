using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

public class SourceGenerator2
{
    public IEnumerable<(string className, string methodName)> FindClassesImplementingMethod(Compilation compilation, string methodName)
    {
        // Get the syntax tree and root node
        var syntaxTree = compilation.SyntaxTrees.First();
        var root = syntaxTree.GetRoot();

        // Get the semantic model
        var model = compilation.GetSemanticModel(syntaxTree);

        // Find all class declarations
        var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

        var ret = new List<(string className, string methodName)>();


        foreach (var classDeclaration in classDeclarations)
        {
            // Get the class symbol
            var classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamespaceSymbol;

            // Check if the class implements the method
            var methodImplementations = classSymbol.GetMembers(methodName).OfType<IMethodSymbol>();
            if (methodImplementations.Any())
            {
                // This class implements the method
                System.Console.WriteLine($"Class {classSymbol.Name} implements method {methodName}");
                ret.Add((classSymbol.Name, methodName));
            }
        }

        return ret;
    }
}