using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsTransferTaskMap : EntityTypeConfiguration<WmsTransferTask>
    {
        public WmsTransferTaskMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.SourceOrder)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.SourceWarehouseCode)
                 .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.SourceZoneCode)
                 .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.SourceLocationCode)
                 .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.TargetWarehouseCode)
                 .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.TargetZoneCode)
                 .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.TargetLocationCode)
                 .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.ItemCode)
                .HasMaxLength(128);

            this.Property(t => t.ItemName)
                .HasMaxLength(720);

            this.Property(t => t.Barcode)
                .HasMaxLength(128);

            this.Property(t => t.Lot)
                .HasMaxLength(128);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Wms_Transfer_Task".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.SourceOrder).HasColumnName("SourceOrder".ToUpper());
            this.Property(t => t.SourceWarehouseCode).HasColumnName("SourceWarehouseCode".ToUpper());
            this.Property(t => t.SourceZoneCode).HasColumnName("SourceZoneCode".ToUpper());
            this.Property(t => t.SourceLocationCode).HasColumnName("SourceLocationCode".ToUpper());
            this.Property(t => t.TargetWarehouseCode).HasColumnName("TargetWarehouseCode".ToUpper());
            this.Property(t => t.TargetZoneCode).HasColumnName("TargetZoneCode".ToUpper());
            this.Property(t => t.TargetLocationCode).HasColumnName("TargetLocationCode".ToUpper());
            this.Property(t => t.Barcode).HasColumnName("Barcode".ToUpper());
            this.Property(t => t.ItemCode).HasColumnName("ItemCode".ToUpper());
            this.Property(t => t.ItemName).HasColumnName("ItemName".ToUpper());
            this.Property(t => t.Lot).HasColumnName("Lot".ToUpper());
            this.Property(t => t.Qty).HasColumnName("Qty".ToUpper());
            this.Property(t => t.TransferFlag).HasColumnName("TransferFlag".ToUpper());
            this.Property(t => t.TransferTime).HasColumnName("TransferTime".ToUpper());
            this.Property(t => t.StockFlag).HasColumnName("StockFlag".ToUpper());
            this.Property(t => t.StockTime).HasColumnName("StockTime".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}