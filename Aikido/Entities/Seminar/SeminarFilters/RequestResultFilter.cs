namespace Aikido.Entities.Seminar.SeminarFilters
{
    public class RequestResultFilter
    {
        public bool IsPending { get; set; } = true;
        public bool IsReviewed { get; set; } = true;

        public RequestResultFilter() { }
    }
}
