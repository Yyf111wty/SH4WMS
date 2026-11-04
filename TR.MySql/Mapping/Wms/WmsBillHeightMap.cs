using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBillHeightMap : EntityTypeConfiguration<WmsBillHeight>
    {
        public WmsBillHeightMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.WarehouseCode)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.SourceOrder)
                .HasMaxLength(128);

            this.Property(t => t.Container)
                .HasMaxLength(30);

            this.Property(t => t.ItemCode)
                .HasMaxLength(128);

            this.Property(t => t.ItemName)
                .HasMaxLength(720);

            this.Property(t => t.Remark)
                .HasMaxLength(200);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Wms_Bill_Height".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.WarehouseCode).HasColumnName("WarehouseCode".ToUpper());
            this.Property(t => t.SourceOrder).HasColumnName("SourceOrder".ToUpper());
            this.Property(t => t.Container).HasColumnName("Container".ToUpper());
            this.Property(t => t.ItemCode).HasColumnName("ItemCode".ToUpper());
            this.Property(t => t.ItemName).HasColumnName("ItemName".ToUpper());
            this.Property(t => t.TheoreticalHeight).HasColumnName("TheoreticalHeight".ToUpper());
            this.Property(t => t.ActualHeight).HasColumnName("ActualHeight".ToUpper());
            this.Property(t => t.Result).HasColumnName("Result".ToUpper());
            this.Property(t => t.StockFlag).HasColumnName("StockFlag".ToUpper());
            this.Property(t => t.Remark).HasColumnName("Remark".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}