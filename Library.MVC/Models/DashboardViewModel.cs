namespace Library.MVC.Models
{
    public class DashboardViewModel
    {
        // Statistics
        public int TotalInspectionsThisMonth { get; set; }
        public int FailedInspectionsThisMonth { get; set; }
        public int OverdueOpenFollowUps { get; set; }

        // Filters
        public string? SelectedTown { get; set; }
        public string? SelectedRiskRating { get; set; }

        // Lists for the Filter Dropdowns
        public List<string> Towns { get; set; } = new();
        public List<string> RiskRatings { get; set; } = new() { "Low", "Medium", "High" };
    }
}