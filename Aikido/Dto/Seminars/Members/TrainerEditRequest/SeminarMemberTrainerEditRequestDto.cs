using Aikido.AdditionalData.Enums;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using Aikido.Entities.Users;

namespace Aikido.Dto.Seminars.Members.TrainerEditRequest
{
    /// <summary>
    /// DTO для просмотра заявки (для тренера и менеджера)
    /// </summary>
    public class SeminarMemberTrainerEditRequestDto
    {
        public long Id { get; set; }
        public long SeminarId { get; set; }
        public long TrainerId { get; set; }
        public string? TrainerName { get; set; }
        public long ClubId { get; set; }
        public string? ClubName { get; set; }

        public long UserId { get; set; }
        public string? UserName { get; set; }
        public long? GroupId { get; set; }
        public string? GroupName { get; set; }
        public long? CoachId { get; set; }
        public string? CoachName { get; set; }
        public long? SeminarGroupId { get; set; }

        public Grade OldGrade { get; set; }
        public Grade? CertificationGrade { get; set; }
        public string? Note { get; set; }

        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ManagerComment { get; set; }
        public string? ManagerName { get; set; }
        public bool IsApplied { get; set; }

        public SeminarMemberTrainerEditRequestDto() { }

        public SeminarMemberTrainerEditRequestDto(SeminarMemberTrainerEditRequestEntity entity)
        {
            Id = entity.Id;
            SeminarId = entity.SeminarId;
            TrainerId = entity.TrainerId;
            TrainerName = entity.Trainer?.FirstName + " " + entity.Trainer?.LastName;
            ClubId = entity.ClubId;
            ClubName = entity.Club?.Name;

            UserId = entity.UserId;
            UserName = entity.User?.FirstName + " " + entity.User?.LastName;
            GroupId = entity.GroupId;
            GroupName = entity.Group?.Name;
            CoachId = entity.CoachId;
            CoachName = entity.Coach?.FirstName + " " + entity.Coach?.LastName;
            SeminarGroupId = entity.SeminarGroupId;

            CertificationGrade = entity.CertificationGrade;
            Note = entity.Note;

            Status = entity.Status.ToString();
            CreatedAt = entity.CreatedAt;
            ReviewedAt = entity.ReviewedAt;
            ManagerComment = entity.ManagerComment;
            ManagerName = entity.Manager?.FirstName + " " + entity.Manager?.LastName;
            IsApplied = entity.IsApplied;
        }
    }
}