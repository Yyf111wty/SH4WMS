using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System.Data.Entity.ModelConfiguration;

namespace TR.MySql.Mapping.Wms
{
    public class WmsStockerTaskMap : EntityTypeConfiguration<WmsStockerTask>
    {
        public WmsStockerTaskMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                 .HasMaxLength(128); ;

            this.Property(t => t.Code)
              .IsRequired()
              .HasMaxLength(20);

            this.Property(t => t.WarehouseCode)
              .IsRequired()
              .HasMaxLength(128);

            this.Property(t => t.Sourceorder)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.Container)
                .IsRequired()
                .HasMaxLength(30);

            this.Property(t => t.StartTaxi)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.EndTaxi)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);


            this.Property(t => t.ItemCode)
                .HasMaxLength(120);

            this.Property(t => t.ItemName)
                .HasMaxLength(120);

            // Table & Column Mappings
            this.ToTable("Wms_Stocker_Task".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.Code).HasColumnName("Code".ToUpper());
            this.Property(t => t.Sourceorder).HasColumnName("Sourceorder".ToUpper());
            this.Property(t => t.WarehouseCode).HasColumnName("WarehouseCode".ToUpper());
            this.Property(t => t.Container).HasColumnName("Container".ToUpper());
            this.Property(t => t.Type).HasColumnName("Type".ToUpper());
            this.Property(t => t.Priority).HasColumnName("Priority".ToUpper());
            this.Property(t => t.StartTaxi).HasColumnName("StartTaxi".ToUpper());
            this.Property(t => t.EndTaxi).HasColumnName("EndTaxi".ToUpper());
            this.Property(t => t.State).HasColumnName("State".ToUpper());
            this.Property(t => t.StartTime).HasColumnName("StartTime".ToUpper());
            this.Property(t => t.ImplementTime).HasColumnName("ImplementTime".ToUpper());
            this.Property(t => t.FinishTime).HasColumnName("FinishTime".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());

            this.Property(t => t.ItemCode).HasColumnName("ItemCode".ToUpper());
            this.Property(t => t.ItemName).HasColumnName("ItemName".ToUpper());

            // Relationships
        }
    }
}