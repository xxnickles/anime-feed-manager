using System.Text.RegularExpressions;
using LanguageExt;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Core.ConstrainedTypes;

public record Email 
{
    private const string EmailPattern =
        @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";

    public readonly Option<string> Value;

    public Email(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Value = IsEmail(value) ? Some(value) : None;
        }
        else
        {
            Value = None;
        }
    }

    public static bool IsEmail(string value) =>  Regex.Match(value, EmailPattern).Success;
    

    public static Email FromString(string value) => new(value);
}