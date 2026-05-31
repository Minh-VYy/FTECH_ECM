namespace FTECH_THUONGMAIDIENTU.Models.Auth
{
    public class AccountAuthRequest
    {
        public string Identifier { get; set; }

        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}