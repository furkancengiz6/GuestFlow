using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core
{
    public class BaseEntity
    {
        public int Id { get; set; }
        
        public DateTime CreatedDate { get; set; }

        public bool IsDeleted { get; set; }
        public BaseEntity()
        {
            CreatedDate = DateTime.Now;
            IsDeleted = false;
        }
    }

    public abstract class BaseConfiguration<TEntity>:IEntityTypeConfiguration<TEntity> where TEntity : BaseEntity
    {
       //Hepsi için geçerli bir yapı

        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasQueryFilter(x => !x.IsDeleted); // Soft delete filtresi
            //bu veritabanı üzerinde yapılacak tüm sogrulamalarda ve diğer linq işlemlerinde geçerli olacak bir giltreleme yazdık. bÖYLELİKLE HİÇBİR ZAMAN BİR DAHA SOft delete atılmış verelerle uğraşmayacağız. 
        }
    }


}
