namespace Aikido.Dto
{
    public class EventDto : DtoBase
    {
        public long? UserId { get; set; }
        public DateTime? PublishDate { get; set; }
        public string? City { get; set; }
        public string? Title { get; set; }
        public DateTime? EventDate { get; set; }
        public string? Description { get; set; }
        public IFormFile? File { get; set; }
    }
}