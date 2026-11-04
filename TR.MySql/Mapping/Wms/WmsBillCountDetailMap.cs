using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBillCountDetailMap : EntityTypeConfiguration<WmsBillCountDetail>
    {
        public WmsBillCountDetailMap()
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

            this.Property(t => t.WarehouseCode)
                .HasMaxLength(20);

            this.Property(t => t.ZoneCode)
                .HasMaxLength(20);

            this.Property(t => t.LocationCode)
                .HasMaxLength(20);

            this.Property(t => t.Barcode)
               .HasMaxLength(128);

            this.Property(t => t.ItemCode)
                .HasMaxLength(128);

            this.Property(t => t.ItemName)
                .HasMaxLength(720);

            this.Property(t => t.Lot)
                .HasMaxLength(128);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Wms_Bill_CountDetail".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.SourceOrder).HasColumnName("SourceOrder".ToUpper());
            this.Property(t => t.WarehouseCode).HasColumnName("WarehouseCode".ToUpper());
            this.Property(t => t.ZoneCode).HasColumnName("ZoneCode".ToUpper());
            this.Property(t => t.LocationCode).HasColumnName("LocationCode".ToUpper());
            this.Property(t => t.Barcode).HasColumnName("Barcode".ToUpper());
            this.Property(t => t.ItemCode).HasColumnName("ItemCode".ToUpper());
            this.Property(t => t.ItemName).HasColumnName("ItemName".ToUpper());
            this.Property(t => t.Lot).HasColumnName("Lot".ToUpper());
            this.Property(t => t.LocalQty).HasColumnName("LocalQty".ToUpper());
            this.Property(t => t.CheckQty).HasColumnName("CheckQty".ToUpper());
            this.Property(t => t.DiffQty).HasColumnName("DiffQty".ToUpper());
            this.Property(t => t.CheckFlag).HasColumnName("CheckFlag".ToUpper());
            this.Property(t => t.CheckTime).HasColumnName("CheckTime".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}