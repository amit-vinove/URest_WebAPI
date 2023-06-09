//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UrestComplaintWebApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class MemberComplaintRegistration
    {
        public int Id { get; set; }
        public string Mobile { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class SubCategory
    {
        public int SubCategoryId { get; set; }
        public int CategoryId { get; set; }
        public string SubCategoryName { get; set; }
    }

    public class FormDataNoticeBoard
    {
        public string StatementType { get; set; }
        public int PropertyId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public int NotificationTypeId { get; set; }
        public int PropertyGroupId { get; set; }
        // 1,2,3
        public int Notify { get; set; }
        public int AlertTypeId { get; set; }
        public string ExpirtyDate { get; set; }
        public string PropertyDetailsId { get; set; }
        public string PropertyTowerId { get; set; }
        public string PropertyRWAMemberId { get; set; }
        public string UserID { get; set; }
        public List<NoticeBoardAttachment> NoticeBoardAttachment { get; set; }
    }

    public class NoticeBoardAttachment
    {
        public string filename { get; set; }
        public string filepath { get; set; }
    }

    public class NoticeBoardNoticeData
    {
        public int NoticeboardAlertMaster { get; set; }
        public string Subject { get; set; }
        public string NotificationType { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public string ContactNumber { get; set; }
        public string EmailAddress { get; set; }
    }

    public class NoticeBoardAttachments
    {
        public int NotifactionAttachmentsId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }

}
