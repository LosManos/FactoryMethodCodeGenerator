using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

internal record struct MappingInfos
{
    internal string RecordName { get; }

    internal IEnumerable<MappingInfo> Infos { get; }

    private MappingInfos(string recordName, IEnumerable<MappingInfo> infos)
    {
        RecordName = recordName;
        Infos = infos;
    }

    internal static MappingInfos Create(string recordName, IEnumerable<MappingInfo> mappingInfos)
    {
        return new MappingInfos(recordName, mappingInfos);
    }
}

internal record struct MappingInfo
{
    internal string MethodName { get; }

    internal string SourceTypeName { get; }

    internal string TargetTypeName { get; }

    // TODO:OF:Get rid of AttributeArgumentSyntax - use something custom, not Roslyn.
    internal AttributeArgumentSyntax? SourceType { get; }

    private MappingInfo(string methodName, string sourceTypeName, string targetTypeName, AttributeArgumentSyntax? sourceType)
    {
        MethodName = methodName;
        SourceTypeName = sourceTypeName;
        TargetTypeName = targetTypeName;
        SourceType = sourceType;
    }

    internal static MappingInfo Create(string methodName, string sourceTypeName, string targetTypeName, AttributeArgumentSyntax? sourceType)
    {
        return new MappingInfo(methodName, sourceTypeName, targetTypeName, sourceType);
    }
}