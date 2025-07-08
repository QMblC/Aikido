using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class EventEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long? UserId { get; set; }
        public DateTime? PublishDate { get; set; }
        public string? City { get; set; }
        public string? Title { get; set; }
        public DateTime? EventDate { get; set; }
        public string? Description { get; set; }
        public byte[] File { get; set; } = [];

        public async Task UpdateFromJson(EventDto eventNewData)
        {
            UserId = eventNewData.UserId;
            PublishDate = eventNewData.PublishDate;
            City = eventNewData.City;
            Title = eventNewData.Title;
            EventDate = eventNewData.EventDate;
            Description = eventNewData.Description;

            if (eventNewData.File != null && eventNewData.File.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await eventNewData.File.CopyToAsync(memoryStream);
                File = memoryStream.ToArray();
            }
        }
    }
}