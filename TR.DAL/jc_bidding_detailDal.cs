using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TR.Common;
using TR.IDAL;
using TR.Model;
using TR.ORM.Model;

namespace TR.DAL
{
    public partial class jc_bidding_detailDal : BaseDal<jc_bidding_detail>, Ijc_bidding_detailDal
	{
      
        public List<BiddingProjectDetailModel> Getlist(
		 Expression<Func<BiddingProjectDetailModel, bool>> whereExpression,
			 ref int intTotalCount,
			 int intPageIndex = 0,
			 int intPageSize = 20,
			 string strOrderByFileds = null)
		{
			return db.SqlQueryable<BiddingProjectDetailModel>(" SELECT  a.COMPANYNAME,a.PROJECTCODE,a.PROJECTNAME,a.PROJECTTYPE,b.USERNAME,b.MYPRICE,b.TRUEPRICE,b.PREPRICE, b.BIDDTIME,b.DESCRIBE,a.BIDDINGTYPE  from jc_bidding_project a	 left JOIN  jc_bidding_history  b     on  a.PROJECTCODE = b.PROJECTCODE ")
			   .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
			   .WhereIF(whereExpression != null, whereExpression)
			   .ToPageList(intPageIndex, intPageSize, ref intTotalCount);
		}

		public List<BiddingProjectDetailModel> GetDetaillist(
		Expression<Func<BiddingProjectDetailModel, bool>> whereExpression,
			ref int intTotalCount,
			int intPageIndex = 0,
			int intPageSize = 20,
			string strOrderByFileds = null)
		{
			return db.SqlQueryable<BiddingProjectDetailModel>(" SELECT  a.COMPANYNAME,a.PROJECTCODE,a.PROJECTNAME,a.PROJECTTYPE,b.USERNAME,b.MYPRICE,b.TRUEPRICE,b.PREPRICE, b.BIDDTIME,b.DESCRIBE ,a.BIDDINGTYPE from jc_bidding_project a	 left JOIN  jc_bidding_detail  b     on  a.PROJECTCODE = b.PROJECTCODE ")
			   .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
			   .WhereIF(whereExpression != null, whereExpression)
			   .ToPageList(intPageIndex, intPageSize, ref intTotalCount);
		}

		public bool DeleteProject(string PROJECTCODE)
		{
            bool result = false;

            try
            {
               

				db.Ado.BeginTran();
				db.Deleteable<jc_bidding_project>().Where(it => it.PROJECTCODE == PROJECTCODE).ExecuteCommand();
				db.Deleteable<jc_bidding_detail>().Where(it => it.PROJECTCODE == PROJECTCODE).ExecuteCommand();
				db.Deleteable<jc_bidding_history>().Where(it => it.PROJECTCODE == PROJECTCODE).ExecuteCommand();
				//db.Updateable<work_process>()
			 //.UpdateColumns(it => new work_process() { group_id = class_id, machine_ids = "", user_ids = "", work_status = "待排产" })
			 //.Where(it => it.job_id == job_id).ExecuteCommand();

				db.Ado.CommitTran();
                result = true;
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
            }

            return result;

        }
        public bool DBiNIT()
        {
            bool result = false;

            try
            {


                db.Ado.BeginTran();
                List<int> uidlist = db.Queryable<sys_userrole>().Where(t => t.RoleId==2).Select(t => t.UserId).Distinct().ToList();
                var list = db.Queryable<sys_users>().Where(t => uidlist.Contains(SqlFunc.ToInt32(t.UserId))).ToList();
                //删除无关数据
                db.Deleteable<jc_customer_level>().Where(p => !uidlist.Contains(p.MYNAME)).ExecuteCommand();
                foreach (var item in list)
                {
                   var templist=  db.Queryable<jc_customer_level>().Where(t => t.MYNAME==item.UserId).ToList();
                    if (templist.Count > 0)
                    {
                        db.Updateable<jc_customer_level>()
                        .SetColumns(it => new jc_customer_level() { CUSTOMERNAME = item.UserName })
                        .Where(it => it.MYNAME == item.UserId).ExecuteCommand();
                    }
                    else
                    {
                        var jc_customer_level = new jc_customer_level
                        {
                            ID = ResultHelper.NewId,
                            MYNAME = item.UserId,
                            CUSTOMERNAME=item.UserName,
                            PREPRICE=100,//默认100
                            CREATOR = "sys",
                            CREATETIME = DateTime.Now
                        };
                        var result1 = db.Insertable(jc_customer_level).ExecuteCommand();
                    }

                }

              

                db.Ado.CommitTran();
                result = true;
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
            }

            return result;

        }

    }
}
