using PetShop.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;

namespace PetShop.Controllers
{
    public class HomeController : Controller
    {
        public SqlConnection X = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\VICTUS\source\repos\efood1016\PetShop\App_Data\FoodDB.mdf;Integrated Security=True");
        public MyDbContext db = new MyDbContext();
        public string Result2 { get; set; }
        //修改會員資料
        [HttpPost]
        public ActionResult UpdateMemberInfo(RegisterUser model)
        {
            string imagename = model.ImageName;

            // 如果沒有舊的 ImageName（可能表單沒傳回），從 Session 補上
            if (string.IsNullOrEmpty(imagename) && Session["ImageName"] != null)
            {
                imagename = Session["ImageName"].ToString();
            }

            // 若上傳了新圖片才取代
            if (Request.Files["ImageFile"] != null && Request.Files["ImageFile"].ContentLength > 0)
            {
                try
                {
                    var file = Request.Files["ImageFile"];
                    string extension = Path.GetExtension(file.FileName).ToLower();
                    string[] allowedExtensions = { ".jpg" };
                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["Note"] = "只允許 JPG";
                        return RedirectToAction("MemberInfo");
                    }

                    // 新圖取代舊圖
                    imagename = Guid.NewGuid().ToString() + extension;
                    string fpath = Path.Combine(Server.MapPath("~/Photo"), imagename);
                    file.SaveAs(fpath);
                }
                catch (Exception ex)
                {
                    TempData["Note"] = "圖片上傳失敗：" + ex.Message;
                    return RedirectToAction("MemberInfo");
                }
            }

            try
            {
                X.Open();
                string sql = @"UPDATE [Member] 
                       SET RealName=@RealName, Phone=@Phone, Birthday=@Birthday,
                           Height=@Height, Weight=@Weight, ImageName=@ImageName
                       WHERE Account=@Account";

                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Account", model.RegisterAccount);
                cmd.Parameters.AddWithValue("@RealName", model.RegisterRealName ?? "");
                cmd.Parameters.AddWithValue("@Phone", model.RegisterPhone ?? "");
                cmd.Parameters.AddWithValue("@Birthday", model.RegisterBirthday ?? "");
                cmd.Parameters.AddWithValue("@Height", model.RegisterHeight);
                cmd.Parameters.AddWithValue("@Weight", model.RegisterWeight);
                cmd.Parameters.AddWithValue("@ImageName", imagename); // ✅ 不用 DBNull.Value，永遠有值

                int rows = cmd.ExecuteNonQuery();
                Debug.WriteLine("更新筆數：" + rows);

                // ✅ 同步更新 Session
                Session["ImageName"] = imagename;
                Session["RealName"] = model.RegisterRealName;

                TempData["Note"] = "會員資料更新成功";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("更新錯誤：" + ex.Message);
                TempData["Note"] = "更新失敗：" + ex.Message;
            }
            finally
            {
                X.Close();
            }

            return RedirectToAction("MemberInfo");
        }
        //會員資料
        public ActionResult MemberInfo()
        {
            string account = Session["LoginUser"].ToString();
            Models.RegisterUser member = null;

            try
            {
                X.Open();
                string sql = "SELECT * FROM [Member] WHERE Account = @Account";
                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Account", account);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    member = new Models.RegisterUser
                    {
                        RegisterAccount = reader["Account"].ToString(),
                        RegisterRealName = reader["RealName"].ToString(),
                        RegisterPhone = reader["Phone"].ToString(),
                        RegisterWeight = float.Parse(reader["Weight"].ToString()),
                        RegisterHeight = float.Parse(reader["Height"].ToString()),
                        RegisterBirthday = reader["Birthday"].ToString(),
                        ImageName = reader["ImageName"].ToString()
                    };
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("會員查詢錯誤：" + ex.Message);
            }
            finally
            {
                X.Close();
            }

            return View(member);
        }

