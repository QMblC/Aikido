using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class StatementEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public string? Name { get; set; }
        public long SeminarId { get; set; }
        public long CoachId { get; set; }
        public string? StatementPath { get; set; }  

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
                StatementPath = Convert.FromBase64String(statementDto.File);
            }
            else
            {
                StatementPath = null;
            }
            Name = statementDto.Name;
        }

        public StatementEntity(long seminarId, long coachId, byte[] table, string name)
        {
            SeminarId = seminarId;
            CoachId = coachId;
            StatementPath = table;
            Name = name;
        }

        public void UpdateStatement(byte[] table, string name)
        {
            StatementPath = table;
            Name = name;
        }
    }
}