using Aikido.Entities;
using Aikido.Entities.Seminar;

namespace Aikido.Dto
{
    public class SeminarMemberDto : DtoBase
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public long SeminarId { get; set; }
        public string SeminarName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? RegistrationDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? Amount { get; set; }
        public bool IsPaid { get; set; }
        public string? Notes { get; set; }

        // Дополнительная информация о пользователе
        public string? Grade { get; set; }
        public List<string>? ClubNames { get; set; }
        public string? CoachName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public int? Age { get; set; }
        public string? SpecialRequirements { get; set; }
        public bool NeedsAccommodation { get; set; }
        public string? EmergencyContact { get; set; }

        public SeminarMemberDto() { }

        public SeminarMemberDto(UserEntity user, ClubEntity? club, UserEntity? coach)
        {
            UserId = user.Id;
            UserName = user.FullName;
            UserFullName = user.FullName;
            Grade = user.Grade.ToString();
            ClubNames = club != null ? new List<string> { club.Name } : new List<string>();
            CoachName = coach?.FullName;
            PhoneNumber = user.PhoneNumber;
            Birthday = user.Birthday;

            if (user.Birthday.HasValue)
            {
                Age = DateTime.Now.Year - user.Birthday.Value.Year;
                if (DateTime.Now < user.Birthday.Value.AddYears(Age.Value))
                    Age--;
            }
        }

        public SeminarMemberDto(SeminarMemberEntity member, UserEntity user)
        {
            Id = member.Id;
            UserId = member.UserId;
            UserName = user.FullName;
            UserFullName = user.FullName;
            SeminarId = member.SeminarId;
            SeminarName = member.Seminar?.Name ?? string.Empty;
            Status = member.Status.ToString();
            RegistrationDate = member.RegistrationDate;
            PaymentDate = member.PaymentDate;
            Amount = member.Amount;
            IsPaid = member.IsPaid;
            Notes = member.Notes;
            Grade = user.Grade.ToString();
            PhoneNumber = user.PhoneNumber;
            Birthday = user.Birthday;
            SpecialRequirements = member.SpecialRequirements;
            NeedsAccommodation = member.NeedsAccommodation;
            EmergencyContact = member.EmergencyContact;

            if (user.Birthday.HasValue)
            {
                Age = DateTime.Now.Year - user.Birthday.Value.Year;
                if (DateTime.Now < user.Birthday.Value.AddYears(Age.Value))
                    Age--;
            }

            // Заполняем клубы пользователя
            ClubNames = user.UserMemberships?.Where(um => um.Club != null)
                                     .Select(uc => uc.Club!.Name)
                                     .ToList() ?? new List<string>();
        }
    }
}