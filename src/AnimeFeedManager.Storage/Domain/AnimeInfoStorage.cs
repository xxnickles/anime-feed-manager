using Microsoft.Azure.Cosmos.Table;
using System;

namespace AnimeFeedManager.Storage.Domain
{
    public class AnimeInfoStorage : TableEntity
    {
        public string? Title { get; set; }
        public string? Synopsis { get; set; }
        public string? FeedTitle { get; set; }
        public ushort Year { get; set; }
        public string? Season { get; set; }
        public DateTime? Date { get; set; }
    }

    public class AnimeInfoWithImageStorage : AnimeInfoStorage
    {
        public string? ImageUrl { get; set; }
    }
}
