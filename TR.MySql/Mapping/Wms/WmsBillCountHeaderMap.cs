using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBillCountHeaderMap : EntityTypeConfiguration<WmsBillCountHeader>
    {
        public WmsBillCountHeaderMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.SourceOrder)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.Type)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.ApproveExecutor)
                .HasMaxLength(20);

            this.Property(t => t.Remark)
                .HasMaxLength(100);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Wms_Bill_CountHeader".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.SourceOrder).HasColumnName("SourceOrder".ToUpper());
            this.Property(t => t.Type).HasColumnName("Type".ToUpper());
            this.Property(t => t.ApproveFlag).HasColumnName("ApproveFlag".ToUpper());
            this.Property(t => t.ApproveTime).HasColumnName("ApproveTime".ToUpper());
            this.Property(t => t.ApproveExecutor).HasColumnName("ApproveExecutor".ToUpper());
            this.Property(t => t.StockFlag).HasColumnName("StockFlag".ToUpper());
            this.Property(t => t.StockTime).HasColumnName("StockTime".ToUpper());
            this.Property(t => t.FinishFlag).HasColumnName("FinishFlag".ToUpper());
            this.Property(t => t.FinishTime).HasColumnName("FinishTime".ToUpper());
            this.Property(t => t.Remark).HasColumnName("Remark".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}