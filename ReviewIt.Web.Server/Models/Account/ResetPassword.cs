using System.ComponentModel.DataAnnotations;

namespace ReviewIt.Web.Models.Account
{
    public class ResetPassword
    {
        public string EmailAddress { get; set; }
    }

}