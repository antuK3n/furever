namespace FurEver.Web.Models.ViewModels;

public class MyAdoptionListViewModel
{
    public List<Adoption> Adoptions { get; set; } = new();
    public string Status { get; set; } = "All";

    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
    public int ReturnedCount { get; set; }
}
