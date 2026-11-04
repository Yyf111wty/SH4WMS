using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBillCountBodyMap : EntityTypeConfiguration<WmsBillCountBody>
    {
        public WmsBillCountBodyMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.HeaderId)
                .IsRequired()
                .HasMaxLength(128);

        
            this.Property(t => t.ItemCode)
                .HasMaxLength(128);

            this.Property(t => t.ItemName)
                .HasMaxLength(720);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Wms_Bill_CountBody".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.HeaderId).HasColumnName("HeaderId".ToUpper());        
            this.Property(t => t.ItemCode).HasColumnName("ItemCode".ToUpper());
            this.Property(t => t.ItemName).HasColumnName("ItemName".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());

            this.HasRequired(t => t.Header)
                .WithMany(t => t.Body)
                .HasForeignKey(d => d.HeaderId);
        }
    }
}