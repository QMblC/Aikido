namespace Aikido.Dto.Clubs
{
    public interface IClubDto
    {
        public string Name { get; set; }
        public string City { get; set; }
        public string Address { get; set; }

        public long? ManagerId { get; set; }
    }
}
