using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AUTOTask
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {   ////调试用
            //if (Environment.UserInteractive) // 调试模式
            //{
            //    var service = new MyService();
            //    service.TestStart(); // 自定义测试方法

            //  //  service.TestStop();
            //}
            //else // 真实服务模式
            //{
            //    ServiceBase.Run(new ServiceBase[] { new MyService() });
            //}
            // 在YourService类中添加


            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MyService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    
    }
    
}
