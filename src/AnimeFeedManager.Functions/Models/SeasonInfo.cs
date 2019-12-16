namespace AnimeFeedManager.Functions.Models
{
    public class SeasonInfo
    {
        public string? Season { get; set; }
        public int Year { get; set; } // Azure tables only works with Int and Int64
    }
}