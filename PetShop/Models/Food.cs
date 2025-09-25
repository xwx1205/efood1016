using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PetShop.Models
{
    public class Food
    {
        public int Id { get; set; }
        public string 食品名稱 { get; set; }
        public float 熱量_kcal { get; set; }
        public float 蛋白質_g { get; set; }
        public float 脂肪_g { get; set; }
        public float 碳水化合物_g { get; set; }
        public string 資料來源 { get; set; }
        public string 來源連結 { get; set; }
    }
}