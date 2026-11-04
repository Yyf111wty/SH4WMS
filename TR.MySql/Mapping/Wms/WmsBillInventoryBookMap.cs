using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBillInventoryBookMap : EntityTypeConfiguration<WmsBillInventoryBook>
    {
        public WmsBillInventoryBookMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.SourceOrder)
                .HasMaxLength(128);

            this.Property(t => t.SourceOrderType)
                .HasMaxLength(20);

            this.Property(t => t.Sysbolcode)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.Barcode)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.ItemCode)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.ItemName)
                .IsRequired()
                .HasMaxLength(720);

            this.Property(t => t.Lot)
                .HasMaxLength(20);

            this.Property(t => t.WarehouseCode)
                .HasMaxLength(20);

            this.Property(t => t.ZoneCode)
                .HasMaxLength(20);

            this.Property(t => t.LocationCode)
             .HasMaxLength(20);

            this.Property(t => t.SupplierId)
                .HasMaxLength(30);

            this.Property(t => t.SupplierName)
                .HasMaxLength(128);

            this.Property(t => t.FreezeReason)
                .HasMaxLength(20);

            this.Property(t => t.Remark)
                .HasMaxLength(128);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Wms_Bill_InventoryBook".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.SourceOrder).HasColumnName("SourceOrder".ToUpper());
            this.Property(t => t.SourceOrderType).HasColumnName("SourceOrderType".ToUpper());
            this.Property(t => t.Sysbolcode).HasColumnName("Sysbolcode".ToUpper());
            this.Property(t => t.Barcode).HasColumnName("Barcode".ToUpper());
            this.Property(t => t.ItemCode).HasColumnName("ItemCode".ToUpper());
            this.Property(t => t.ItemName).HasColumnName("ItemName".ToUpper());
            this.Property(t => t.Lot).HasColumnName("Lot".ToUpper());
            this.Property(t => t.Qty).HasColumnName("Qty".ToUpper());
            this.Property(t => t.WarehouseCode).HasColumnName("WarehouseCode".ToUpper());
            this.Property(t => t.ZoneCode).HasColumnName("ZoneCode".ToUpper());
            this.Property(t => t.LocationCode).HasColumnName("LocationCode".ToUpper());
            this.Property(t => t.SupplierId).HasColumnName("SupplierId".ToUpper());
            this.Property(t => t.SupplierName).HasColumnName("SupplierName".ToUpper());
            this.Property(t => t.StockFlag).HasColumnName("StockFlag".ToUpper());
            this.Property(t => t.StockTime).HasColumnName("StockTime".ToUpper());
            this.Property(t => t.FreezeFlag).HasColumnName("FreezeFlag".ToUpper());
            this.Property(t => t.FreezeTime).HasColumnName("FreezeTime".ToUpper());
            this.Property(t => t.FreezeReason).HasColumnName("FreezeReason".ToUpper());
            this.Property(t => t.Remark).HasColumnName("Remark".ToUpper());
            this.Property(t => t.FeedbackFlag).HasColumnName("FeedbackFlag".ToUpper());
            this.Property(t => t.FeedbackTime).HasColumnName("FeedbackTime".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}