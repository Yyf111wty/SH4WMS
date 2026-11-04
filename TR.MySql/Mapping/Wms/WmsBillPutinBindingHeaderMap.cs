using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBillPutinBindingHeaderMap : EntityTypeConfiguration<WmsBillPutinBindingHeader>
    {
        public WmsBillPutinBindingHeaderMap()
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
                .HasMaxLength(20);

            this.Property(t => t.Container)
                .IsRequired()
                .HasMaxLength(30);

            this.Property(t => t.ItemCode)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.ItemName)
                .IsRequired()
                .HasMaxLength(720);

            this.Property(t => t.TunnelNum)
                .HasMaxLength(20);

            this.Property(t => t.LocationCode)
                .HasMaxLength(20);

            this.Property(t => t.Remark)
                .HasMaxLength(200);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Wms_Bill_PutinBindingHeader".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.SourceOrder).HasColumnName("SourceOrder".ToUpper());
            this.Property(t => t.WarehouseCode).HasColumnName("WarehouseCode".ToUpper());
            this.Property(t => t.Container).HasColumnName("Container".ToUpper());
            this.Property(t => t.ItemCode).HasColumnName("ItemCode".ToUpper());
            this.Property(t => t.ItemName).HasColumnName("ItemName".ToUpper());
            this.Property(t => t.QuestFlag).HasColumnName("QuestFlag".ToUpper());
            this.Property(t => t.QuestTime).HasColumnName("QuestTime".ToUpper());
            this.Property(t => t.TunnelNum).HasColumnName("TunnelNum".ToUpper());
            this.Property(t => t.StockFlag).HasColumnName("StockFlag".ToUpper());
            this.Property(t => t.StockTime).HasColumnName("StockTime".ToUpper());
            this.Property(t => t.LocationCode).HasColumnName("LocationCode".ToUpper());
            this.Property(t => t.Enabled).HasColumnName("Enabled".ToUpper());
            this.Property(t => t.Remark).HasColumnName("Remark".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}