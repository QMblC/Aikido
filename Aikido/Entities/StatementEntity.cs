using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class StatementEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long SeminarId { get; set; }
        public long CoachId { get; set; }
        public byte[]? StatementFile { get; set; }  

        public StatementEntity() { }

        public StatementEntity(StatementDto statementDto)
        {
            if (statementDto.SeminarId != null)
            {
                SeminarId = statementDto.SeminarId.Value;
            }
            if (statementDto.CoachId != null)
            {
                CoachId = statementDto.CoachId.Value;
            }
            if (statementDto.File != null)
            {
                StatementFile = Convert.FromBase64String(statementDto.File);
            }
            else
            {
                StatementFile = null;
            }
        }

        public StatementEntity(long seminarId, long coachId, byte[] table)
        {
            SeminarId = seminarId;
            CoachId = coachId;
            StatementFile = table;
        }

        public void UpdateStatement(byte[] table)
        {
            StatementFile = table;
        }
    }
}