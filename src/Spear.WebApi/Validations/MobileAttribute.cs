using System.ComponentModel.DataAnnotations;

namespace Spear.WebApi.Validations
{
    public class MobileAttribute : RegularExpressionAttribute
    {
        private const string MobileRegex = "^1[0-9]{10}$";

        public MobileAttribute() : base(MobileRegex)
        {
            ErrorMessage = "手机号码格式不正确";
        }
    }
}
