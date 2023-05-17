using Microsoft.Ajax.Utilities; 
using Microsoft.SqlServer.Server;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.SqlClient;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.UI.WebControls;
using System.Web.Util;
using UrestComplaintWebApi.Models;
using EventTask = UrestComplaintWebApi.Models.EventTask;
using File = System.IO.File;

namespace UrestComplaintWebApi.Controllers
{
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UrestController : ApiController
    {
        string constr = string.Empty;
        Integrations integrations = new Integrations();
        public UrestController()
        {
            constr = ConfigurationManager.ConnectionStrings["adoConnectionstring"].ConnectionString;

        }

        // POST: api/MemberComplaintRegistrationsSignup
        [Route("Signup")]
        [HttpPost]
        public IHttpActionResult Signup(MemberComplaintRegistration e)
        {

            string errorMsg = "", succMsg = "Registration successful !";

            if (e.Mobile.Length != 10 || !Utilities.IsNumeric(e.Mobile.ToString()))
            { return Ok("Invalid Mobile No. !"); }

            if (e.Name.Length == 0 || e.Name == "string")
            { return Ok("Invalid User Name !"); }

            if (e.Email == "string")
            { return Ok("Invalid email id !"); }

            if (e.Password.Length == 0 || e.Password == "string" || e.ConfirmPassword.Length == 0 || e.ConfirmPassword == "string")
            { return Ok("Invalid Password !"); }

            if (e.Password != e.ConfirmPassword)
            { return Ok("Password and Confirm Password not matching !"); }

            try
            {
                MemberComplaintRegistration er = new MemberComplaintRegistration();
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();

                    using (SqlCommand cmd0 = new SqlCommand())
                    {
                        cmd0.Connection = con;
                        cmd0.CommandType = CommandType.Text;
                        cmd0.CommandText = "select count(Mobile) from [app].[MemberComplaintRegistration] where mobile=@Mobile";
                        cmd0.Parameters.AddWithValue("@Mobile", e.Mobile);
                        var resp = cmd0.ExecuteScalar();
                        if (Convert.ToInt32(resp) > 0)
                        { errorMsg = "Mobile No. " + e.Mobile + "  Already Exists !"; }
                        else
                        {
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Connection = con;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = "SP_MemberComplaintRegistration";
                                cmd.Parameters.AddWithValue("@Mobile", e.Mobile);
                                cmd.Parameters.AddWithValue("@Name", e.Name);
                                cmd.Parameters.AddWithValue("@Email", e.Email);
                                cmd.Parameters.AddWithValue("@Password", e.Password);
                                cmd.Parameters.AddWithValue("@ConfirmPassword", e.ConfirmPassword);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    con.Close();
                }
                if (errorMsg.Length > 0)
                {
                    return Ok(errorMsg);
                }
                else
                {
                    return Ok(succMsg);
                }
            }
            catch (Exception ex)
            { return Ok(ex.Message); }
        }

        // POST: api/MemberComplaintRegistrationsSignin
        [Route("Signin")]
        [HttpPost]
        public IHttpActionResult Signin(Signin e)
        {
            DataSet ds = new DataSet();
            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();
                string Query = "select Mobile,Password from [App].[MemberComplaintRegistration] where Mobile=@Mobile and Password=@Password";
                SqlCommand cmd = new SqlCommand(Query, con);
                cmd.Parameters.AddWithValue("@Mobile", e.Mobile);
                cmd.Parameters.AddWithValue("@Password", e.Password);
                var mob = e.Mobile;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                con.Close();
            }

            var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new Signin
            {
                Mobile = dataRow.Field<string>("Mobile"),
                Password = dataRow.Field<string>("Password")
            })).ToList();

