using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PetShop.Models
{
    public class Food
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int Calories { get; set; }
        public decimal? Protein { get; set; }
        public decimal? Fat { get; set; }
        public decimal? Carbs { get; set; }
        public string 資料來源 { get; set; }
        public string 來源連結 { get; set; }
    }
}