        public ActionResult LeaveHome()
        {
            TempData["Choice"] = "One";
            return RedirectToAction("LoginRegister");
        }
        public string AddUser(string phone, string pwd, string realname, string user, float weight, float height, string birthday, string imagename)
        {
            string Response;
            try
            {
                X.Open();
                string G = "INSERT INTO [Member](Account, Password,RealName,Phone,Weight,Height,BirthDay,ImageName) VALUES(@Account,@Password,@RealName,@Phone,@Weight,@Height,@BirthDay,@ImageName)";
                Debug.WriteLine(G);
                SqlCommand Q = new SqlCommand(G, X);
                Q.Parameters.AddWithValue("@Account", user);
                Q.Parameters.AddWithValue("@Password", pwd);
                Q.Parameters.AddWithValue("@RealName", realname);
                Q.Parameters.AddWithValue("@Phone", phone);
                Q.Parameters.AddWithValue("@Weight", weight);
                Q.Parameters.AddWithValue("@Height", height);
                Q.Parameters.AddWithValue("@BirthDay", birthday);
                Q.Parameters.AddWithValue("@ImageName", imagename);
                Q.ExecuteNonQuery();
                Response = "註冊成功-" + Result2;
            }
            catch (Exception ex)
            {
                Response = "開檔失敗" + ex.Message;
            }
            finally
            {
                X.Close();
            }
            return Response;
        }
        public string FindUser(string user)
        {
            string Result;
            try
            {
                X.Open();

                string G = "Select * from [Member] where Account=@User";

                SqlCommand Q = new SqlCommand(G, X);
                Q.Parameters.AddWithValue("@User", user);
                Q.ExecuteNonQuery();

                SqlDataReader R = Q.ExecuteReader();
                if (R.Read() == true)
                {
                    Debug.WriteLine("AAAAA");
                    Result = R["Password"].ToString().Trim();
                }
                else
                {
                    Debug.WriteLine("BBBB");
                    Result = "非會員";
                }

            }
            catch (Exception)
            {
                Debug.WriteLine("CCCC");
                Result = "開檔失敗X";
            }
            finally { X.Close(); }
            return Result;
        }
        public ActionResult Register()
        {
            string User = Request["RegisterAccount"];
            string Pwd = Request["RegisterPassword"];
            string RealName = Request["RegisterRealName"];
            string Phone = Request["RegisterPhone"];
            string Weight = Request["RegisterWeight"];
            string Height = Request["RegisterHeight"];
            string BirthDay = Request["RegisterBirthDay"];
            string imagename = "";

            User = User.Trim();
            Pwd = Pwd.Trim();
            RealName = RealName.Trim();
            Phone = Phone.Trim();
            BirthDay = BirthDay.Trim();
            float W = float.Parse(Weight);
            float H = float.Parse(Height);
            string Result = FindUser(User);
            imagename = imagename.Trim();

            try
            {
                if (Request.Files["imagename"] != null && Request.Files["imagename"].ContentLength > 0)
                {
                    var file = Request.Files["imagename"];
                    string extension = Path.GetExtension(file.FileName).ToLower();

                    string[] allowedExtensions = { ".jpg" };
                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["Note"] = "只允許上傳 JPG 檔案";
                        return RedirectToAction("LoginRegister");
                    }

                    imagename = Guid.NewGuid().ToString() + extension;
                    string photoFolder = Server.MapPath("~/Photo");
                    string fpath = Path.Combine(photoFolder, imagename);
                    file.SaveAs(fpath);
                }
                X.Open();
                X.Close();
                Result2 = "資料庫成功";

            }
            catch (Exception e)
            {
                X.Close();
                Result2 = "資料庫失敗:" + e;
            }
            ;

            string Ans;
            if (Result == "非會員")
            {
                Ans = AddUser(Phone, Pwd, RealName, User, W, H, BirthDay, imagename);
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
            string Phone = Request["Phone"];
            string Ans;
            if (string.IsNullOrWhiteSpace(User))
            {
                TempData["Note"] = "請輸入帳號";
                TempData["Choice"] = "One";
                return View("~/Views/Home/LoginRegister.cshtml");
            }

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
                    // 登入成功，取會員資料
                    Models.RegisterUser member = null;
                    try
                    {
                        X.Open();
                        string sql = "SELECT * FROM [Member] WHERE Account = @Account";
                        SqlCommand cmd = new SqlCommand(sql, X);
                        cmd.Parameters.AddWithValue("@Account", User);
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            member = new Models.RegisterUser
                            {
                                RegisterAccount = reader["Account"].ToString(),
                                RegisterRealName = reader["RealName"].ToString(),
                                RegisterPhone = reader["Phone"].ToString(),
                                RegisterWeight = float.Parse(reader["Weight"].ToString()),
                                RegisterHeight = float.Parse(reader["Height"].ToString()),
                                RegisterBirthday = reader["Birthday"].ToString(),
                                ImageName = reader["ImageName"].ToString()
                            };
                        }
                        reader.Close();
                    }
                    finally
                    {
                        X.Close();
                    }

                    ViewBag.Account = User;
                    Session["LoginUser"] = User;

                    if (member != null)
                    {
                        Session["RealName"] = member.RegisterRealName;
                        ViewBag.ImageName = member.ImageName;
                        Session["ImageName"] = member.ImageName;
                    }
                    else
                    {
                        Ans = "查無會員資料";
                        TempData["Note"] = Ans + " | " + Result2;
                        TempData["Choice"] = "One";
                        return View("~/Views/Home/LoginRegister.cshtml");
                    }

