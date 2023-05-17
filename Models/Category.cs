using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UrestComplaintWebApi.Models
{
    public class User
    {
        public int UserIdId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public string MobileNumber { get; set; }
        public string ProfileImageUrl { get; set; }
        public string UserRole { get; set; }

        //[JsonIgnore]
        //public string Password { get; set; }
    }
    public static class UserDetails
    {
        //public static User GetUserData(HttpContext contect)
        //{
        //    //var email = contect.User.Claims.First(x => x.Type == "EmailAddress").Value;
        //    //var fistname = contect.User.Claims.First(x => x.Type == "FirstName").Value;
        //    //var lastname = contect.User.Claims.First(x => x.Type == "LastName").Value;
        //    //var mobilenumber = contect.User.Claims.First(x => x.Type == "MobileNumber").Value;
        //    //var userid = Convert.ToInt32(contect.User.Claims.First(x => x.Type == "UserIdId").Value);
        //    User oUser = new User();
        //    oUser.EmailAddress = contect.User.Claims.First(x => x.Type == "EmailAddress").Value; ;
        //    //oUser.MobileNumber = "6260983195";
        //    oUser.MobileNumber = contect.User.Claims.First(x => x.Type == "MobileNumber").Value; ;
        //    oUser.UserIdId = Convert.ToInt32(contect.User.Claims.First(x => x.Type == "UserIdId").Value); ;
        //    oUser.FirstName = contect.User.Claims.First(x => x.Type == "FirstName").Value; ;
        //    oUser.LastName = contect.User.Claims.First(x => x.Type == "LastName").Value; ;
        //    oUser.UserRole = contect.User.Claims.First(x => x.Type == "UserRole").Value; ;
        //    oUser.ProfileImageUrl = contect.User.Claims.First(x => x.Type == "ProfileImageUrl").Value; ;
        //    return oUser;
        //}
    }

    public class EventTask
    {
        public int Id { get; set; }
        public int EventID { get; set; }
        public int TaskID { get; set; }
        public string EventName { get; set; }
        public string TaskName { get; set; }
        public string BookedByName { get; set; }
        public int CreateBy { get; set; }
        public DateTime CreateOn { get; set; }
        public int IsActive { get; set; }
        public int IsDeleted { get; set; }
        public int BookedBy { get; set; }
        public int IsApproved { get; set; }
    }

    public class FromDataFacilityMemberModel
    {
        public int FacilityMemberId { get; set; }
        public int PropertyId { get; set; }
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public int FacilityMasterId { get; set; }
        public int PropertyTowerId { get; set; }
        public int PropertyFloorId { get; set; }
        public int PropertyFlatId { get; set; }
        public string PropertyDetailsIds { get; set; }
        public int ApprovedBy { get; set; }
        public string ApprovedOn { get; set; }
        public byte[] ImageFile { get; set; }
        public string ImageFileName { get; set; }
        public string ImageExt { get; set; }
        public string Document { get; set; }
        public IList<byte[]> Files { get; set; }
        public string SaveType { get; set; }
    }
    public class DocumentModelNew
    {
        public int facilityMemberDocumentId { get; set; }
        public string DocumentTypeName { get; set; }
        public string DocumentTypeId { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentName { get; set; }
        public byte[] DocumentURL { get; set; }
        public string DocumentFileName { get; set; }
        public string DocumentExt { get; set; }
    }

    public class AssetsMaster
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string QRCode { get; set; }
        public string Flag { get; set; }
        public string AssetType { get; set; }
        public string Manufacturer { get; set; }
        public string AssetModel { get; set; }
        public bool IsMovable { get; set; }
    }

    public class Category
    {
        public int SubCategoryId { get; set; }

        public int CategoryId { get; set; }

        public string SubCategoryName { get; set; }


    }

    public class Employee
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string FatherName { get; set; }
        public string Designation { get; set; }
        public string MobileNo { get; set; }
        public int IsDeleted { get; set; }
        public int Approved { get; set; }
    }

    public class GuardMaster
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string FatherName { get; set; }
        public string Designation { get; set; }
        public string MobileNo { get; set; }
        public int IsDeleted { get; set; }
        public int Approved { get; set; }
    }

    public class AttendanceLogs
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string PunchTime { get; set; }
        public string PunchType { get; set; }
        public string GateNo { get; set; }
    }

    public class AmenitiesBookings
    {
        public int Id { get; set; }
        public int AmenitiesId { get; set; }
        public int PropertyId { get; set; }
        public string AmenitiesName { get; set; }
        public string TimeSlot { get; set; }
        public DateTime TimeSlotFr { get; set; }
        public DateTime TimeSlotTo { get; set; }
        public int NosOfPersons { get; set; }
        public int Approved { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string MobileNo { get; set; }
        public string ApproveStatus { get; set; }

    }

    public class KycDetails
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Gender { get; set; }
        public string JobProfile { set; get; }
        public string MobileNo { set; get; }
        public string IdDoc { get; set; }
        public string Image { get; set; }
        public string ImageExt { get; set; }
        public byte[] ImageData { get; set; }
        public string IdImage { get; set; }
        public string ApproveStatus { get; set; }
    }

    public class TaskWiseQuestionnaire
    {
        public int TransactionID { get; set; }
        public int CategoryID { get; set; }
        public int SubCategoryID { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public string Occurance { get; set; }
        public int QuestID { get; set; }
        public string QuestionName { get; set; }
        public string Action { get; set; }
        public string Remarks { get; set; }
    }

    public class TaskWiseQuestions
    {
        public int TaskID { get; set; }
        public int QuestID { get; set; }
        public string QuestionName { get; set; }
    }

    public class AssignToList
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TaskTransactionModel
    {
        public TaskTransactionModel()
        {
            this.TaskTransactionModel1 = new HashSet<TaskTransactionModel>();
        }

        public virtual ICollection<TaskTransactionModel> TaskTransactionModel1 { get; set; }
        public virtual TaskTransactionModel TaskTransactionModel2 { get; set; }
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int TaskCategoryId { get; set; }
        public int TaskSubCategoryId { get; set; }
        public string TaskStatus { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? TimeFrom { get; set; }
        public DateTime? TimeTo { get; set; }
        public string Remarks { get; set; }
        public string Occurence { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public int EntryType { get; set; }
        public string AssignedTo { get; set; }
        public int AssignedToId { get; set; }
        public string QRCode { get; set; }
        public string QuestionName { get; set; }
        public string RemarksQuestion { get; set; }
        public string UpdatedOn { get; set; }
        public string AssetName { get; set; }
        public int AssetId { get; set; }
    }

    public class TaskMaster
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? TimeFrom { get; set; }
        public DateTime? TimeTo { get; set; }
        public string Remarks { get; set; }
        public string Occurence { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int AssignTo { get; set; }
        public string RemindMe { get; set; }
        public string Location { get; set; }
        public int AssetsID { get; set; }
        public string QRCode { get; set; }
    }

    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } // = product.ProductName,
        public decimal UnitPrice { get; set; } //  = product.UnitPrice,
        public int UnitsInStock { get; set; } //  = product.UnitsInStock,
        public int TotalSales { get; set; }
        public string QuantityPerUnit { get; set; } //  = product.QuantityPerUnit,
        public bool Discontinued { get; set; } //  = rand.Next(1, 3) % 2 == 0 ? true : false,
        public int? CategoryID { get; set; }
        public int? CountryID { get; set; }
        public int UnitsOnOrder { get; set; } //  = product.UnitsOnOrder,
        //public string Country { get; set; } //  = countries[rand.Next(0, 7)],
        public int CustomerRating { get; set; } //  = rand.Next(0, 6),
                                                   //public double TargetSales { get; set; }// = rand.Next(7, 101),
        private int targetSales;
        public int TargetSales
        {
            get
            {
                return targetSales;
            }
            set
            {
                targetSales = value;
                TotalSales = value * 100;
            }
        }
        public CategoryViewModel Category { get; set; }// = new CategoryViewModel()
                                                       //{
                                                       //    CategoryID = product.Category.CategoryID,
                                                       //    CategoryName = product.Category.CategoryName
                                                       //},
        public DateTime LastSupply { get; set; }

        public CountryViewModel Country { get; set; }
    }

    public class SpotVisitDetail
    {
        public int Id { get; set; }
        public string MobileNo { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime? VisitDate { get; set; }
    }

    public class CategoryViewModel
    {
        public string CategoryName { get; set; }
        public int CategoryID { get; set; }
    }

    public class CountryViewModel
    {
        public string CountryNameLong { get; set; }
        public string CountryNameShort { get; set; }
    }

}