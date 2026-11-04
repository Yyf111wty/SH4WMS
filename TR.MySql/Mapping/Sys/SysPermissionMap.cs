
using System.Data.Entity.ModelConfiguration;
using TR.MySql.Models.Sys;

namespace TR.MySql.Mapping.Sys
{
    public class SysPermissionMap : EntityTypeConfiguration<SysPermission>
    {
        public SysPermissionMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            this.Property(t => t.PermissionName)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.Icon)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.LinkUrl)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.Area)
                .HasMaxLength(20);

            this.Property(t => t.Controller)
                .HasMaxLength(20);

            this.Property(t => t.Action)
                .HasMaxLength(20);


            this.Property(t => t.Description)
                .HasMaxLength(128);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Sys_Permission".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.PermissionId).HasColumnName("PermissionId".ToUpper());
            this.Property(t => t.ParentId).HasColumnName("ParentId".ToUpper());
            this.Property(t => t.PermissionName).HasColumnName("PermissionName".ToUpper());
            this.Property(t => t.Enabled).HasColumnName("Enabled".ToUpper());
            this.Property(t => t.Icon).HasColumnName("Icon".ToUpper());
            this.Property(t => t.LinkUrl).HasColumnName("LinkUrl".ToUpper()); ;
            this.Property(t => t.Area).HasColumnName("Area".ToUpper());
            this.Property(t => t.Controller).HasColumnName("Controller".ToUpper());
            this.Property(t => t.Action).HasColumnName("Action".ToUpper());
            this.Property(t => t.SortNo).HasColumnName("SortNo".ToUpper());
            this.Property(t => t.IsLast).HasColumnName("IsLast".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}