using Aikido.Entities;

namespace Aikido.Dto
{
    public class UserClubDto : DtoBase
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? UserName { get; set; }
        public long ClubId { get; set; }
        public string? ClubName { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime? LeaveDate { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public string? MembershipType { get; set; }
        public decimal? MembershipFee { get; set; }
        public DateTime? LastPaymentDate { get; set; }

        public UserClubDto() { }

        public UserClubDto(UserClub userClub)
        {
            Id = userClub.Id;
            UserId = userClub.UserId;
            UserName = userClub.User?.FullName;
            ClubId = userClub.ClubId;
            ClubName = userClub.Club?.Name;
            JoinDate = userClub.JoinDate;
            LeaveDate = userClub.LeaveDate;
            IsActive = userClub.IsActive;
            Notes = userClub.Notes;
            MembershipType = userClub.MembershipType;
            MembershipFee = userClub.MembershipFee;
            LastPaymentDate = userClub.LastPaymentDate;
        }
    }
}