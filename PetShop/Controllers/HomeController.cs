using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Diagnostics;
using System.Web.UI.WebControls;
using System.Security.Policy;
using PetShop.Models;
using System.Web.Helpers;

namespace PetShop.Controllers
{
    public class HomeController : Controller
    {
        public SqlConnection X = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\佳芸\Desktop\web\PetShop\PetShop\App_Data\Pet.mdf;Integrated Security=True");

        public ActionResult LeaveHome()
        {
            TempData["Choice"] = "One";
            return RedirectToAction("LoginRegister");
        }
        public string AddUser(string user, string pwd,string realname,string phone)
        {
            string Response;
            try
            {
                X.Open();
                string G = "Insert[Member](Account,Password,RealName,Phone)Values(@Account,@Password,@RealName,@Phone)";
                Debug.WriteLine(G);
                SqlCommand Q = new SqlCommand(G, X);
                Q.Parameters.AddWithValue("@Account",user);
                Q.Parameters.AddWithValue("@Password",pwd);
                Q.Parameters.AddWithValue("@RealName",realname);
                Q.Parameters.AddWithValue("@Phone",phone);
                Q.ExecuteNonQuery();
                Response = "註冊成功";
            }
            catch (Exception)
            {
                Response = "開檔失敗";
            }
            finally { X.Close(); }
            return Response;
        }
        public string FindUser(string user)
        {
            string Result;
            try
            {
                X.Open();
                string G = "Select * from [Member] where Account= @User";
                Debug.WriteLine("SQL="+G);
                SqlCommand Q=new SqlCommand(G,X);
                Q.Parameters.AddWithValue("@User",user);
                Q.ExecuteNonQuery();
                SqlDataReader R=Q.ExecuteReader();
                if (R.Read() == true)
                {
                    Result = R["Password"].ToString().Trim();
                }
                else
                {
                    Result = "非會員";
                }
            }
            catch (Exception)
            {
                Result = "開檔失敗";
            }
            finally { X.Close(); }
            return Result;
        }
        public ActionResult Register()
        {
            string User = Request["RegisterAccount"];
            string Pwd = Request["RegisterPassword"];
            string Realname = Request["RegisterRealname"];
            string Phone = Request["RegisterPhone"];
            User=User.Trim();
            Pwd = Pwd.Trim();
            Realname = Realname.Trim();
            Phone = Phone.Trim();
            String Result = FindUser(User);
            string Ans;
            if (Result == "非會員")
            {
                Ans =AddUser(User,Pwd,Realname,Phone);
            }
            else
            {
                Ans = User + "已是會員，無法註冊";
            }
            TempData["Note"] = Ans;
            return RedirectToAction("LoginRegister");
        }
        public ActionResult CheckIn() 
        {
            string User = Request["UserName"];
            string Pwd = Request["Password"];
            string Ans;
            string CorrectPwd = FindUser(User.Trim());
            if (CorrectPwd=="非會員")
            {
                Ans = "查無此人";
            }
            else
            {
                if (CorrectPwd != Pwd)
                {
                    Ans = "密碼錯誤";
                }
                else
                {
                    
                    ViewBag.Account = User;
                    Session["LoginUser"] = User;
                    return View("~/Views/Home/Index.cshtml");
                }
            }
            TempData["Note"] = Ans;
            TempData["Choice"]="One";
            return View("~/Views/Home/LoginRegister.cshtml");
        }
        public ActionResult LoginRegister() 
        {
            return View(); 
        }
        public ActionResult Logout()
        {
            Session["LoginUser"] = null;
            return View("~/Views/Home/Index.cshtml");
        }
        [HttpPost]
        public ActionResult DiaryArea()
        {
            string Content = Request["A"]?.ToString();
            string Response;
            try
            {
                string account = Session["LoginUser"]?.ToString();

                X.Open();
                string G = "INSERT INTO Diary (Account, Content) VALUES (@Account, @Content)";
                SqlCommand Q = new SqlCommand(G, X);
                Q.Parameters.AddWithValue("@Account", account);
                Q.Parameters.AddWithValue("@Content", Content);
                Q.ExecuteNonQuery();
                Response = "建立成功";
            }
            catch (Exception ex)
            {
                Response = "建立失敗：" + ex.Message;
            }
            finally
            {
                X.Close();
            }

            TempData["Msg"] = Response;
            return RedirectToAction("DiaryIndex");
        }

