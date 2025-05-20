using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomMiddleWare.Models
{
    public class RegistrationModel
    {
        [JsonIgnore]
        public int id {  get; set; }
        [Required]
        public string firstname { get; set; }
        [Required]
        public string lastname { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string phone { get; set; }
        [Required]
        public string address { get; set; }
        [Required]
        public string city { get; set; }
        [Required]
        public string country { get; set; }
        [Required]
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
