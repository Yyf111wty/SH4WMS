using System.Data.Entity.ModelConfiguration;
using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
namespace TR.MySql.Mapping.Sys
{
    public class SysLogMap : EntityTypeConfiguration<SysLog>
    {
        public SysLogMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .HasMaxLength(128);

            this.Property(t => t.Message);

            this.Property(t => t.Result)
                .HasMaxLength(20);

            this.Property(t => t.Type)
                .HasMaxLength(50);

            this.Property(t => t.Module)
                .HasMaxLength(50);

            this.Property(t => t.IP)
                .HasMaxLength(20);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Sys_Log".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.Message).HasColumnName("Message".ToUpper());
            this.Property(t => t.Result).HasColumnName("Result".ToUpper());
            this.Property(t => t.Type).HasColumnName("Type".ToUpper());
            this.Property(t => t.Module).HasColumnName("Module".ToUpper());
            this.Property(t => t.IP).HasColumnName("IP".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}