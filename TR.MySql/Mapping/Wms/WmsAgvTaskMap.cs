using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.MySql.Mapping.Wms
{
    public class WmsAgvTaskMap : EntityTypeConfiguration<WmsAgvTask>
    {
        public WmsAgvTaskMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.SourceOrder)
                .HasMaxLength(128);

            this.Property(t => t.Barcode)
                .HasMaxLength(128);
           
            this.Property(t => t.ItemCode)
                .HasMaxLength(128);

            this.Property(t => t.ItemName)
                .HasMaxLength(720);

            this.Property(t => t.Creator)
                .HasMaxLength(20);

            this.Property(t => t.LastEditor)
                .HasMaxLength(20);

            this.Property(t => t.GroupCode)
                .HasMaxLength(32);

            this.Property(t => t.StartTaxi)
                .HasMaxLength(32);

            this.Property(t => t.EndTaxi)
                .HasMaxLength(32);

            this.Property(t => t.WarehouseCode)
               .HasMaxLength(32);

            // Table & Column Mappings
            this.ToTable("Wms_Agv_Task".ToUpper());
            this.Property(t => t.Id).HasColumnName("Id".ToUpper());
            this.Property(t => t.SourceOrder).HasColumnName("SourceOrder".ToUpper());
            this.Property(t => t.Barcode).HasColumnName("Barcode".ToUpper());
            this.Property(t => t.ItemCode).HasColumnName("ItemCode".ToUpper());
            this.Property(t => t.ItemName).HasColumnName("ItemName".ToUpper());
            this.Property(t => t.Type).HasColumnName("Type".ToUpper());
            this.Property(t => t.Priority).HasColumnName("Priority".ToUpper());
            this.Property(t => t.StartTaxi).HasColumnName("StartTaxi".ToUpper());
            this.Property(t => t.EndTaxi).HasColumnName("EndTaxi".ToUpper());
            this.Property(t => t.State).HasColumnName("State".ToUpper());
            this.Property(t => t.Creator).HasColumnName("Creator".ToUpper());
            this.Property(t => t.CreateTime).HasColumnName("CreateTime".ToUpper());
            this.Property(t => t.LastEditor).HasColumnName("LastEditor".ToUpper());
            this.Property(t => t.LastEditTime).HasColumnName("LastEditTime".ToUpper());
            this.Property(t => t.GroupCode).HasColumnName("GroupCode".ToUpper());
            this.Property(t => t.Container).HasColumnName("Container".ToUpper());
            this.Property(t => t.WarehouseCode).HasColumnName("WarehouseCode".ToUpper());
            // Relationships
        }
    }
}
