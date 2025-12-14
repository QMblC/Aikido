using Aikido.AdditionalData;
using Aikido.Entities;
using Aikido.Entities.Seminar;
using Aikido.Entities.Seminar.SeminarMember;
using Aikido.Entities.Users;

namespace Aikido.Dto.Seminars.Members
{
    public class SeminarMemberDto : DtoBase
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
        public string CoachName { get; set; }

        public long? ClubId { get; set; }
        public string? ClubName { get; set; }
        public string? ClubCity { get; set; }

        public long? SeminarGroupId { get; set; }
        public string? SeminarGroupName { get; set; }

        public string? OldGrade { get; set; } = string.Empty;
        public string? CertificationGrade { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

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

        public SeminarMemberDto() { }

        public SeminarMemberDto(
            UserMembershipEntity membership,
            SeminarEntity seminar,
            bool isAnnualFeePayed = false)
        {
            UserId = membership.UserId;
            UserFullName = membership.User?.FullName ?? string.Empty;

            SeminarId = seminar.Id;
            SeminarName = seminar.Name;
            SeminarDate = seminar.Date;

            GroupId = membership.GroupId;
            GroupName = membership.Group?.Name;

            OldGrade = EnumParser.ConvertEnumToString(membership.User.Grade);

            SeminarPriceInRubles = seminar.SeminarPriceInRubles;
            BudoPassportPriceInRubles = membership.User.HasBudoPassport ? null : seminar.BudoPassportPriceInRubles;
            AnnualFeePriceInRubles = isAnnualFeePayed ? null : seminar.AnnualFeePriceInRubles;
        }

        public SeminarMemberDto(SeminarMemberEntity seminarMember)
        {
            Id = seminarMember.Id;
            UserId = seminarMember.UserId;
            UserFullName = seminarMember.User?.FullName ?? string.Empty;

            SeminarId = seminarMember.SeminarId;
            SeminarName = seminarMember.Seminar?.Name ?? string.Empty;
            SeminarDate = seminarMember.Seminar?.Date;

            GroupId = seminarMember.GroupId;
            GroupName = seminarMember.Group?.Name;
            AgeGroup = seminarMember.Group != null 
                ? EnumParser.ConvertEnumToString(seminarMember.Group.AgeGroup) 
                : EnumParser.ConvertEnumToString(AdditionalData.AgeGroup.Adult);

            ClubId = seminarMember?.ClubId;
            ClubName = seminarMember?.Club?.Name;
            ClubCity = seminarMember?.Club.City;

            SeminarGroupId = seminarMember.SeminarGroupId;
            SeminarGroupName = seminarMember.SeminarGroup?.Name;

            OldGrade = seminarMember.OldGrade.ToString();
            CertificationGrade = seminarMember.CertificationGrade.ToString();
            Status = seminarMember.Status.ToString();

            IsSeminarPayed = seminarMember.SeminarPayment != null 
                ? seminarMember.SeminarPayment.Status == PaymentStatus.Completed 
                : false;
            IsBudoPassportPayed = seminarMember.BudoPassportPayment != null 
                ? seminarMember.BudoPassportPayment.Status == PaymentStatus.Completed 
                : false;
            IsAnnualFeePayed = seminarMember.AnnualFeePayment != null 
                ? seminarMember.AnnualFeePayment.Status == PaymentStatus.Completed 
                : false;
            IsCertificationPayed = seminarMember.CertificationPayment != null 
                ? seminarMember.CertificationPayment.Status == PaymentStatus.Completed 
                : false;

            SeminarPriceInRubles = seminarMember.SeminarPayment?.Amount;
            AnnualFeePriceInRubles = seminarMember.AnnualFeePayment?.Amount;
            BudoPassportPriceInRubles = seminarMember.BudoPassportPayment?.Amount;
            CertificationPriceInRubles = seminarMember.CertificationPayment?.Amount;

            CoachId = seminarMember.CoachId;
            CoachName = seminarMember.Coach?.FullName;


        }
    }
}
