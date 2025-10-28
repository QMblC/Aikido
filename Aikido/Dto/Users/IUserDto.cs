namespace Aikido.Dto.Users
{
    public interface IUserDto
    {
        public string Role { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? Grade { get; set; }
        public byte[]? Photo { get; set; }
        public string? Sex { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Education { get; set; }
        public string? City { get; set; }
        public string? ParentFullName { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public bool? HasBudoPassport { get; set; }
    }
}
