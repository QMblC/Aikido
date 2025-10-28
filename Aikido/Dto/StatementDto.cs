using Aikido.Dto.Seminars.Members;
using Aikido.Entities;

namespace Aikido.Dto
{
    public class StatementDto : DtoBase
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public long? UserId { get; set; }
        public string? UserName { get; set; }
        public long? ClubId { get; set; }
        public string? ClubName { get; set; }
        public long? GroupId { get; set; }
        public string? GroupName { get; set; }
        public List<SeminarMemberDto> Members { get; set; } = new();
        public string? Notes { get; set; }
        public string? FilePath { get; set; }

        public StatementDto() { }

        public StatementDto(StatementEntity statement)
        {
            Id = statement.Id;
            Title = statement.Title ?? string.Empty;
            CreatedDate = statement.CreatedDate;
            SubmittedDate = statement.SubmittedDate;
            Status = statement.Status?.ToString() ?? string.Empty;
            Type = statement.Type?.ToString() ?? string.Empty;
            UserId = statement.UserId;
            ClubId = statement.ClubId;
            GroupId = statement.GroupId;
            Notes = statement.Notes;
            FilePath = statement.FilePath;
        }
    }
}
