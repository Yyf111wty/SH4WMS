using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace testservice
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            ////调试用
            if (Environment.UserInteractive) // 调试模式
            {
                var service = new TestService();
                service.TestStart(); // 自定义测试方法

                //  service.TestStop();
            }
            else // 真实服务模式
            {
                ServiceBase.Run(new ServiceBase[] { new TestService() });
            }

            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //    new TestService()
            //};
            //ServiceBase.Run(ServicesToRun);
        }
    }
}
