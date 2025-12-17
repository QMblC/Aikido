using Aikido.AdditionalData.Enums;
using Aikido.Entities;
using Aikido.Entities.Seminar;

public interface ISeminarMemberData
{
    public long SeminarId { get; }
    public SeminarEntity Seminar { get; set; }
    public long UserId { get; }
    public long? GroupId { get; }
    public long? ClubId { get; }
    public long? CoachId { get; }
    public long? SeminarGroupId { get; }
    public Grade OldGrade { get; }
    public Grade? CertificationGrade { get; }
    public string? Note { get; }

    //public ICollection<PaymentEntity> AllPayments { get; set; }
}
