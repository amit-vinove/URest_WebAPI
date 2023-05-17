﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class UfirmApp_ProductionEntities : DbContext
    {
        public UfirmApp_ProductionEntities()
            : base("name=UfirmApp_ProductionEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<MemberComplaintRegistration> MemberComplaintRegistrations { get; set; }
        public virtual DbSet<PropertyMember> PropertyMembers { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
    
        public virtual ObjectResult<GetComplaints_Result> GetComplaints(string mobile)
        {
            var mobileParameter = mobile != null ?
                new ObjectParameter("Mobile", mobile) :
                new ObjectParameter("Mobile", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetComplaints_Result>("GetComplaints", mobileParameter);
        }
    }
}
