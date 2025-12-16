using Aikido.AdditionalData.Enums;
using Aikido.Entities;
using Aikido.Entities.Seminar;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using Aikido.Entities.Users;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Aikido.Dto.Seminars.Members
{
    public class SeminarMemberManagerRequestDto : DtoBase, ISeminarMemberDataDto
    {
        public long UserId { get; set; }
        public string? UserFullName { get; set; } = string.Empty;
        public DateTime? UserBirthday { get; set; }

        public long? SeminarId { get; set; }
        public string? SeminarName { get; set; } = string.Empty;
        public DateTime? SeminarDate { get; set; }

        public long? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? AgeGroup { get; set; }

        public long? CoachId { get; set; }
        public string? CoachName { get; set; }

        public long? ClubId { get; set; }
        public string? ClubName { get; set; }
        public string? ClubCity { get; set; }

        public long? SeminarGroupId { get; set; }
        public string? SeminarGroupName { get; set; }

        public string? OldGrade { get; set; } = string.Empty;
        public string? CertificationGrade { get; set; } = string.Empty;

        public long? ManagerId { get; set; }
        public string? ManagerFullName { get; set; }

        public bool IsSeminarPayed { get; set; } = false;
        public decimal? SeminarPriceInRubles { get; set; }

        public bool IsBudoPassportPayed { get; set; } = false;
        public decimal? BudoPassportPriceInRubles { get; set; }

        public bool IsAnnualFeePayed { get; set; } = false;
        public decimal? AnnualFeePriceInRubles { get; set; }

        public bool IsCertificationPayed { get; set; } = false;
        public decimal? CertificationPriceInRubles { get; set; }

        public bool IsConfirmed { get; set; } = false;
        public string? Note { get; set; }

        public SeminarMemberManagerRequestDto(SeminarEntity seminar, UserMembershipEntity mainUserMembership, List<PaymentEntity> payments)
        {
            UserId = mainUserMembership.UserId;
            UserFullName = mainUserMembership.User?.FullName ?? string.Empty;
            UserBirthday = mainUserMembership.User?.Birthday;

            SeminarId = seminar.Id;
            SeminarName = seminar?.Name ?? string.Empty;
            SeminarDate = seminar?.Date;

            GroupId = mainUserMembership.GroupId;
            GroupName = mainUserMembership.Group?.Name;
            AgeGroup = mainUserMembership.Group != null
                ? EnumParser.ConvertEnumToString(mainUserMembership.Group.AgeGroup)
                : EnumParser.ConvertEnumToString(AdditionalData.Enums.AgeGroup.Adult);

            ClubId = mainUserMembership.ClubId;
            ClubName = mainUserMembership.Club?.Name;
            ClubCity = mainUserMembership.Club?.City;

            OldGrade = mainUserMembership.User?.Grade.ToString();
            CertificationGrade = Grade.None.ToString();

            CoachId = mainUserMembership.Group?.UserMemberships.FirstOrDefault(um => um.RoleInGroup == Role.Coach)?.UserId;
            CoachName = mainUserMembership.Group?.UserMemberships.FirstOrDefault(um => um.RoleInGroup == Role.Coach)?.User?.FullName;

            ManagerId = mainUserMembership.Club?.ManagerId;
            ManagerFullName = mainUserMembership.Club?.Manager?.FullName;

            IsConfirmed = false;

            if (payments.Any(p => p.Type == PaymentType.Seminar))
            {
                SeminarPriceInRubles = payments
                    .Where(p => p.Type == PaymentType.Seminar)
                    .Select(p => p.Amount)
                    .First();

                IsSeminarPayed = payments
                    .Where(p => p.Type == PaymentType.Seminar)
                    .Select(p => p.Status == PaymentStatus.Completed)
                    .FirstOrDefault();
            }
            if (payments.Any(p => p.Type == PaymentType.AnnualFee))
            {
                AnnualFeePriceInRubles = payments
                    .Where(p => p.Type == PaymentType.AnnualFee)
                    .Select(p => p.Amount)
                    .First();

                IsAnnualFeePayed = payments
                    .Where(p => p.Type == PaymentType.AnnualFee)
                    .Select(p => p.Status == PaymentStatus.Completed)
                    .FirstOrDefault();
            }
            if (payments.Any(p => p.Type == PaymentType.BudoPassport))
            {
                BudoPassportPriceInRubles = payments
                    .Where(p => p.Type == PaymentType.BudoPassport)
                    .Select(p => p.Amount)
                    .First();

                IsBudoPassportPayed = payments
                    .Where(p => p.Type == PaymentType.BudoPassport)
                    .Select(p => p.Status == PaymentStatus.Completed)
                    .FirstOrDefault();
            }
            if (payments.Any(p => p.Type == PaymentType.Certification))
            {
                CertificationPriceInRubles = payments
                    .Where(p => p.Type == PaymentType.Certification)
                    .Select(p => p.Amount)
                    .First();

                IsCertificationPayed = payments
                    .Where(p => p.Type == PaymentType.Certification)
                    .Select(p => p.Status == PaymentStatus.Completed)
                    .FirstOrDefault();
            }
        }

        public SeminarMemberManagerRequestDto(SeminarMemberManagerRequestEntity member, List<PaymentEntity> payments)
        {
            Id = member.Id;
            UserId = member.UserId;
            UserFullName = member.User?.FullName ?? string.Empty;
            UserBirthday = member.User?.Birthday;

            SeminarId = member.SeminarId;
            SeminarName = member.Seminar?.Name ?? string.Empty;
            SeminarDate = member.Seminar?.Date;

            GroupId = member.GroupId;
            GroupName = member.Group?.Name;
            AgeGroup = member.Group != null
                ? EnumParser.ConvertEnumToString(member.Group.AgeGroup)
                : EnumParser.ConvertEnumToString(AdditionalData.Enums.AgeGroup.Adult);

            ClubId = member.ClubId;
            ClubName = member.Club?.Name;
            ClubCity = member.Club?.City;

            SeminarGroupId = member.SeminarGroupId;
            SeminarGroupName = member.SeminarGroup?.Name;

            OldGrade = member.OldGrade.ToString();
            CertificationGrade = member.CertificationGrade.ToString();

            CoachId = member.CoachId;
            CoachName = member.Coach?.FullName;

            ManagerId = member.ManagerId;
            ManagerFullName = member.Manager?.FullName;

            Note = member.Note;
            IsConfirmed = member.IsConfirmed;

            if (payments.Any(p => p.Type == PaymentType.Seminar))
            {
                SeminarPriceInRubles = payments
                    .Where(p => p.Type == PaymentType.Seminar)
                    .Select(p => p.Amount)
                    .First();

                IsSeminarPayed = payments
                    .Where(p => p.Type == PaymentType.Seminar)
                    .Select(p => p.Status == PaymentStatus.Completed)
                    .FirstOrDefault();
            }
            if (payments.Any(p => p.Type == PaymentType.AnnualFee))
            {
                AnnualFeePriceInRubles = payments
                    .Where(p => p.Type == PaymentType.AnnualFee)
                    .Select(p => p.Amount)
                    .First();

                IsAnnualFeePayed = payments
                    .Where(p => p.Type == PaymentType.AnnualFee)
                    .Select(p => p.Status == PaymentStatus.Completed)
                    .FirstOrDefault();
            }
            if (payments.Any(p => p.Type == PaymentType.BudoPassport))
            {
                BudoPassportPriceInRubles = payments
                    .Where(p => p.Type == PaymentType.BudoPassport)
                    .Select(p => p.Amount)
                    .First();

                IsBudoPassportPayed = payments
                    .Where(p => p.Type == PaymentType.BudoPassport)
                    .Select(p => p.Status == PaymentStatus.Completed)
                    .FirstOrDefault();
            }
            if (payments.Any(p => p.Type == PaymentType.Certification))
            {
                CertificationPriceInRubles = payments
                    .Where(p => p.Type == PaymentType.Certification)
                    .Select(p => p.Amount)
                    .First();

                IsCertificationPayed = payments
                    .Where(p => p.Type == PaymentType.Certification)
                    .Select(p => p.Status == PaymentStatus.Completed)
                    .FirstOrDefault();
            }
        }
    }
}
