
public class TrainerRequestStatsDto
{
    public long TrainerId { get; set; }
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int AppliedRequests { get; set; }
    public int AddRequests { get; set; }
    public int UpdateRequests { get; set; }
    public int DeleteRequests { get; set; }
}