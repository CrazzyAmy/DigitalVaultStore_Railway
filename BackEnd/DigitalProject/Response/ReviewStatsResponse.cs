namespace DigitalProject.Response
{
    public class ReviewStatsResponse
    {
        public double AverageRating { get; set; }
        public int TotalCount { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
    }
}