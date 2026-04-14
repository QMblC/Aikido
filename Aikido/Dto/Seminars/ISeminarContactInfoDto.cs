namespace Aikido.Dto.Seminars
{
    public interface ISeminarContactInfoDto
    {
        public string Name { get; set; }
        public string FirstContact { get; set; }
        public string? SecondContact { get; set; }
        public string Description { get; set; }
    }
}
