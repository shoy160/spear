using System.ComponentModel.DataAnnotations;

namespace Spear.WebApi.Validations
{
    /// <summary> 邮箱验证 </summary>
    public class EmailAttribute : RegularExpressionAttribute
    {
        private const string EmailRegex = @"^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$";

        public EmailAttribute() : base(EmailRegex)
        {
            ErrorMessage = "请输入正确的Email格式,示例：abc@123.com";
        }
    }
}
