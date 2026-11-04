using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBillPutoutHeaderMap : EntityTypeConfiguration<WmsBillPutoutHeader>
    {
        public WmsBillPutoutHeaderMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .HasMaxLength(128);

            this.Property(t => t.SourceOrder)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.SourceOrderType)
                .HasMaxLength(20);

            this.Property(t => t.AppoveExecutor)
               .HasMaxLength(20);

            this.Property(t => t.Remark)
                .HasMaxLength(200);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Wms_Bill_PutoutHeader".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.SourceOrder).HasColumnName("SourceOrder".ToUpper());
            this.Property(t => t.SourceOrderType).HasColumnName("SourceOrderType".ToUpper());
            this.Property(t => t.SuggestTime).HasColumnName("SuggestTime".ToUpper());
            this.Property(t => t.AppoveFlag).HasColumnName("AppoveFlag".ToUpper());
            this.Property(t => t.AppoveTime).HasColumnName("AppoveTime".ToUpper());
            this.Property(t => t.AppoveExecutor).HasColumnName("AppoveExecutor".ToUpper());
            this.Property(t => t.StockFlag).HasColumnName("StockFlag".ToUpper());
            this.Property(t => t.StockTime).HasColumnName("StockTime".ToUpper());
            this.Property(t => t.FeedbackFlag).HasColumnName("FeedbackFlag".ToUpper());
            this.Property(t => t.FeedbackTime).HasColumnName("FeedbackTime".ToUpper());
            this.Property(t => t.Remark).HasColumnName("Remark".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
        }
    }
}