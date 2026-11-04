using System.Data.Entity.ModelConfiguration;
using TR.MySql.Models.Sys;

namespace TR.MySql.Mapping.Sys
{
    public class SysSettingMap : EntityTypeConfiguration<SysSetting>
    {
        public SysSettingMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.Value)
                .IsRequired()
                .HasMaxLength(200);

            this.Property(t => t.ReMark)
                .HasMaxLength(200);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Sys_Setting".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.Name).HasColumnName("Name".ToUpper());
            this.Property(t => t.Value).HasColumnName("Value".ToUpper());
            this.Property(t => t.Type).HasColumnName("Type".ToUpper());
            this.Property(t => t.Enabled).HasColumnName("Enabled".ToUpper());
            this.Property(t => t.ReMark).HasColumnName("ReMark".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}