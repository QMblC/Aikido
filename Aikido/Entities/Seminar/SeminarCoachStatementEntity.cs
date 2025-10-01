using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class SeminarCoachStatementEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public string? Name { get; set; }

        public long SeminarId { get; set; }
        public long CoachId { get; set; }

        // Хранение файла в бинарном виде
        public byte[]? StatementFile { get; set; }

        // А также хранение пути к файлу, если нужно
        public string? FilePath { get; set; }

        public SeminarCoachStatementEntity() { }

        public SeminarCoachStatementEntity(StatementDto statementDto)
        {
            if (statementDto.Id != 0)
                Id = statementDto.Id;

            Name = statementDto.Title; 

            FilePath = statementDto.FilePath;
        }

        public SeminarCoachStatementEntity(long seminarId, long coachId, byte[] fileContent, string name, string? filePath = null)
        {
            SeminarId = seminarId;
            CoachId = coachId;
            StatementFile = fileContent;
            Name = name;
            FilePath = filePath;
        }

        public void UpdateStatement(byte[] fileContent, string name, string? filePath = null)
        {
            StatementFile = fileContent;
            Name = name;
            FilePath = filePath;
        }
    }
}
