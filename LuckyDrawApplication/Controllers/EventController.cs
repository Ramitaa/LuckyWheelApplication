using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LuckyDrawApplication.Controllers
{
    public class EventController : Controller
    {
        // GET: Event
        public ActionResult Index()
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;

            List<Models.EventView> eventList = GetEventList();

            return View(eventList);
        }

        // GET: Event/Create
        public ActionResult Create()
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;

            return View();
        }

        // POST: Event/Create
        [HttpPost]
        public ActionResult Create(Models.EventView eventView)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;

            try
            {
                if(ModelState.IsValid)
                {
                    if (eventView.StaffPrizeData != null)
                    {
                        Tuple<bool, string> results = CreateNewEvent(eventView);

                        if (results.Item1)
                            return RedirectToAction("Index", "Event");
                        else
                        {
                            ModelState.AddModelError(string.Empty, results.Item2);
                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Staff Lucky draw prizes data file is missing!");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Creation of event failed. Invalid field columns!");
                    return View();
                }   
            }
            catch
            {
                ViewBag.ErrorMessage = "Creation of event failed.";
                return View();
            }
        }

        // GET: Event/Edit/5
        public ActionResult Edit(int id)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;
            Models.EventView eventView = GetEventView(id);

            return View(eventView);
        }

        // POST: Event/Edit/5
        [HttpPost]
        public ActionResult Edit(Models.EventView eventView)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;

            try
            {
                if (ModelState.IsValid)
                {
                    if (eventView.StaffPrizeData != null)
                    {
                        Tuple<bool, string> results = EditExistingEvent(eventView);

                        if (results.Item1)
                            return RedirectToAction("Index", "Event");
                        else
                        {
                            ModelState.AddModelError(string.Empty, results.Item2);
                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Staff lucky draw prizes data file is missing!");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Edition of event failed. Invalid field columns!");
                    return View();
                }
            }
            catch
            {
                ViewBag.ErrorMessage = "Modification of event failed.";
                return View();
            }
        }

        // GET: Event/Delete/5
        public ActionResult Delete(int id)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;
            Models.EventView eventView = GetEventView(id);

            return View(eventView);
        }

        // POST: Event/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, Models.EventView eventView)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;

            try
            {
                bool deleted = DeleteExistingEvent(id);

                if (deleted)
                    return RedirectToAction("Index", "Event");
                else
                {
                    ModelState.AddModelError(string.Empty, "Deletion of event failed due to database problem! Ensure all project data and lucky draw data associated with this event is deleted before deleting this event.");
                    return View();
                }
            }
            catch
            {
                ViewBag.ErrorMessage = "Deletion of event failed.";
                return View();
            }
        }

        //Create new event
        [NonAction]
        public Tuple<bool, string> CreateNewEvent(Models.EventView eventView)
        {
            int last_inserted_id = 0;

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "luckydrawapplication20200108092548dbserver.database.windows.net";
                builder.UserID = "sqladmin";
                builder.Password = "luckywheel123@";
                builder.InitialCatalog = "luckywheeldb";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO event(EventCode, EventPassword, EventLocation) VALUES ('" + eventView.EventCode + "', '" + eventView.EventPassword + "', '" + eventView.EventLocation.ToUpper() + "'); SELECT SCOPE_IDENTITY() AS id;");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader rd = command.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                last_inserted_id = Convert.ToInt32(rd["id"]);
                            }
                        }
                    }
                }

                return UploadStaffLuckyDrawPrizeData(last_inserted_id, eventView);
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return new Tuple<bool, string>(false, "Error! Unable to create basic event");
            }
        }

        [NonAction]
        public Tuple<bool, string> UploadStaffLuckyDrawPrizeData(int eventID, Models.EventView ev)
        {
            try
            {
                StreamReader csvreader = new StreamReader(ev.StaffPrizeData.InputStream);

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "luckydrawapplication20200108092548dbserver.database.windows.net";
                builder.UserID = "sqladmin";
                builder.Password = "luckywheel123@";
                builder.InitialCatalog = "luckywheeldb";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO agent(orderNo, prizeAmount, won, EventID) VALUES ");

                    while (!csvreader.EndOfStream)
                    {
                        string line = csvreader.ReadLine();
                        string[] values = line.Split(',');
                        sb.Append("(" + values[0] + ", " + values[1] + ", 0, " + eventID + ") ,");
                    }

                    String sql = sb.ToString();
                    sql = sql.Substring(0, sql.Length - 1).ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        SqlDataReader rd = command.ExecuteReader();
                    }
                }

                return new Tuple<bool, string>(true, "");
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return new Tuple<bool, string>(false, "File uploaded is not correctly formatted!" + e.ToString());
            }
        }

        //Edit new event
        [NonAction]
        public Tuple<bool, string> EditExistingEvent(Models.EventView eventView)
        {

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "luckydrawapplication20200108092548dbserver.database.windows.net";
                builder.UserID = "sqladmin";
                builder.Password = "luckywheel123@";
                builder.InitialCatalog = "luckywheeldb";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("UPDATE event SET EventCode = '" + eventView.EventCode + "', EventPassword = '" + eventView.EventPassword + "', EventLocation = '" + eventView.EventLocation.ToUpper() + "' WHERE EventID = " + eventView.EventID);
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        SqlDataReader rd = command.ExecuteReader();
                    }
                }

                return UploadEditedStaffLuckyDrawPrizeData(eventView);
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return new Tuple<bool, string>(false, "File uploaded is not correctly formatted!");
            }
        }

        [NonAction]
        public Tuple<bool, string> UploadEditedStaffLuckyDrawPrizeData(Models.EventView ev)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "luckydrawapplication20200108092548dbserver.database.windows.net";
                builder.UserID = "sqladmin";
                builder.Password = "luckywheel123@";
                builder.InitialCatalog = "luckywheeldb";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("DELETE FROM agent WHERE EventID = " + ev.EventID);
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        SqlDataReader rd = command.ExecuteReader();
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return new Tuple<bool, string>(false, "Previous data cannot be deleted!");
            }

            try
            {
                StreamReader csvreader = new StreamReader(ev.StaffPrizeData.InputStream);

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "luckydrawapplication20200108092548dbserver.database.windows.net";
                builder.UserID = "sqladmin";
                builder.Password = "luckywheel123@";
                builder.InitialCatalog = "luckywheeldb";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO agent(orderNo, prizeAmount, won, EventID) VALUES ");

                    while (!csvreader.EndOfStream)
                    {
                        string line = csvreader.ReadLine();
                        string[] values = line.Split(',');
                        sb.Append("(" + values[0] + ", " + values[1] + ", 0, " + ev.EventID + ") ,");
                    }

                    String sql = sb.ToString();
                    sql = sql.Substring(0, sql.Length - 1).ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        SqlDataReader rd = command.ExecuteReader();
                    }
                }

                return new Tuple<bool, string>(true, "");
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return new Tuple<bool, string>(false, "File uploaded is not correctly formatted!");
            }
        }

        //Delete existing event
        [NonAction]
        public bool DeleteExistingEvent(int id)
        {
            if (CheckIfDataExists(id))
                return false;

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "luckydrawapplication20200108092548dbserver.database.windows.net";
                builder.UserID = "sqladmin";
                builder.Password = "luckywheel123@";
                builder.InitialCatalog = "luckywheeldb";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("DELETE FROM event WHERE EventID = " + id);
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        SqlDataReader rd = command.ExecuteReader();
                    }
                }

                return true;
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        // Check if data exists
        [NonAction]
        public static bool CheckIfDataExists(int id)
        {
            int count = 0;

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "luckydrawapplication20200108092548dbserver.database.windows.net";
                builder.UserID = "sqladmin";
                builder.Password = "luckywheel123@";
                builder.InitialCatalog = "luckywheeldb";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT COUNT(ProjectID) AS count FROM project WHERE EventID = " + id);
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader rd = command.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                count = Convert.ToInt32(rd["count"].ToString());
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            return count > 0;
        }

        // Get event
        [NonAction]
        public static Models.EventView GetEventView(int id)
        {
            Models.EventView eventView = new Models.EventView();

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "luckydrawapplication20200108092548dbserver.database.windows.net";
                builder.UserID = "sqladmin";
                builder.Password = "luckywheel123@";
                builder.InitialCatalog = "luckywheeldb";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * FROM event WHERE EventID = " + id);
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader rd = command.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                eventView.EventID = Convert.ToInt32(rd["EventID"].ToString());
                                eventView.EventCode = rd["EventCode"].ToString();
                                eventView.EventPassword = "";
                                eventView.EventLocation = rd["EventLocation"].ToString();
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            return eventView;
        }

        //Get Event List
        [NonAction]
        public List<Models.EventView> GetEventList()
        {
            List<Models.EventView> Events = new List<Models.EventView>();

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "luckydrawapplication20200108092548dbserver.database.windows.net";
                builder.UserID = "sqladmin";
                builder.Password = "luckywheel123@";
                builder.InitialCatalog = "luckywheeldb";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * FROM event");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader rd = command.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                Models.EventView ev = new Models.EventView();
                                ev.EventID = Convert.ToInt32(rd["EventID"].ToString());
                                ev.EventCode = rd["EventCode"].ToString();
                                ev.EventLocation = rd["EventLocation"].ToString();
                                Events.Add(ev);
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            return Events;
        }
    }
}
