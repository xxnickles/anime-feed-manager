using LanguageExt;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Core.ConstrainedTypes
{
    public class NonEmptyString : Record<NonEmptyString>
    {
        public readonly Option<string> Value;

        public NonEmptyString(string? value)
        {
            Value = !string.IsNullOrWhiteSpace(value) ? Some(value) : None;
        }

        public static NonEmptyString FromString(string? s) => new NonEmptyString(s);
    }
}
