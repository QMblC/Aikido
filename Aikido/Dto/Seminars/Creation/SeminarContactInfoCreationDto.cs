namespace Aikido.Dto.Seminars.Creation
{
    public class SeminarContactInfoCreationDto : ISeminarContactInfoDto
    {
        public string Name { get; set; }
        public string FirstContact { get; set; }
        public string? SecondContact { get; set; }
        public string Description { get; set; }
    }
}
