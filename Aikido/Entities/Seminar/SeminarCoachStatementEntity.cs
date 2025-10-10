using Aikido.Services.DatabaseServices.Base;

namespace Aikido.Entities.Seminar
{
    public class SeminarCoachStatementEntity : SeminarStatementEntity
    {
        public long CoachId { get; set; }
        public virtual UserEntity? Coach { get; set; }
    }
}