            return Ok(eList);
        }

        [Route("SendOTPSMS")]
        [HttpPost]
        public async Task<string> SendOTPSMS(string ToSendMobileNo)
        {
            return await integrations.SendOTP(ToSendMobileNo, 4);
        }

        [HttpGet]
        public IHttpActionResult Complaint(string Mobile)
        {

            string connetionString = null;
            SqlConnection connection;
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            SqlParameter param;
            DataSet ds = new DataSet();
            DataSet dss = new DataSet();
            DataSet dsss = new DataSet();


            int i = 0;

            //connetionString = "Data Source=WIN-HBSI1RRBVE0;Initial Catalog=UfirmApp_Production;integrated security=true";
            connection = new SqlConnection(constr);

            connection.Open();
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[App].[GetComplaints]";

            param = new SqlParameter("@Mobile", Mobile);
            param.Direction = ParameterDirection.Input;
            param.DbType = DbType.String;
            command.Parameters.Add(param);

            adapter = new SqlDataAdapter(command);
            adapter.Fill(ds);


            //selecting Name

            command.CommandType = CommandType.Text;
            command.CommandText = "select Name from [App].[PropertyMember] where ContactNumber = @Mobiles";
            param = new SqlParameter("@Mobiles", Mobile);
            param.Direction = ParameterDirection.Input;
            param.DbType = DbType.String;
            command.Parameters.Add(param);
            //adapter = new SqlDataAdapter(command);
            //adapter.Fill(dss);



            //Selecting Flatname
            command.CommandType = CommandType.Text;
            command.CommandText = "select Flat from [App].[PropertyDetails] where PropertyDetailsId = @PropertyDetaildId";
            param = new SqlParameter("@PropertyDetaildId", 7);
            param.Direction = ParameterDirection.Input;
            param.DbType = DbType.String;
            command.Parameters.Add(param);
            adapter = new SqlDataAdapter(command);
            adapter.Fill(dsss);
            //var listes = dss.Tables[0].AsEnumerable().Select(dataRow => new PropertyMember { Name = dataRow.Field<string>("Name") }).ToList();

            var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new Ticket
            {
                TicketNumber = dataRow.Field<string>("TicketNumber"),
                Description = dataRow.Field<string>("Description"),
                TicketOrigin = dataRow.Field<string>("TicketOrigin"),
                Title = dataRow.Field<string>("Title"),
                Visibility = dataRow.Field<string>("Visibility"),
                TicketId = dataRow.Field<int>("TicketId"),
                PropertyDetaildId = dataRow.Field<int>("PropertyDetaildId"),
                StatusTypeId = dataRow.Field<int>("StatusTypeId")
            })).ToList();






            connection.Close();


            return Ok(eList);

        }

        [Route("GetCategory")]
        [HttpGet]
        public IHttpActionResult GetCategory(int categoryId = 0)
        {
            Category cat = new Category()
            { CategoryId = categoryId };
            if (categoryId == 0)
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    SqlConnection connection;
                    SqlDataAdapter adapter;
                    SqlCommand command = new SqlCommand();
                    SqlParameter param;
                    DataSet ds = new DataSet();
                    DataSet dss = new DataSet();
                    DataSet dsss = new DataSet();


                    int i = 0;

                    connection = new SqlConnection(constr);

                    connection.Open();
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[App].[GetAllCategory]";

                    adapter = new SqlDataAdapter(command);
                    adapter.Fill(ds);

                    var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new Category
                    {
                        SubCategoryId = dataRow.Field<int>("SubCategoryId"),
                        CategoryId = dataRow.Field<int>("CategoryId"),
                        SubCategoryName = dataRow.Field<string>("SubCategoryName")

                    })).ToList();
                    connection.Close();
                    return Ok(eList);



                }
            }
            else if (categoryId != null)
            {
                using (SqlConnection con = new SqlConnection(constr))
                {


                    SqlConnection connection;
                    SqlDataAdapter adapter;
                    SqlCommand command = new SqlCommand();
                    SqlParameter param;
                    DataSet ds = new DataSet();
                    DataSet dss = new DataSet();
                    DataSet dsss = new DataSet();


                    int i = 0;

                    //connetionString = "Data Source=WIN-HBSI1RRBVE0;Initial Catalog=UfirmApp_Production;integrated security=true";
                    connection = new SqlConnection(constr);

                    connection.Open();
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[App].[GetCategoryById]";

                    param = new SqlParameter("@CategoryId", cat.CategoryId);
                    param.Direction = ParameterDirection.Input;
                    param.DbType = DbType.String;
                    command.Parameters.Add(param);

                    adapter = new SqlDataAdapter(command);
                    adapter.Fill(ds);
                    var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new Category
                    {
                        SubCategoryId = dataRow.Field<int>("SubCategoryId"),
                        CategoryId = dataRow.Field<int>("CategoryId"),
                        SubCategoryName = dataRow.Field<string>("SubCategoryName")

                    })).ToList();
                    connection.Close();

                    return Ok(eList);
                }
            }
            return Ok();
        }

        [Route("QuestionsDetailsOfTask")]
        [HttpGet]
        public IHttpActionResult QuestionsDetailsOfTask(int taskID)
        {
            SqlConnection connection = new SqlConnection(constr);
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            SqlParameter param;
            DataSet ds = new DataSet();

            try
            {
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                bool exits = false;
                command.CommandText = "select t.TransactionID, q.QuestionID, q.QuestionName, t.TaskName, tm.Occurence, t.Action, t.Remarks from TaskWiseTransaction t inner join TaskMaster tm on tm.Name=t.TaskName right join TaskWiseQuestionnaire q on tm.id=q.TaskID and t.QuestID=q.QuestionID where q.taskid=@taskID order by TransactionID"; 
                param = new SqlParameter("@taskID", taskID);
                param.Direction = ParameterDirection.Input;
                param.DbType = DbType.String;
                command.Parameters.Add(param);
                adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        exits = true;
                    }
                }
                if (!exits)
                {
                    command.CommandText = "select 0 TransactionID, q.QuestionID, q.QuestionName, t.Name taskName, t.Occurence, '' Action, '' Remarks from TaskWiseQuestionnaire q inner join TaskMaster t on q.taskid = t.id where taskid = @taskID order by id";
                    adapter = new SqlDataAdapter(command);
                    adapter.Fill(ds);
                }
                var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new TaskWiseQuestionnaire
                {
                    TransactionID = dataRow["TransactionID"] == DBNull.Value ? 0 : dataRow.Field<int>("TransactionID"),
                    TaskID = taskID,
                    TaskName = dataRow["taskName"] == DBNull.Value ? "" : dataRow.Field<string>("taskName"),
                    Occurance = dataRow["Occurence"] == DBNull.Value ? "" : dataRow.Field<string>("Occurence"),
                    QuestionName = dataRow.Field<string>("QuestionName"),
                    QuestID = dataRow.Field<int>("QuestionID"),
                    Action = dataRow["Action"] == DBNull.Value ? "" : dataRow.Field<string>("Action"),
                    Remarks = dataRow["Remarks"] == DBNull.Value ? "" : dataRow.Field<string>("Remarks")
                })).ToList();
                return Ok(eList);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            finally
            {
                ds.Dispose();
                connection.Close();
            }
            return Ok();
        }

        [Route("NoticeBoardSave0")]
        [HttpPost]
        public IHttpActionResult CreateNoticeBoard0(FormDataNoticeBoard model, int userid)
        {
            SqlConnection con = new SqlConnection(constr);
            con.Open();
            int returnvalue = 0;
            
            try
            {
                //string wwwPath = _hostEnvironment.WebRootPath;
                //string path = Path.Combine(wwwPath, "Notifications");

                if (model.PropertyId != 0 && model.NotificationTypeId != 0 && model.PropertyGroupId != 0 && model.Notify != 0 && model.AlertTypeId != 0 && model.Subject != "")
                {
                    List<NoticeBoardAttachment> finalAttachmentData = new List<NoticeBoardAttachment>();

                    //if (model.NoticeBoardAttachment.Count > 0)
                    //{
                    //    string foldername = DateTime.Now.ToString("ddMMyyyy");
                    //    string notificationDir = path + "\\" + foldername;
                    //    if (!Directory.Exists(notificationDir))
                    //    {
                    //        Directory.CreateDirectory(notificationDir);
                    //    }
                    //    foreach (var item in model.NoticeBoardAttachment)
                    //    {
                    //        NoticeBoardAttachment noticeBoardAttachment = new NoticeBoardAttachment();

                    //        string splitFilename = item.filename.Substring(0, item.filename.LastIndexOf('.'));
                    //        string splitFilenameext = item.filename.Substring(item.filename.LastIndexOf('.') + 1);
                    //        string newFilename = splitFilename + DateTime.Now.ToString("yyyyMMddhhmmss") + "." + splitFilenameext;
                    //        string ffilename = notificationDir + "\\" + newFilename;

                    //        string dbpath = foldername + "\\" + newFilename;

                    //        File.WriteAllBytes(ffilename, Convert.FromBase64String(item.filepath));
                    //        noticeBoardAttachment.filename = newFilename;
                    //        noticeBoardAttachment.filepath = dbpath;

                    //        finalAttachmentData.Add(noticeBoardAttachment);
                    //    }
                    //}

                    DataTable NoticeBoardAttachments = new DataTable();// CommonService.ToDataTable(finalAttachmentData);
                    DateTime? expirydate = null;
                    if (!string.IsNullOrEmpty(model.ExpirtyDate))
                    {
                        expirydate = DateTime.Parse(model.ExpirtyDate.Trim());
                    }

                    System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand();

                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[App].[ManageNoticeBoard]";

                    command.Parameters.AddWithValue("PropertyId", Convert.ToString(model.PropertyId));
                    command.Parameters.AddWithValue("Subject", Convert.ToString(model.Subject));
                    command.Parameters.AddWithValue("Message", Convert.ToString(model.Message));
                    command.Parameters.AddWithValue("NotificationTypeId", Convert.ToString(model.NotificationTypeId));
                    command.Parameters.AddWithValue("PropertyGroupId", Convert.ToString(model.PropertyGroupId));
                    command.Parameters.AddWithValue("Notify", Convert.ToString(model.Notify));
                    command.Parameters.AddWithValue("AlertTypeId", Convert.ToString(model.AlertTypeId));
                    command.Parameters.AddWithValue("ExpirtyDate", expirydate);
                    command.Parameters.AddWithValue("NoticeBoardAttachment", NoticeBoardAttachments);
                    command.Parameters.AddWithValue("PropertyDetailsId", model.PropertyDetailsId);
                    command.Parameters.AddWithValue("PropertyTowerId", model.PropertyTowerId);
                    //{"RolesIds",  model.PropertyRWAMemberId},
                    command.Parameters.AddWithValue("CmdType", model.StatementType);
                    command.Parameters.AddWithValue("CurrentUserId", userid.ToString());

                    command.Parameters.Add("@ret", SqlDbType.Int);
                    command.Parameters["@ret"].Direction = ParameterDirection.Output;

                    command.Connection = con;
                    int retVal = command.ExecuteNonQuery();

                    // get inserted notification id
                    returnvalue = Convert.ToInt32(command.Parameters["@ret"].Value);
                    //if record not saved into db then delete saved files
                    //if (returnvalue <= 0)
                    //{
                    //    if (NoticeBoardAttachments.Rows.Count > 0)
                    //    {
                    //        foreach (DataRow row in NoticeBoardAttachments.Rows)
                    //        {
                    //            var items = row.ItemArray;
                    //            string filePath = path + "\\" + items[1].ToString();
                    //            File.Delete(filePath);
                    //        }
                    //    }
                    //}
                    //if (returnvalue > 0)
                    //{
                    //    List<NoticeBoardNoticeData> emailData = await GetNoticeBoardsNoticeData("R", returnvalue);

                    //    List<NoticeBoardAttachments> attachments = await GetNoticeBoardsAttachment("NATT", model.PropertyId, returnvalue);
                    //    string body = string.Empty;
                    //    foreach (var item in emailData)
                    //    {
                    //        string fSubject = $"{item.NotificationType} - {item.Subject}";
                    //        string fbody = item.Message;
                    //        if (attachments.Count > 0)
                    //        {
                    //            fbody += "<br>Please Click to Download Below Attachments <br>";
                    //            int i = 1;
                    //            foreach (var att in attachments)
                    //            {
                    //                fbody += $"<br <br> {i}. <a href='{att.FilePath}'> {att.FileName} <a/>";
                    //                i++;
                    //            }
                    //        }

                    //        //BackgroundJob.Schedule(() => emailHelper.SendNoticeBoardMail(fSubject, "rakhmaji.ghule@gmail.com", fbody, item.NoticeboardAlertMaster), TimeSpan.FromSeconds(30));
                    //        if (!string.IsNullOrEmpty(item.EmailAddress))
                    //        {
                    //            BackgroundJob.Schedule(() => emailHelper.SendNoticeBoardMail(fSubject, item.EmailAddress, fbody, item.NoticeboardAlertMaster), TimeSpan.FromSeconds(30));
                    //        }

                    //        //emailHelper.SendNoticeBoardMail(fSubject, "rakhmaji.ghule@gmail.com", fbody, item.NoticeboardAlertMaster);
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            con.Close();
            return Ok(returnvalue);
        }

        [Route("AmenitiesBookingApprove")]
        [HttpPost]
        public IHttpActionResult AmenitiesBookingApprove(int Id)
        {
            SqlConnection connection = new SqlConnection(constr);
            SqlCommand command = new SqlCommand();
            SqlParameter param;
            DataSet ds = new DataSet();
            string msg = "Approved Successfully !";
            connection.Open();

            try
            {
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = "update [dbo].[AmenitiesBooking] set Approved = 1 where Id=@Id";
                param = new SqlParameter("@Id", Id);
                param.Direction = ParameterDirection.Input;
                param.DbType = DbType.String;
                command.Parameters.Add(param);
                command.ExecuteNonQuery();

                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = "Select * from [dbo].[AmenitiesBooking] where Id=@Id";
                param = new SqlParameter("@Id", Id);
                param.Direction = ParameterDirection.Input;
                param.DbType = DbType.String;
                command.Parameters.Add(param);

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                string MobileNo = "", UserID=ds.Tables[0].Rows[0]["userid"].ToString();
                string sql = "select ContactNumber from [Identity].Users where userid= "+ UserID;
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = sql;
                MobileNo = command.ExecuteScalar().ToString();
                string ameName = "";//
                sql = "select AmenitiesName from [master].amenities where AmenitiesId = " + ds.Tables[0].Rows[0]["AmenitiesId"].ToString();
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = sql;
                ameName = command.ExecuteScalar().ToString();

                sql = "select top(1) PropertyId from [App].[UserPropertyAssignment] where userid=" + UserID + " order by UserPropertyAssignmentId desc";
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = sql;
                string propertyID = command.ExecuteScalar() == null ? "1" : command.ExecuteScalar().ToString();
                if (command.ExecuteScalar() == null || command.ExecuteScalar() == DBNull.Value)
                {
                    sql = "select top 1 PropertyId from [App].[PropertyDetails] where ContactNumber = '" + MobileNo + "'";
                    command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sql;
                    propertyID = command.ExecuteScalar() == null ? "1" : command.ExecuteScalar().ToString();
                }

                string sp = "INSERT INTO app.Notification (PropertyId,Subject,Message,NotificationTypeId,PropertyGroupId,IsActive,IsDeleted,CreatedOn,CreatedBy,ExpirtyDate) VALUES (@PropertyId,@Subject,@Message,@NotificationTypeId,@PropertyGroupId,@IsActive,0,GETDATE(),@CurrentUserId,@ExpirtyDate)";//"App.ManageNoticeBoard_Amen";

                SqlConnection con = new SqlConnection(constr);
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sp;
                cmd.Parameters.AddWithValue("@CmdType", "C");
                cmd.Parameters.AddWithValue("@NotificationId", 1);
                cmd.Parameters.AddWithValue("@PropertyId", propertyID);
                cmd.Parameters.AddWithValue("@Subject", "Amenity Booking #" + ds.Tables[0].Rows[0]["id"].ToString() + " Approved !");
                string message = "<p>Your " + ameName + " booking #" + ds.Tables[0].Rows[0]["id"].ToString() + " for " + ds.Tables[0].Rows[0]["NosOfPersons"].ToString() + " persons on time slot " + Convert.ToDateTime(ds.Tables[0].Rows[0]["TimeSlotFr"]).ToString("hh:mm") + " to " + Convert.ToDateTime(ds.Tables[0].Rows[0]["TimeSlotFr"]).ToString("hh:mm") + " has been approved !</p>";
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@NotificationTypeId", "1");
                cmd.Parameters.AddWithValue("@PropertyGroupId", "3");
                cmd.Parameters.AddWithValue("@ExpirtyDate", DateTime.Now.Date.AddDays(3));
                cmd.Parameters.AddWithValue("@IsActive", 1);
                cmd.Parameters.AddWithValue("@PropertyDetailsId", "");
                cmd.Parameters.AddWithValue("@PropertyTowerId", "");
                cmd.Parameters.AddWithValue("@Notify", "1");
                cmd.Parameters.AddWithValue("@AlertTypeId", "1");
                cmd.Parameters.AddWithValue("@IsSent", 0);
                cmd.Parameters.AddWithValue("@CurrentUserId", UserID);

                con.Open();
                cmd.ExecuteNonQuery();

                con.Close();
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            connection.Close();

            return Ok(msg);
        }

        [Route("KYCApprove")]
        [HttpPost]
        public IHttpActionResult KYCApprove(int Id)
        {
            SqlConnection connection = new SqlConnection(constr);
            SqlCommand command = new SqlCommand();
            SqlParameter param;
            DataSet ds = new DataSet();
            string msg = "Approved Successfully !";
            connection.Open();

            try
            {
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = "update [dbo].[kycdetails] set Approved = 1 where Id=@Id";
                param = new SqlParameter("@Id", Id);
                param.Direction = ParameterDirection.Input;
                param.DbType = DbType.String;
                command.Parameters.Add(param);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            connection.Close();

            return Ok(msg);
        }

        [Route("KYCUpate")]
        [HttpPost]
        public IHttpActionResult KYCUpate(KycDetails kycDetails)
        {
            //string path = HttpContext.Current.Server.MapPath("~");

            SqlConnection connection = new SqlConnection(constr);
            SqlCommand command = new SqlCommand();
            SqlParameter param;
            DataSet ds = new DataSet();
            string msg = "Updated Successfully !";
            connection.Open();

            try
            {
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                command.CommandText = "update [dbo].[kycdetails] set EmployeeId = '" + kycDetails.EmployeeId + "', EmployeeName = '" + kycDetails.EmployeeName + "', JobProfile = '" + kycDetails.JobProfile + "', Gender ='" + kycDetails.Gender + "', IdDoc = '" + kycDetails.IdDoc + "' where Id=@Id";

                param = new SqlParameter("@Id", kycDetails.Id);
                param.Direction = ParameterDirection.Input;
                param.DbType = DbType.String;
                command.Parameters.Add(param);
                command.ExecuteNonQuery();

                if (kycDetails.Image != null)
                {
                    if (kycDetails.Image.Length > 0)
                    {
                        byte[] image = Convert.FromBase64String(kycDetails.Image);

                        SqlCommand sqlCommand = new SqlCommand();
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.Connection = connection;

                        sqlCommand.CommandText = "delete from KycImages where kycid='" + kycDetails.Id + "'";// and description = '" + "" + "'";
                        sqlCommand.ExecuteScalar();

                        sqlCommand.CommandText = "Insert into KycImages([Title],[Description],[ProfileImage],[Contents],[KycID],[Image]) values('" + kycDetails.EmployeeId + "', 'ProfileImage', @Image, ' ', " + kycDetails.Id + ",' ')";
                        sqlCommand.Parameters.AddWithValue("@Image", image);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            connection.Close();

            return Ok(msg);
        }

        [Route("AmenitiesBookings")]
        [HttpGet]
        public IHttpActionResult AmenitiesBookings(int PropertyID, int UserID = 0, string DateFr = null, string DateTo = null)
        {
            SqlConnection connection = new SqlConnection(constr);
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            SqlParameter param;
            DataSet ds = new DataSet();

            try
            {
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                command.CommandText = "select amt.id, amt.UserId, amm.AmenitiesId, amm.AmenitiesName, CONCAT(format(amt.timeslotfr, 'hh:mm'), ' to ', format(amt.timeslotto, 'hh:mm')) timeslot, amt.nosofpersons, amt.timeslotfr, amt.timeslotto, concat(usr.FirstName, ' ', usr.LastName) UserName, amt.Approved, amp.Capacity, usr.contactNumber from [dbo].[AmenitiesBooking] amt inner join [Master].[Amenities] amm on amm.AmenitiesId=amt.AmenitiesId inner join[App].[PropertyAmenities] amp on amp.AminitiesId=amm.AmenitiesId and amp.PropertyId=@PropertyID inner join [Identity].[Users] usr on usr.UserId=amt.UserId where (amt.IsDeleted is null or amt.IsDeleted=0) and amp.PropertyId=@PropertyID";

                param = new SqlParameter("@PropertyID", PropertyID);
                param.Direction = ParameterDirection.Input;
                param.DbType = DbType.String;
                command.Parameters.Add(param);

                //if (DateFr != null)
                //{
                //    param = new SqlParameter("@DateFr", DateFr);// Convert.ToDateTime(DateFr).ToString("yyyy/MM/dd"));
                //    param.Direction = ParameterDirection.Input;
                //    param.DbType = DbType.String;
                //    command.Parameters.Add(param);
                //    command.CommandText += " and format(amt.BookingDate, 'yyyyMMdd') >= @DateFr";

                //    param = new SqlParameter("@DateTo", DateTo);// Convert.ToDateTime(DateTo).ToString("yyyy/MM/dd"));
                //    param.Direction = ParameterDirection.Input;
                //    param.DbType = DbType.String;
                //    command.Parameters.Add(param);
                //    command.CommandText += " and format(amt.BookingDate, 'yyyyMMdd') <= @DateTo";
                //}
                if (UserID > 0)
                {
                    param = new SqlParameter("@UserID", UserID);
                    param.Direction = ParameterDirection.Input;
                    param.DbType = DbType.String;
                    command.Parameters.Add(param);
                    command.CommandText += " and amt.userid = @UserID";
                }

                command.CommandText += " order by amm.AmenitiesName";

                adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);
                var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new AmenitiesBookings
                {
                    PropertyId = PropertyID,
                    Id = dataRow.Field<int>("Id"),
                    AmenitiesId = dataRow.Field<int>("AmenitiesId"),
                    AmenitiesName = dataRow.Field<string>("AmenitiesName"),
                    NosOfPersons = dataRow.Field<int>("nosofpersons"),
                    TimeSlot = dataRow.Field<string>("timeslot"),
                    TimeSlotFr = dataRow.Field<DateTime>("timeslotfr"),
                    TimeSlotTo = dataRow.Field<DateTime>("timeslotto"),
                    Approved = dataRow["Approved"]==DBNull.Value ? 0 : dataRow.Field<int>("Approved"),
                    ApproveStatus = dataRow["Approved"] == DBNull.Value ? "Not Approved" : "Approved",
                    UserName = dataRow.Field<string>("UserName"),
                    MobileNo = dataRow.Field<string>("contactNumber"),
                    UserId = dataRow.Field<int>("UserId")
                })).ToList();
                return Ok(eList);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            finally
            {
                ds.Dispose();
                connection.Close();
            }

            return Ok();
        }

        //select att.EmployeeID, kyc.EmployeeName, att.PunchTime, att.PunchType from [dbo].[attendancelogs] att inner join [dbo].kycdetails kyc on att.employeeid=kyc.employeeid order by att.id

        [Route("AttendanceLogs")]
        [HttpGet]
        public IHttpActionResult AttendanceLogs()
        {
            SqlConnection connection = new SqlConnection(constr);
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            DataSet ds = new DataSet();

            try
            {
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                command.CommandText = "select kyc.EmployeeID, kyc.EmployeeName, minAtt.inTime, minAttGate.GateNo minAttGateNo, minAttGate.EmployeeName minAttCreatedBy, maxAtt.outTime, maxAttGate.GateNo maxAttGateNo, maxAttGate.EmployeeName maxAttCreatedBy from [dbo].[KycDetails] kyc inner join (select min(punchtime) inTime, EmployeeId from [dbo].attendancelogs mAtt where cast(punchtime as date) = cast(getdate() as date) group by EmployeeId) minAtt on minAtt.employeeid=kyc.employeeid inner join (select mAtt.GateNo, EmployeeId, PunchTime, grd.EmployeeName from [dbo].attendancelogs mAtt inner join guardmaster grd on mAtt.CreatedBy=grd.id where cast(punchtime as date) = cast(getdate() as date)) minAttGate on minAttGate.employeeid=minAtt.employeeid and minAttGate.PunchTime=minAtt.inTime inner join (select max(punchtime) outTime, EmployeeId from [dbo].attendancelogs mAtt where cast(punchtime as date) =cast(getdate() as date) group by EmployeeId) maxAtt on minAtt.employeeid=kyc.employeeid inner join (select GateNo, EmployeeId, PunchTime, grd.EmployeeName from [dbo].attendancelogs mAtt inner join guardmaster grd on mAtt.CreatedBy=grd.id where cast(punchtime as date) = cast(getdate() as date)) maxAttGate on maxAttGate.employeeid=maxAtt.employeeid and maxAttGate.PunchTime=maxAtt.outTime where kyc.Approved = 1 order by kyc.id";

                adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);
                var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new AttendanceLogs
                {
                    EmployeeId = dataRow.Field<string>("EmployeeId"),
                    EmployeeName = dataRow.Field<string>("EmployeeName"),
                    GateNo = "",
                    PunchTime = dataRow.Field<DateTime>("InTime").ToString("hh:mm:ss t") + ", Gate No.: " + dataRow.Field<string>("minAttGateNo") + ", Guard: " + dataRow.Field<string>("minAttCreatedBy"),
                    PunchType = dataRow.Field<DateTime>("InTime") == dataRow.Field<DateTime>("OutTime") ? "" : dataRow.Field<DateTime>("OutTime").ToString("hh:mm:ss t") + ", Gate No.: " + dataRow.Field<string>("maxAttGateNo") + ", Guard: " + dataRow.Field<string>("maxAttCreatedBy")
                })).ToList();
                return Ok(eList);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            finally
            {
                ds.Dispose();
                connection.Close();
            }

            return Ok();
        }

        [Route("EmployeeList")]
        [HttpGet]
        public IHttpActionResult EmployeeList()
        {
            SqlConnection connection = new SqlConnection(constr);
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            DataSet ds = new DataSet();
            connection.Open();

            try
            {
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                command.CommandText = "select Id,EmployeeName,FatherName,Designation,MobileNo,IsDeleted,Approved from [dbo].employeelist where (IsDeleted is null or IsDeleted = 0) and (Approved = 1) order by EmployeeName";

                adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);
                var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new Employee
                {
                    Id = dataRow.Field<int>("Id"),
                    EmployeeName = dataRow.Field<string>("EmployeeName"),
                    FatherName = dataRow.Field<string>("FatherName"),
                    Designation = dataRow.Field<string>("Designation"),
                    MobileNo = dataRow.Field<string>("MobileNo"),
                    IsDeleted = dataRow["IsDeleted"] == DBNull.Value ? 0 : dataRow["IsDeleted"] == null ? 0 : dataRow.Field<int>("IsDeleted"),
                    Approved = dataRow["Approved"] == DBNull.Value ? 0 : dataRow["Approved"] == null ? 0 : dataRow.Field<int>("Approved")
                })).ToList();
                return Ok(eList);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            finally
            {
                ds.Dispose();
                connection.Close();
            }

            return Ok();
        }

        [Route("CreateEmployee")]
        [HttpPost]
        public IHttpActionResult CreateEmployee(Employee employee)
        {
            string res = "1";
            SqlConnection connection = new SqlConnection(constr);
            SqlCommand command = new SqlCommand();
            connection.Open();

            try
            {
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = "select count(id) from [dbo].employeelist where (IsDeleted is null or IsDeleted = 0) and ((EmployeeName=@EmployeeName and FatherName = @FatherName) or MobileNo=@MobileNo) and not id = @Id";
                command.Parameters.AddWithValue("@EmployeeName", employee.EmployeeName);
                command.Parameters.AddWithValue("@FatherName", employee.FatherName);
                command.Parameters.AddWithValue("@MobileNo", employee.MobileNo);
                command.Parameters.AddWithValue("@Id", employee.Id);

                var resp = command.ExecuteScalar();
                res = resp == DBNull.Value ? "1" : Convert.ToInt32(resp) > 0 ? "Name or mobile number already exists !" : "1";

                if (res == "1")
                {
                    command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;

                    if (employee.Id > 0)
                        command.CommandText = "update [dbo].employeelist set EmployeeName=@EmployeeName,FatherName=@FatherName,Designation=@Designation,MobileNo=@MobileNo where id = @Id";
                    else
                        command.CommandText = "insert into [dbo].employeelist (EmployeeName,FatherName,Designation,MobileNo,Approved) values(@EmployeeName,@FatherName,@Designation,@MobileNo,@Approved)";


                    command.Parameters.AddWithValue("@Id", employee.Id);
                    command.Parameters.AddWithValue("@EmployeeName", employee.EmployeeName);
                    command.Parameters.AddWithValue("@FatherName", employee.FatherName);
                    command.Parameters.AddWithValue("@Designation", employee.Designation);
                    command.Parameters.AddWithValue("@MobileNo", employee.MobileNo);
                    command.Parameters.AddWithValue("@Approved", "1");

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                res = ex.Message;
            }
            finally
            {
                connection.Close();
            }

            return Ok(res);
        }

        [Route("GuardList")]
        [HttpGet]
        public IHttpActionResult GuardList()
        {
            SqlConnection connection = new SqlConnection(constr);
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            DataSet ds = new DataSet();
            connection.Open();

            try
            {
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                command.CommandText = "select Id,EmployeeName,FatherName,Designation,MobileNo,IsDeleted,Approved from [dbo].GuardMaster where (IsDeleted is null or IsDeleted = 0) and (Approved = 1) order by EmployeeName";

                adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);
                var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new GuardMaster
                {
                    Id = dataRow.Field<int>("Id"),
                    EmployeeName = dataRow.Field<string>("EmployeeName"),
                    FatherName = dataRow.Field<string>("FatherName"),
                    Designation = dataRow.Field<string>("Designation"),
                    MobileNo = dataRow.Field<string>("MobileNo"),
                    IsDeleted = dataRow["IsDeleted"] == DBNull.Value ? 0 : dataRow["IsDeleted"] == null ? 0 : dataRow.Field<int>("IsDeleted"),
                    Approved = dataRow["Approved"] == DBNull.Value ? 0 : dataRow["Approved"] == null ? 0 : dataRow.Field<int>("Approved")
                })).ToList();
                return Ok(eList);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            finally
            {
                ds.Dispose();
                connection.Close();
            }

            return Ok();
        }

        [Route("CreateGuard")]
        [HttpPost]
        public IHttpActionResult CreateGuard(GuardMaster employee)
        {
            string res = "1";
            SqlConnection connection = new SqlConnection(constr);
            SqlCommand command = new SqlCommand();
            connection.Open();

            try
            {
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = "select count(id) from [dbo].guardmaster where (IsDeleted is null or IsDeleted = 0) and ((EmployeeName=@EmployeeName and FatherName = @FatherName) or MobileNo=@MobileNo) and not id = @Id";
                command.Parameters.AddWithValue("@EmployeeName", employee.EmployeeName);
                command.Parameters.AddWithValue("@FatherName", employee.FatherName);
                command.Parameters.AddWithValue("@MobileNo", employee.MobileNo);
                command.Parameters.AddWithValue("@Id", employee.Id);

                var resp = command.ExecuteScalar();
                res = resp == DBNull.Value ? "1" : Convert.ToInt32(resp) > 0 ? "Name or mobile number already exists !" : "1";

                if (res == "1")
                {
                    command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;

                    if (employee.Id > 0)
                        command.CommandText = "update [dbo].GuardMaster set EmployeeName=@EmployeeName,FatherName=@FatherName,Designation=@Designation,MobileNo=@MobileNo where id = @Id";
                    else
                        command.CommandText = "insert into [dbo].GuardMaster (EmployeeName,FatherName,Designation,MobileNo,Approved) values(@EmployeeName,@FatherName,@Designation,@MobileNo,@Approved)";


                    command.Parameters.AddWithValue("@Id", employee.Id);
                    command.Parameters.AddWithValue("@EmployeeName", employee.EmployeeName);
                    command.Parameters.AddWithValue("@FatherName", employee.FatherName);
                    command.Parameters.AddWithValue("@Designation", employee.Designation);
                    command.Parameters.AddWithValue("@MobileNo", employee.MobileNo);
                    command.Parameters.AddWithValue("@Approved", "1");

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                res = ex.Message;
            }
            finally
            {
                connection.Close();
            }

            return Ok(res);
        }

        [Route("KycDetailsList")]
        [HttpGet]
        public IHttpActionResult KycDetailsList()
        {
            SqlConnection connection = new SqlConnection(constr);
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            DataSet ds = new DataSet();

            try
            {
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                command.CommandText = "select Id,MobileNo,EmployeeId,EmployeeName,Gender,JobProfile,Approved,Image,IdDoc from KYCdetails where (IsDeleted is null or IsDeleted = 0) order by id";

                adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);
                var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new KycDetails
                {
                    Id = dataRow.Field<int>("Id"),
                    EmployeeId = dataRow.Field<string>("EmployeeId"),
                    EmployeeName = dataRow.Field<string>("EmployeeName"),
                    Gender = dataRow.Field<string>("Gender"),
                    JobProfile = dataRow.Field<string>("JobProfile"),
                    MobileNo = dataRow.Field<string>("MobileNo"),
                    IdDoc = dataRow.Field<string>("IdDoc"),
                    Image = dataRow.Field<string>("Image"),
                    ApproveStatus = dataRow["Approved"] == DBNull.Value ? "Not Approved" : "Approved"
                })).ToList();
                ds.Dispose();
                connection.Close();
                return Ok(eList);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                ds.Dispose();
                connection.Close();
                return null;
            }
        }

        [Route("QuestionsOfTask")]
        [HttpGet]
        public IHttpActionResult QuestionsOfTask(int taskID)
        {
            SqlConnection connection = new SqlConnection(constr);
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            SqlParameter param;
            DataSet ds = new DataSet();

            try
            {
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                command.CommandText = "select 0 TransactionID, q.QuestionID, q.QuestionName, t.Name taskName, t.Occurence, '' Action, '' Remarks from TaskWiseQuestionnaire q inner join TaskMaster t on q.taskid = t.id where taskid = @taskID order by id";
                param = new SqlParameter("@taskID", taskID);
                param.Direction = ParameterDirection.Input;
                param.DbType = DbType.String;
                command.Parameters.Add(param);
                adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);
                var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new TaskWiseQuestionnaire
                {
                    TransactionID = dataRow.Field<int>("TransactionID"),
                    TaskID = taskID,
                    TaskName = dataRow.Field<string>("taskName"),
                    Occurance = dataRow.Field<string>("Occurence"),
                    QuestionName = dataRow.Field<string>("QuestionName"),
                    QuestID = dataRow.Field<int>("QuestionID"),
                    Action = dataRow.Field<string>("Action"),
                    Remarks = dataRow.Field<string>("Remarks")
                })).ToList();
                return Ok(eList);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            finally
            {
                ds.Dispose();
                connection.Close();
            }

            return Ok();
        }

        [Route("AssignToList")]
        [HttpGet]
        public IHttpActionResult AssignToList()
        {
            SqlConnection connection = new SqlConnection(constr);
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            DataSet ds = new DataSet();

            try
            {
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                command.CommandText = "select distinct fm.facilitymemberid, CONCAT(fm.name, ' - ', fm.mobilenumber, ' - ', fmas.FacilityName) name, fm.IsActive, fm.facilitymasterid from app.facilitymember fm inner join [App].[FacilityMaster] fmas on fmas.FacilityMasterId=fm.FacilityMasterId where 1=1 and fm.IsActive=1 order by name";

                adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);
                var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new AssignToList
                {
                    Id = dataRow.Field<int>("FacilityMemberId"),
                    Name = dataRow.Field<string>("name")
                })).ToList();

                return Ok(eList);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            finally
            {
                ds.Dispose();
                connection.Close();
            }

            return Ok();
        }

        [Route("TaskDetails")]
        [HttpGet]
        public IHttpActionResult TaskDetails(int catID = 0, int subCatID = 0, string occurrence = "0", int assingedtoID = 0, DateTime? dteFr=null, DateTime? dteTo=null, string taskstatus=null)
        {
            SqlConnection connection = new SqlConnection(constr);
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            DataSet ds = new DataSet();

            if (occurrence == "D"){ occurrence = "2"; }
            else if (occurrence == "W") { occurrence = "1"; }
            else if (occurrence == "M") { occurrence = "3"; }
            else if (occurrence == "Y") { occurrence = "4"; }
            else if (occurrence == "N") { occurrence = "0"; }

            if (catID > 0 || subCatID > 0 || Convert.ToInt32(occurrence) > 0 || assingedtoID > 0 || (dteFr != null && dteTo != null))
            {
                try
                {
                    connection.Open();
                    command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;

                    command.CommandText = " select cm.CategoryName CategoryName, sm.SubCategoryName SubCategoryName, t.Name Name, isnull(t.Description,'')  Description, (trim(t.Occurence) + ' (' + cast(t.TimeFrom as varchar(8)) + ' - ' + cast(t.TimeTo as varchar(8)) + ')') Occurence, isnull(stat.Remarks,'') Remarks, t.id taskID, ";


                    if (dteFr == null)
                    {
                        command.CommandText += " t.datefrom DateFrom, ";
                    }
                    else
                    {
                        command.CommandText += " iif(t.occurence='D', '" + Convert.ToDateTime(dteFr).ToString("MM/dd/yyyy") + "', t.datefrom) DateFrom, ";
                    }

                    command.CommandText += " t.dateto, t.timefrom, t.timeto, tm.modify, fm.name AassignedTo, t.CategoryId, t.SubCategoryId, t.AssignTo, t.qrcode, iif(stat.TotalQuests>0,iif((stat.TotalQuests-stat.ComplQuests)=0,'Compleate', iif(stat.ComplQuests>0,iif(stat.ComplQuests=stat.TotalQuests,'Compleate', 'Actionable'),'Pending')), 'Pending') as taskStatus, stat.updatedon from TaskMaster t left join [calendar].Category cm on t.CategoryId=cm.ScheduleCategoryId left join [calendar].[SubCategory] sm on t.SubCategoryId=sm.SubCategoryId left outer join (select distinct tm.id taskid, 1 as modify from TaskWiseTransaction t inner join taskmaster tm on t.TaskName=tm.Name) tm on t.id=tm.taskid left join app.facilitymember fm on t.assignto = fm.FacilityMemberId left join dbo.taskstatus stat on t.id=stat.taskid where 1=1";

                    if (catID > 0)
                    {
                        command.Parameters.AddWithValue("@Id", catID);
                        command.CommandText += " and t.categoryId = @Id";
                    }
                    if (subCatID > 0)
                    {
                        command.Parameters.AddWithValue("@SId", subCatID);
                        command.CommandText += " and t.SubcategoryId = @SId";
                    }

                    if (occurrence.ToUpper() == "D" || occurrence.ToUpper() == "W" || occurrence.ToUpper() == "M" || occurrence.ToUpper() == "Y" || occurrence == "")
                    {
                        if (occurrence.ToUpper() == "D")
                        { occurrence = "2"; }
                        else if (occurrence.ToUpper() == "W")
                        { occurrence = "1"; }
                        else if (occurrence.ToUpper() == "M")
                        { occurrence = "3"; }
                        else if (occurrence.ToUpper() == "Y")
                        { occurrence = "4"; }
                    }

                    if (Convert.ToInt32(occurrence) > 0 || occurrence.Length > 0)//0-all, 1-w, 2-d, 3-monthly, 4-year
                    {
                        if (Convert.ToInt32(occurrence) == 1 || occurrence.ToUpper() == "W")//0-all, 1-w, 2-d, 3-monthly, 4-year
                        {
                            command.Parameters.AddWithValue("@Occurance", "W");
                            command.CommandText += " and t.Occurence = @Occurance";// and ([dbo].IsTransDateWeekly(t.DateFrom, cast(getdate() as date)))>0";
                        }
                        else if (Convert.ToInt32(occurrence) == 2 || occurrence.ToUpper() == "D")
                        {
                            command.Parameters.AddWithValue("@Occurance", "D");
                            command.CommandText += " and t.Occurence = @Occurance";
                        }
                        else if (Convert.ToInt32(occurrence) == 3 || occurrence.ToUpper() == "M")
                        {
                            command.Parameters.AddWithValue("@Occurance", "M");
                            command.CommandText += " and t.Occurence = @Occurance";// and datepart(month,t.datefrom) = datepart(month, getdate())";
                        }
                        else if (Convert.ToInt32(occurrence) == 4 || occurrence.ToUpper() == "Y")
                        {
                            command.Parameters.AddWithValue("@Occurance", "Y");
                            command.CommandText += " and t.Occurence = @Occurance";
                        }
                    }
                    if (assingedtoID > 0)
                    {
                        command.Parameters.AddWithValue("@AssignTo", assingedtoID);
                        command.CommandText += " and t.AssignTo = @AssignTo";
                    }
                    if (Convert.ToInt32(occurrence) == 2 || occurrence.ToUpper() == "D" || Convert.ToInt32(occurrence) == 1 || occurrence.ToUpper() == "W" || Convert.ToInt32(occurrence) == 3 || occurrence.ToUpper() == "M")
                    {
                    }
                    else
                    { 
                        if (dteFr != null)
                        {
                            command.Parameters.AddWithValue("@DateFrom", Convert.ToDateTime(dteFr).ToString("MM/dd/yyyy"));
                            command.CommandText += " and t.DateFrom >= @DateFrom";
                        }
                        if (dteTo != null)
                        {
                            command.Parameters.AddWithValue("@DateTo", Convert.ToDateTime(dteTo).ToString("MM/dd/yyyy"));
                            command.CommandText += " and t.DateTo <= @DateTo";
                        }
                    }
                    if (taskstatus != null)
                    {
                        if (taskstatus.ToUpper() == "PENDING")
                        {
                            //command.Parameters.AddWithValue("@AssignTo", assingedtoID);
                            command.CommandText += " and stat.ComplQuests=0";
                        }
                        if (taskstatus.ToUpper() == "COMPLETE")
                        {
                            command.CommandText += " and (stat.TotalQuests>0 and stat.ComplQuests=stat.TotalQuests)";
                        }
                        if (taskstatus.ToUpper() == "ACTIONABLE")
                        {
                            command.CommandText += " and (stat.ComplQuests<stat.TotalQuests and stat.ComplQuests>0)";
                        }
                    }

                    command.CommandText += " order by t.id desc";

                    adapter = new SqlDataAdapter(command);
                    adapter.Fill(ds);

                    var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new TaskTransactionModel
                    {
                        CategoryName = dataRow.Field<string>("CategoryName"),
                        SubCategoryName = dataRow.Field<string>("SubCategoryName"),
                        Name = dataRow.Field<string>("Name"),
                        Description = dataRow.Field<string>("Description"),
                        Occurence = dataRow.Field<string>("Occurence"),
                        Remarks = dataRow.Field<string>("Remarks"),
                        TaskCategoryId = dataRow.Field<int>("CategoryId"),
                        TaskSubCategoryId = dataRow.Field<int>("SubCategoryId"),
                        TaskId = dataRow.Field<int>("taskID"),
                        DateFrom = dataRow["datefrom"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : dataRow.Field<DateTime>("datefrom"),
                        DateTo = dataRow["dateto"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : dataRow.Field<DateTime>("dateto"),
                        TimeFrom = dataRow["timefrom"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : Convert.ToDateTime(dataRow["timefrom"].ToString()),
                        TimeTo = dataRow["timeto"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : Convert.ToDateTime(dataRow["timeto"].ToString()),
                        AssignedTo = dataRow.Field<string>("AassignedTo"),
                        AssignedToId = dataRow.Field<int>("AssignTo"),
                        QRCode = dataRow.Field<string>("QRCode"),
                        TaskStatus = dataRow.Field<string>("taskStatus"),
                        UpdatedOn = dataRow["updatedon"] == DBNull.Value ? "" : dataRow.Field<DateTime>("updatedon").ToString("dd-MM-yyyy")
                    })).ToList();

                    return Ok(eList);
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                }
                finally
                {
                    ds.Dispose();
                    connection.Close();
                }
            }
            else
            {
                var data = new TaskTransactionModel()
                {
                    CategoryName = "",
                    SubCategoryName = "",
                    Name = "",
                    Description = "",
                    Occurence = "",
                    Remarks = "",
                    TaskCategoryId = 0,
                    TaskSubCategoryId = 0,
                    TaskId = 0,
                    DateFrom = Convert.ToDateTime("1900/01/01"),
                    DateTo = Convert.ToDateTime("1900/01/01"),
                    TimeFrom = Convert.ToDateTime("1900/01/01"),
                    TimeTo = Convert.ToDateTime("1900/01/01"),
                    AssignedTo = "",
                    AssignedToId = 0,
                    QRCode = "",
                    TaskStatus = ""
                };
                List<TaskTransactionModel> eList = new List<TaskTransactionModel>();
                eList.Add(data);
                return Ok(eList);
            }
            return Ok();
        }


        [Route("TaskDetailsWithQuestion")]
        [HttpGet]
        public IHttpActionResult TaskDetailsWithQuestion(int catID = 0, int subCatID = 0, string occurrence = "0", int assingedtoID = 0)
        {
            if (occurrence == "D") { occurrence = "2"; }
            else if (occurrence == "W") { occurrence = "1"; }
            else if (occurrence == "M") { occurrence = "3"; }
            else if (occurrence == "Y") { occurrence = "4"; }

            SqlConnection connection = new SqlConnection(constr);
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            DataSet ds = new DataSet();

            try
            {
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                string sql = "select cm.CategoryName CategoryName, sm.SubCategoryName SubCategoryName, t.Name Name, isnull(t.Description, '')  Description, (trim(t.Occurence) + ' (' + cast(t.TimeFrom as varchar(8)) + ' - ' + cast(t.TimeTo as varchar(8)) + ')') Occurence, isnull(t.Remarks, '') Remarks, t.id taskID, t.datefrom, t.dateto, t.timefrom, t.timeto, tm.modify, fm.name AassignedTo, t.CategoryId, t.SubCategoryId, t.AssignTo, t.qrcode, iif(stat.TotalQuests > 0, iif((stat.TotalQuests - stat.ComplQuests) = 0, 'Compleate', iif(stat.ComplQuests > 0, 'Actionable', 'Pending')), 'Pending') as taskStatus,stat.remarks RemarksQ, stat.updatedon from TaskMaster t left join[calendar].Category cm on t.CategoryId = cm.ScheduleCategoryId left join[calendar].[SubCategory] sm on t.SubCategoryId = sm.SubCategoryId left outer join(select distinct tm.id taskid, 1 as modify from TaskWiseTransaction t inner join taskmaster tm on t.TaskName = tm.Name) tm on t.id = tm.taskid left join app.facilitymember fm on t.assignto = fm.FacilityMemberId left join dbo.taskstatus stat on t.id = stat.taskid where 1 = 1 and t.Occurence = 'D' union select cm.CategoryName CategoryName, sm.SubCategoryName SubCategoryName, t.Name Name, isnull(t.Description, '')  Description, (trim(t.Occurence) + ' (' + cast(t.TimeFrom as varchar(8)) + ' - ' + cast(t.TimeTo as varchar(8)) + ')') Occurence, isnull(t.Remarks, '') Remarks, t.id taskID, t.datefrom, t.dateto, t.timefrom, t.timeto, tm.modify, fm.name AassignedTo, t.CategoryId, t.SubCategoryId, t.AssignTo, t.qrcode, iif(stat.TotalQuests > 0, iif((stat.TotalQuests - stat.ComplQuests) = 0, 'Compleate', iif(stat.ComplQuests > 0, 'Actionable', 'Pending')), 'Pending') as taskStatus, stat.remarks RemarksQ, stat.updatedon from TaskMaster t left join[calendar].Category cm on t.CategoryId = cm.ScheduleCategoryId left join[calendar].[SubCategory] sm on t.SubCategoryId = sm.SubCategoryId left outer join(select distinct tm.id taskid, 1 as modify from TaskWiseTransaction t inner join taskmaster tm on t.TaskName = tm.Name) tm on t.id = tm.taskid left join app.facilitymember fm on t.assignto = fm.FacilityMemberId left join dbo.taskstatus stat on t.id = stat.taskid where 1 = 1 and t.Occurence = 'W' and([dbo].IsTransDateWeekly(t.DateFrom, cast(getdate() as date)))> 0 union select cm.CategoryName CategoryName, sm.SubCategoryName SubCategoryName, t.Name Name, isnull(t.Description, '')  Description, (trim(t.Occurence) + ' (' + cast(t.TimeFrom as varchar(8)) + ' - ' + cast(t.TimeTo as varchar(8)) + ')') Occurence, isnull(t.Remarks, '') Remarks, t.id taskID, t.datefrom, t.dateto, t.timefrom, t.timeto, tm.modify, fm.name AassignedTo, t.CategoryId, t.SubCategoryId, t.AssignTo, t.qrcode, iif(stat.TotalQuests > 0, iif((stat.TotalQuests - stat.ComplQuests) = 0, 'Compleate', iif(stat.ComplQuests > 0, 'Actionable', 'Pending')), 'Pending') as taskStatus, stat.remarks RemarksQ, stat.updatedon from TaskMaster t left join[calendar].Category cm on t.CategoryId = cm.ScheduleCategoryId left join[calendar].[SubCategory] sm on t.SubCategoryId = sm.SubCategoryId left outer join(select distinct tm.id taskid, 1 as modify from TaskWiseTransaction t inner join taskmaster tm on t.TaskName = tm.Name) tm on t.id = tm.taskid left join app.facilitymember fm on t.assignto = fm.FacilityMemberId left join dbo.taskstatus stat on t.id = stat.taskid where 1 = 1 and t.Occurence = 'M' and datepart(day, t.datefrom) = datepart(day, getdate()) union select cm.CategoryName CategoryName, sm.SubCategoryName SubCategoryName, t.Name Name, isnull(t.Description, '')  Description, (trim(t.Occurence) + ' (' + cast(t.TimeFrom as varchar(8)) + ' - ' + cast(t.TimeTo as varchar(8)) + ')') Occurence, isnull(t.Remarks, '') Remarks, t.id taskID, t.datefrom, t.dateto, t.timefrom, t.timeto, tm.modify, fm.name AassignedTo, t.CategoryId, t.SubCategoryId, t.AssignTo, t.qrcode, iif(stat.TotalQuests > 0, iif((stat.TotalQuests - stat.ComplQuests) = 0, 'Compleate', iif(stat.ComplQuests > 0, 'Actionable', 'Pending')), 'Pending') as taskStatus, stat.remarks RemarksQ, stat.updatedon from TaskMaster t left join[calendar].Category cm on t.CategoryId = cm.ScheduleCategoryId left join[calendar].[SubCategory] sm on t.SubCategoryId = sm.SubCategoryId left outer join(select distinct tm.id taskid, 1 as modify from TaskWiseTransaction t inner join taskmaster tm on t.TaskName = tm.Name) tm on t.id = tm.taskid left join app.facilitymember fm on t.assignto = fm.FacilityMemberId left join dbo.taskstatus stat on t.id = stat.taskid where 1 = 1 and t.Occurence = 'Y' and t.datefrom = getdate() order by taskStatus desc";


                command.CommandText = sql;

                adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);
                var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new TaskTransactionModel
                {
                    CategoryName = dataRow.Field<string>("CategoryName"),
                    SubCategoryName = dataRow.Field<string>("SubCategoryName"),
                    Name = dataRow.Field<string>("Name"),
                    Description = dataRow.Field<string>("Description"),
                    Occurence = dataRow.Field<string>("Occurence"),
                    Remarks = dataRow.Field<string>("Remarks"),
                    TaskCategoryId = dataRow.Field<int>("CategoryId"),
                    TaskSubCategoryId = dataRow.Field<int>("SubCategoryId"),
                    TaskId = dataRow.Field<int>("taskID"),
                    DateFrom = dataRow["datefrom"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : dataRow.Field<DateTime>("datefrom"),
                    DateTo = dataRow["dateto"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : dataRow.Field<DateTime>("dateto"),
                    TimeFrom = dataRow["timefrom"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : Convert.ToDateTime(dataRow["timefrom"].ToString()),
                    TimeTo = dataRow["timeto"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : Convert.ToDateTime(dataRow["timeto"].ToString()),
                    AssignedTo = dataRow.Field<string>("AassignedTo"),
                    AssignedToId = dataRow.Field<int>("AssignTo"),
                    QRCode = dataRow.Field<string>("QRCode"),
                    TaskStatus = dataRow.Field<string>("taskStatus"),
                    QuestionName = "",//dataRow.Field<string>("QuestionName"),
                    RemarksQuestion = dataRow.Field<string>("RemarksQ"),
                    UpdatedOn = dataRow["UpdatedOn"] == DBNull.Value ? "" : dataRow.Field<DateTime>("UpdatedOn").ToString("dd-MM-yyyy")
                })).ToList();

                return Ok(eList);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            finally
            {
                ds.Dispose();
                connection.Close();
            }

            return Ok();
        }

        [Route("TaskDetailsEvent")]
        [HttpGet]
        public IHttpActionResult TaskDetailsEvent(int catID = 0, int subCatID = 0, int occurrence = 0, int assingedtoID = 0)
        {
            SqlConnection connection = new SqlConnection(constr);
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            DataSet ds = new DataSet();

            try
            {
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                command.CommandText = "select t.* from (select t0.*, row_number() over (partition by t0.DateFrom order by t0.id desc) as seqnum from( select cm.CategoryName CategoryName, sm.SubCategoryName SubCategoryName, t.Name Name, isnull(t.Description,'')  Description, (trim(t.Occurence) + ' (' + cast(t.TimeFrom as varchar(8)) + ' - ' + cast(t.TimeTo as varchar(8)) + ')') Occurence, isnull(t.Remarks,'') Remarks, t.id taskID, t.datefrom, t.dateto, t.timefrom, t.timeto, tm.modify, fm.name AassignedTo, t.id, iif(ttlTask.ttlTasks<=1, 'G', iif(ttlTask.ttlTasks>2,'R', 'O')) as TaskStatus from TaskMaster t  left join [calendar].Category cm on t.CategoryId=cm.ScheduleCategoryId  left join [calendar].[SubCategory] sm on t.SubCategoryId=sm.SubCategoryId left outer join (select COUNT(transactionid) ttlQuests, TransactionDate, TaskName from TaskWiseTransaction group by TransactionDate, TaskName) ttlQuest on ttlQuest.TransactionDate=t.datefrom and ttlQuest.TaskName=t.Name left outer join (select count(id) ttlTasks, datefrom from TaskMaster group by DateFrom) ttlTask on ttlTask.DateFrom=t.DateFrom left outer join (select distinct tm.id taskid, 1 as modify from TaskWiseTransaction t  inner join taskmaster tm on t.TaskName=tm.Name) tm on t.id=tm.taskid  left join app.facilitymember fm on t.assignto = fm.FacilityMemberId where 1=1 and t.DateFrom>= dateadd(day,1, EOMONTH ( getdate(), -2 )) and t.DateFrom<= EOMONTH ( getdate(), 1 )";

                if (catID > 0)
                {
                    command.Parameters.AddWithValue("@Id", catID);
                    command.CommandText += " and t.categoryId = @Id";
                }
                if (subCatID > 0)
                {
                    command.Parameters.AddWithValue("@SId", subCatID);
                    command.CommandText += " and t.SubcategoryId = @SId";
                }
                if (occurrence > 0)//0-all, 1-w, 2-d, 3-monthly, 4-year
                {
                    if (occurrence == 1)//0-all, 1-w, 2-d, 3-monthly, 4-year
                    {
                        command.Parameters.AddWithValue("@Occurance", "W");
                        command.CommandText += " and t.Occurence = @Occurance";
                    }
                    else if (occurrence == 2)
                    {
                        command.Parameters.AddWithValue("@Occurance", "D");
                        command.CommandText += " and t.Occurence = @Occurance";
                    }
                    else if (occurrence == 3)
                    {
                        command.Parameters.AddWithValue("@Occurance", "M");
                        command.CommandText += " and t.Occurence = @Occurance";
                    }
                    else if (occurrence == 4)
                    {
                        command.Parameters.AddWithValue("@Occurance", "Y");
                        command.CommandText += " and t.Occurence = @Occurance";
                    }
                }
                if (assingedtoID > 0)
                {
                    command.Parameters.AddWithValue("@AssignTo", assingedtoID);
                    command.CommandText += " and t.AssignTo = @AssignTo";
                }

                command.CommandText += ")t0 where t0.DateFrom>= dateadd(day,1, EOMONTH ( getdate(), -2 )) and t0.DateFrom<= EOMONTH ( getdate(), 1 )) t where seqnum <= 3; ";

                adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);
                var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new TaskTransactionModel
                {
                    CategoryName = dataRow.Field<string>("CategoryName"),
                    SubCategoryName = dataRow.Field<string>("SubCategoryName"),
                    Name = dataRow.Field<string>("Name"),
                    Description = dataRow.Field<string>("Description"),
                    Occurence = dataRow.Field<string>("Occurence"),
                    Remarks = dataRow.Field<string>("Remarks"),
                    TaskCategoryId = catID,
                    TaskSubCategoryId = subCatID,
                    TaskId = dataRow.Field<int>("taskID"),
                    DateFrom = dataRow["datefrom"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : dataRow.Field<DateTime>("datefrom"),
                    DateTo = dataRow["dateto"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : dataRow.Field<DateTime>("dateto"),
                    TimeFrom = dataRow["timefrom"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : Convert.ToDateTime(dataRow["timefrom"].ToString()),
                    TimeTo = dataRow["timeto"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : Convert.ToDateTime(dataRow["timeto"].ToString()),
                    AssignedTo = dataRow.Field<string>("AassignedTo"),
                    TaskStatus= dataRow.Field<string>("TaskStatus")
                })).ToList();

                return Ok(eList);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            finally
            {
                ds.Dispose();
                connection.Close();
            }

            return Ok();
        }

        [Route("CreateQuestionnaire")]
        [HttpPost]
        public IHttpActionResult CreateQuestionnaire(List<TaskWiseQuestions> taskWiseQuestions)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();
                    foreach (var item in taskWiseQuestions)
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = "insert into [dbo].[TaskWiseQuestionnaire](TaskID, QuestionName, CreatedBy, CreatedOn)values(@TaskID, @Question, @CreatedBy, getdate())";
                            cmd.Parameters.AddWithValue("@TaskID", item.TaskID);
                            cmd.Parameters.AddWithValue("@Question", item.QuestionName);
                            cmd.Parameters.AddWithValue("@CreatedBy", "1");
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = "update [dbo].taskstatus set totalquests = (select count(QuestionID) from[TaskWiseQuestionnaire] tr where tr.taskid = @TaskID) where [dbo].taskstatus.taskid = @TaskID";
                            cmd.Parameters.AddWithValue("@TaskID", item.TaskID);
                            cmd.ExecuteNonQuery();
                        }

                    }

                    con.Close();
                }

                return Ok("Created !");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [Route("UpdateQuestionnaire")]
        [HttpPost]
        public IHttpActionResult UpdateQuestionnaire(TaskWiseQuestions quest)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "update [dbo].[TaskWiseQuestionnaire] set QuestionName = '" + quest.QuestionName + "' where QuestionID = @QuestID";
                        cmd.Parameters.AddWithValue("@QuestID", quest.QuestID);
                        cmd.ExecuteNonQuery();
                    }

                    con.Close();
                }

                return Ok("sucess !");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [Route("NoticeBoardSave")]
        [HttpPost]
        public IHttpActionResult CreateNoticeBoard(FormDataNoticeBoard model)
        {
            string userID = "1"; 
            var resp = "1";

            //var userID = Convert.ToInt32(HttpContext.User.Claims.First(x => x.Type == "UserIdId").Value); ;

            try
            {
                if (model.PropertyId != 0 && model.NotificationTypeId != 0 && model.PropertyGroupId != 0 && model.Notify != 0 && model.AlertTypeId != 0 && model.Subject != "")
                {
                    List<NoticeBoardAttachment> finalAttachmentData = new List<NoticeBoardAttachment>();

                    string sp = "INSERT INTO app.Notification (PropertyId,Subject,Message,NotificationTypeId,PropertyGroupId,IsActive,IsDeleted,CreatedOn,CreatedBy,ExpirtyDate) VALUES (@PropertyId,@Subject,@Message,@NotificationTypeId,@PropertyGroupId,@IsActive,0,GETDATE(),@CurrentUserId,@ExpirtyDate)";//"App.ManageNoticeBoard_Amen";

                    SqlConnection con = new SqlConnection(constr);
                    SqlCommand cmd = new SqlCommand();

                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sp;
                    cmd.Parameters.AddWithValue("@CmdType", model.StatementType);
                    cmd.Parameters.AddWithValue("@NotificationId", userID);
                    cmd.Parameters.AddWithValue("@PropertyId", Convert.ToString(model.PropertyId));
                    cmd.Parameters.AddWithValue("@Subject", Convert.ToString(model.Subject));
                    cmd.Parameters.AddWithValue("@Message", Convert.ToString(model.Message));
                    cmd.Parameters.AddWithValue("@NotificationTypeId", Convert.ToString(model.NotificationTypeId));
                    cmd.Parameters.AddWithValue("@PropertyGroupId", Convert.ToString(model.PropertyGroupId));
                    cmd.Parameters.AddWithValue("@ExpirtyDate", model.ExpirtyDate);
                    cmd.Parameters.AddWithValue("@ExecutionDate", model.ExpirtyDate);
                    cmd.Parameters.AddWithValue("@IsActive", 1);
                    cmd.Parameters.AddWithValue("@PropertyDetailsId", model.PropertyDetailsId);
                    cmd.Parameters.AddWithValue("@PropertyTowerId", model.PropertyTowerId);
                    cmd.Parameters.AddWithValue("@Notify", Convert.ToString(model.Notify));
                    cmd.Parameters.AddWithValue("@AlertTypeId", Convert.ToString(model.AlertTypeId));
                    cmd.Parameters.AddWithValue("@IsSent", 0);
                    cmd.Parameters.AddWithValue("@CurrentUserId", userID);

                    con.Open();
                    cmd.ExecuteNonQuery();

                    con.Close();
                }
            }
            catch (Exception ex)
            {
                resp = ex.Message;

            }
            return Ok(resp);
        }

        [Route("CreateSubCategory")]
        [HttpPost]
        public IHttpActionResult CreateSubCategory(SubCategory subCategory)
        {
            string resp = "";
            SqlConnection con = new SqlConnection(constr);
            con.Open();

            try
            {
                if (subCategory.CategoryId == 0)
                { return Ok("Invalid Category ID !"); }
                if (subCategory.SubCategoryName.Length == 0)
                { return Ok("Invalid Sub Category Name !"); }

                string sp = "INSERT INTO [calendar].[SubCategory] (CategoryId,SubCategoryName) VALUES (@CategoryId,@SubCategoryName)";

                SqlCommand cmd = new SqlCommand();

                cmd.Connection = con;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sp;
                cmd.Parameters.AddWithValue("@CategoryId", subCategory.CategoryId);
                cmd.Parameters.AddWithValue("@SubCategoryName", subCategory.SubCategoryName);

                cmd.ExecuteNonQuery();
                resp = "Saved Succefully !";
            }
            catch(Exception ex)
            {
                resp = ex.Message; 
            }

            con.Close();
            return Ok(resp);
        }

        [Route("DeleteQuestionnaire")]
        [HttpDelete]
        public IHttpActionResult DeleteQuestionnaire(int questID)
        {
            string taskId = ""; int complQuests = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "select taskid from [dbo].[TaskWiseQuestionnaire] where QuestionID = @QuestID";
                        cmd.Parameters.AddWithValue("@QuestID", questID);
                        taskId = cmd.ExecuteScalar().ToString();
                    }

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "select ComplQuests from TaskStatus where taskid = @TaskId";
                        cmd.Parameters.AddWithValue("@TaskId", taskId);
                        complQuests = cmd.ExecuteScalar() == DBNull.Value ? 0 : Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    con.Close();
                }
                if (complQuests == 0)
                {
                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            con.Open();
                            cmd.Connection = con;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = "delete from [dbo].[TaskWiseQuestionnaire] where QuestionID = @QuestID";
                            cmd.Parameters.AddWithValue("@QuestID", questID);
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }

                    return Ok("Deleted !");
                }
                else
                {
                    return Ok("Can not delete, this question already responded !");
                }
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [Route("CreateTask")]
        [HttpPost]
        public IHttpActionResult CreateTask(TaskMaster taskMaster)
        {
            if (taskMaster.Name.Length == 0)
            {
                return Ok("Task name can not be blank ?");
            }
            if (taskMaster.Occurence.Length == 0)
            {
                return Ok("Task occurance can not be blank ?");
            }
            if (taskMaster.CategoryId == 0)
            {
                return Ok("Task category can not be blank ?");
            }
            if (taskMaster.SubCategoryId == 0)
            {
                return Ok("Task sub category can not be blank ?");
            }
            if (taskMaster.Location.Length == 0)
            {
                return Ok("Task location can not be blank ?");
            }
            if (taskMaster.CategoryId == 0)
            {
                return Ok("Task category can not be blank ?");
            }

            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        con.Open();
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.Text;
                        
                        if (taskMaster.Id == 0)
                        {
                            cmd.CommandText = "insert into [dbo].[TaskMaster](CategoryId,SubCategoryId,Name,Description,DateFrom,DateTo,TimeFrom,TimeTo,Remarks,Occurence,createdby,createdon,AssignTo,RemindMe,Location, QRCode, AssetsID)values(@CategoryId,@SubCategoryId,@Name,@Description,@DateFrom,@DateTo,@TimeFrom,@TimeTo,@Remarks,@Occurence,@createdby,getdate(), @AssignTo,@RemindMe,@Location, @QRCode, @AssetsID)";
                        }
                        else
                        {
                            cmd.CommandText = "update [dbo].[TaskMaster] set CategoryId=@CategoryId,SubCategoryId=@SubCategoryId,Name=@Name,Description=@Description,DateFrom=@DateFrom,DateTo=@DateTo,TimeFrom=@TimeFrom,TimeTo=@TimeTo,Remarks=@Remarks,Occurence=@Occurence,AssignTo=@AssignTo,RemindMe=@RemindMe,Location=@Location, QRCode=@QRCode, AssetsID=@AssetsID where id = @Id";
                        }

                        cmd.Parameters.AddWithValue("@Id", taskMaster.Id);
                        cmd.Parameters.AddWithValue("@CategoryId", taskMaster.CategoryId);
                        cmd.Parameters.AddWithValue("@SubCategoryId", taskMaster.SubCategoryId);
                        cmd.Parameters.AddWithValue("@Name", taskMaster.Name);
                        cmd.Parameters.AddWithValue("@Description", taskMaster.Description);
                        cmd.Parameters.AddWithValue("@DateFrom", Convert.ToDateTime(taskMaster.DateFrom).ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@DateTo", Convert.ToDateTime(taskMaster.DateTo).ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@TimeFrom", Convert.ToDateTime(taskMaster.TimeFrom).ToString("hh:mm:ss tt"));
                        cmd.Parameters.AddWithValue("@TimeTo", Convert.ToDateTime(taskMaster.TimeTo).ToString("hh:mm:ss tt"));
                        cmd.Parameters.AddWithValue("@Remarks", taskMaster.Remarks);
                        cmd.Parameters.AddWithValue("@Occurence", taskMaster.Occurence);
                        cmd.Parameters.AddWithValue("@createdby", taskMaster.CreatedBy);
                        cmd.Parameters.AddWithValue("@AssignTo", taskMaster.AssignTo);
                        cmd.Parameters.AddWithValue("@RemindMe", taskMaster.RemindMe);
                        cmd.Parameters.AddWithValue("@Location", taskMaster.Location);
                        cmd.Parameters.AddWithValue("@QRCode", taskMaster.QRCode);
                        cmd.Parameters.AddWithValue("@AssetsID", taskMaster.AssetsID);
                        cmd.ExecuteNonQuery();

                        con.Close();
                    }
                }




                return Ok("Created !");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [Route("DeleteTask")]
        [HttpDelete]
        public IHttpActionResult DeleteTask(int taskID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        con.Open();
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "delete from [dbo].[TaskMaster] where id = @taskID";
                        cmd.Parameters.AddWithValue("@taskID", taskID);
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }

                return Ok("Deleted !");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [Route("GetAssets")]
        [HttpGet]
        public IHttpActionResult GetAssets()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    SqlConnection connection;
                    SqlDataAdapter adapter;
                    SqlCommand command = new SqlCommand();
                    SqlParameter param;
                    DataSet ds = new DataSet();

                    connection = new SqlConnection(constr);

                    connection.Open();
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "select Name, ID, Description, QRCode from [dbo].[AssetMaster] where (isdeleted = 0 or isdeleted is null) order by name";

                    adapter = new SqlDataAdapter(command);
                    adapter.Fill(ds);

                    var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new AssetsMaster
                    {
                        Id = dataRow.Field<int>("id"),
                        Name = dataRow.Field<string>("Name"),
                        Description = dataRow.Field<string>("Description"),
                        QRCode = dataRow.Field<string>("QRCode")

                    })).ToList();
                    connection.Close();
                    return Ok(eList);
                }
            }
            catch (Exception ex)
            { return Ok(ex.Message); }
        }

        [Route("ManageAssets")]
        [HttpPost]
        public IHttpActionResult ManageAssets(AssetsMaster assetsMaster)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    SqlConnection connection = new SqlConnection(constr);
                    connection.Open();
                    SqlCommand command = new SqlCommand();

                    string strSql = "";

                    if (assetsMaster.Flag == "I")
                    { strSql = "insert into [dbo].[AssetMaster](Name, Description, QRCode, AssetType, Manufacturer, AssetModel, IsMovable) values(@Name, @Description, @QRCode, @AssetType, @Manufacturer, @AssetModel, @IsMovable)"; }
                    else if (assetsMaster.Flag == "U")
                    { strSql = "update [dbo].[AssetMaster] set Name=@Name, Description=@Description, QRCode=@QRCode, AssetType=@AssetType, Manufacturer=@Manufacturer, AssetModel=@AssetModel, IsMovable=@IsMovable where Id=@Id"; }
                    else if (assetsMaster.Flag == "D")
                    { strSql = "update [dbo].[AssetMaster] set isdeleted=1 where Id=@Id"; }

                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = strSql;
                    command.Parameters.AddWithValue("@Id", assetsMaster.Id);
                    command.Parameters.AddWithValue("@Name", assetsMaster.Name);
                    command.Parameters.AddWithValue("@Description", assetsMaster.Description);
                    command.Parameters.AddWithValue("@QRCode", assetsMaster.QRCode);
                    command.Parameters.AddWithValue("@AssetType", assetsMaster.AssetType);
                    command.Parameters.AddWithValue("@Manufacturer", assetsMaster.Manufacturer);
                    command.Parameters.AddWithValue("@AssetModel", assetsMaster.AssetModel);
                    command.Parameters.AddWithValue("@IsMovable", assetsMaster.IsMovable);

                    command.ExecuteNonQuery();

                    connection.Close();
                    return Ok("success");
                }
            }
            catch (Exception ex)
            { return Ok(ex.Message); }
        }

        [Route("FacilityMemberSave")]
        [HttpPost]
        public IHttpActionResult FacilityMemberSave(FromDataFacilityMemberModel model)
        {
            var id=0;
            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "select count(FacilityMemberId) from [App].[FacilityMember] where FacilityMemberId = @FacilityMemberId";
                        cmd.Parameters.AddWithValue("@FacilityMemberId", model.FacilityMemberId);
                        id= cmd.ExecuteScalar() == DBNull.Value ? 0 : Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    if (id > 0)
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.CommandType = CommandType.Text;

                            cmd.CommandText = "update [App].[FacilityMember] set PropertyId=@PropertyId,Name=@Name,Gender=@Gender,MobileNumber=@MobileNumber,Address=@Address,FacilityMasterId=@FacilityMasterId,ProfileImageUrl=@ProfileImageUrl,IsBlocked=@IsBlocked,IsApproved=@IsApproved,ApprovedOn=getdate(),ApprovedBy=@ApprovedBy,IsActive=@IsActive,IsDeleted=@IsDeleted where FacilityMemberId = @FacilityMemberId";

                            cmd.Parameters.AddWithValue("@FacilityMemberId", model.FacilityMemberId);
                            cmd.Parameters.AddWithValue("@PropertyId", model.PropertyId);
                            cmd.Parameters.AddWithValue("@Name", model.Name);
                            cmd.Parameters.AddWithValue("@Gender", model.Gender);
                            cmd.Parameters.AddWithValue("@MobileNumber", model.MobileNumber);
                            cmd.Parameters.AddWithValue("@Address", model.Address);
                            cmd.Parameters.AddWithValue("@FacilityMasterId", model.FacilityMasterId);
                            cmd.Parameters.AddWithValue("@ProfileImageUrl", "");
                            cmd.Parameters.AddWithValue("@IsBlocked", 0);
                            cmd.Parameters.AddWithValue("@AccessCode", GenerateNewRandom());
                            cmd.Parameters.AddWithValue("@IsApproved", 1);
                            cmd.Parameters.AddWithValue("@ApprovedOn", "");
                            cmd.Parameters.AddWithValue("@ApprovedBy", "1");
                            cmd.Parameters.AddWithValue("@IsActive", "1");
                            cmd.Parameters.AddWithValue("@IsDeleted", "0");
                            cmd.Parameters.AddWithValue("@CreatedBy", "1");
                            cmd.Parameters.AddWithValue("@CreatedOn", "");
                            cmd.Parameters.AddWithValue("@UpdatedBy", "");
                            cmd.Parameters.AddWithValue("@UpdatedOn", "");

                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.CommandType = CommandType.Text;

                            cmd.CommandText = "insert into [App].[FacilityMember] (PropertyId,Name,Gender,MobileNumber,Address,FacilityMasterId,ProfileImageUrl,IsBlocked,AccessCode,IsApproved,ApprovedOn,ApprovedBy,IsActive,IsDeleted,CreatedBy,CreatedOn) values(@PropertyId,@Name,@Gender,@MobileNumber,@Address,@FacilityMasterId,@ProfileImageUrl,@IsBlocked,@AccessCode,1,getdate(),@ApprovedBy,1,@IsDeleted,@CreatedBy,getdate())";

                            cmd.Parameters.AddWithValue("@FacilityMemberId", model.FacilityMemberId);
                            cmd.Parameters.AddWithValue("@PropertyId", model.PropertyId);
                            cmd.Parameters.AddWithValue("@Name", model.Name);
                            cmd.Parameters.AddWithValue("@Gender", model.Gender);
                            cmd.Parameters.AddWithValue("@MobileNumber", model.MobileNumber);
                            cmd.Parameters.AddWithValue("@Address", model.Address);
                            cmd.Parameters.AddWithValue("@FacilityMasterId", model.FacilityMasterId);
                            cmd.Parameters.AddWithValue("@ProfileImageUrl", "");
                            cmd.Parameters.AddWithValue("@IsBlocked", 0);
                            cmd.Parameters.AddWithValue("@AccessCode", GenerateNewRandom());
                            cmd.Parameters.AddWithValue("@IsApproved", 1);
                            cmd.Parameters.AddWithValue("@ApprovedOn", "");
                            cmd.Parameters.AddWithValue("@ApprovedBy", "1");
                            cmd.Parameters.AddWithValue("@IsActive", "1");
                            cmd.Parameters.AddWithValue("@IsDeleted", "0");
                            cmd.Parameters.AddWithValue("@CreatedBy", "1");
                            cmd.Parameters.AddWithValue("@CreatedOn", "");
                            cmd.Parameters.AddWithValue("@UpdatedBy", "");
                            cmd.Parameters.AddWithValue("@UpdatedOn", "");

                            cmd.ExecuteNonQuery();
                        }
                    }

                    //Uploading Profile Image ...
                    string serverPath = HttpContext.Current.Server.MapPath("~");
                    //serverPath = "C:\\inetpub\\wwwroot\\admin-uat-api\\wwwroot";
                    //serverPath = "D:\\GSV\\Ufirm\\Dashboard-API-New\\UFirm\\Ufirm.Service\\Ufirm.Service.Common\\wwwroot";
                    var imagePath = Path.Combine(serverPath, "FacilityMemberImages", model.ImageFileName);
                    using (var writer = new BinaryWriter(File.OpenWrite(imagePath)))
                    {
                        writer.Write(model.ImageFile);
                    }
                    //End Uploading Profile Image ...

                    if (model.Document != null)
                    {
                        if (model.Document.Length > 0)
                        {
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Connection = con;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = "delete from [App].[FacilityMemberDocument] where FacilityMemberId=@FacilityMemberId";
                                cmd.Parameters.AddWithValue("@FacilityMemberId", model.FacilityMemberId);
                                cmd.ExecuteNonQuery();
                            }

                            List<DocumentModelNew> documentList = model.Document != null ? GetDocumentModel(model.Document) : new List<DocumentModelNew>();
                            foreach (var item in documentList)
                            {
                                var file = item.DocumentURL;
                                string imageName = item.DocumentName;
                                imagePath = Path.Combine(serverPath, "FacilityMemberDocuments", imageName);

                                using (var writer = new BinaryWriter(File.OpenWrite(imagePath)))
                                {
                                    writer.Write(file); 
                                }

                                using (SqlCommand cmd = new SqlCommand())
                                {
                                    cmd.Connection = con;
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = "Insert into [App].[FacilityMemberDocument] (FacilityMemberId,DocumentTypeId,DocumentName,DocumentUrl,CreatedBy,CreatedOn,IsDeleted) values(@FacilityMemberId,@DocumentTypeId,@DocumentName,@DocumentUrl,@CreatedBy,@CreatedOn,@IsDeleted)";
                                    cmd.Parameters.AddWithValue("@FacilityMemberId", model.FacilityMemberId);
                                    cmd.Parameters.AddWithValue("@DocumentTypeId", Convert.ToInt32(item.DocumentTypeId));
                                    cmd.Parameters.AddWithValue("@DocumentName", item.DocumentName);
                                    cmd.Parameters.AddWithValue("@DocumentUrl", imageName);
                                    cmd.Parameters.AddWithValue("@CreatedBy", "1");
                                    cmd.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@IsDeleted", false);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    con.Close();
                }

                return Ok("success");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [Route("FacilityMemberKYCUpload")]
        [HttpPost]
        public IHttpActionResult FacilityMemberKYCSave(FromDataFacilityMemberModel model)
        {
            var id = 0;
            string err = "";
            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();

                    List<DocumentModelNew> documents = model.Document != null ? GetDocumentModel(model.Document) : new List<DocumentModelNew>();

                    //Setting Server to Upload Documents ...
                    string serverPath = HttpContext.Current.Server.MapPath("~");
                    serverPath = HttpContext.Current.Server.MapPath("").Substring(0, HttpContext.Current.Server.MapPath("").LastIndexOf("\\"));
                    //End Setting Server to Upload Documents ...

                    if (model.Document != null)
                    {
                        if (model.Document.Length > 0)
                        {
                            //using (SqlCommand cmd = new SqlCommand())
                            //{
                            //    cmd.Connection = con;
                            //    cmd.CommandType = CommandType.Text;
                            //    cmd.CommandText = "delete from [App].[FacilityMemberDocument] where FacilityMemberId=@FacilityMemberId";
                            //    cmd.Parameters.AddWithValue("@FacilityMemberId", model.FacilityMemberId);
                            //    cmd.ExecuteNonQuery();
                            //}

                            List<DocumentModelNew> documentList = model.Document != null ? GetDocumentModel(model.Document) : new List<DocumentModelNew>();
                            foreach (var item in documentList)
                            {
                                var file = item.DocumentURL;
                                var imagePath = Path.Combine(serverPath, "admin-uat-api\\wwwroot\\FacilityMemberDocuments", item.DocumentName);
                                
                                //serverPath = HttpContext.Current.Server.MapPath("~");
                                //imagePath = Path.Combine(serverPath, "FacilityMemberImages", item.DocumentName);

                                using (var writer = new BinaryWriter(File.OpenWrite(imagePath)))
                                {
                                    writer.Write(file);
                                }

                                using (SqlCommand cmd = new SqlCommand())
                                {
                                    cmd.Connection = con;
                                    cmd.CommandType = CommandType.Text;

                                    cmd.Parameters.AddWithValue("@FacilityMemberId", model.FacilityMemberId);
                                    cmd.Parameters.AddWithValue("@facilityMemberDocumentId", item.facilityMemberDocumentId);
                                    cmd.Parameters.AddWithValue("@DocumentTypeId", Convert.ToInt32(item.DocumentTypeId));
                                    cmd.Parameters.AddWithValue("@DocumentName", item.DocumentName);
                                    cmd.Parameters.AddWithValue("@DocumentUrl", item.DocumentName);
                                    cmd.Parameters.AddWithValue("@CreatedBy", "1");
                                    cmd.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@IsDeleted", false);

                                    if (item.facilityMemberDocumentId > 0)
                                    {
                                        cmd.CommandText = "update [App].[FacilityMemberDocument] set FacilityMemberId=@FacilityMemberId,DocumentTypeId=@DocumentTypeId,DocumentName=@DocumentName,DocumentUrl=@DocumentUrl where FacilityMemberDocumentId = @facilityMemberDocumentId";
                                    }
                                    else
                                    {
                                        cmd.CommandText = "Insert into [App].[FacilityMemberDocument] (FacilityMemberId,DocumentTypeId,DocumentName,DocumentUrl,CreatedBy,CreatedOn,IsDeleted) values(@FacilityMemberId,@DocumentTypeId,@DocumentName,@DocumentUrl,@CreatedBy,@CreatedOn,@IsDeleted)";
                                    }

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    con.Close();
                }

                return Ok("success");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        private List<DocumentModelNew> GetDocumentModel(string str)
        {
            var objects = JArray.Parse(str);
            List<DocumentModelNew> docModelList = new List<DocumentModelNew>();
            foreach (JObject root in objects)
            {
                DocumentModelNew docModel = new DocumentModelNew();
                foreach (KeyValuePair<String, JToken> app in root)
                {
                    switch (app.Key)
                    {
                        case "documentTypeName":
                            docModel.DocumentTypeName = app.Value.ToObject<string>().ToString();
                            break;
                        case "documentTypeId":
                            docModel.DocumentTypeId = app.Value.ToObject<string>().ToString();
                            break;
                        case "documentNumber":
                            docModel.DocumentNumber = app.Value.ToObject<string>().ToString();
                            break;
                        case "documentName":
                            docModel.DocumentName = app.Value.ToObject<string>().ToString();
                            break;
                        case "documentUrl":
                            docModel.DocumentURL = app.Value.ToObject<byte[]>();
                            break;
                        case "documentFileName":
                            docModel.DocumentFileName = app.Value.ToObject<string>().ToString();
                            break;
                        case "documentExt":
                            docModel.DocumentExt = app.Value.ToObject<string>().ToString();
                            break;
                        case "facilityMemberDocumentId":
                            docModel.facilityMemberDocumentId = app.Value.ToObject<int>();
                            break;
                        default:
                            break;
                    }
                }
                docModelList.Add(docModel);
            }
            return docModelList;
        }

        private string GenerateNewRandom()
        {
            Random generator = new Random();
            String r = generator.Next(0, 1000000).ToString("D6");
            if (r.Distinct().Count() == 1)
            {
                r = GenerateNewRandom();
            }
            //int facilityMasterId = _ufirmUnitOfWork.GetRepository<FacilityMember>().GetAll().Where(o => o.AccessCode == r).Select(o => o.FacilityMasterId).FirstOrDefault();
            //if (facilityMasterId != 0)
            //{
            //    r = GenerateNewRandom();
            //}
            return r;
        }



        //select bkg.id, eve.Title, tsk.Name, mem.Name, bkg.isapprove from [dbo].[EventTasksResiBooking] bkg
        //inner join[calendar].[Event] eve on eve.EventId= bkg.eventid
        //inner join [dbo].TaskMaster tsk on tsk.Id= bkg.taskid
        //inner join [App].[PropertyMember] mem on mem.PropertyMemberId= bkg.bookedby

        [Route("EventTaskBooking")]
        [HttpGet]
        public IHttpActionResult EventTaskBooking()
        {
            var id = 0;
            string err = "";
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.Text;

                        cmd.CommandText = "select bkg.id, eve.Title EventName, tsk.Name TaskName, mem.Name BookedByName, bkg.isapprove from [dbo].[EventTasksResiBooking] bkg inner join[calendar].[Event] eve on eve.EventId= bkg.eventid inner join [dbo].TaskMaster tsk on tsk.Id= bkg.taskid inner join [App].[PropertyMember] mem on mem.PropertyMemberId= bkg.bookedby order by bkg.id";

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(ds);
                    }
                    con.Close();
                }

                var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new EventTask
                {
                    Id = dataRow.Field<int>("Id"),
                    EventName = dataRow.Field<string>("EventName"),
                    TaskName = dataRow.Field<string>("TaskName"),
                    BookedByName = dataRow.Field<string>("BookedByName"),
                    IsApproved = dataRow["isapprove"] == DBNull.Value ? 0 : dataRow.Field<int>("isapprove")
                })).ToList();

                return Ok(eList);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [Route("ApproveEventTaskBooking")]
        [HttpPost]
        public IHttpActionResult ApproveEventTaskBooking(int BookingId)
        {
            var id = 0;
            string err = "";
            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.AddWithValue("@BookingId", BookingId);

                        cmd.CommandText = "update [dbo].[EventTasksResiBooking] set isapprove = 1 where id=@BookingId";

                        cmd.ExecuteNonQuery();
                    }

                    con.Close();
                }

                return Ok("success");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [Route("SaveEventTask")]
        [HttpPost]
        public IHttpActionResult SaveEventTask(List<EventTask> model)
        {
            var id = 0;
            string err = "";
            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    con.Open();

                    foreach (var item in model)
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.AddWithValue("@EventId", item.EventID);
                            cmd.Parameters.AddWithValue("@TaskId", item.TaskID);
                            cmd.Parameters.AddWithValue("@CreatedBy", "1");
                            cmd.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
                            cmd.Parameters.AddWithValue("@IsActive", 1);

                            if (item.Id > 0)
                            {
                                //cmd.CommandText = "update [App].[FacilityMemberDocument] set FacilityMemberId=@FacilityMemberId,DocumentTypeId=@DocumentTypeId,DocumentName=@DocumentName,DocumentUrl=@DocumentUrl where FacilityMemberDocumentId = @facilityMemberDocumentId";
                            }
                            else
                            {
                                cmd.CommandText = "insert into [dbo].EventTasks(EventId, TaskId, CreatedBy, CreatedOn, IsActive)values(@EventId, @TaskId, @CreatedBy, @CreatedOn, @IsActive)";
                            }

                            cmd.ExecuteNonQuery();
                        }
                    }

                    con.Close();
                }

                return Ok("success");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [Route("ProductList")]
        [HttpGet]
        public IHttpActionResult ProductList(int catID = 0, int subCatID = 0, string occurrence = "0", int assingedtoID = 0, DateTime? dteFr = null, DateTime? dteTo = null, string taskstatus = null)
        {
            SqlConnection connection = new SqlConnection(constr);
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            DataSet ds = new DataSet();

            try
            {
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                command.CommandText = "select top 10 cm.CategoryName CategoryName, sm.SubCategoryName SubCategoryName, t.Name Name, isnull(t.Description,'')  Description, (trim(t.Occurence) + ' (' + cast(t.TimeFrom as varchar(8)) + ' - ' + cast(t.TimeTo as varchar(8)) + ')') Occurence, isnull(stat.Remarks,'') Remarks, t.id taskID, t.datefrom, t.dateto, t.timefrom, t.timeto, tm.modify, fm.name AassignedTo, t.CategoryId, t.SubCategoryId, t.AssignTo, t.qrcode, iif(stat.TotalQuests>0,iif((stat.TotalQuests-stat.ComplQuests)=0,'Compleate', iif(stat.ComplQuests>0,iif(stat.ComplQuests=stat.TotalQuests,'Compleate', 'Actionable'),'Pending')), 'Pending') as taskStatus, stat.updatedon from TaskMaster t left join [calendar].Category cm on t.CategoryId=cm.ScheduleCategoryId left join [calendar].[SubCategory] sm on t.SubCategoryId=sm.SubCategoryId left outer join (select distinct tm.id taskid, 1 as modify from TaskWiseTransaction t inner join taskmaster tm on t.TaskName=tm.Name) tm on t.id=tm.taskid left join app.facilitymember fm on t.assignto = fm.FacilityMemberId left join dbo.taskstatus stat on t.id=stat.taskid where 1=1";


                command.CommandText += " order by t.id desc";

                adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                //var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new TaskTransactionModel
                //{
                //    CategoryName = dataRow.Field<string>("CategoryName"),
                //    SubCategoryName = dataRow.Field<string>("SubCategoryName"),
                //    Name = dataRow.Field<string>("Name"),
                //    Description = dataRow.Field<string>("Description"),
                //    Occurence = dataRow.Field<string>("Occurence"),
                //    Remarks = dataRow.Field<string>("Remarks"),
                //    TaskCategoryId = dataRow.Field<int>("CategoryId"),
                //    TaskSubCategoryId = dataRow.Field<int>("SubCategoryId"),
                //    TaskId = dataRow.Field<int>("taskID"),
                //    DateFrom = dataRow["datefrom"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : dataRow.Field<DateTime>("datefrom"),
                //    DateTo = dataRow["dateto"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : dataRow.Field<DateTime>("dateto"),
                //    TimeFrom = dataRow["timefrom"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : Convert.ToDateTime(dataRow["timefrom"].ToString()),
                //    TimeTo = dataRow["timeto"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : Convert.ToDateTime(dataRow["timeto"].ToString()),
                //    AssignedTo = dataRow.Field<string>("AassignedTo"),
                //    AssignedToId = dataRow.Field<int>("AssignTo"),
                //    QRCode = dataRow.Field<string>("QRCode"),
                //    TaskStatus = dataRow.Field<string>("taskStatus"),
                //    UpdatedOn = dataRow["updatedon"] == DBNull.Value ? "" : dataRow.Field<DateTime>("updatedon").ToString("dd-MM-yyyy")
                //})).ToList();


                var productList = (ds.Tables[0].AsEnumerable().Select(dataRow => new Product
                {

                    ProductID = dataRow.Field<int>("taskID"),
                    ProductName = dataRow.Field<string>("Name"),
                    UnitPrice = 1200,// product.UnitPrice,
                    UnitsInStock = 1000,// product.UnitsInStock,
                    QuantityPerUnit = "1000",// product.QuantityPerUnit,
                    TotalSales = 36879,
                    Discontinued = true,//rand.Next(1, 3) % 2 == 0 ? true : false,
                    UnitsOnOrder = 1000,// product.UnitsOnOrder,
                    CategoryID = dataRow.Field<int>("CategoryId"),//product.CategoryID,
                    Country = new CountryViewModel() { CountryNameLong = "", CountryNameShort = "" },//countries[rand.Next(0, 7)],
                    CustomerRating = 2,//rand.Next(0, 6),
                    TargetSales = 3156,//rand.Next(7, 101),
                    CountryID = 0,
                    Category = new CategoryViewModel()
                    {
                        CategoryID = dataRow.Field<int>("CategoryId"),
                        CategoryName = dataRow.Field<string>("CategoryName")
                    },

                    LastSupply = Convert.ToDateTime("2023-01-12")//dataRow["updatedon"] == DBNull.Value ? Convert.ToDateTime("1900-01-01") : dataRow.Field<DateTime>("updatedon")
                })).ToList();

                return Ok(productList);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            finally
            {
                ds.Dispose();
                connection.Close();
            }
            
            return Ok();
        }


        [Route("SpotVisitDetails")]
        [HttpGet]
        public IHttpActionResult SpotVisitDetails(int guardId, DateTime? visitDate = null)
        {
            SqlConnection connection = new SqlConnection(constr);
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            DataSet ds = new DataSet();

            try
            {
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                command.CommandText = "select visit.Id, EmployeeName, MobileNo, createdon as VisitDate, Cast(createdon as date) VisitTime, Lat Latitude, Longi Longitude from SpotVisitDetails visit inner join GuardMaster guard on visit.createdby=guard.id where guard.Id = @guardId";

                command.Parameters.AddWithValue("@guardId", guardId);

                if (visitDate != null)
                {
                    command.CommandText += " and cast(createdon as date) = cast(@visitdate as date)";
                    command.Parameters.AddWithValue("@visitdate", visitDate);
                }

                command.CommandText += " order by visit.Id";

                adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                var spotVisitDetails = (ds.Tables[0].AsEnumerable().Select(dataRow => new SpotVisitDetail
                {
                    Id= dataRow.Field<int>("Id"),
                    MobileNo = dataRow.Field<string>("MobileNo"),
                    Latitude = dataRow.Field<decimal>("Latitude"),
                    Longitude = dataRow.Field<decimal>("Longitude"),
                    VisitDate = dataRow.Field<DateTime>("VisitDate")
                })).ToList();

                return Ok(spotVisitDetails);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            finally
            {
                ds.Dispose();
                connection.Close();
            }

            return Ok();
        }

        [Route("AssetTracking")]
        [HttpGet]
        public IHttpActionResult AssetTracking(int assetId = 0, DateTime? dteFr = null, DateTime? dteTo = null, string taskstatus = null)
        {
            SqlConnection connection = new SqlConnection(constr);
            SqlDataAdapter adapter;
            SqlCommand command = new SqlCommand();
            DataSet ds = new DataSet();

            try
            {
                connection.Open();
                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                command.CommandText = " select cm.CategoryName CategoryName, sm.SubCategoryName SubCategoryName, t.Name Name, isnull(t.Description,'')  Description, (trim(t.Occurence) + ' (' + cast(t.TimeFrom as varchar(8)) + ' - ' + cast(t.TimeTo as varchar(8)) + ')') Occurence, isnull(stat.Remarks,'') Remarks, t.id taskID, ";


                if (dteFr == null)
                {
                    command.CommandText += " t.datefrom DateFrom, ";
                }
                else
                {
                    command.CommandText += " iif(t.occurence='D', '" + Convert.ToDateTime(dteFr).ToString("MM/dd/yyyy") + "', t.datefrom) DateFrom, ";
                }

                command.CommandText += " t.dateto, t.timefrom, t.timeto, tm.modify, fm.name AassignedTo, t.CategoryId, t.SubCategoryId, t.AssignTo, t.qrcode, iif(stat.TotalQuests>0,iif((stat.TotalQuests-stat.ComplQuests)=0,'Compleate', iif(stat.ComplQuests>0,iif(stat.ComplQuests=stat.TotalQuests,'Compleate', 'Actionable'),'Pending')), 'Pending') as taskStatus, stat.updatedon, assets.Name AssetName, assets.id AssetId from TaskMaster t left join [calendar].Category cm on t.CategoryId=cm.ScheduleCategoryId left join [calendar].[SubCategory] sm on t.SubCategoryId=sm.SubCategoryId left outer join (select distinct tm.id taskid, 1 as modify from TaskWiseTransaction t inner join taskmaster tm on t.TaskName=tm.Name) tm on t.id=tm.taskid left join app.facilitymember fm on t.assignto = fm.FacilityMemberId left join dbo.taskstatus stat on t.id=stat.taskid inner join AssetMaster assets on t.AssetsId=assets.Id where 1=1 and assets.id = " + assetId;

                if (taskstatus != null)
                {
                    if (taskstatus.ToUpper() == "PENDING")
                    {
                        //command.Parameters.AddWithValue("@AssignTo", assingedtoID);
                        command.CommandText += " and stat.ComplQuests=0";
                    }
                    if (taskstatus.ToUpper() == "COMPLETE")
                    {
                        command.CommandText += " and (stat.TotalQuests>0 and stat.ComplQuests=stat.TotalQuests)";
                    }
                    if (taskstatus.ToUpper() == "ACTIONABLE")
                    {
                        command.CommandText += " and (stat.ComplQuests<stat.TotalQuests and stat.ComplQuests>0)";
                    }
                }

                command.CommandText += " order by t.id desc";

                adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                var eList = (ds.Tables[0].AsEnumerable().Select(dataRow => new TaskTransactionModel
                {
                    CategoryName = dataRow.Field<string>("CategoryName"),
                    SubCategoryName = dataRow.Field<string>("SubCategoryName"),
                    Name = dataRow.Field<string>("Name"),
                    Description = dataRow.Field<string>("Description"),
                    Occurence = dataRow.Field<string>("Occurence"),
                    Remarks = dataRow.Field<string>("Remarks"),
                    TaskCategoryId = dataRow.Field<int>("CategoryId"),
                    TaskSubCategoryId = dataRow.Field<int>("SubCategoryId"),
                    TaskId = dataRow.Field<int>("taskID"),
                    DateFrom = dataRow["datefrom"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : dataRow.Field<DateTime>("datefrom"),
                    DateTo = dataRow["dateto"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : dataRow.Field<DateTime>("dateto"),
                    TimeFrom = dataRow["timefrom"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : Convert.ToDateTime(dataRow["timefrom"].ToString()),
                    TimeTo = dataRow["timeto"] == DBNull.Value ? Convert.ToDateTime("1900/01/01") : Convert.ToDateTime(dataRow["timeto"].ToString()),
                    AssignedTo = dataRow.Field<string>("AassignedTo"),
                    AssignedToId = dataRow.Field<int>("AssignTo"),
                    QRCode = dataRow.Field<string>("QRCode"),
                    TaskStatus = dataRow.Field<string>("taskStatus"),
                    UpdatedOn = dataRow["updatedon"] == DBNull.Value ? "" : dataRow.Field<DateTime>("updatedon").ToString("dd-MM-yyyy"),
                    AssetName = dataRow.Field<string>("AssetName"),
                    AssetId = dataRow.Field<int>("AssetId")
                })).ToList();

                return Ok(eList);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            finally
            {
                ds.Dispose();
                connection.Close();
            }

            var data = new TaskTransactionModel()
            {
                CategoryName = "",
                SubCategoryName = "",
                Name = "",
                Description = "",
                Occurence = "",
                Remarks = "",
                TaskCategoryId = 0,
                TaskSubCategoryId = 0,
                TaskId = 0,
                DateFrom = Convert.ToDateTime("1900/01/01"),
                DateTo = Convert.ToDateTime("1900/01/01"),
                TimeFrom = Convert.ToDateTime("1900/01/01"),
                TimeTo = Convert.ToDateTime("1900/01/01"),
                AssignedTo = "",
                AssignedToId = 0,
                QRCode = "",
                TaskStatus = "",
                AssetName="",
                AssetId=0
            };
            List<TaskTransactionModel> eListB = new List<TaskTransactionModel>();
            eListB.Add(data);
            return Ok(eListB);
        }


    }
}
