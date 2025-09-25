using PetShop.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace PetShop.Controllers
{
    public class HomeController : Controller
    {
        public SqlConnection X = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\佳芸\Desktop\efood\efood\PetShop\App_Data\FoodDB.mdf;Integrated Security=True");
        public MyDbContext db = new MyDbContext();
        public ActionResult LeaveHome()
        {
            TempData["Choice"] = "One";
            return RedirectToAction("LoginRegister");
        }
        public string AddUser(string user, string pwd, string realname, string phone)
        {
            string Response;
            try
            {
                X.Open();
                string G = "Insert INTO [Member] (Account, [Password], RealName, Phone) Values (@Account, @Password, @RealName, @Phone)";
                Debug.WriteLine(G);
                SqlCommand Q = new SqlCommand(G, X);
                Q.Parameters.AddWithValue("@Account", user);
                Q.Parameters.AddWithValue("@Password", pwd);
                Q.Parameters.AddWithValue("@RealName", realname);
                Q.Parameters.AddWithValue("@Phone", phone);
                Q.ExecuteNonQuery();
                Response = "註冊成功";
            }
            catch (Exception ex)
            {
                Response = "註冊失敗" + ex.Message;
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
                //Debug.WriteLine("SQL=" + G);
                SqlCommand Q = new SqlCommand(G, X);
                Q.Parameters.AddWithValue("@User", user);
                SqlDataReader R = Q.ExecuteReader();
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
            User = User.Trim();
            Pwd = Pwd.Trim();
            Realname = Realname.Trim();
            Phone = Phone.Trim();
            String Result = FindUser(User);
            string Ans;
            if (Result == "非會員")
            {
                Ans = AddUser(User, Pwd, Realname, Phone);
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
            string User = Request["Account"];
            string Pwd = Request["Password"];

            if (string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Pwd))
            {
                TempData["Note"] = "請輸入帳號與密碼";
                TempData["Choice"] = "One";
                return View("~/Views/Home/LoginRegister.cshtml");
            }

            string Ans;
            string CorrectPwd = FindUser(User.Trim());
            if (CorrectPwd == "非會員")
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
            TempData["Choice"] = "One";
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
            string FoodName = Request["foodName"]?.ToString();
            string FoodCaloriesStr = Request["FoodCalories"]?.ToString();
            int FoodCalories = 0;
            int.TryParse(FoodCaloriesStr, out FoodCalories);

            string Response;
            try
            {
                string account = Session["LoginUser"]?.ToString();

                X.Open();
                string G = "INSERT INTO Diary (Account, Content, Food, Calories) VALUES (@Account, @Content, @Food, @Calories)";
                SqlCommand Q = new SqlCommand(G, X);
                Q.Parameters.AddWithValue("@Account", account);
                Q.Parameters.AddWithValue("@Content", Content);
                Q.Parameters.AddWithValue("@Food", FoodName);
                Q.Parameters.AddWithValue("@Calories", FoodCalories);
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

        public ActionResult DiaryIndex(string date)
        {
            string account = Session["LoginUser"]?.ToString();
            List<DiaryEntry> userDiaries = new List<DiaryEntry>();
            List<Food> foodList = new List<Food>();

            try
            {
                X.Open();
                // 取得日記
                string G = "SELECT Content, CreateTime, Food, Calories FROM Diary WHERE Account = @Account";
                if (!string.IsNullOrEmpty(date))
                {
                    G += " AND CONVERT(date, CreateTime) = @SelectedDate";
                }
                G += " ORDER BY CreateTime DESC";

                SqlCommand Q = new SqlCommand(G, X);
                Q.Parameters.AddWithValue("@Account", account);
                if (!string.IsNullOrEmpty(date))
                {
                    DateTime selectedDate;
                    if (DateTime.TryParse(date, out selectedDate))
                    {
                        Q.Parameters.AddWithValue("@SelectedDate", selectedDate.Date);
                    }
                }
                SqlDataReader reader = Q.ExecuteReader();

                while (reader.Read())
                {
                    string content = reader["Content"] as string ?? "";
                    DateTime createTime = reader["CreateTime"] != DBNull.Value ? Convert.ToDateTime(reader["CreateTime"]) : DateTime.MinValue;
                    string food = reader["Food"] as string ?? "";
                    int calories = reader["Calories"] != DBNull.Value ? Convert.ToInt32(reader["Calories"]) : 0;
                    userDiaries.Add(new DiaryEntry
                    {
                        Content = content,
                        CreateTime = createTime,
                        Food = food,
                        Calories = calories
                    });
                }
                reader.Close();

                // 取得 Food 資料
                string foodSql = "SELECT * FROM Food";
                SqlCommand foodCmd = new SqlCommand(foodSql, X);
                SqlDataReader foodReader = foodCmd.ExecuteReader();
                while (foodReader.Read())
                {
                    foodList.Add(new Food
                    {
                        食品名稱 = foodReader["食品名稱"].ToString(),
                        熱量_kcal = Convert.ToInt32(foodReader["熱量_kcal"]),
                        蛋白質_g = Convert.ToInt32(foodReader["蛋白質_g"]),
                        脂肪_g = Convert.ToInt32(foodReader["脂肪_g"]),
                        碳水化合物_g = Convert.ToInt32(foodReader["碳水化合物_g"])
                    });
                }
                foodReader.Close();
            }
            finally
            {
                X.Close();
            }
            ViewBag.Account = account;
            ViewBag.UserDiaries = userDiaries;
            ViewBag.SelectedDate = date;
            ViewBag.FoodList = foodList;
            return View("~/Views/Diary/DiaryArea.cshtml");
        }
        [HttpPost]
        public JsonResult Check(int count)
        {
            string G = "", Problem = "";
            List<string> Options = new List<string>();

            try
            {
                X.Open();
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

        [HttpPost]
        public JsonResult SaveMealChoice(string mealType, string selectedOption, string date)
        {
            string account = Session["LoginUser"]?.ToString();
            if (string.IsNullOrEmpty(account))
                return Json(new { success = false, message = "未登入" });

            DateTime choiceDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

            try
            {
                X.Open();
                // 同一天同餐別更新，不重複新增
                string sql = @"IF EXISTS (SELECT 1 FROM MealChoice WHERE Account=@Account AND MealType=@MealType AND ChoiceDate=@ChoiceDate) UPDATE MealChoice SET Choice=@Choice WHERE Account=@Account AND MealType=@MealType AND ChoiceDate=@ChoiceDate ELSE INSERT INTO MealChoice(Account, MealType, Choice, ChoiceDate) VALUES(@Account, @MealType, @Choice, @ChoiceDate)";
                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Account", account);
                cmd.Parameters.AddWithValue("@MealType", mealType);
                cmd.Parameters.AddWithValue("@Choice", selectedOption);
                cmd.Parameters.AddWithValue("@ChoiceDate", choiceDate.Date);
                cmd.ExecuteNonQuery();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            finally
            {
                X.Close();
            }
        }

        public ActionResult MealHistory()
        {
            string account = Session["LoginUser"]?.ToString();
            List<MealChoiceEntry> meals = new List<MealChoiceEntry>();

            try
            {
                X.Open();
                string sql = "SELECT MealType, Choice, CreateTime FROM MealChoice WHERE Account=@Account ORDER BY CreateTime";
                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Account", account);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    meals.Add(new MealChoiceEntry
                    {
                        MealType = reader["MealType"].ToString(),
                        Choice = reader["Choice"].ToString(),
                        CreateTime = Convert.ToDateTime(reader["CreateTime"])
                    });
                }
                reader.Close();
            }
            finally { X.Close(); }

            ViewBag.Meals = meals;
            return View("~/Views/Diary/MealArea.cshtml");
        }

        public JsonResult Invoice()
        {
            String P = "Vote Success";
            var candidates = new List<object>();


            try
            {
                X.Open();
                string G = "select * from[Pictogram]";
                SqlCommand Q = new SqlCommand(G, X);
                Q.ExecuteNonQuery();
                SqlDataReader R = Q.ExecuteReader();
                while (R.Read() == true)
                {
                    string MyName = Convert.ToString(R["Candidate"]);
                    int MyVotes = Convert.ToInt16(R["Count"]);
                    candidates.Add(new
                    {
                        Name = MyName,
                        Votes = MyVotes
                    });
                }
            }
            catch (Exception) { }
            finally { X.Close(); }

            return Json(new { P, Q = candidates }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllMealRecords()
        {
            string account = Session["LoginUser"]?.ToString();
            if (string.IsNullOrEmpty(account))
            {
                return Json(new { success = false, message = "未登入" }, JsonRequestBehavior.AllowGet);
            }

            var meals = new List<object>();

            try
            {
                X.Open();
                string sql = "SELECT MealType, Choice, ChoiceDate FROM MealChoice WHERE Account=@Account ORDER BY ChoiceDate DESC";
                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Account", account);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    meals.Add(new
                    {
                        日期 = Convert.ToDateTime(reader["ChoiceDate"]).ToString("yyyy-MM-dd"),
                        餐別 = reader["MealType"].ToString(),
                        餐點 = reader["Choice"].ToString()
                    });
                }
                reader.Close();
            }
            finally
            {
                X.Close();
            }

            return Json(new { success = true, data = meals }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult PictogramIndex()
        {
            string account = Session["LoginUser"]?.ToString();
            ViewBag.Account = account;
            return View("~/Views/Diary/PictogramArea.cshtml");
        }
        public ActionResult MealIndex(string date)
        {
            string account = Session["LoginUser"]?.ToString();
            DateTime selectedDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);
            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");

            List<MealChoiceEntry> meals = new List<MealChoiceEntry>();
            var mealOptions = new Dictionary<string, List<string>>(); 

            try
            {
                X.Open();

                // 抓當日紀錄
                string sql = "SELECT MealType, Choice, ChoiceDate FROM MealChoice WHERE Account=@Account AND ChoiceDate=@ChoiceDate";
                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Account", account);
                cmd.Parameters.AddWithValue("@ChoiceDate", selectedDate.Date);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    meals.Add(new MealChoiceEntry
                    {
                        MealType = reader["MealType"].ToString(),
                        Choice = reader["Choice"].ToString(),
                        ChoiceDate = Convert.ToDateTime(reader["ChoiceDate"])
                    });
                }
                reader.Close();

                // 抓 Meal.sql 選項
                string sql2 = "SELECT * FROM Meal";
                SqlCommand cmd2 = new SqlCommand(sql2, X);
                SqlDataReader r2 = cmd2.ExecuteReader();
                while (r2.Read())
                {
                    string problem = r2["Problem"].ToString(); 
                    var list = new List<string>();

                    if (r2["Option1"] != DBNull.Value) list.Add(r2["Option1"].ToString());
                    if (r2["Option2"] != DBNull.Value) list.Add(r2["Option2"].ToString());
                    if (r2["Option3"] != DBNull.Value) list.Add(r2["Option3"].ToString());

                    mealOptions[problem] = list;
                }
                r2.Close();
            }
            finally { X.Close(); }

            ViewBag.Meals = meals;
            ViewBag.MealOptions = mealOptions;
            ViewBag.Account = Session["LoginUser"];

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

        public Member FindMember(string account)
        {
            Member user = null;
            string Response;
            try
            {
                X.Open();
                string sql = "SELECT * FROM [Member] WHERE Account = @Account";
                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Account", account);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    user = new Member()
                    {
                        Account = reader["Account"].ToString(),
                        Password = reader["Password"].ToString(),
                        RealName = reader["RealName"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        Birthyear = Convert.ToInt32(reader["Birthyear"]),
                        ResetToken = reader["ResetToken"] == DBNull.Value ? null : reader["ResetToken"].ToString(),
                        ResetTokenExpire = reader["ResetTokenExpire"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ResetTokenExpire"])
                    };
                }
            }
            catch (Exception ex)
            {
                Response = ex.Message;
            }
            finally
            {
                X.Close();
            }
            return user;
        }

        [HttpPost]
        public ActionResult SendResetLink(string account)
        {
            if (string.IsNullOrEmpty(account))
            {
                TempData["Note"] = "請輸入帳號";
                return RedirectToAction("ForgotPassword");
            }

            account = account.Trim().ToLower();

            var user = FindMember(account);
            if (user == null)
            {
                TempData["Note"] = "查無此帳號";
                return RedirectToAction("ForgotPassword");
            }

            var token = Guid.NewGuid().ToString();
            var expire = DateTime.Now.AddMinutes(30);

            // 更新資料庫的 ResetToken 與 ResetTokenExpire 欄位
            try
            {
                X.Open();
                string sql = "UPDATE [Member] SET ResetToken = @Token, ResetTokenExpire = @Expire WHERE Account = @Account";
                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Token", token);
                cmd.Parameters.AddWithValue("@Expire", expire);
                cmd.Parameters.AddWithValue("@Account", account);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                TempData["Note"] = "更新資料庫失敗：" + ex.Message;
                return RedirectToAction("ForgotPassword");
            }
            finally
            {
                X.Close();
            }

            var resetLink = Url.Action("ResetPassword", "Home", new { token = token }, Request.Url.Scheme);

            // 實際環境應用 Email 寄送，這裡先用 TempData 模擬
            return RedirectToAction("ResetPassword", new { token = token });
        }

        public ActionResult ResetPassword(string token)
        {
            Debug.WriteLine("收到的 token: " + token);
            if (string.IsNullOrEmpty(token))
            {
                TempData["Note"] = "重設密碼連結無效";
                return RedirectToAction("LoginRegister");
            }

            // 驗證 token 是否存在且未過期
            try
            {
                X.Open();
                string sql = "SELECT COUNT(*) FROM [Member] WHERE ResetToken = @Token AND ResetTokenExpire > GETDATE()";
                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Token", token);
                int count = (int)cmd.ExecuteScalar();

                if (count == 0)
                {
                    TempData["Note"] = "重設密碼連結無效或已過期";
                    return RedirectToAction("LoginRegister");
                }
            }
            finally
            {
                X.Close();
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Note"] = "重設密碼連結無效";
                return RedirectToAction("LoginRegister");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["Note"] = "請輸入完整密碼";
                ViewBag.Token = token;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                TempData["Note"] = "兩次輸入的密碼不一致";
                ViewBag.Token = token;
                return View();
            }

            // 更新密碼
            try
            {
                X.Open();
                string sql = "UPDATE [Member] SET Password = @Password, ResetToken = NULL, ResetTokenExpire = NULL WHERE ResetToken = @Token AND ResetTokenExpire > GETDATE()";
                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Password", newPassword);
                cmd.Parameters.AddWithValue("@Token", token);
                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                {
                    TempData["Note"] = "重設密碼連結無效或已過期";
                    return RedirectToAction("LoginRegister");
                }
            }
            finally
            {
                X.Close();
            }

            TempData["Note"] = "密碼已成功重設，請重新登入";
            return RedirectToAction("LoginRegister");
        }

    }
}