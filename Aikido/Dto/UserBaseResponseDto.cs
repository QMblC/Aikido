namespace Aikido.Dto
{
    public class UserBaseResponseDto
    {
        public long Id { get; set; }
        public string Role { get; set; }
        public string Login { get; set; }
        public string FullName { get; set; }
        public string? Photo { get; set; }
        public DateTime? Birthday { get; set; }
        public string City { get; set; }
        public string Grade { get; set; }
        public long? ClubId { get; set; }
        public string ClubName { get; set; }
    }
}
