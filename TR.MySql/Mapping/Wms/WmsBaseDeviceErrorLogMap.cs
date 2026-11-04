using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBaseDeviceErrorLogMap : EntityTypeConfiguration<WmsBaseDeviceErrorLog>
    {
        public WmsBaseDeviceErrorLogMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.DeviceCode)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.DeviceType)
                .HasMaxLength(20);


            this.Property(t => t.DeviceProperty)
               .HasMaxLength(20);

            this.Property(t => t.DeviceAtion)
               .HasMaxLength(20);

            this.Property(t => t.ErrorCode)
               .HasMaxLength(20);

            this.Property(t => t.ZoonCode)
               .HasMaxLength(20);

            this.Property(t => t.ZoonCode)
              .HasMaxLength(20);

            this.Property(t => t.Ip)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Wms_Base_DeviceErrorLog".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.DeviceCode).HasColumnName("DeviceCode".ToUpper());
            this.Property(t => t.DeviceType).HasColumnName("DeviceType".ToUpper());
            this.Property(t => t.DeviceProperty).HasColumnName("DeviceProperty".ToUpper());
            this.Property(t => t.DeviceAtion).HasColumnName("DeviceAtion".ToUpper());
            this.Property(t => t.ErrorCode).HasColumnName("ErrorCode".ToUpper());
            this.Property(t => t.ZoonCode).HasColumnName("ZoonCode".ToUpper());
            this.Property(t => t.Ip).HasColumnName("Ip".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}