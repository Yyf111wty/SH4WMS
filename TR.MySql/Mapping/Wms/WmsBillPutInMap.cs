using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBillPutInMap : EntityTypeConfiguration<WmsBillPutIn>
    {
        public WmsBillPutInMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .HasMaxLength(128);

            this.Property(t => t.SourceOrder)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.WarehouseCode)
               .IsRequired()
               .HasMaxLength(36);

            this.Property(t => t.Remark)
                .HasMaxLength(200);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            this.Property(t => t.Container)
                .IsRequired()
                .HasMaxLength(36);

            this.Property(t => t.Barcode)
               .IsRequired()
               .HasMaxLength(36);

            this.Property(t => t.Lot)
                .HasMaxLength(36);

            this.Property(t => t.ItemCode)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.ItemName)
                .IsRequired()
                .HasMaxLength(720);

            // Table & Column Mappings
            this.ToTable("Wms_Bill_PutIn".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.SourceOrder).HasColumnName("SourceOrder".ToUpper());
            this.Property(t => t.WarehouseCode).HasColumnName("WarehouseCode".ToUpper());
            this.Property(t => t.Container).HasColumnName("Container".ToUpper());
            this.Property(t => t.ItemCode).HasColumnName("ItemCode".ToUpper());
            this.Property(t => t.ItemName).HasColumnName("ItemName".ToUpper());
            this.Property(t => t.Barcode).HasColumnName("Barcode".ToUpper());
            this.Property(t => t.Enabled).HasColumnName("Enabled".ToUpper());
            this.Property(t => t.Remark).HasColumnName("Remark".ToUpper());
            this.Property(t => t.Qty).HasColumnName("Qty".ToUpper());
            this.Property(t => t.Lot).HasColumnName("Lot".ToUpper());
            this.Property(t => t.QuestFlag).HasColumnName("QuestFlag".ToUpper());
            this.Property(t => t.QuestTime).HasColumnName("QuestTime".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}
