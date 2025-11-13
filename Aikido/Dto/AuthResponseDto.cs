namespace Aikido.Dto.Auth
{
    public class AuthResponseDto
    {
        public long UserId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}