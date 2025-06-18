namespace LibraryManagement.Common;

public static class ResponseMessages
{
    public const string NotFound = "{0} not found.";
    public const string FetchSuccess = "{0} fetched successfully.";
}

public static class StringExtensions
{
    public static string With(this string template, params object[] args)
        => string.Format(template, args);
}

