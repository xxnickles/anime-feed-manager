using LanguageExt;
using System.Text.RegularExpressions;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Core.ConstrainedTypes;

public class Email : Record<Email>
{
    private const string EmailPattern =
        @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
    public readonly Option<string> Value;


    public Email(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            var validation = Regex.Match(value, EmailPattern);
            Value = validation.Success ? Some(value) : None;
        }
        else
        {
            Value = None;
        }
         
    }

    public static Email FromString(string value) => new Email(value);
}