using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBillPutoutDetailMap : EntityTypeConfiguration<WmsBillPutoutDetail>
    {
        public WmsBillPutoutDetailMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .HasMaxLength(128);

            this.Property(t => t.SourceOrder)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.ItemCode)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.ItemName)
                .IsRequired()
                .HasMaxLength(720);

            this.Property(t => t.Lot)
                .HasMaxLength(128);

            this.Property(t => t.Lot)
                .HasMaxLength(128);

            this.Property(t => t.WarehouseCode)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.ZoneCode)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.LocationCode)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.Plant)
                .HasMaxLength(30);

            this.Property(t => t.OutLine)
                .HasMaxLength(30);

            this.Property(t => t.AgvLine)
                .HasMaxLength(30);

            this.Property(t => t.WorkCell)
                .HasMaxLength(30);

            this.Property(t => t.Executor)
                .HasMaxLength(20);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Wms_Bill_PutoutDetail".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.SourceOrder).HasColumnName("SourceOrder".ToUpper());
            this.Property(t => t.ItemCode).HasColumnName("ItemCode".ToUpper());
            this.Property(t => t.ItemName).HasColumnName("ItemName".ToUpper());
            this.Property(t => t.Lot).HasColumnName("Lot".ToUpper());
            this.Property(t => t.Qty).HasColumnName("Qty".ToUpper());
            this.Property(t => t.CtnQty).HasColumnName("CtnQty".ToUpper());
            this.Property(t => t.Priority).HasColumnName("Priority".ToUpper());
            this.Property(t => t.WarehouseCode).HasColumnName("WarehouseCode".ToUpper());
            this.Property(t => t.ZoneCode).HasColumnName("ZoneCode".ToUpper());
            this.Property(t => t.LocationCode).HasColumnName("LocationCode".ToUpper());
            this.Property(t => t.Plant).HasColumnName("Plant".ToUpper());
            this.Property(t => t.OutLine).HasColumnName("OutLine".ToUpper());
            this.Property(t => t.AgvLine).HasColumnName("AgvLine".ToUpper());
            this.Property(t => t.WorkCell).HasColumnName("WorkCell".ToUpper());
            this.Property(t => t.State).HasColumnName("State".ToUpper());
            this.Property(t => t.Executor).HasColumnName("Executor".ToUpper());
            this.Property(t => t.SelectTime).HasColumnName("SelectTime".ToUpper());
            this.Property(t => t.FinishTime).HasColumnName("FinishTime".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}