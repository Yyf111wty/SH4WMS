using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsBaseMasterMap : EntityTypeConfiguration<WmsBaseMaster>
    {
        public WmsBaseMasterMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.FactoryCode)
                .IsRequired()
                .HasMaxLength(30);

            this.Property(t => t.ItemCode)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.ItemName)
                .IsRequired()
                .HasMaxLength(720);

            this.Property(t => t.ItemType)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.ItemPec)
               .IsRequired()
               .HasMaxLength(50);

            this.Property(t => t.StoraGetime)
               .HasMaxLength(20);

            this.Property(t => t.Batch)
               .HasMaxLength(128);

            this.Property(t => t.EntRance)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.OutErmode)
               .IsRequired()
               .HasMaxLength(50);

            this.Property(t => t.StorgeAreas)
                .HasMaxLength(50);

            this.Property(t => t.PickPosition)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Wms_Base_Master".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.FactoryCode).HasColumnName("FactoryCode".ToUpper());         
            this.Property(t => t.ItemCode).HasColumnName("ItemCode".ToUpper());
            this.Property(t => t.ItemName).HasColumnName("ItemName".ToUpper());
            this.Property(t => t.ItemType).HasColumnName("ItemType".ToUpper());
            this.Property(t => t.ItemPec).HasColumnName("ItemPec".ToUpper());
            this.Property(t => t.PackageLength).HasColumnName("PackageLength".ToUpper());
            this.Property(t => t.PackageWidth).HasColumnName("PackageWidth".ToUpper());
            this.Property(t => t.PackageHeight).HasColumnName("PackageHeight".ToUpper());
            this.Property(t => t.PackageWeight).HasColumnName("PackageWeight".ToUpper());
            this.Property(t => t.PackageOffset).HasColumnName("PackageOffset".ToUpper());
            this.Property(t => t.StoraGetime).HasColumnName("StoraGetime".ToUpper());          
            this.Property(t => t.UnuSetime).HasColumnName("UnuSetime".ToUpper());
            this.Property(t => t.MinStorage).HasColumnName("MinStorage".ToUpper());
            this.Property(t => t.MaxStorage).HasColumnName("MaxStorage".ToUpper());
            this.Property(t => t.Ifuse).HasColumnName("Ifuse".ToUpper());         
            this.Property(t => t.Ifbindmul).HasColumnName("Ifbindmul".ToUpper());
            this.Property(t => t.Batch).HasColumnName("Batch".ToUpper());
            this.Property(t => t.EntRance).HasColumnName("EntRance".ToUpper());
            this.Property(t => t.OutErmode).HasColumnName("OutErmode".ToUpper());
            this.Property(t => t.StorgeAreas).HasColumnName("StorgeAreas".ToUpper());
            this.Property(t => t.PickPosition).HasColumnName("PickPosition".ToUpper());
            this.Property(t => t.ThresholdEnable).HasColumnName("ThresholdEnable".ToUpper());
            this.Property(t => t.Threshold).HasColumnName("Threshold".ToUpper());
            this.Property(t => t.PriorityLevel).HasColumnName("PriorityLevel".ToUpper());
            this.Property(t => t.PackageQty).HasColumnName("PackageQty".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
           
        }
    }
}