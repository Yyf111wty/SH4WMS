
using System.Data.Entity.ModelConfiguration;
using TR.MySql.Models.Sys;

namespace TR.MySql.Mapping.Sys
{
    public class SysUserMap : EntityTypeConfiguration<SysUser>
    {
        public SysUserMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            this.Property(t => t.UserName)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.Password)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.TrueName)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.Email)
                .HasMaxLength(100);

            this.Property(t => t.Mobile)
                .HasMaxLength(20);

            this.Property(t => t.Address)
                .HasMaxLength(200);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Sys_User".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.UserName).HasColumnName("UserName".ToUpper());
            this.Property(t => t.Password).HasColumnName("Password".ToUpper());
            this.Property(t => t.TrueName).HasColumnName("TrueName".ToUpper());
            this.Property(t => t.Email).HasColumnName("Email".ToUpper());
            this.Property(t => t.Mobile).HasColumnName("Mobile".ToUpper());
            this.Property(t => t.UserGroupId).HasColumnName("UserGroupId".ToUpper()); ;
            this.Property(t => t.Address).HasColumnName("Address".ToUpper());
            this.Property(t => t.CSOnlineFlag).HasColumnName("CSOnlineFlag".ToUpper());
            this.Property(t => t.CSLoginTime).HasColumnName("CSLoginTime".ToUpper());
            this.Property(t => t.Enabled).HasColumnName("Enabled".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());

            // Relationships
            this.HasRequired(t => t.SysGroup)
                .WithMany(t => t.BodyUser)
                .HasForeignKey(d => d.UserGroupId);
        }
    }
}