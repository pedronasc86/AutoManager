namespace Identity.API.DTOs
{
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty; // Token JWT
        public DateTime Expiration { get; set; }          // Validade do Token
    }
}
