using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TR.Service;
using TR.ORM.Model;
//using System.Threading;

namespace AUTOTask
{
    public partial class MyService : ServiceBase
    {
        public MyService()
        {
            InitializeComponent();
        }
        private     Timer timerDelay;
        jc_bidding_projectService service = new jc_bidding_projectService();
        jc_bidding_detailService bodyservice = new jc_bidding_detailService();
        jc_bidding_project model = new jc_bidding_project();
        GV   gV = new GV();
        protected override void OnStart(string[] args)
        {

            //keepAliveThread = new Thread(KeepAlive);
            //keepAliveThread.IsBackground = true;
            //keepAliveThread.Start();
            ////延迟启动
            //timerDelay = new Timer(timerDelay_Elapsed, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5));
            //timerDelay.Dispose();
            // 使用System.Timers.Timer（推荐用于Windows服务）

            timerDelay = new System.Timers.Timer(5000);
            timerDelay.Elapsed += new System.Timers.ElapsedEventHandler(timerDelay_Elapsed);
            timerDelay.Start();

            //timerDelay = new Timer(5000); // 每5秒触发一次
            //timerDelay.Elapsed += timerDelay_Elapsed;
            //timerDelay.Start();
            //timerDelay.AutoReset = true;
            //timerDelay.Enabled = true; // 显式启动定时器
        }
     
        //private void timerDelay_Elapsed(object state)
        //{
        //    try
        //    {

        //        //获取截取时间的项目 ，符合条件的就更新项目状态 Convert.ToDateTime(ENDTIME)
        //        DateTime now = DateTime.Now;
        //        var list = service.Query(t => t.ENDTIME >= now).ToList();
        //        if (list.Count > 0)
        //        {

        //            foreach (var item in list)
        //            {
        //                var sstemplist = bodyservice.Query(t => t.PROJECTCODE == item.PROJECTCODE).OrderBy(t => t.MYPRICE)
        //                                                          .ThenBy(t => t.BIDDTIME).ToList().FirstOrDefault();
        //                if (sstemplist == null) continue;
        //                var detailcount = service.Query(t => t.PROJECTCODE == item.PROJECTCODE);

        //                jc_bidding_project newmodel = new jc_bidding_project();
        //                model = detailcount.FirstOrDefault();
        //                newmodel.ID = model.ID;
        //                newmodel.PROJECTCODE = model.PROJECTCODE;
        //                newmodel.STATE = 1;

        //                newmodel.BID = sstemplist.USERNAME;


        //                newmodel.BIDPRICE = sstemplist.MYPRICE;

        //                newmodel.BIDTIME = sstemplist.BIDDTIME;


        //                newmodel.LASTEDITTIME = DateTime.Now;
        //                service.Update(newmodel);

        //            }

        //        }


        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        private void timerDelay_Elapsed(object sender, ElapsedEventArgs e)
        {
            timerDelay.Enabled = false;
            timerDelay.Close();
            //定义连接资源
            gV.Init();

        }
        protected override void OnStop()
        {

            gV.Dispose();
            //timerDelay?.Stop();
            // timerDelay?.Dispose();
        }
        //public void TestStart() => OnStart(null);
        //public void TestStop() => OnStop();
    }
}
