namespace FTECH_THUONGMAIDIENTU.Models.Auth
{
    public class AuthResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string RedirectUrl { get; set; }

        public string RoleKey { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }
    }
}