using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using UrestComplaintWebApi.Models;

namespace UrestComplaintWebApi.Controllers
{
    public class PropertyMembersController : ApiController
    {
        private UfirmApp_ProductionEntities db = new UfirmApp_ProductionEntities();

        // GET: api/PropertyMembers
        public IQueryable<PropertyMember> GetPropertyMembers()
        {
            return db.PropertyMembers;
        }

        // GET: api/PropertyMembers/5
        [ResponseType(typeof(PropertyMember))]
        public IHttpActionResult GetPropertyMember(int id)
        {
            PropertyMember propertyMember = db.PropertyMembers.Find(id);
            if (propertyMember == null)
            {
                return NotFound();
            }

            return Ok(propertyMember);
        }

        // PUT: api/PropertyMembers/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutPropertyMember(int id, PropertyMember propertyMember)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != propertyMember.PropertyMemberId)
            {
                return BadRequest();
            }

            db.Entry(propertyMember).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PropertyMemberExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/PropertyMembers
        [ResponseType(typeof(PropertyMember))]
        public IHttpActionResult PostPropertyMember(PropertyMember propertyMember)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.PropertyMembers.Add(propertyMember);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = propertyMember.PropertyMemberId }, propertyMember);
        }

        // DELETE: api/PropertyMembers/5
        [ResponseType(typeof(PropertyMember))]
        public IHttpActionResult DeletePropertyMember(int id)
        {
            PropertyMember propertyMember = db.PropertyMembers.Find(id);
            if (propertyMember == null)
            {
                return NotFound();
            }

            db.PropertyMembers.Remove(propertyMember);
            db.SaveChanges();

            return Ok(propertyMember);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PropertyMemberExists(int id)
        {
            return db.PropertyMembers.Count(e => e.PropertyMemberId == id) > 0;
        }
    }
}