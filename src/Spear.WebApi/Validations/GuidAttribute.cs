using System.ComponentModel.DataAnnotations;

namespace Spear.WebApi.Validations
{
    /// <summary> Guid验证 </summary>
    public class GuidAttribute : RegularExpressionAttribute
    {
        private const string GuidRegex = "^[0-9a-z]{8}(-[0-9a-z]{4}){3}-[0-9a-z]{12}$";

        public GuidAttribute() : base(GuidRegex)
        {
            ErrorMessage = "请输入正确的Guid";
        }
    }
}
