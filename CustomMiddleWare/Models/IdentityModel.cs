namespace CustomMiddleWare.Models
{
    public class IdentityModel
    {
    }

    public class AccessTokenDetails
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
        public string user_type { get; set; }
        public long RoleId { get; set; }
    }
}
