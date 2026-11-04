using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBaseWarehousMap : EntityTypeConfiguration<WmsBaseWarehouse>
    {
        public WmsBaseWarehousMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.Code)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.Description)
                .HasMaxLength(200);

            this.Property(t => t.FactoryCode)
                .HasMaxLength(20);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Wms_Base_Warehouse".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.Code).HasColumnName("Code".ToUpper());
            this.Property(t => t.Description).HasColumnName("Description".ToUpper());
            this.Property(t => t.FactoryCode).HasColumnName("FactoryCode".ToUpper());
            this.Property(t => t.EntPrinciple).HasColumnName("EntPrinciple".ToUpper());
            this.Property(t => t.OutPrinciple).HasColumnName("OutPrinciple".ToUpper());
            this.Property(t => t.Qty).HasColumnName("Qty".ToUpper());
            this.Property(t => t.SiginNum).HasColumnName("SiginNum".ToUpper());
            this.Property(t => t.RowNum).HasColumnName("RowNum".ToUpper());
            this.Property(t => t.ColumnNum).HasColumnName("ColumnNum".ToUpper());
            this.Property(t => t.LayerNum).HasColumnName("LayerNum".ToUpper());
            this.Property(t => t.Enabled).HasColumnName("Enabled".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}