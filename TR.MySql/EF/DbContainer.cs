using MySql.Data.Entity;
using System.Data.Entity;
using TR.MySql.Mapping.Sys;
using TR.MySql.Mapping.Wms;
using TR.MySql.Models.Sys;
using TR.MySql.Models.Wms;

namespace TR.MySql.EF
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class DbContainer : DbContext
    {
        public static string DbConnStr
        {
            get
            {
                return "Data Source=49.234.209.152;Initial Catalog=SHACH008;User ID=root;Password=Admin123456";
            }
        }
        public DbContainer()
            : base(DbConnStr)
        {
        }
        #region SYS
        public DbSet<SysLog> SysLog { get; set; }
        public DbSet<SysException> SysException { get; set; }
        public DbSet<SysGroup> SysGroup { get; set; }
        public DbSet<SysPermission> SysPermission { get; set; }
        public DbSet<SysRole> SysRole { get; set; }
        public DbSet<SysRolePermission> SysRolePermission { get; set; }
        public DbSet<SysSetting> SysSetting { get; set; }
        public DbSet<SysUserConfig> SysUserConfig { get; set; }
        public DbSet<SysUser> SysUser { get; set; }
        public DbSet<SysUserRole> SysUserRole { get; set; }
        #endregion

        #region WMS
        public DbSet<WmsAgvTask> WmsAgvTask { get; set; }
        public DbSet<WmsAgvTaxi> WmsAgvTaxi { get; set; }
        public DbSet<WmsBaseDeviceErrorLog> WmsBaseDeviceErrorLog { get; set; }
        public DbSet<WmsBaseMaster> WmsBaseMaster { get; set; }
        public DbSet<WmsBaseWarehouse> WmsBaseWarehouse { get; set; }
        public DbSet<WmsBaseLocation> WmsBaseLocation { get; set; }
        public DbSet<WmsBaseZone> WmsBaseZone { get; set; }
        public DbSet<WmsBillCountBody> WmsBillCountBody { get; set; }
        public DbSet<WmsBillCountDetail> WmsBillCountDetail { get; set; }
        public DbSet<WmsBillCountHeader> WmsBillCountHeader { get; set; }
        public DbSet<WmsBillHeight> WmsBillHeight { get; set; }
        public DbSet<WmsBillInventoryBook> WmsBillInventoryBook { get; set; }
        public DbSet<WmsBillPutIn> WmsBillPutIn { get; set; }
        public DbSet<WmsBillPutinBindingBody> WmsBillPutinBindingBody { get; set; }
        public DbSet<WmsBillPutinBindingHeader> WmsBillPutinBindingHeader { get; set; }
        public DbSet<WmsBillPutoutBody> WmsBillPutoutBody { get; set; }
        public DbSet<WmsBillPutoutDetail> WmsBillPutoutDetail { get; set; }
        public DbSet<WmsBillPutoutHeader> WmsBillPutoutHeader { get; set; }
        public DbSet<WmsReportWork> WmsReportWork { get; set; }
        public DbSet<WmsStockerTask> WmsStockerTask { get; set; }
        public DbSet<WmsTransferTask> WmsTransferTask { get; set; }
        public DbSet<WmsBillWeigh> WmsBillWeigh { get; set; }
        public DbSet<WmsConveryTask> WmsConveryTask { get; set; } 
        #endregion

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            //modelBuilder.HasDefaultSchema("NEWSPUBLISH");
            modelBuilder.HasDefaultSchema(string.Empty);
            #region SYS
            modelBuilder.Configurations.Add(new SysLogMap());
            modelBuilder.Configurations.Add(new SysExceptionMap());
            modelBuilder.Configurations.Add(new SysGroupMap());
            modelBuilder.Configurations.Add(new SysPermissionMap());
            modelBuilder.Configurations.Add(new SysRoleMap());
            modelBuilder.Configurations.Add(new SysRolePermissionMap());
            modelBuilder.Configurations.Add(new SysSettingMap());
            modelBuilder.Configurations.Add(new SysUserConfigMap());
            modelBuilder.Configurations.Add(new SysUserMap());
            modelBuilder.Configurations.Add(new SysUserRoleMap()); 
            #endregion
            #region WMS
            modelBuilder.Configurations.Add(new WmsAgvTaskMap());
            modelBuilder.Configurations.Add(new WmsAgvTaxiMap());
            modelBuilder.Configurations.Add(new WmsBaseDeviceErrorLogMap());
            modelBuilder.Configurations.Add(new WmsBaseLocationMap());
            modelBuilder.Configurations.Add(new WmsBaseMasterMap());
            modelBuilder.Configurations.Add(new WmsBaseWarehousMap());
            modelBuilder.Configurations.Add(new WmsBaseZoneMap());
            modelBuilder.Configurations.Add(new WmsBillCountDetailMap());
            modelBuilder.Configurations.Add(new WmsBillCountBodyMap());
            modelBuilder.Configurations.Add(new WmsBillCountHeaderMap());
            modelBuilder.Configurations.Add(new WmsBillHeightMap());
            modelBuilder.Configurations.Add(new WmsBillInventoryBookMap());
            modelBuilder.Configurations.Add(new WmsBillPutinBindingBodyMap());
            modelBuilder.Configurations.Add(new WmsBillPutinBindingHeaderMap());
            modelBuilder.Configurations.Add(new WmsBillPutInMap());
            modelBuilder.Configurations.Add(new WmsBillPutoutBodyMap());
            modelBuilder.Configurations.Add(new WmsBillPutoutDetailMap());
            modelBuilder.Configurations.Add(new WmsBillPutoutHeaderMap());
            modelBuilder.Configurations.Add(new WmsBillWeighMap());
            modelBuilder.Configurations.Add(new WmsConveryTaskMap());
            modelBuilder.Configurations.Add(new WmsReportWorkMap());
            modelBuilder.Configurations.Add(new WmsStockerTaskMap());
            modelBuilder.Configurations.Add(new WmsTransferTaskMap()); 
            #endregion

        }
    }
}