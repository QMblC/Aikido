using Aikido.Entities.Seminar;

namespace Aikido.Dto.Seminars
{
    public class SeminarGroupDto : DtoBase, ISeminarGroupDto
    {
        public long? Id { get; set; }

        public string Name { get; set; }

        public SeminarGroupDto() { }

        public SeminarGroupDto(SeminarGroupEntity entity)
        {
            Id = entity.Id;
            Name = entity.Name;
        }
    }
}
