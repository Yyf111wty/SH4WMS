using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBillPutoutBodyMap : EntityTypeConfiguration<WmsBillPutoutBody>
    {
        public WmsBillPutoutBodyMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .HasMaxLength(128);

            this.Property(t => t.HeaderId)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.ItemCode)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.ItemName)
                .IsRequired()
                .HasMaxLength(720);

            this.Property(t => t.Plant)
                .HasMaxLength(30);

            this.Property(t => t.OutLine)
                .HasMaxLength(30);

            this.Property(t => t.AgvLine)
                .HasMaxLength(30);

            this.Property(t => t.WorkCell)
                .HasMaxLength(30);

            this.Property(t => t.Lot)
                .HasMaxLength(128);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            this.Property(t => t.Remark)
              .HasMaxLength(128);

            this.Property(t => t.Sequence)
              .HasMaxLength(36);

            // Table & Column Mappings
            this.ToTable("Wms_Bill_PutoutBody".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.HeaderId).HasColumnName("HeaderId".ToUpper());
            this.Property(t => t.ItemCode).HasColumnName("ItemCode".ToUpper());
            this.Property(t => t.ItemName).HasColumnName("ItemName".ToUpper());
            this.Property(t => t.Plant).HasColumnName("Plant".ToUpper());
            this.Property(t => t.OutLine).HasColumnName("OutLine".ToUpper());
            this.Property(t => t.AgvLine).HasColumnName("AgvLine".ToUpper());
            this.Property(t => t.WorkCell).HasColumnName("WorkCell".ToUpper());
            this.Property(t => t.Lot).HasColumnName("Lot".ToUpper());
            this.Property(t => t.Qty).HasColumnName("Qty".ToUpper());        
            this.Property(t => t.CtnQty).HasColumnName("CtnQty".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
            this.Property(t => t.Remark).HasColumnName("Remark".ToUpper());
            this.Property(t => t.Sequence).HasColumnName("Sequence".ToUpper());
            this.HasRequired(t => t.Header)
                .WithMany(t => t.Body)
                .HasForeignKey(d => d.HeaderId);
        }
    }
}