using System;
using System.Web;

namespace PetShop.Models
{
    public class MealChoiceEntry
    {
        public int Id { get; set; }
        public string Account { get; set; }
        public string MealType { get; set; }  // 早餐 / 午餐 / 晚餐
        public string Choice { get; set; }    // 選擇餐點
        public DateTime CreateTime { get; set; } // 選餐時間
        public DateTime ChoiceDate { get; set; } // 選餐日期
    }
}
