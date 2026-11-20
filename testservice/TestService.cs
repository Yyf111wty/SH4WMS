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

namespace testservice
{
    public partial class TestService : ServiceBase
    {
        public TestService()
        {
            InitializeComponent();
        }
        private Timer timerDelay;
        protected override void OnStart(string[] args)
        {
            timerDelay = new Timer(5000); // 每5秒触发一次
            timerDelay.Elapsed += timerDelay_Elapsed;
            timerDelay.Start();
            timerDelay.AutoReset = true;
            timerDelay.Enabled = true; // 显式启动定时器
        }

        protected override void OnStop()
        {
        }
        private void timerDelay_Elapsed(object sender, ElapsedEventArgs e)
        {
            EventLog.WriteEntry(ServiceName, "开始执行数据库更新...", EventLogEntryType.Information);
            //定义连接资源


        }
        public void TestStart() => OnStart(null);
        public void TestStop() => OnStop();
    }
}
