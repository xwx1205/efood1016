using System.ComponentModel.DataAnnotations;

namespace PetShop.Models
{
    public class RegisterUser
    {
        [Required(ErrorMessage = "帳號不能留白")]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "請輸入電子信箱")]
        [Display(Name = "信箱:")]
        public string RegisterAccount { get; set; }
        [MinLength(3, ErrorMessage = "長度不能小於3")]
        [MaxLength(8, ErrorMessage = "長度不能大於8")]
        [Display(Name = "密碼:")]
        public string RegisterPassword { get; set; }
        [RegularExpression(@"[\u4E00-\u9FFF]*", ErrorMessage = "請輸入中文")]
        [Display(Name = "姓名:")]
        public string RegisterRealname { get; set; }
        [RegularExpression(@"([0][0-9]{8,9})|([0][0-9][-][0-9]{7,8})", ErrorMessage = "電話格式錯誤")]
        [Display(Name = "電話:")]
        public string RegisterPhone { get; set; }
    }
}