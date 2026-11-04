
using System.Data.Entity.ModelConfiguration;
using TR.MySql.Models.Sys;

namespace TR.MySql.Mapping.Sys
{
    public class SysUserRoleMap : EntityTypeConfiguration<SysUserRole>
    {
        public SysUserRoleMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Sys_User_Role".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.UserId).HasColumnName("UserId".ToUpper());
            this.Property(t => t.RoleId).HasColumnName("RoleId".ToUpper());
           
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());


            this.HasRequired(t => t.SysRole)
                .WithMany(t => t.Body)
                .HasForeignKey(d => d.RoleId);
            this.HasRequired(t => t.SysUser)
                .WithMany(t => t.Body)
                .HasForeignKey(d => d.UserId);
        }
    }
}