using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using TR.ORM.Model;
using TR.Service;

namespace WinService
{
    public class Antotask
    {
        private System.Timers.Timer timerDelay;
        jc_bidding_projectService service = new jc_bidding_projectService();
        jc_bidding_detailService bodyservice = new jc_bidding_detailService();
        jc_bidding_project model = new jc_bidding_project();
        public Antotask() { OnStart(); }
      

        private  void OnStart()
        {
            timerDelay = new System.Timers.Timer(5000); // 每5秒触发一次
            timerDelay.Elapsed += timerDelay_Elapsed;
            timerDelay.Start();
            timerDelay.AutoReset = true;
            timerDelay.Enabled = true; // 显式启动定时器
        }
        private void timerDelay_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

                {
                    DateTime now = DateTime.Now;
                    var list = service.Query(t => t.ENDTIME <= now).ToList();
                    if (list.Count > 0)
                    {

                        foreach (var item in list)
                        {
                            var sstemplist = bodyservice.Query(t => t.PROJECTCODE == item.PROJECTCODE).OrderBy(t => t.MYPRICE)
                                                                      .ThenBy(t => t.BIDDTIME).ToList().FirstOrDefault();

                            var detailcount = service.Query(t => t.PROJECTCODE == item.PROJECTCODE);

                            jc_bidding_project newmodel = new jc_bidding_project();
                            model = detailcount.FirstOrDefault();
                            newmodel.ID = model.ID;
                            newmodel.PROJECTCODE = model.PROJECTCODE;
                            newmodel.STATE = 1;
                            newmodel.COMPANYNAME = model.COMPANYNAME;
                            newmodel.PROJECTCODE = model.PROJECTCODE;
                            newmodel.PROJECTNAME = model.PROJECTNAME;
                            newmodel.PROJECTTYPE = model.PROJECTTYPE;
                            newmodel.UNITPRICE = model.UNITPRICE;
                            //model.COLUMNNUM = int.Parse(LOCATIONCODE.Substring(2, 2));
                            newmodel.NUMBER = model.NUMBER;
                            newmodel.DESCRIBE = model.DESCRIBE;
                            newmodel.BUDGET = model.BUDGET; ;
                            newmodel.MINPRICE = model.MINPRICE;
                            newmodel.MAXPRICE = model.MAXPRICE;
                            newmodel.RANGEPRICE = model.RANGEPRICE;
                            newmodel.FILEADDRESS = model.FILEADDRESS;
                            newmodel.DELAYTIMES = model.DELAYTIMES;
                            newmodel.REMAINDELAYTIMES = model.REMAINDELAYTIMES;
                            newmodel.STARTTIME = model.STARTTIME;
                            newmodel.ENDTIME = model.ENDTIME;

                            newmodel.BIDDINGTYPE = model.BIDDINGTYPE;

                            newmodel.CREATOR = model.CREATOR;
                            newmodel.CREATETIME = model.CREATETIME;
                            newmodel.LASTEDITOR = model.LASTEDITOR;
                            if (sstemplist != null && item.BIDDINGTYPE == "标准项目")
                            {
                                newmodel.BID = sstemplist.USERNAME;

                                newmodel.BIDPRICE = sstemplist.MYPRICE;

                                newmodel.BIDTIME = sstemplist.BIDDTIME;
                            }
                            else
                            {
                                newmodel.BID = model.BID;

                                newmodel.BIDPRICE = model.BIDPRICE;

                                newmodel.BIDTIME = model.BIDTIME;
                            }
                            newmodel.LASTEDITTIME = DateTime.Now;

                            service.Update(newmodel);

                        }

                    }

                }

            }
            catch (Exception ex)
            {

            }



        }
    }

}
