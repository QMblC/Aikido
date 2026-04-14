using Aikido.AdditionalData.Enums;
using Aikido.Entities.Seminar;

namespace Aikido.Dto.Seminars
{
    public class SeminarDto : DtoBase, ISeminarDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }

        public bool? IsFinalStatementApplied { get; set; }

        public long? CreatorId { get; set; }
        public string? CreatorName { get; set; }

        public List<long> Editors { get; set; } = new();

        public bool? RegulationExists { get; set; } = false;

        public DateTime? CreatedTime { get; set; }

        public List<SeminarPriceDto>? SeminarPrices { get; set; }
        public List<SeminarContactInfoDto>? ContactInfo { get; set; }
        public List<SeminarScheduleDto>? TrainingSchedule { get; set; }
        public List<SeminarScheduleDto>? CertificationSchedule { get; set; }
        public List<SeminarGroupDto>? Groups { get; set; }

        public SeminarDto() { }

        public SeminarDto(SeminarEntity seminar)
        {
            Id = seminar.Id;
            Name = seminar.Name;
            Description = seminar.Description;
            Date = seminar.Date;
            Location = seminar.Location;

            CreatorId = seminar.CreatorId;
            CreatorName = seminar.Creator?.FullName;

            Editors = seminar.Editors.Select(u => u.Id).ToList();

            IsFinalStatementApplied = seminar.IsFinalStatementApplied;

            RegulationExists = seminar.RegulationId != null;

            CreatedTime = seminar.CreatedDate;

            if (seminar.ContactInfo != null)
                ContactInfo = seminar.ContactInfo.Select(ci => new SeminarContactInfoDto(ci)).ToList();

            if (seminar.Schedule != null)
            {
                TrainingSchedule = seminar.Schedule.Where(s => s.Type == SeminarScheduleType.Training)
                    .Select(s => new SeminarScheduleDto(s))
                    .ToList();
                CertificationSchedule = seminar.Schedule.Where(s => s.Type == SeminarScheduleType.Certification)
                    .Select(s => new SeminarScheduleDto(s))
                    .ToList();
            }

            if (seminar.Groups != null)
            {
                Groups = seminar.Groups.Select(s => new SeminarGroupDto(s)).ToList();
            }
        }
    }
}
