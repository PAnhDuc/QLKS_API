namespace QLKS_API.Models.Dtos
{
    public class ResetPasswordWithTokenDto
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}