namespace AnimeFeedManager.Features.Seasons.Types
{
    public readonly struct SeasonType
    {
        public string Value { get; }

        private SeasonType(string value)
        {
            Value = value;
        }

        public override string ToString() => Value;
        public static implicit operator string(SeasonType seasonType) => seasonType.Value;

        public bool IsLatest() => Value == "Latest";

        public static SeasonType Latest => new("Latest");
        public static SeasonType Season => new("Season");
    }
}