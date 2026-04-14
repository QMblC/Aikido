
using Aikido.Entities.Seminar;

namespace Aikido.Dto.Seminars
{
    public class SeminarShortDto : DtoBase, ISeminarDto
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }

        public SeminarShortDto() { }

        public SeminarShortDto(SeminarEntity seminar)
        {
            Id = seminar.Id;
            Name = seminar.Name;
            Date = seminar.Date;
            Location = seminar.Location;
            Description = seminar.Description;
        }
    }
}
