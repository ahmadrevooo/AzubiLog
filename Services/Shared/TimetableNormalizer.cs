namespace AzubiLog.Services.Shared;

public static class TimetableNormalizer
{
    public static string NormalizeSchool(string school) => school.Trim().ToUpperInvariant();

    public static string NormalizeClassName(string className) => className.Trim().ToUpperInvariant();

    public static (string School, string ClassName) Normalize(string school, string className)
        => (NormalizeSchool(school), NormalizeClassName(className));
}
