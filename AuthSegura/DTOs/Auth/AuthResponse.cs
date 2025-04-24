namespace AuthSegura.DTOs
{
    public class AuthResponse
    {
        public string username { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
    }
}
