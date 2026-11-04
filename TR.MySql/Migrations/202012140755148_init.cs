namespace TR.MySql.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "SYS_EXCEPTION",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        HELPLINK = c.String(unicode: false),
                        MESSAGE = c.String(unicode: false),
                        SOURCE = c.String(unicode: false),
                        STACKTRACE = c.String(unicode: false),
                        TARGETSITE = c.String(unicode: false),
                        DATA = c.String(unicode: false),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "SYS_GROUP",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        GROUPID = c.Int(nullable: false),
                        GROUPNAME = c.String(unicode: false),
                        PARENTGROUPID = c.Int(nullable: false),
                        ENABLED = c.Boolean(),
                        DESCRIPTION = c.String(maxLength: 128, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "SYS_USER",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        USERNAME = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        PASSWORD = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        TRUENAME = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        USERGROUPID = c.Int(nullable: false),
                        MOBILE = c.String(maxLength: 20, storeType: "nvarchar"),
                        EMAIL = c.String(maxLength: 100, storeType: "nvarchar"),
                        ADDRESS = c.String(maxLength: 200, storeType: "nvarchar"),
                        CSONLINEFLAG = c.Boolean(nullable: false),
                        CSLOGINTIME = c.DateTime(precision: 0),
                        ENABLED = c.Boolean(nullable: false),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("SYS_GROUP", t => t.USERGROUPID, cascadeDelete: true)
                .Index(t => t.USERGROUPID);
            
            CreateTable(
                "SYS_USER_ROLE",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        USERID = c.Int(nullable: false),
                        ROLEID = c.Int(nullable: false),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("SYS_ROLE", t => t.ROLEID, cascadeDelete: true)
                .ForeignKey("SYS_USER", t => t.USERID, cascadeDelete: true)
                .Index(t => t.USERID)
                .Index(t => t.ROLEID);
            
            CreateTable(
                "SYS_ROLE",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ROLEID = c.Int(nullable: false),
                        ROLENAME = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        ENABLED = c.Boolean(),
                        DESCRIPTION = c.String(maxLength: 200, storeType: "nvarchar"),
                        ROLELEVEL = c.Int(),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                        SysUser_Id = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("SYS_USER", t => t.SysUser_Id)
                .Index(t => t.SysUser_Id);
            
            CreateTable(
                "SYS_ROLE_PERMISSION",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ROLEID = c.Int(nullable: false),
                        PERMISSIONID = c.Int(nullable: false),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("SYS_PERMISSION", t => t.PERMISSIONID, cascadeDelete: true)
                .ForeignKey("SYS_ROLE", t => t.ROLEID, cascadeDelete: true)
                .Index(t => t.ROLEID)
                .Index(t => t.PERMISSIONID);
            
            CreateTable(
                "SYS_PERMISSION",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PERMISSIONID = c.Int(nullable: false),
                        PARENTID = c.Int(),
                        PERMISSIONNAME = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        ENABLED = c.Boolean(),
                        ICON = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Type = c.Int(nullable: false),
                        LINKURL = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        AREA = c.String(maxLength: 20, storeType: "nvarchar"),
                        CONTROLLER = c.String(maxLength: 20, storeType: "nvarchar"),
                        ACTION = c.String(maxLength: 20, storeType: "nvarchar"),
                        SORTNO = c.Int(),
                        ISLAST = c.Boolean(nullable: false),
                        Description = c.String(maxLength: 128, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "SYS_USER_CONFIG",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        NAME = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        VALUE = c.String(nullable: false, maxLength: 200, storeType: "nvarchar"),
                        TYPE = c.String(nullable: false, maxLength: 50, storeType: "nvarchar"),
                        ENABLED = c.Boolean(nullable: false),
                        SYSUSERID = c.Int(nullable: false),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("SYS_USER", t => t.SYSUSERID, cascadeDelete: true)
                .Index(t => t.SYSUSERID);
            
            CreateTable(
                "SYS_LOG",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        MESSAGE = c.String(unicode: false),
                        RESULT = c.String(maxLength: 20, storeType: "nvarchar"),
                        TYPE = c.String(maxLength: 50, storeType: "nvarchar"),
                        MODULE = c.String(maxLength: 50, storeType: "nvarchar"),
                        IP = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "SYS_SETTING",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        NAME = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        VALUE = c.String(nullable: false, maxLength: 200, storeType: "nvarchar"),
                        TYPE = c.Int(nullable: false),
                        ENABLED = c.Boolean(nullable: false),
                        REMARK = c.String(maxLength: 200, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_AGV_TASK",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        SOURCEORDER = c.String(maxLength: 128, storeType: "nvarchar"),
                        WAREHOUSECODE = c.String(maxLength: 32, storeType: "nvarchar"),
                        CONTAINER = c.String(unicode: false),
                        BARCODE = c.String(maxLength: 128, storeType: "nvarchar"),
                        ITEMCODE = c.String(maxLength: 128, storeType: "nvarchar"),
                        ITEMNAME = c.String(maxLength: 720, storeType: "nvarchar"),
                        TYPE = c.Int(nullable: false),
                        PRIORITY = c.Int(nullable: false),
                        STARTTAXI = c.String(maxLength: 32, storeType: "nvarchar"),
                        ENDTAXI = c.String(maxLength: 32, storeType: "nvarchar"),
                        STATE = c.Int(nullable: false),
                        GROUPCODE = c.String(maxLength: 32, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_AGV_TAXI",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CODE = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ITEMCODE = c.String(maxLength: 128, storeType: "nvarchar"),
                        ITEMNAME = c.String(maxLength: 128, storeType: "nvarchar"),
                        BARCODE = c.String(unicode: false),
                        TYPE = c.Int(nullable: false),
                        STATE = c.Int(nullable: false),
                        ENABELD = c.Int(nullable: false),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_BASE_DEVICEERRORLOG",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        DEVICECODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        DEVICETYPE = c.String(maxLength: 20, storeType: "nvarchar"),
                        DEVICEPROPERTY = c.String(maxLength: 20, storeType: "nvarchar"),
                        DEVICEATION = c.String(maxLength: 20, storeType: "nvarchar"),
                        ERRORCODE = c.String(maxLength: 20, storeType: "nvarchar"),
                        ZOONCODE = c.String(maxLength: 20, storeType: "nvarchar"),
                        IP = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_BASE_LOCATION",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        WAREHOUSECODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        ZONEID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ZONECODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        CODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        TYPE = c.String(maxLength: 20, storeType: "nvarchar"),
                        ROWNUM = c.Int(nullable: false),
                        COLUMNNUM = c.Int(nullable: false),
                        LAYERNUM = c.Int(nullable: false),
                        TUNNELNUM = c.Int(),
                        LIMITHEIGHT = c.Int(),
                        LIMITWEIGHT = c.Int(),
                        PICKORDER = c.Int(),
                        SYMBOL = c.String(maxLength: 128, storeType: "nvarchar"),
                        SORTTYPE = c.Int(),
                        EXITOROUT = c.Int(),
                        GROUPCODE = c.Int(),
                        QTY = c.Int(nullable: false),
                        TIMES = c.Int(),
                        ENABLED = c.Int(),
                        ISAVIALABLE = c.Int(),
                        REMARK = c.String(maxLength: 200, storeType: "nvarchar"),
                        XYZ = c.String(maxLength: 50, storeType: "nvarchar"),
                        CHECKXYZ = c.String(maxLength: 50, storeType: "nvarchar"),
                        SPECS = c.String(maxLength: 50, storeType: "nvarchar"),
                        IFMIXING = c.String(unicode: false),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("WMS_BASE_ZONE", t => t.ZONEID, cascadeDelete: true)
                .Index(t => t.ZONEID);
            
            CreateTable(
                "WMS_BASE_ZONE",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        CODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        DESCRIPTION = c.String(maxLength: 200, storeType: "nvarchar"),
                        WAREHOUSEID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        WAREHOUSECODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        ENABLED = c.Int(nullable: false),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("WMS_BASE_WAREHOUSE", t => t.WAREHOUSEID, cascadeDelete: true)
                .Index(t => t.WAREHOUSEID);
            
            CreateTable(
                "WMS_BASE_WAREHOUSE",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        CODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        FACTORYCODE = c.String(maxLength: 20, storeType: "nvarchar"),
                        DESCRIPTION = c.String(maxLength: 200, storeType: "nvarchar"),
                        QTY = c.Int(),
                        LAYERNUM = c.Int(),
                        ROWNUM = c.Int(),
                        COLUMNNUM = c.Int(),
                        SIGINNUM = c.Int(),
                        ENTPRINCIPLE = c.Int(),
                        OUTPRINCIPLE = c.Int(nullable: false),
                        ENABLED = c.Int(nullable: false),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_BASE_MASTER",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        FACTORYCODE = c.String(nullable: false, maxLength: 30, storeType: "nvarchar"),
                        ITEMCODE = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ITEMNAME = c.String(nullable: false, maxLength: 720, storeType: "nvarchar"),
                        ITEMTYPE = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ITEMPEC = c.String(nullable: false, maxLength: 50, storeType: "nvarchar"),
                        PACKAGELENGTH = c.Int(),
                        PACKAGEWIDTH = c.Int(),
                        PACKAGEHEIGHT = c.Int(),
                        PACKAGEWEIGHT = c.Int(),
                        PACKAGEOFFSET = c.Int(),
                        STORAGETIME = c.String(maxLength: 20, storeType: "nvarchar"),
                        UNUSETIME = c.DateTime(precision: 0),
                        MINSTORAGE = c.Int(),
                        MAXSTORAGE = c.Int(),
                        IFUSE = c.Int(nullable: false),
                        IFBINDMUL = c.Int(nullable: false),
                        BATCH = c.String(maxLength: 128, storeType: "nvarchar"),
                        ENTRANCE = c.String(nullable: false, maxLength: 50, storeType: "nvarchar"),
                        OUTERMODE = c.String(nullable: false, maxLength: 50, storeType: "nvarchar"),
                        STORGEAREAS = c.String(maxLength: 50, storeType: "nvarchar"),
                        PICKPOSITION = c.String(maxLength: 50, storeType: "nvarchar"),
                        THRESHOLDENABLE = c.Int(),
                        THRESHOLD = c.String(unicode: false),
                        PRIORITYLEVEL = c.Int(),
                        PACKAGEQTY = c.Int(nullable: false),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_BILL_COUNTBODY",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        HEADERID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ITEMCODE = c.String(maxLength: 128, storeType: "nvarchar"),
                        ITEMNAME = c.String(maxLength: 720, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("WMS_BILL_COUNTHEADER", t => t.HEADERID, cascadeDelete: true)
                .Index(t => t.HEADERID);
            
            CreateTable(
                "WMS_BILL_COUNTHEADER",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        SOURCEORDER = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        TYPE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        APPROVEFLAG = c.Int(nullable: false),
                        APPROVETIME = c.DateTime(precision: 0),
                        APPROVEEXECUTOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        STOCKFLAG = c.Int(nullable: false),
                        STOCKTIME = c.DateTime(precision: 0),
                        FINISHFLAG = c.Int(nullable: false),
                        FINISHTIME = c.DateTime(precision: 0),
                        REMARK = c.String(maxLength: 100, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_BILL_COUNTDETAIL",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        SOURCEORDER = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        WAREHOUSECODE = c.String(maxLength: 20, storeType: "nvarchar"),
                        ZONECODE = c.String(maxLength: 20, storeType: "nvarchar"),
                        LOCATIONCODE = c.String(maxLength: 20, storeType: "nvarchar"),
                        BARCODE = c.String(maxLength: 128, storeType: "nvarchar"),
                        ITEMCODE = c.String(maxLength: 128, storeType: "nvarchar"),
                        ITEMNAME = c.String(maxLength: 720, storeType: "nvarchar"),
                        LOT = c.String(maxLength: 128, storeType: "nvarchar"),
                        LOCALQTY = c.Int(),
                        CHECKQTY = c.Int(),
                        DIFFQTY = c.Int(),
                        CHECKFLAG = c.Int(nullable: false),
                        CHECKTIME = c.DateTime(precision: 0),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_BILL_HEIGHT",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        WAREHOUSECODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        SOURCEORDER = c.String(maxLength: 128, storeType: "nvarchar"),
                        CONTAINER = c.String(maxLength: 30, storeType: "nvarchar"),
                        ITEMCODE = c.String(maxLength: 128, storeType: "nvarchar"),
                        ITEMNAME = c.String(maxLength: 720, storeType: "nvarchar"),
                        THEORETICALHEIGHT = c.Decimal(precision: 18, scale: 2),
                        ACTUALHEIGHT = c.Decimal(precision: 18, scale: 2),
                        RESULT = c.Int(nullable: false),
                        STOCKFLAG = c.Int(nullable: false),
                        REMARK = c.String(maxLength: 200, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_BILL_INVENTORYBOOK",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        SOURCEORDER = c.String(maxLength: 128, storeType: "nvarchar"),
                        SOURCEORDERTYPE = c.String(maxLength: 20, storeType: "nvarchar"),
                        SYSBOLCODE = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        BARCODE = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ITEMCODE = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ITEMNAME = c.String(nullable: false, maxLength: 720, storeType: "nvarchar"),
                        LOT = c.String(maxLength: 20, storeType: "nvarchar"),
                        QTY = c.Int(),
                        WAREHOUSECODE = c.String(maxLength: 20, storeType: "nvarchar"),
                        ZONECODE = c.String(maxLength: 20, storeType: "nvarchar"),
                        LOCATIONCODE = c.String(maxLength: 20, storeType: "nvarchar"),
                        SUPPLIERID = c.String(maxLength: 30, storeType: "nvarchar"),
                        SUPPLIERNAME = c.String(maxLength: 128, storeType: "nvarchar"),
                        STOCKFLAG = c.Int(nullable: false),
                        STOCKTIME = c.DateTime(precision: 0),
                        FREEZEFLAG = c.Int(nullable: false),
                        FREEZETIME = c.DateTime(precision: 0),
                        FREEZEREASON = c.String(maxLength: 20, storeType: "nvarchar"),
                        FEEDBACKFLAG = c.Int(nullable: false),
                        FEEDBACKTIME = c.DateTime(precision: 0),
                        REMARK = c.String(maxLength: 128, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_BILL_PUTIN",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        SOURCEORDER = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        WAREHOUSECODE = c.String(nullable: false, maxLength: 36, storeType: "nvarchar"),
                        CONTAINER = c.String(nullable: false, maxLength: 36, storeType: "nvarchar"),
                        ITEMCODE = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ITEMNAME = c.String(nullable: false, maxLength: 720, storeType: "nvarchar"),
                        BARCODE = c.String(nullable: false, maxLength: 36, storeType: "nvarchar"),
                        QUESTFLAG = c.Int(nullable: false),
                        QUESTTIME = c.DateTime(precision: 0),
                        QTY = c.Int(nullable: false),
                        LOT = c.String(maxLength: 36, storeType: "nvarchar"),
                        ENABLED = c.Boolean(nullable: false),
                        REMARK = c.String(maxLength: 200, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_BILL_PUTINBINDINGBODY",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        HEADERID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        CONTAINER = c.String(maxLength: 30, storeType: "nvarchar"),
                        BARCODE = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        LOT = c.String(maxLength: 128, storeType: "nvarchar"),
                        QTY = c.Int(nullable: false),
                        ENABLED = c.Int(nullable: false),
                        REMARK = c.String(maxLength: 128, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("WMS_BILL_PUTINBINDINGHEADER", t => t.HEADERID, cascadeDelete: true)
                .Index(t => t.HEADERID);
            
            CreateTable(
                "WMS_BILL_PUTINBINDINGHEADER",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        SOURCEORDER = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        WAREHOUSECODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        CONTAINER = c.String(nullable: false, maxLength: 30, storeType: "nvarchar"),
                        ITEMCODE = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ITEMNAME = c.String(nullable: false, maxLength: 720, storeType: "nvarchar"),
                        QUESTFLAG = c.Int(nullable: false),
                        QUESTTIME = c.DateTime(precision: 0),
                        TUNNELNUM = c.String(maxLength: 20, storeType: "nvarchar"),
                        STOCKFLAG = c.Int(nullable: false),
                        STOCKTIME = c.DateTime(precision: 0),
                        LOCATIONCODE = c.String(maxLength: 20, storeType: "nvarchar"),
                        ENABLED = c.Int(nullable: false),
                        REMARK = c.String(maxLength: 200, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_BILL_PUTOUTBODY",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        HEADERID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ITEMCODE = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ITEMNAME = c.String(nullable: false, maxLength: 720, storeType: "nvarchar"),
                        PLANT = c.String(maxLength: 30, storeType: "nvarchar"),
                        OUTLINE = c.String(maxLength: 30, storeType: "nvarchar"),
                        AGVLINE = c.String(maxLength: 30, storeType: "nvarchar"),
                        WORKCELL = c.String(maxLength: 30, storeType: "nvarchar"),
                        LOT = c.String(maxLength: 128, storeType: "nvarchar"),
                        QTY = c.Int(nullable: false),
                        CTNQTY = c.Int(nullable: false),
                        SEQUENCE = c.String(maxLength: 36, storeType: "nvarchar"),
                        REMARK = c.String(maxLength: 128, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("WMS_BILL_PUTOUTHEADER", t => t.HEADERID, cascadeDelete: true)
                .Index(t => t.HEADERID);
            
            CreateTable(
                "WMS_BILL_PUTOUTHEADER",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        SOURCEORDER = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        SOURCEORDERTYPE = c.String(maxLength: 20, storeType: "nvarchar"),
                        SUGGESTTIME = c.DateTime(nullable: false, precision: 0),
                        APPOVEFLAG = c.Int(nullable: false),
                        APPOVETIME = c.DateTime(precision: 0),
                        APPOVEEXECUTOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        STOCKFLAG = c.Int(nullable: false),
                        STOCKTIME = c.DateTime(precision: 0),
                        FEEDBACKFLAG = c.Int(nullable: false),
                        FEEDBACKTIME = c.DateTime(precision: 0),
                        REMARK = c.String(maxLength: 200, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_BILL_PUTOUTDETAIL",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        SOURCEORDER = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ITEMCODE = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ITEMNAME = c.String(nullable: false, maxLength: 720, storeType: "nvarchar"),
                        LOT = c.String(maxLength: 128, storeType: "nvarchar"),
                        QTY = c.Int(nullable: false),
                        CTNQTY = c.Int(nullable: false),
                        PRIORITY = c.Int(nullable: false),
                        WAREHOUSECODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        ZONECODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        LOCATIONCODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        PLANT = c.String(maxLength: 30, storeType: "nvarchar"),
                        OUTLINE = c.String(maxLength: 30, storeType: "nvarchar"),
                        AGVLINE = c.String(maxLength: 30, storeType: "nvarchar"),
                        WORKCELL = c.String(maxLength: 30, storeType: "nvarchar"),
                        STATE = c.Int(nullable: false),
                        EXECUTOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        SELECTTIME = c.DateTime(precision: 0),
                        FINISHTIME = c.DateTime(precision: 0),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_BILL_WEIGH",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        WAREHOUSECODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        SOURCEORDER = c.String(maxLength: 128, storeType: "nvarchar"),
                        CONTAINER = c.String(maxLength: 30, storeType: "nvarchar"),
                        ITEMCODE = c.String(maxLength: 128, storeType: "nvarchar"),
                        ITEMNAME = c.String(maxLength: 720, storeType: "nvarchar"),
                        LIMITWEIGHT = c.Decimal(precision: 18, scale: 2),
                        ACTUALWEIGHT = c.Decimal(precision: 18, scale: 2),
                        RESULT = c.Int(nullable: false),
                        STOCKFLAG = c.Int(nullable: false),
                        REMARK = c.String(maxLength: 200, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_CONVEYOR_TASK",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        WAREHOUSECODE = c.String(maxLength: 32, storeType: "nvarchar"),
                        SOURCEORDER = c.String(maxLength: 128, storeType: "nvarchar"),
                        BARCODE = c.String(maxLength: 128, storeType: "nvarchar"),
                        CONTAINER = c.String(unicode: false),
                        QTY = c.String(unicode: false),
                        BATCHQTY = c.String(unicode: false),
                        ITEMCODE = c.String(maxLength: 128, storeType: "nvarchar"),
                        ITEMNAME = c.String(maxLength: 720, storeType: "nvarchar"),
                        TYPE = c.Int(nullable: false),
                        STATE = c.Int(nullable: false),
                        STARTTAXI = c.String(maxLength: 32, storeType: "nvarchar"),
                        ENDTAXI = c.String(maxLength: 32, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_REPORT_WORK",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        SOURCEORDER = c.String(maxLength: 128, storeType: "nvarchar"),
                        WAREHOUSECODE = c.String(maxLength: 32, storeType: "nvarchar"),
                        BARCODE = c.String(maxLength: 128, storeType: "nvarchar"),
                        ITEMCODE = c.String(maxLength: 128, storeType: "nvarchar"),
                        ITEMNAME = c.String(maxLength: 720, storeType: "nvarchar"),
                        TYPE = c.Int(nullable: false),
                        QTY = c.Int(nullable: false),
                        STATE = c.Int(nullable: false),
                        REMARK = c.String(maxLength: 128, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_STOCKER_TASK",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        CODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        SOURCEORDER = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        WAREHOUSECODE = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        CONTAINER = c.String(nullable: false, maxLength: 30, storeType: "nvarchar"),
                        TYPE = c.Int(nullable: false),
                        PRIORITY = c.Int(nullable: false),
                        STARTTAXI = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        ENDTAXI = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        STATE = c.Int(nullable: false),
                        STARTTIME = c.DateTime(precision: 0),
                        IMPLEMENTTIME = c.DateTime(precision: 0),
                        FINISHTIME = c.DateTime(precision: 0),
                        ITEMCODE = c.String(maxLength: 120, storeType: "nvarchar"),
                        ITEMNAME = c.String(maxLength: 120, storeType: "nvarchar"),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "WMS_TRANSFER_TASK",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        SOURCEORDER = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ITEMCODE = c.String(maxLength: 128, storeType: "nvarchar"),
                        ITEMNAME = c.String(maxLength: 720, storeType: "nvarchar"),
                        BARCODE = c.String(maxLength: 128, storeType: "nvarchar"),
                        LOT = c.String(maxLength: 128, storeType: "nvarchar"),
                        QTY = c.Int(),
                        SOURCEWAREHOUSECODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        SOURCEZONECODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        SOURCELOCATIONCODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        TARGETWAREHOUSECODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        TARGETZONECODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        TARGETLOCATIONCODE = c.String(nullable: false, maxLength: 20, storeType: "nvarchar"),
                        STOCKFLAG = c.Int(nullable: false),
                        STOCKTIME = c.DateTime(precision: 0),
                        TRANSFERFLAG = c.Int(nullable: false),
                        TRANSFERTIME = c.DateTime(precision: 0),
                        CREATOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        CREATETIME = c.DateTime(precision: 0),
                        LASTEDITOR = c.String(maxLength: 20, storeType: "nvarchar"),
                        LASTEDITTIME = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("WMS_BILL_PUTOUTBODY", "HEADERID", "WMS_BILL_PUTOUTHEADER");
            DropForeignKey("WMS_BILL_PUTINBINDINGBODY", "HEADERID", "WMS_BILL_PUTINBINDINGHEADER");
            DropForeignKey("WMS_BILL_COUNTBODY", "HEADERID", "WMS_BILL_COUNTHEADER");
            DropForeignKey("WMS_BASE_LOCATION", "ZONEID", "WMS_BASE_ZONE");
            DropForeignKey("WMS_BASE_ZONE", "WAREHOUSEID", "WMS_BASE_WAREHOUSE");
            DropForeignKey("SYS_USER_CONFIG", "SYSUSERID", "SYS_USER");
            DropForeignKey("SYS_ROLE", "SysUser_Id", "SYS_USER");
            DropForeignKey("SYS_USER", "USERGROUPID", "SYS_GROUP");
            DropForeignKey("SYS_USER_ROLE", "USERID", "SYS_USER");
            DropForeignKey("SYS_USER_ROLE", "ROLEID", "SYS_ROLE");
            DropForeignKey("SYS_ROLE_PERMISSION", "ROLEID", "SYS_ROLE");
            DropForeignKey("SYS_ROLE_PERMISSION", "PERMISSIONID", "SYS_PERMISSION");
            DropIndex("WMS_BILL_PUTOUTBODY", new[] { "HEADERID" });
            DropIndex("WMS_BILL_PUTINBINDINGBODY", new[] { "HEADERID" });
            DropIndex("WMS_BILL_COUNTBODY", new[] { "HEADERID" });
            DropIndex("WMS_BASE_ZONE", new[] { "WAREHOUSEID" });
            DropIndex("WMS_BASE_LOCATION", new[] { "ZONEID" });
            DropIndex("SYS_USER_CONFIG", new[] { "SYSUSERID" });
            DropIndex("SYS_ROLE_PERMISSION", new[] { "PERMISSIONID" });
            DropIndex("SYS_ROLE_PERMISSION", new[] { "ROLEID" });
            DropIndex("SYS_ROLE", new[] { "SysUser_Id" });
            DropIndex("SYS_USER_ROLE", new[] { "ROLEID" });
            DropIndex("SYS_USER_ROLE", new[] { "USERID" });
            DropIndex("SYS_USER", new[] { "USERGROUPID" });
            DropTable("WMS_TRANSFER_TASK");
            DropTable("WMS_STOCKER_TASK");
            DropTable("WMS_REPORT_WORK");
            DropTable("WMS_CONVEYOR_TASK");
            DropTable("WMS_BILL_WEIGH");
            DropTable("WMS_BILL_PUTOUTDETAIL");
            DropTable("WMS_BILL_PUTOUTHEADER");
            DropTable("WMS_BILL_PUTOUTBODY");
            DropTable("WMS_BILL_PUTINBINDINGHEADER");
            DropTable("WMS_BILL_PUTINBINDINGBODY");
            DropTable("WMS_BILL_PUTIN");
            DropTable("WMS_BILL_INVENTORYBOOK");
            DropTable("WMS_BILL_HEIGHT");
            DropTable("WMS_BILL_COUNTDETAIL");
            DropTable("WMS_BILL_COUNTHEADER");
            DropTable("WMS_BILL_COUNTBODY");
            DropTable("WMS_BASE_MASTER");
            DropTable("WMS_BASE_WAREHOUSE");
            DropTable("WMS_BASE_ZONE");
            DropTable("WMS_BASE_LOCATION");
            DropTable("WMS_BASE_DEVICEERRORLOG");
            DropTable("WMS_AGV_TAXI");
            DropTable("WMS_AGV_TASK");
            DropTable("SYS_SETTING");
            DropTable("SYS_LOG");
            DropTable("SYS_USER_CONFIG");
            DropTable("SYS_PERMISSION");
            DropTable("SYS_ROLE_PERMISSION");
            DropTable("SYS_ROLE");
            DropTable("SYS_USER_ROLE");
            DropTable("SYS_USER");
            DropTable("SYS_GROUP");
            DropTable("SYS_EXCEPTION");
        }
    }
}