                    if (User == "Manager")
                    {
                        return RedirectToAction("Manage", "Manager");
                    }
                    else
                    {
                        return View("~/Views/Home/Index.cshtml");
                    }
                }

            }
            TempData["Note"] = Ans + " | " + Result2; ;
            TempData["Choice"] = "One";
            return View("~/Views/Home/LoginRegister.cshtml");
        }
        public ActionResult LoginRegister()
        {
            if (Session["Choice"] != null)
            {
                TempData["Choice"] = Session["Choice"].ToString();
            }
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
            string Category = Request["Category"]?.ToString();
            string MealType = Request["MealType"]?.ToString();
            string food = "";
            if (!string.IsNullOrEmpty(Request["CommonFood"]) && Request["CommonFood"] != "其他")
                food = Request["CommonFood"];
            else if (Request["CommonFood"] == "其他")
                food = Request["FoodOther"];
            int quantity = 0;
            int.TryParse(Request["Quantity"], out quantity);
            decimal calories = 0, protein = 0, fat = 0, carbs = 0;
            decimal.TryParse(Request["Calories"], out calories);
            decimal.TryParse(Request["Protein"], out protein);
            decimal.TryParse(Request["Fat"], out fat);
            decimal.TryParse(Request["Carbs"], out carbs);

            DateTime createTime = DateTime.Now;
            if (!string.IsNullOrEmpty(Request["selectedDateTime"]))
            {
                DateTime.TryParse(Request["selectedDateTime"], out createTime);
            }

            string Response;
            try
            {
                string account = Session["LoginUser"]?.ToString();
                X.Open();
                string G = "INSERT INTO Diary (Account, Category, Food, Calories, Protein, Fat, Carbs, MealType, Quantity, CreateTime) VALUES (@Account, @Category, @Food, @Calories, @Protein, @Fat, @Carbs, @MealType, @Quantity, @CreateTime)";

                SqlCommand Q = new SqlCommand(G, X);
                Q.Parameters.AddWithValue("@Account", account);
                Q.Parameters.AddWithValue("@Category", Category);
                Q.Parameters.AddWithValue("@Food", food);
                Q.Parameters.AddWithValue("@Calories", calories);
                Q.Parameters.AddWithValue("@Protein", protein);
                Q.Parameters.AddWithValue("@Fat", fat);
                Q.Parameters.AddWithValue("@Carbs", carbs);
                Q.Parameters.AddWithValue("@MealType", MealType);
                Q.Parameters.AddWithValue("@Quantity", quantity);
                Q.Parameters.AddWithValue("@CreateTime", createTime);

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
            //TempData["Msg"] = Response;
            return RedirectToAction("DiaryIndex");
        }
        public ActionResult DiaryIndex(string date)
        {
            DateTime selectedDate;
            if (!DateTime.TryParse(date, out selectedDate))
            {
                selectedDate = DateTime.Today;
            }
            DateTime nextDay = selectedDate.AddDays(1); // 取得隔天

            // 用範圍比對，不用 .Date
            //var entries = db.DiaryEntries
            //                .Where(e => e.CreateTime >= selectedDate && e.CreateTime < nextDay)
            //                .OrderByDescending(e => e.CreateTime)
            //                .ToList();

            string account = Session["LoginUser"]?.ToString();
            List<DiaryEntry> userDiaries = new List<DiaryEntry>();
            //List<Food> foodList = new List<Food>();
            List<CommonFood> commonFoods = new List<CommonFood>();

            try
            {
                X.Open();
                // 取得日記
                string G = "SELECT Id, CreateTime, Food, Calories, Protein, Fat, Carbs, MealType, Category, Quantity FROM Diary WHERE Account = @Account";

                if (!string.IsNullOrEmpty(date))
                {
                    G += " AND CreateTime >= @SelectedDate AND CreateTime < @NextDay";
                }

                G += " ORDER BY CreateTime DESC";

                SqlCommand Q = new SqlCommand(G, X);
                Q.Parameters.AddWithValue("@Account", account);

                if (!string.IsNullOrEmpty(date))
                {
                    Q.Parameters.AddWithValue("@SelectedDate", selectedDate);
                    Q.Parameters.AddWithValue("@NextDay", selectedDate.AddDays(1));
                }

                SqlDataReader reader = Q.ExecuteReader();

                while (reader.Read())
                {
                    var entry = new DiaryEntry
                    {
                        Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                        CreateTime = reader["CreateTime"] != DBNull.Value ? Convert.ToDateTime(reader["CreateTime"]) : DateTime.MinValue,
                        Food = reader["Food"] != DBNull.Value ? reader["Food"].ToString() : "",
                        Calories = reader["Calories"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Calories"]) : null,
                        Protein = reader["Protein"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Protein"]) : null,
                        Fat = reader["Fat"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Fat"]) : null,
                        Carbs = reader["Carbs"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Carbs"]) : null,
                        MealType = reader["MealType"] != DBNull.Value ? reader["MealType"].ToString() : "",
                        Quantity = reader["Quantity"] != DBNull.Value ? Convert.ToInt32(reader["Quantity"]) : 0,
                        Category = reader["Category"] != DBNull.Value ? reader["Category"].ToString() : ""
                    };
                    userDiaries.Add(entry);
                }
                reader.Close();

                //// 取得 Food 資料
                //string foodSql = "SELECT * FROM Food";
                //SqlCommand foodCmd = new SqlCommand(foodSql, X);
                //SqlDataReader foodReader = foodCmd.ExecuteReader();
                //while (foodReader.Read())
                //{
                //    foodList.Add(new Food
                //    {
                //        Name = foodReader["Name"].ToString(),
                //        Category = foodReader["Category"].ToString(),
                //        Calories = Convert.ToInt32(foodReader["Calories"]),
                //        Protein = Convert.ToInt32(foodReader["Protein"]),
                //        Fat = Convert.ToInt32(foodReader["Fat"]),
                //        Carbs = Convert.ToInt32(foodReader["Carbs"])
                //    });
                //}
                //foodReader.Close();

                // 取得 CommonFood 資料
                string CommonSql = "SELECT * FROM CommonFoods";
                SqlCommand CommonCmd = new SqlCommand(CommonSql, X);
                SqlDataReader CommonReader = CommonCmd.ExecuteReader();
                while (CommonReader.Read())
                {
                    commonFoods.Add(new CommonFood
                    {
                        Name = CommonReader["Name"].ToString(),
                        Category = CommonReader["Category"].ToString(),
                        Calories = CommonReader["Calories"] != DBNull.Value ? (decimal?)Convert.ToDecimal(CommonReader["Calories"]) : null,
                        Protein = CommonReader["Protein"] != DBNull.Value ? (decimal?)Convert.ToDecimal(CommonReader["Protein"]) : null,
                        Fat = CommonReader["Fat"] != DBNull.Value ? (decimal?)Convert.ToDecimal(CommonReader["Fat"]) : null,
                        Carbs = CommonReader["Carbs"] != DBNull.Value ? (decimal?)Convert.ToDecimal(CommonReader["Carbs"]) : null
                    });
                }
                CommonReader.Close();
            }
            finally
            {
                X.Close();
            }
            ViewBag.Account = account;
            ViewBag.UserDiaries = userDiaries;
            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");
            //ViewBag.FoodList = foodList;
            ViewBag.CommonFoods = commonFoods;
            return View("~/Views/Diary/DiaryArea.cshtml");
        }
        // 取得單筆日記
        public ActionResult EditDiary(int id)
        {
            DiaryEntry entry = null;
            try
            {
                X.Open();
                string sql = "SELECT * FROM Diary WHERE Id=@Id";
                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Id", id);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    entry = new DiaryEntry
                    {
                        Id = (int)reader["Id"],
                        Food = reader["Food"].ToString(),
                        Calories = reader["Calories"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Calories"]) : null,
                        Protein = reader["Protein"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Protein"]) : null,
                        Fat = reader["Fat"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Fat"]) : null,
                        Carbs = reader["Carbs"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Carbs"]) : null,
                        Quantity = reader["Quantity"] != DBNull.Value ? Convert.ToInt32(reader["Quantity"]) : 0,
                        CreateTime = Convert.ToDateTime(reader["CreateTime"]),
                        MealType = reader["MealType"].ToString()
                    };
                }
                reader.Close();
            }
            finally { X.Close(); }
            return View("~/Views/Diary/EditDiary.cshtml", entry);
        }
        // 修改日記
        [HttpPost]
        public ActionResult EditDiary(DiaryEntry entry)
        {
            try
            {
                X.Open();
                string sql = "UPDATE Diary SET Food=@Food, Calories=@Calories, Protein=@Protein, Fat=@Fat,MealType=@MealType, Carbs=@Carbs, Quantity=@Quantity WHERE Id=@Id";
                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Food", entry.Food);
                cmd.Parameters.AddWithValue("@Calories", entry.Calories);
                cmd.Parameters.AddWithValue("@Protein", entry.Protein ?? 0);
                cmd.Parameters.AddWithValue("@Fat", entry.Fat ?? 0);
                cmd.Parameters.AddWithValue("@Carbs", entry.Carbs ?? 0);
                cmd.Parameters.AddWithValue("@Id", entry.Id);
                cmd.Parameters.AddWithValue("@Quantity", entry.Quantity);
                cmd.Parameters.AddWithValue("@MealType", entry.MealType);
                cmd.ExecuteNonQuery();
            }
            finally { X.Close(); }
            return RedirectToAction("DiaryIndex");
        }
        [HttpPost]
        public JsonResult DeleteDiary(int id)
        {
            bool success = false;
            string message = "";
            try
            {
                X.Open();
                string sql = "DELETE FROM Diary WHERE Id=@Id";
                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Id", id);
                success = cmd.ExecuteNonQuery() > 0;
                if (!success) message = "找不到該日記";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                X.Close();
            }
            return Json(new { success, message });
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
        //public JsonResult Invoice()
        //{
        //    String P = "Vote Success";
        //    var candidates = new List<object>();

        //    try
        //    {
        //        X.Open();
        //        string G = "select * from[Pictogram]";
        //        SqlCommand Q = new SqlCommand(G, X);
        //        Q.ExecuteNonQuery();
        //        SqlDataReader R = Q.ExecuteReader();
        //        while (R.Read() == true)
        //        {
        //            string MyName = Convert.ToString(R["Candidate"]);
        //            int MyVotes = Convert.ToInt16(R["Count"]);
        //            candidates.Add(new
        //            {
        //                Name = MyName,
        //                Votes = MyVotes
        //            });
        //        }
        //    }
        //    catch (Exception) { }
        //    finally { X.Close(); }
        //    return Json(new { P, Q = candidates }, JsonRequestBehavior.AllowGet);
        //}
        //public JsonResult GetAllMealRecords()
        //{
        //    string account = Session["LoginUser"]?.ToString();
        //    if (string.IsNullOrEmpty(account))
        //    {
        //        return Json(new { success = false, message = "未登入" }, JsonRequestBehavior.AllowGet);
        //    }

        //    var meals = new List<object>();

        //    try
        //    {
        //        X.Open();
        //        string sql = "SELECT MealType, Choice, ChoiceDate FROM MealChoice WHERE Account=@Account ORDER BY ChoiceDate DESC";
        //        SqlCommand cmd = new SqlCommand(sql, X);
        //        cmd.Parameters.AddWithValue("@Account", account);

        //        SqlDataReader reader = cmd.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            meals.Add(new
        //            {
        //                日期 = Convert.ToDateTime(reader["ChoiceDate"]).ToString("yyyy-MM-dd"),
        //                餐別 = reader["MealType"].ToString(),
        //                餐點 = reader["Choice"].ToString()
        //            });
        //        }
        //        reader.Close();
        //    }
        //    finally
        //    {
        //        X.Close();
        //    }
        //    return Json(new { success = true, data = meals }, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult PictogramIndex()
        {
            string account = Session["LoginUser"]?.ToString();
            ViewBag.Account = account;
            return View("~/Views/Diary/PictogramArea.cshtml");
        }

        [HttpPost]
        public JsonResult SaveObjective(string account, int targetDays, float targetWeight, decimal dailyCalories)
        {
            if (string.IsNullOrEmpty(account))
            {
                return Json(new { success = false, message = "未登入" });
            }
            if (targetDays < 1)
            {
                return Json(new { success = false, message = "目標天數必須大於或等於1" });
            }
            if (targetWeight <= 0)
            {
                return Json(new { success = false, message = "目標體重必須大於0" });
            }
            if (dailyCalories <= 0)
            {
                return Json(new { success = false, message = "每日所需熱量必須大於0" });
            }

            float currentWeight = 0;
            string goalStatus = "維持";
            try
            {
                X.Open();
                // 取得會員身高、生日、目前體重
                string sql = "SELECT Height, BirthDay, Weight FROM [Member] WHERE Account = @Account";
                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Account", account);
                SqlDataReader reader = cmd.ExecuteReader();
                float height = 0;
                int age = 30;
                if (reader.Read())
                {
                    height = reader["Height"] != DBNull.Value ? Convert.ToSingle(reader["Height"]) : 0;
                    currentWeight = reader["Weight"] != DBNull.Value ? Convert.ToSingle(reader["Weight"]) : 0;
                    string birthDayStr = reader["BirthDay"]?.ToString();
                    DateTime birthDay;
                    if (!string.IsNullOrEmpty(birthDayStr) && birthDayStr.Length == 8)
                    {
                        if (DateTime.TryParseExact(birthDayStr, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out birthDay))
                        {
                            age = DateTime.Now.Year - birthDay.Year;
                            if (DateTime.Now < birthDay.AddYears(age)) age--;
                        }
                    }
                }
                reader.Close();

                // 儲存或更新目標
                string objSql = @"IF EXISTS (SELECT 1 FROM Objectives WHERE Account = @Account)
                       UPDATE Objectives SET TargetDays = @TargetDays, TargetWeight = @TargetWeight, DailyCalories = @DailyCalories, CreatedAt = GETDATE() WHERE Account = @Account
                       ELSE
                       INSERT INTO Objectives (Account, TargetDays, TargetWeight, DailyCalories) VALUES (@Account, @TargetDays, @TargetWeight, @DailyCalories)";
                SqlCommand objCmd = new SqlCommand(objSql, X);
                objCmd.Parameters.AddWithValue("@Account", account);
                objCmd.Parameters.AddWithValue("@TargetDays", targetDays);
                objCmd.Parameters.AddWithValue("@TargetWeight", targetWeight);
                objCmd.Parameters.AddWithValue("@DailyCalories", dailyCalories);
                objCmd.ExecuteNonQuery();

                // 重新計算每日所需熱量
                double newBmr = 10 * targetWeight + 6.25 * height - 5 * age + 5;
                double newDailyCalories = Math.Round(newBmr * 1.55, 0);

                // 判斷目標狀態
                float diff = targetWeight - currentWeight;
                if (diff > 0)
                    goalStatus = "目標:增重";
                else if (diff < 0)
                    goalStatus = "目標:減重";
                else
                    goalStatus = "目標:維持";

                return Json(new { success = true, message = "儲存成功", dailyCalories = newDailyCalories, goalStatus = goalStatus }, JsonRequestBehavior.AllowGet);
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
        public ActionResult ObjectiveIndex()
        {
            string account = Session["LoginUser"]?.ToString();
            ViewBag.Account = account;
            Models.RegisterUser member = null;

            if (!string.IsNullOrEmpty(account))
            {
                try
                {
                    X.Open();
                    string sql = "SELECT * FROM [Member] WHERE Account = @Account";
                    SqlCommand cmd = new SqlCommand(sql, X);
                    cmd.Parameters.AddWithValue("@Account", account);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        member = new Models.RegisterUser
                        {
                            RegisterAccount = reader["Account"].ToString(),
                            RegisterRealName = reader["RealName"].ToString(),
                            RegisterPhone = reader["Phone"].ToString(),
                            RegisterWeight = float.Parse(reader["Weight"].ToString()),
                            RegisterHeight = float.Parse(reader["Height"].ToString()),
                            RegisterBirthday = reader["Birthday"].ToString(),
                            ImageName = reader["ImageName"].ToString()
                        };
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("會員查詢錯誤：" + ex.Message);
                }
                finally
                {
                    X.Close();
                }
            }

            // 取得當日飲食加總熱量
            decimal totalCalories = 0;
            try
            {
                X.Open();
                string sql = @"SELECT SUM(Calories) FROM Diary 
                       WHERE Account = @Account AND CONVERT(date, CreateTime) = @Today";
                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Account", account);
                cmd.Parameters.AddWithValue("@Today", DateTime.Today);
                object result = cmd.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                    totalCalories = Convert.ToDecimal(result);
            }
            finally
            {
                X.Close();
            }

            //取得每日所需熱量
            decimal dailyCalories = 0;
            float targetWeight = 0;
            int targetDays = 0;
            try
            {
                X.Open();
                string sql = "SELECT DailyCalories, TargetWeight, TargetDays FROM Objectives WHERE Account = @Account";
                SqlCommand cmd = new SqlCommand(sql, X);
                cmd.Parameters.AddWithValue("@Account", account);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    dailyCalories = reader["DailyCalories"] != DBNull.Value ? Convert.ToDecimal(reader["DailyCalories"]) : 0;
                    targetWeight = reader["TargetWeight"] != DBNull.Value ? Convert.ToSingle(reader["TargetWeight"]) : 0;
                    targetDays = reader["TargetDays"] != DBNull.Value ? Convert.ToInt32(reader["TargetDays"]) : 0;
                }
                reader.Close();
            }
            finally
            {
                X.Close();
            }

            // 計算熱量差距並推薦食物
            decimal calorieDiff = (decimal)dailyCalories - totalCalories;
            List<CommonFood> recommendFoods = new List<CommonFood>();
            try
            {
                X.Open();
                if (calorieDiff > 0)
                {
                    // 推薦熱量小於等於差距的食物（補充）
                    string sql = @"SELECT TOP 3 * FROM CommonFoods WHERE Calories <= @Diff ORDER BY Calories DESC";
                    SqlCommand cmd = new SqlCommand(sql, X);
                    cmd.Parameters.AddWithValue("@Diff", calorieDiff);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        recommendFoods.Add(new CommonFood
                        {
                            Name = reader["Name"].ToString(),
                            Calories = reader["Calories"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Calories"]) : null
                            // 可加上蛋白質、脂肪、碳水等
                        });
                    }
                    reader.Close();
                }
                else if (calorieDiff < 0)
                {
                    // 推薦高熱量食物（減少攝取）
                    string sql = @"SELECT TOP 3 * FROM CommonFoods WHERE Calories >= @HighCal ORDER BY Calories DESC";
                    SqlCommand cmd = new SqlCommand(sql, X);
                    cmd.Parameters.AddWithValue("@HighCal", Math.Abs(calorieDiff));
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        recommendFoods.Add(new CommonFood
                        {
                            Name = reader["Name"].ToString(),
                            Calories = reader["Calories"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["Calories"]) : null
                        });
                    }
                    reader.Close();
                }
            }
            finally
            {
                X.Close();
            }
            ViewBag.TargetDays = targetDays;
            ViewBag.TargetWeight = targetWeight;
            ViewBag.DailyCalories = dailyCalories;
            ViewBag.RecommendFoods = recommendFoods;
            ViewBag.CalorieDiff = calorieDiff;
            ViewBag.TodayCalories = totalCalories;
            ViewBag.Member = member;
            return View("~/Views/Diary/ObjectiveArea.cshtml");
        }
        public ActionResult Analysis1Index(string selectedDate)
        {
            string account = Session["LoginUser"]?.ToString();
            ViewBag.Account = account;

            // 設定近七天日期和數據
            DateTime endDate = string.IsNullOrEmpty(selectedDate) ? DateTime.Today : DateTime.Parse(selectedDate);
            DateTime startDate = endDate.AddDays(-6);
            Dictionary<string, string> weekData = new Dictionary<string, string>();
            double totalCalories = 0;
            int daysWithData = 0;
            Dictionary<string, (double TotalCalories, int Count)> mealTypeData = new Dictionary<string, (double, int)>
            {
                { "早餐", (0, 0) },
                { "午餐", (0, 0) },
                { "晚餐", (0, 0) }
            };

            try
            {
                X.Open();
                for (int i = 0; i < 7; i++)
                {
                    DateTime currentDate = startDate.AddDays(i);
                    string dateKey = currentDate.ToString("yyyy-MM-dd");
                    weekData[dateKey] = "無記錄";

                    // 查詢當日餐點記錄並計算熱量
                    string sql = @"
                        SELECT d.Food, d.Calories, d.MealType 
                        FROM Diary d 
                        WHERE d.Account = @Account AND CONVERT(date, d.CreateTime) = @Date";
                    SqlCommand cmd = new SqlCommand(sql, X);
                    cmd.Parameters.AddWithValue("@Account", account);
                    cmd.Parameters.AddWithValue("@Date", currentDate.Date);
                    SqlDataReader reader = cmd.ExecuteReader();

                    string mealDetails = "";
                    bool hasData = false;
                    while (reader.Read())
                    {
                        hasData = true;
                        string food = reader["Food"]?.ToString() ?? "未知餐點";
                        double calories = reader["Calories"] != DBNull.Value ? Convert.ToDouble(reader["Calories"]) : 0;
                        string mealType = reader["MealType"]?.ToString();

                        mealDetails += $"{food} ({calories} kcal), ";
                        totalCalories += calories;

                        // 累計每餐類別的熱量和記錄數
                        if (mealTypeData.ContainsKey(mealType))
                        {
                            var (total, count) = mealTypeData[mealType];
                            mealTypeData[mealType] = (total + calories, count + 1);
                        }
                    }
                    reader.Close();
                    if (hasData)
                    {
                        daysWithData++;
                        weekData[dateKey] = mealDetails.TrimEnd(',', ' ');
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "無法載入數據：" + ex.Message;
            }
            finally
            {
                X.Close();
            }

            // 計算平均熱量
            double averageCalories = daysWithData > 0 ? Math.Round(totalCalories / daysWithData, 1) : 0;
            double avgBreakfastCalories = mealTypeData["早餐"].Count > 0 ? Math.Round(mealTypeData["早餐"].TotalCalories / mealTypeData["早餐"].Count, 1) : 0;
            double avgLunchCalories = mealTypeData["午餐"].Count > 0 ? Math.Round(mealTypeData["午餐"].TotalCalories / mealTypeData["午餐"].Count, 1) : 0;
            double avgDinnerCalories = mealTypeData["晚餐"].Count > 0 ? Math.Round(mealTypeData["晚餐"].TotalCalories / mealTypeData["晚餐"].Count, 1) : 0;

            // 設定 ViewBag 數據
            ViewBag.AverageCalories = averageCalories;
            ViewBag.AvgBreakfastCalories = avgBreakfastCalories;
            ViewBag.AvgLunchCalories = avgLunchCalories;
            ViewBag.AvgDinnerCalories = avgDinnerCalories;
            ViewBag.WeekData = weekData;
            ViewBag.SelectedDate = endDate.ToString("yyyy-MM-dd");

            return View("~/Views/Diary/Analysis1Area.cshtml");
        }
        public ActionResult Analysis2Index(string selectedDate)
        {
            string account = Session["LoginUser"]?.ToString();
            ViewBag.Account = account;

            // 設定近七天日期和熱量數據
            string[] dates = new string[7];
            int[] calories = new int[7];
            DateTime startDate = string.IsNullOrEmpty(selectedDate) ? DateTime.Today : DateTime.Parse(selectedDate);
            Dictionary<string, string> weekData = new Dictionary<string, string>();

            try
            {
                X.Open();
                for (int i = 0; i < 7; i++)
                {
                    DateTime currentDate = startDate.AddDays(-6 + i);
                    dates[i] = currentDate.ToString("MM/dd");

                    // 查詢當日熱量總和
                    string sql = "SELECT SUM(Calories) as TotalCalories FROM Diary WHERE Account = @Account AND CONVERT(date, CreateTime) = @Date";
                    SqlCommand cmd = new SqlCommand(sql, X);
                    cmd.Parameters.AddWithValue("@Account", account);
                    cmd.Parameters.AddWithValue("@Date", currentDate.Date);
                    object result = cmd.ExecuteScalar();
                    calories[i] = result != DBNull.Value ? Convert.ToInt32(result) : 0;

                    // 查詢當日餐點記錄
                    string mealSql = "SELECT Food, Calories FROM Diary WHERE Account = @Account AND CONVERT(date, CreateTime) = @Date";
                    SqlCommand mealCmd = new SqlCommand(mealSql, X);
                    mealCmd.Parameters.AddWithValue("@Account", account);
                    mealCmd.Parameters.AddWithValue("@Date", currentDate.Date);
                    SqlDataReader reader = mealCmd.ExecuteReader();
                    string mealDetails = "";
                    while (reader.Read())
                    {
                        string food = reader["Food"]?.ToString() ?? "未知餐點";
                        int mealCalories = reader["Calories"] != DBNull.Value ? Convert.ToInt32(reader["Calories"]) : 0;
                        mealDetails += $"{food} ({mealCalories} kcal), ";
                    }
                    reader.Close();
                    weekData[currentDate.ToString("yyyy-MM-dd")] = string.IsNullOrEmpty(mealDetails) ? "無記錄" : mealDetails.TrimEnd(',', ' ');
                }
            }
            catch (Exception ex)
            {
                // 錯誤處理
                ViewBag.Error = "無法載入數據：" + ex.Message;
            }
            finally
            {
                X.Close();
            }

            ViewBag.Dates = dates;
            ViewBag.Calories = calories;
            ViewBag.WeekData = weekData;
            ViewBag.SelectedDate = startDate.ToString("yyyy-MM-dd");

            return View("~/Views/Diary/Analysis2Area.cshtml");
        }
        public ActionResult Analysis3Index(string selectedDate)
{
    string account = Session["LoginUser"]?.ToString();
    ViewBag.Account = account;

    // 設定近七天日期和營養比例數據
    DateTime startDate = string.IsNullOrEmpty(selectedDate) ? DateTime.Today : DateTime.Parse(selectedDate);
    Dictionary<string, string> weekData = new Dictionary<string, string>();
    decimal totalCarbs = 0, totalFat = 0, totalProtein = 0;

    try
    {
        X.Open();
        for (int i = 0; i < 7; i++)
        {
            DateTime currentDate = startDate.AddDays(-6 + i);

            // 查詢當日餐點記錄並計算營養總和
            string sql = @"
                SELECT d.Food, d.Calories, d.Carbs, d.Fat, d.Protein 
                FROM Diary d 
                WHERE d.Account = @Account AND CONVERT(date, d.CreateTime) = @Date";
            SqlCommand cmd = new SqlCommand(sql, X);
            cmd.Parameters.AddWithValue("@Account", account);
            cmd.Parameters.AddWithValue("@Date", currentDate.Date);
            SqlDataReader reader = cmd.ExecuteReader();

            string mealDetails = "";
            while (reader.Read())
            {
                string food = reader["Food"]?.ToString() ?? "未知餐點";
                int calories = reader["Calories"] != DBNull.Value ? Convert.ToInt32(reader["Calories"]) : 0;
                decimal carbs = reader["Carbs"] != DBNull.Value ? Convert.ToDecimal(reader["Carbs"]) : 0;
                decimal fat = reader["Fat"] != DBNull.Value ? Convert.ToDecimal(reader["Fat"]) : 0;
                decimal protein = reader["Protein"] != DBNull.Value ? Convert.ToDecimal(reader["Protein"]) : 0;

                mealDetails += $"{food} ({calories} kcal), ";
                totalCarbs += carbs;
                totalFat += fat;
                totalProtein += protein;
            }
            reader.Close();
            weekData[currentDate.ToString("yyyy-MM-dd")] = string.IsNullOrEmpty(mealDetails) ? "無記錄" : mealDetails.TrimEnd(',', ' ');
        }
    }
    catch (Exception ex)
    {
        ViewBag.Error = "無法載入數據：" + ex.Message;
    }
    finally
    {
        X.Close();
    }

    // 設定圓餅圖數據
    ViewBag.Labels = new string[] { "碳水化合物", "脂肪", "蛋白質" };
    ViewBag.Values = new decimal[] { totalCarbs, totalFat, totalProtein };
    ViewBag.WeekData = weekData;
    ViewBag.SelectedDate = startDate.ToString("yyyy-MM-dd");

    return View("~/Views/Diary/Analysis3Area.cshtml");
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
        //public ActionResult Index(string date)
        //{
        //    DateTime selectedDate;
        //    if (!DateTime.TryParse(date, out selectedDate))
        //    {
        //        selectedDate = DateTime.Today;
        //    }

        //    // 篩選同一天的紀錄（不管時間）
        //    //var entries = db.DiaryEntries
        //    //.Where(e => DbFunctions.TruncateTime(e.CreateTime) == selectedDate.Date)
        //    //.OrderByDescending(e => e.CreateTime)
        //    //.ToList();

        //    return View();
        //}
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
                        BirthDay = reader["BirthDay"].ToString(),
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