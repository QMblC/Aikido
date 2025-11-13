namespace Aikido.Dto.Auth
{
    public class LoginRequestDto
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? DeviceInfo { get; set; }
    }
}