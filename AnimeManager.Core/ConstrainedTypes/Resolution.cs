namespace AnimeFeedManager.Core.ConstrainedTypes
{
    public struct Resolution
    {
        public readonly string Value;

        private Resolution(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }

        public static Resolution Sd = new Resolution("sd");
        public static Resolution Hd = new Resolution("720");
        public static Resolution FullHd = new Resolution("1080");
    }
}
