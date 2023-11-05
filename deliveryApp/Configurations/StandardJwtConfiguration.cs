using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace deliveryApp.Models
{
    public class StandardJwtConfiguration
    {
        public const string Issuer = "backend";
        public const string Audience = "frontend";
        //допустим, токен будет существовать полтора часа
        public const int Lifetime = 90;
        public const string Key = "asd1841bcvxla138";
        public static SymmetricSecurityKey GenerateSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
        }
    }
}
