using System.Data.Entity.ModelConfiguration;
using TR.MySql.Models.Sys;

namespace TR.MySql.Mapping.Sys
{
    public class SysRoleMap : EntityTypeConfiguration<SysRole>
    {
        public SysRoleMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            this.Property(t => t.RoleName)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.Description)
                .HasMaxLength(200);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Sys_Role".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.RoleId).HasColumnName("RoleId".ToUpper());
            this.Property(t => t.Description).HasColumnName("Description".ToUpper());
            this.Property(t => t.Enabled).HasColumnName("Enabled".ToUpper());
            this.Property(t => t.RoleName).HasColumnName("RoleName".ToUpper());
            this.Property(t => t.RoleLevel).HasColumnName("RoleLevel".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}