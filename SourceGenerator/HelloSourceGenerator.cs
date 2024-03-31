﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MyInterface;

namespace SourceGenerator;

[Generator]
public class HelloSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // Find the main method
        var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken)
                         ?? throw new Exception("Cannot find entry point when generating code");

        var typeDeclarationSyntaxes = new List<TypeDeclarationSyntax>();
        var syntaxTrees = context.Compilation.SyntaxTrees;
        foreach (var syntaxTree in syntaxTrees)
        {
            var root = syntaxTree.GetRoot();

            typeDeclarationSyntaxes.AddRange(root.DescendantNodes().OfType<ClassDeclarationSyntax>());
            typeDeclarationSyntaxes.AddRange(root.DescendantNodes().OfType<RecordDeclarationSyntax>());
        }

        var interesting1 = typeDeclarationSyntaxes
            .SelectMany(td => td.AttributeLists
                .SelectMany(attr => attr.GetAttributes(context.Compilation))
                .Where(attr => attr?.AttributeClass?.ToDisplayString() == "MyInterface.DtoAttribute")
            );

        var classAndAttribute = typeDeclarationSyntaxes
            .Select(td =>
                (
                    declaringSymbol: td.GetDeclaredSymbol(context.Compilation),
                    attribute: td.AttributeLists.SelectMany(al => al.Attributes.Select(a => a.Name.ToString()))
                )
            );

        // var interesting = typeDeclarationSyntaxes
        //     // .Where(td => td is RecordDeclarationSyntax)
        //     // .Where(rd => rd.AttributeLists.Count >= 1)
        //     // .Select(rd =>( rd.Identifier.ToString(), rd.AttributeLists.First().Attributes.First().Name));
        //     .SelectMany(rd => rd.AttributeLists.SelectMany( al =>
        //         al.Attributes.Where(a => a is DtoAttribute)
        //             .Select(a => ("rd", a.Name))));

        var interesting = context.Compilation.SyntaxTrees
            .SelectMany(st => st
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(r => r.AttributeLists
                    .SelectMany(al => al.Attributes)
                    // It would be better to get to the very type of DtoAttribute
                    // but right no I cannot get a dependency of project type to work
                    // even though looking at
                    // https://github.com/dotnet/roslyn/discussions/47517
                    // and similar, like the Kathleen Dollard code that uses
                    // <TargetPathWithTargetPlatformMoniker Include="$(OutputPath)MyInterface.dll" IncludeRuntimeDependency="false" />
                    // in a nice way.
                    // So until I get MyInterface/DtoAttribute into a nuget package
                    // i use the name. Unfortunately.
                    .Any(a => a.Name.GetText().ToString() == "Dto")));


        var classNames = typeDeclarationSyntaxes.Select(cd => cd.Identifier.Text);
        string classNamesString = string.Join(",", classNames);

        // Build up the source code
        string source = $@"// <auto-generated/>
// At {DateTime.Now:O}
using System;

// classNames: {classNamesString} C7
// Interesting1: 
// {string.Join(",\n// ", interesting1)}
// Interesting2: 
// {string.Join(",\n// ", classAndAttribute)}
// Interesting: 
// {string.Join(",\n// ", interesting)}

namespace {mainMethod.ContainingNamespace.ToDisplayString()}
{{
    public static partial class {mainMethod.ContainingType.Name}
    {{
        static partial void HelloFrom(string name) =>
            Console.WriteLine($""Generator says: Hi from '{{name}} from 8'"");
    }}
}}
";
        var typeName = mainMethod.ContainingType.Name;

        // Add the source code to the compilation
        context.AddSource($"{typeName}.g.cs", source);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
//        throw new Exception("Test exception!"); // delete me after test
        // No initialization required for this one
    }
}