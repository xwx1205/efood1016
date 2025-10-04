using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Win32;

namespace PetShop.Models
{
    public class RegisterUser
    {
        [RegularExpression(@"[\u4E00-\u9FFF]*", ErrorMessage = "請輸入中文")]
        [Display(Name = "姓名:")]
        public string RegisterRealName { get; set; }

        [Required(ErrorMessage = "帳號不能留白")]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "請輸入電郵信箱")]
        [Display(Name = "帳號:")]
        public string RegisterAccount { get; set; }

        [MinLength(3, ErrorMessage = "長度不可小於3")]
        [MaxLength(8, ErrorMessage = "長度不可大於8")]
        [Display(Name = "密碼:")]
        public string RegisterPassword { get; set; }

        [RegularExpression(@"[0][0-9]{8,9}|([0][0-9][-][0-9]{7})", ErrorMessage = "電話格式錯誤")]
        [Display(Name = "電話:")]
        public string RegisterPhone { get; set; }

        [Range(1, 500, ErrorMessage = "體重請輸入合理數值")]
        [Display(Name = "體重(kg):")]
        public float RegisterWeight { get; set; }

        [Range(30, 250, ErrorMessage = "身高請輸入合理數值")]
        [Display(Name = "身高(cm):")]
        public float RegisterHeight { get; set; }

        [StringLength(8, ErrorMessage = "長度等於8")]
        [Display(Name = "生日:")]
        public string RegisterBirthday { get; set; }

        [RegularExpression(@".*\.(jpg)$", ErrorMessage = "只允許上傳 JPG圖片")]
        [Display(Name = "照片上傳:")]
        public string ImageName { get; set; }

    }

}
