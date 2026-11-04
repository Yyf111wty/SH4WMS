using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TR.MySql.Models.Sys;

namespace TR.MySql.Mapping.Sys
{
    public class SysExceptionMap : EntityTypeConfiguration<SysException>
    {
        public SysExceptionMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Sys_Exception".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.HelpLink).HasColumnName("HelpLink".ToUpper());
            this.Property(t => t.Message).HasColumnName("Message".ToUpper());
            this.Property(t => t.Source).HasColumnName("Source".ToUpper());
            this.Property(t => t.StackTrace).HasColumnName("StackTrace".ToUpper());
            this.Property(t => t.TargetSite).HasColumnName("TargetSite".ToUpper());
            this.Property(t => t.Data).HasColumnName("Data".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}
