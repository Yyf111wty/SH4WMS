using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBaseLocationMap : EntityTypeConfiguration<WmsBaseLocation>
    {
        public WmsBaseLocationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .HasMaxLength(128);

            this.Property(t => t.WarehouseCode)
                .IsRequired()
               .HasMaxLength(20);

            this.Property(t => t.ZoneId)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.ZoneCode)
                .IsRequired()
               .HasMaxLength(20);

            this.Property(t => t.Code)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.Type)
                .HasMaxLength(20);

            this.Property(t => t.Symbol)
                .HasMaxLength(128);

            this.Property(t => t.Remark)
                .HasMaxLength(200);

            this.Property(t => t.Xyz)
                .HasMaxLength(50);

            this.Property(t => t.CheckXyz)
                .HasMaxLength(50);

            this.Property(t => t.Specs)
                .HasMaxLength(50);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Wms_Base_Location".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.WarehouseCode).HasColumnName("WarehouseCode".ToUpper());
            this.Property(t => t.ZoneId).HasColumnName("ZoneId".ToUpper());
            this.Property(t => t.ZoneCode).HasColumnName("ZoneCode".ToUpper());
            this.Property(t => t.Code).HasColumnName("Code".ToUpper());
            this.Property(t => t.Type).HasColumnName("Type".ToUpper());
            this.Property(t => t.RowNum).HasColumnName("RowNum".ToUpper());
            this.Property(t => t.ColumnNum).HasColumnName("ColumnNum".ToUpper());
            this.Property(t => t.LayerNum).HasColumnName("LayerNum".ToUpper());
            this.Property(t => t.TunnelNum).HasColumnName("TunnelNum".ToUpper());
            this.Property(t => t.LimitHeight).HasColumnName("LimitHeight".ToUpper());
            this.Property(t => t.LimitWeight).HasColumnName("LimitWeight".ToUpper());
            this.Property(t => t.PickOrder).HasColumnName("PickOrder".ToUpper());
            this.Property(t => t.Symbol).HasColumnName("Symbol".ToUpper());
            this.Property(t => t.SortType).HasColumnName("SortType".ToUpper());
            this.Property(t => t.QTY).HasColumnName("QTY".ToUpper());      
            this.Property(t => t.Times).HasColumnName("Times".ToUpper());
            this.Property(t => t.Enabled).HasColumnName("Enabled".ToUpper());           
            this.Property(t => t.IsAvialable).HasColumnName("IsAvialable".ToUpper());
            this.Property(t => t.ExitOrout).HasColumnName("ExitOrout".ToUpper());
            this.Property(t => t.GroupCode).HasColumnName("GroupCode".ToUpper());
            this.Property(t => t.Xyz).HasColumnName("Xyz".ToUpper());
            this.Property(t => t.CheckXyz).HasColumnName("CheckXyz".ToUpper());
            this.Property(t => t.Specs).HasColumnName("Specs".ToUpper());
            this.Property(t => t.IfMixing).HasColumnName("IfMixing".ToUpper());
            this.Property(t => t.Remark).HasColumnName("Remark".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
            // Relationships
            this.HasRequired(t => t.Zone)
                .WithMany(t => t.Locations)
                .HasForeignKey(d => d.ZoneId);
        }
    }
}