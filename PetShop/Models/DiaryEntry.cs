using System;

namespace PetShop.Models
{
    public class DiaryEntry
    {
        public string Content { get; set; }
        public string Food {  get; set; }
        public int Calories {  get; set; }
        public DateTime CreateTime { get; set; }
    }

}