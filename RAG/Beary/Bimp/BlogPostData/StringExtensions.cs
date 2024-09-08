﻿using System.Text.RegularExpressions;

namespace Bimp.BlogPostData;

internal static class StringExtensions
{
    // Order matters with this list
    private static string[] _patterns = new string[]
    {
        @"\{[^}]*Link:[^|]+\|(.+?)\}",
        @"\{[^}]*Link:(.+?)\}"
    };

    internal static string Clean(this string text)
    {
        // string pattern = @"\{[^}]*Link:(.+?)\}";
        var result = text;

        bool foundMatch = false;

        if (!string.IsNullOrEmpty(result))
            foreach (var pattern in _patterns)
            {
                var matches = Regex.Matches(result, pattern);
                foreach (Match match in matches)
                    if (match.Success)
                    {
                        var textToBeReplaced = match.Value;

                        // I think I want the text to be replaced with an empty string
                        var textToReplaceWith = string.Empty; // "Link:" + match.Groups[1].Value;

                        result = result.Replace(textToBeReplaced, textToReplaceWith);
                        foundMatch = true;
                    }
            }

        return result;
    }
}
