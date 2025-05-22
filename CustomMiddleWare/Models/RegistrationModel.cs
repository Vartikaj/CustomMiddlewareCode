using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace CustomMiddleWare.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class RegistrationModel
    {
        [JsonIgnore]
        public int id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        [Required]
        public string email { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string postalcode { get; set; }
    }

    public class LoginModel
    {
        [Required]
        public string firstname { get; set; }
        [Required]
        public string email { get; set; }
    }
}
