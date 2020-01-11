using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LuckyDrawApplication.Controllers
{
    public class ProjectController : Controller
    {
        // GET: Project
        public ActionResult Index()
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;

            List<Models.ProjectView> projectList = GetProjectViewList();

            return View(projectList);
        }

        // GET: Project/Create
        public ActionResult Create()
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;
            ViewBag.Events = LoginController.GetEventList();

            return View();
        }

        // POST: Project/Create
        [HttpPost]
        public ActionResult Create(Models.ProjectView projectView)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;
            ViewBag.Events = LoginController.GetEventList();

            try
            {
                if (ModelState.IsValid)
                {
                    if (projectView.PrizeData != null)
                    {
                        Tuple<bool, string> results = CreateNewProject(projectView);

                        if (results.Item1)
                            return RedirectToAction("Index", "Project");
                        else
                        {
                            ModelState.AddModelError(string.Empty, results.Item2);
                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Lucky draw prizes data file is missing!");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Creation of project failed. Invalid field columns!");
                    return View();
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, "Creation of project failed. Database error!");
                return View();
            }
        }

        // GET: Project/Edit/5
        public ActionResult Edit(int id)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;
            Models.ProjectView projectView = GetProjectView(id);
            ViewBag.Events = LoginController.GetEventList();

            return View(projectView);
        }

        // POST: Project/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, Models.ProjectView projectView)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;
            ViewBag.Events = LoginController.GetEventList();

            try
            {
                if (ModelState.IsValid)
                {
                    if (projectView.PrizeData != null)
                    {
                        Tuple<bool, string> results = EditExistingProject(projectView);

                        if (results.Item1)
                            return RedirectToAction("Index", "Project");
                        else
                        {
                            ModelState.AddModelError(string.Empty, results.Item2);
                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Lucky draw prizes data file is missing!");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Edition of project failed. Invalid field columns!");
                    return View();
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, "Edition of project failed. Database error!");
                return View();
            }
        }

        // GET: Project/Delete/5
        public ActionResult Delete(int id)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;
            Models.ProjectView projectView = GetProjectView(id);

            return View(projectView);
        }

        // POST: Project/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, Models.ProjectView projectView)
        {
            Models.Admin a_user = (Models.Admin)Session["admin"];
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (a_user == null || luckydrawevent == null)
                return RedirectToAction("AdminIndex", "Login");

            ViewBag.Name = a_user.Name;

            try
            {
                bool deleted = DeleteExistingProject(id);

                if (deleted)
                    return RedirectToAction("Index", "Project");
                else
                {
                    ViewBag.ErrorMessage = "Deletion of event failed due to database problem!";
                    return View();
                }
            }
            catch
            {
                ViewBag.ErrorMessage = "Deletion of project failed.";
                return View();
            }
        }

        //Get Project List
        [NonAction]
        public List<Models.ProjectView> GetProjectViewList()
        {
            List<Models.ProjectView> Projects = new List<Models.ProjectView>();

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
                    sb.Append("SELECT project.*, event.EventLocation, event.EventCode FROM project INNER JOIN event ON project.EventID = event.EventID");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader rd = command.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                Models.ProjectView pv = new Models.ProjectView();
                                pv.ProjectID = Convert.ToInt32(rd["ProjectID"].ToString());
                                pv.ProjectName = rd["ProjectName"].ToString();
                                pv.EventID = Convert.ToInt32(rd["EventID"].ToString());
                                pv.NoOfProject = Convert.ToInt32(rd["NoOfProject"].ToString());
                                pv.PrizeCategory = rd["PrizeCategory"].ToString();
                                pv.EventName = rd["EventCode"].ToString() + " - " + rd["EventLocation"].ToString();
                                Projects.Add(pv);
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            return Projects;
        }

        //Create new project
        [NonAction]
        public Tuple<bool, string> CreateNewProject(Models.ProjectView projectView)
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
                    sb.Append("INSERT INTO project(ProjectName, EventID, NoOfProject, PrizeCategory) VALUES ('" + projectView.ProjectName.ToUpper() + "', " + projectView.EventID + ", 0, '" + projectView.PrizeCategory.Trim() + "'); SELECT SCOPE_IDENTITY() AS id;");

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

                return UploadLuckyDrawPrizeData(last_inserted_id, projectView);
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return new Tuple<bool, string>(false, "Error! Unable to create basic project");
            }
        }

        [NonAction]
        public Tuple<bool, string> UploadLuckyDrawPrizeData(int project_id, Models.ProjectView pv)
        {
            try
            {
                StreamReader csvreader = new StreamReader(pv.PrizeData.InputStream);

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "luckydrawapplication20200108092548dbserver.database.windows.net";
                builder.UserID = "sqladmin";
                builder.Password = "luckywheel123@";
                builder.InitialCatalog = "luckywheeldb";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO luckydraw(ProjectID, OrderNo, Prize) VALUES ");

                    while (!csvreader.EndOfStream)
                    {
                        string line = csvreader.ReadLine();
                        string[] values = line.Split(',');
                        sb.Append("(" + project_id + ", " + values[0] + ", " + values[1] + ") ,");
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

        // Get event
        [NonAction]
        public static Models.ProjectView GetProjectView(int id)
        {
            Models.ProjectView pv = new Models.ProjectView();

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
                    sb.Append("SELECT project.*, event.EventCode, event.EventLocation FROM project INNER JOIN event ON project.EventID = event.EventID WHERE project.ProjectID = " + id);
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader rd = command.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                pv.ProjectID = Convert.ToInt32(rd["ProjectID"].ToString());
                                pv.ProjectName = rd["ProjectName"].ToString();
                                pv.EventID = Convert.ToInt32(rd["EventID"].ToString());
                                pv.NoOfProject = Convert.ToInt32(rd["NoOfProject"].ToString());
                                pv.PrizeCategory = rd["PrizeCategory"].ToString();
                                pv.EventName = rd["EventCode"].ToString() + " - " + rd["EventLocation"].ToString();
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            return pv;
        }

        //Edit existing project
        [NonAction]
        public Tuple<bool, string> EditExistingProject(Models.ProjectView projectView)
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
                    sb.Append("UPDATE project SET ProjectName = '" + projectView.ProjectName.ToUpper() + "', EventID = " + projectView.EventID + ", NoOfProject = " + projectView.NoOfProject + ", PrizeCategory = '" + projectView.PrizeCategory.Trim() + "' WHERE ProjectID = " + projectView.ProjectID);
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        SqlDataReader rd = command.ExecuteReader();
                    }
                }

                return UploadEditedLuckyDrawPrizeData(projectView.ProjectID, projectView);
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return new Tuple<bool, string>(false, "File uploaded is not correctly formatted!");
            }
        }

        [NonAction]
        public Tuple<bool, string> UploadEditedLuckyDrawPrizeData(int project_id, Models.ProjectView pv)
        {
            try
            {
                StreamReader csvreader = new StreamReader(pv.PrizeData.InputStream);

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "luckydrawapplication20200108092548dbserver.database.windows.net";
                builder.UserID = "sqladmin";
                builder.Password = "luckywheel123@";
                builder.InitialCatalog = "luckywheeldb";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("DELETE FROM luckydraw WHERE ProjectID = " + project_id);
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
                return new Tuple<bool, string>(false, "File uploaded is not correctly formatted!");
            }

            try
            {
                StreamReader csvreader = new StreamReader(pv.PrizeData.InputStream);

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "luckydrawapplication20200108092548dbserver.database.windows.net";
                builder.UserID = "sqladmin";
                builder.Password = "luckywheel123@";
                builder.InitialCatalog = "luckywheeldb";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO luckydraw(ProjectID, OrderNo, Prize) VALUES ");

                    while (!csvreader.EndOfStream)
                    {
                        string line = csvreader.ReadLine();
                        string[] values = line.Split(',');
                        sb.Append("(" + project_id + ", " + values[0] + ", " + values[1] + ") ,");
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

        //Delete existing project
        [NonAction]
        public bool DeleteExistingProject(int id)
        {
            if ((!DeleteLuckyDrawData(id)) && (!DeleteUserData(id)))
                return false;

            else
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
                        sb.Append("DELETE FROM project WHERE ProjectID = " + id);
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
        }

        // Check if user data exists
        [NonAction]
        public static bool DeleteUserData(int id)
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
                    sb.Append("DELETE FROM users WHERE ProjectID = " + id);
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
        public static bool DeleteLuckyDrawData(int id)
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
                    sb.Append("DELETE FROM luckydraw WHERE ProjectID = " + id);
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

    }
}
