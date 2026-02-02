using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GuestFlow.Domain.Entities.Interfaces;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Base entity with full audit traceability.
    /// 
    /// AUDIT TRACEABILITY (LOCKED PRODUCT DECISION):
    /// - Date changes MUST leave an operational trace
    /// - Track who created, who updated, and when
    /// - Preserve historical truth at all times
    /// </summary>
    public class BaseEntity : ITenantEntity, ISoftDelete
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        
        /// <summary>
        /// When this record was first created
        /// </summary>
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// Personnel ID who created this record (nullable for system-created records)
        /// </summary>
        public int? CreatedByPersonnelId { get; set; }
        
        /// <summary>
        /// When this record was last updated (null if never updated)
        /// </summary>
        public DateTime? UpdatedDate { get; set; }
        
        /// <summary>
        /// Personnel ID who last updated this record
        /// </summary>
        public int? UpdatedByPersonnelId { get; set; }

        public bool IsDeleted { get; set; }
        
        public BaseEntity()
        {
            CreatedDate = DateTime.UtcNow; // Use UTC for consistency
            IsDeleted = false;
        }
        
        /// <summary>
        /// Mark this entity as updated with audit trace
        /// </summary>
        public void MarkAsUpdated(int? personnelId = null)
        {
            UpdatedDate = DateTime.UtcNow;
            UpdatedByPersonnelId = personnelId;
        }
    }

    public abstract class BaseConfiguration<TEntity>:IEntityTypeConfiguration<TEntity> where TEntity : BaseEntity
    {
       //Hepsi için geçerli bir yapı

        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            // Query filters are now handled centrally in GuestFlowDbContext for multi-tenancy support
            // builder.HasQueryFilter(x => !x.IsDeleted); 
            
            // Audit trail fields indexing
            builder.Property(x => x.CreatedDate).IsRequired();
            builder.Property(x => x.UpdatedDate).IsRequired(false);
            builder.Property(x => x.CreatedByPersonnelId).IsRequired(false);
            builder.Property(x => x.UpdatedByPersonnelId).IsRequired(false);
        }
    }


}
