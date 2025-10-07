using System;

namespace PetShop.Models
{
    public class DiaryEntry
    {
        public int Id { get; set; }
        public string Food {  get; set; }
        public int Calories {  get; set; }
        public decimal? Protein {  get; set; }
        public decimal? Fat {  get; set; }
        public decimal? Carbs {  get; set; }
        public DateTime CreateTime { get; set; }
    }

}