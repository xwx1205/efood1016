using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PetShop.Models
{
    public class Member
    {
        public string Realname;
        public int Birthyear;
        public Member(string realname,int birthyear)
        {
            this.Realname = realname;
            this.Birthyear = birthyear;
        }
    }
}