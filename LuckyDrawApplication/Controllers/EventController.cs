using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
                    bool created = CreateNewEvent(eventView);

                    if (created)
                        return RedirectToAction("Index", "Event");
                    else
                    {
                        ViewBag.ErrorMessage = "Creation of event failed due to database problem!";
                        return View();
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Creation of event failed. Invalid field columns!";
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
                    bool edited = EditExistingEvent(eventView);

                    if (edited)
                        return RedirectToAction("Index", "Event");
                    else
                    {
                        ViewBag.ErrorMessage = "Modification of event failed due to database problem!";
                        return View();
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Modification of event failed. Invalid field columns!";
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
        public bool CreateNewEvent(Models.EventView eventView)
        {
            string salt = LoginController.getSalt();
            string hash = LoginController.createPasswordHash(salt, eventView.EventPassword);

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
                    sb.Append("INSERT INTO event(EventCode, EventPassword, EventSalt, EventLocation) VALUES ('" + eventView.EventCode + "', '" + hash + "', '" + salt + "', '" + eventView.EventLocation.ToUpper() + "')");
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

        //Edit new event
        [NonAction]
        public bool EditExistingEvent(Models.EventView eventView)
        {
            string salt = LoginController.getSalt();
            string hash = LoginController.createPasswordHash(salt, eventView.EventPassword);

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
                    sb.Append("UPDATE event SET EventCode = '" + eventView.EventCode + "', EventPassword = '" + hash + "', EventSalt = '" + salt + "', EventLocation = '" + eventView.EventLocation.ToUpper() + "' WHERE EventID = " + eventView.EventID);
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
