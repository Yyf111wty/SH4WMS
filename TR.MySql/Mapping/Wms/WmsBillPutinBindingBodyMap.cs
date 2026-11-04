using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBillPutinBindingBodyMap : EntityTypeConfiguration<WmsBillPutinBindingBody>
    {
        public WmsBillPutinBindingBodyMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .HasMaxLength(128);

            this.Property(t => t.HeaderId)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.Container)
                .HasMaxLength(30);

            this.Property(t => t.Barcode)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.Lot)
                .HasMaxLength(128);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            this.Property(t => t.Remark)
               .HasMaxLength(128);

            // Table & Column Mappings
            this.ToTable("Wms_Bill_PutinBindingBody".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.HeaderId).HasColumnName("HeaderId".ToUpper());
            this.Property(t => t.Container).HasColumnName("Container".ToUpper());
            this.Property(t => t.Barcode).HasColumnName("Barcode".ToUpper());
            this.Property(t => t.Lot).HasColumnName("Lot".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
            this.Property(t => t.Enabled).HasColumnName("Enabled".ToUpper());
            this.Property(t => t.Qty).HasColumnName("Qty".ToUpper());
            this.Property(t => t.Remark).HasColumnName("Remark".ToUpper());
            this.HasRequired(t => t.Header)
                .WithMany(t => t.Body)
                .HasForeignKey(d => d.HeaderId);
        }
    }
}