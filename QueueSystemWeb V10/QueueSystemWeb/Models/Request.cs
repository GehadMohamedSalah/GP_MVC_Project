//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QueueSystemWeb.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Request
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Request()
        {
            this.Request_attachement = new HashSet<Request_attachement>();
        }
    
        public int id { get; set; }
        public Nullable<int> user_id { get; set; }
        public Nullable<int> empoyee_id { get; set; }
        public Nullable<int> service_id { get; set; }
        public string state { get; set; }
        public Nullable<int> branch_id { get; set; }
        public string notes { get; set; }
        public Nullable<System.DateTime> timeOfReq { get; set; }
        public Nullable<System.DateTime> timeOfChange { get; set; }
    
        public virtual Branch Branch { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Request_attachement> Request_attachement { get; set; }
        public virtual Services_ Services_ { get; set; }
        public virtual user user { get; set; }
        public virtual user user1 { get; set; }
    }
}
