using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBaseZoneMap : EntityTypeConfiguration<WmsBaseZone>
    {
        public WmsBaseZoneMap()
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

            this.Property(t => t.WarehouseId)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.WarehouseCode)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.Description)
                .HasMaxLength(200);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Wms_Base_Zone".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.Code).HasColumnName("Code".ToUpper());
            this.Property(t => t.WarehouseId).HasColumnName("WarehouseId".ToUpper());
            this.Property(t => t.WarehouseCode).HasColumnName("WarehouseCode".ToUpper());
            this.Property(t => t.Description).HasColumnName("Description".ToUpper());
            this.Property(t => t.Enabled).HasColumnName("Enabled".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());

            // Relationships
            this.HasRequired(t => t.Warehouse)
                .WithMany(t => t.Zones)
                .HasForeignKey(d => d.WarehouseId);
        }
    }
}