        public ActionResult DiaryIndex()
        {
            string account = Session["LoginUser"]?.ToString();

            List<DiaryEntry> userDiaries = new List<DiaryEntry>();
            try
            {
                X.Open();
                string G = "SELECT Content, CreateTime FROM Diary WHERE Account = @Account ORDER BY CreateTime DESC";
                SqlCommand Q = new SqlCommand(G, X);
                Q.Parameters.AddWithValue("@Account", account);

                SqlDataReader reader = Q.ExecuteReader();
                while (reader.Read())
                {
                    userDiaries.Add(new DiaryEntry
                    {
                        Content = reader["Content"].ToString(),
                        CreateTime = Convert.ToDateTime(reader["CreateTime"])
                    });
                }
                reader.Close();
            }
            finally
            {
                X.Close();
            }
            ViewBag.Account = account;
            ViewBag.UserDiaries = userDiaries;
            return View("~/Views/Diary/DiaryArea.cshtml");
        }
        [HttpPost]
        public JsonResult Check(int count)
        {
            string Conn, G = "", Problem = "";
            List<string> Options = new List<string>();

            try
            {
                X.Open();
                Conn = "Success";
                G = "SELECT * FROM [Meal] WHERE Id = @id";
                SqlCommand Q = new SqlCommand(G, X);
                Q.Parameters.AddWithValue("@id", count);

                SqlDataReader R = Q.ExecuteReader();
                if (R.Read())
                {
                    Problem = Convert.ToString(R["Problem"]);
                    Options.Add(Convert.ToString(R["Option1"]));
                    Options.Add(Convert.ToString(R["Option2"]));
                    Options.Add(Convert.ToString(R["Option3"]));
                }
            }
            catch (Exception ex)
            {
                Conn = "Fail";
                Problem = "錯誤：" + ex.Message;
            }
            finally
            {
                X.Close();
            }

            // 回傳 JSON 給前端
            var result = new
            {
                Problem = Problem,
                Options = Options
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MealIndex()
        {
            string account = Session["LoginUser"]?.ToString();
            ViewBag.Account = account;
            return View("~/Views/Diary/MealArea.cshtml");
        }

        public ActionResult Index()
        {
            ViewBag.Account = Session["LoginUser"];
            return View();
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendResetLink(string email)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                TempData["Note"] = "查無此帳號";
                return RedirectToAction("ForgotPassword");
            }

            var token = Guid.NewGuid().ToString();
            user.ResetToken = token;
            user.ResetTokenExpire = DateTime.Now.AddMinutes(30);
            db.SaveChanges();

            var resetLink = Url.Action("ResetPassword", "Home", new { token = token }, Request.Url.Scheme);
            // 這裡應該用 Email 寄出 resetLink，先用 TempData 模擬
            TempData["Note"] = $"重設密碼連結：{resetLink}";
            return RedirectToAction("CheckIn");
        }

        // 重設密碼頁面
        public ActionResult ResetPassword(string token)
        {
            var user = db.Users.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpire > DateTime.Now);
            if (user == null)
            {
                TempData["Note"] = "連結無效或已過期";
                return RedirectToAction("CheckIn");
            }

            ViewBag.Token = token;
            return View();
        }

        // 處理密碼更新
        [HttpPost]
        public ActionResult ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["Note"] = "兩次密碼輸入不一致";
                return RedirectToAction("ResetPassword", new { token });
            }

            var user = db.Users.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpire > DateTime.Now);
            if (user == null)
            {
                TempData["Note"] = "連結無效或已過期";
                return RedirectToAction("CheckIn");
            }

            // 建議使用加密存密碼
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpire = null;
            db.SaveChanges();

            TempData["Note"] = "密碼已重設成功，請重新登入";
            return RedirectToAction("CheckIn");
        }
    }
}