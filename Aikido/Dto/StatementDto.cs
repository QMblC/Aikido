using Aikido.Entities.Seminar;

namespace Aikido.Dto
{
    public class StatementDto : DtoBase
    {
        public string? Name { get; set; }
        public long? SeminarId { get; set; }
        public long? CoachId { get; set; }
        public string? File { get; set; }

        public StatementDto() { }

        public StatementDto(StatementEntity entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            SeminarId = entity.SeminarId;
            CoachId = entity.CoachId;

            File = entity.StatementPath != null ? Convert.ToBase64String(entity.StatementPath) : null;

        }
    }
}
