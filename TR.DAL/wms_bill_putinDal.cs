using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TR.IDAL;
using TR.Model;
using TR.ORM.Model;

namespace TR.DAL
{
	public partial class wms_bill_putinDal : BaseDal<wms_bill_putin>, Iwms_bill_putinDal
	{
      
        public List<wms_bill_putinbindingheader> Getlist(
		 Expression<Func<PutInModel, bool>> whereExpression,		 
			 ref int intTotalCount,
			 int intPageIndex = 0,
			 int intPageSize = 20,
			 string strOrderByFileds = null)
		{
			//b.ITEMCODE, b.LOT
			//(" SELECT  a.ID,a.SOURCEORDER,a.QUESTFLAG,a.CONTAINER,a.TASKTYPE,a.CONTAINERTYPE,a.LOCATIONCODE,a.TARGET,a.STATE,a.REMARK,a.QUESTTIME,a.FINISHTIME,a.ENABLED,a.CREATOR, a.CREATETIME, a.LASTEDITOR, a.LASTEDITTIME ,b.ITEMCODE, b.LOT  from wms_bill_putinbindingheader a     FORCE  INDEX(PRIMARY) left JOIN  wms_bill_putinbindingbody  b  on  a.ID = b.HEADERID   ")
			// 明细仅用于筛选单头，避免按 ID 分组时任意选取物料/批次。
			var matchingHeaders = db.SqlQueryable<PutInModel>(" SELECT  a.ID,a.SOURCEORDER,a.QUESTFLAG,a.CONTAINER,a.TASKTYPE,a.CONTAINERTYPE,a.LOCATIONCODE,a.TARGET,a.STATE,a.REMARK,a.QUESTTIME,a.FINISHTIME,a.ENABLED,a.CREATOR, a.CREATETIME, a.LASTEDITOR, a.LASTEDITTIME, b.ITEMCODE, b.LOT from wms_bill_putinbindingheader a      left JOIN  wms_bill_putinbindingbody  b     on  a.ID = b.HEADERID   ")
			   .WhereIF(whereExpression != null, whereExpression)
			   .Select(a => a.ID).ToSql();
			return db.Queryable<wms_bill_putinbindingheader>()
			   .Where("ID IN (" + matchingHeaders.Key + ")", matchingHeaders.Value.ToArray())
			   .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
			   .ToPageList(intPageIndex, intPageSize, ref intTotalCount)
				
			   ;
		}

		public string DeletePutIn(string ID)
		{
			string result = "";

			try
			{
				db.Ado.BeginTran();
				var putin = db.Queryable<wms_bill_putinbindingheader>().Where(t => t.ID == ID).First();
				//若未分配库位则直接删除 ，若已分配库位则查看堆垛机任务若有则不可删除，否则可以删除并更新库位信息
				if (string.IsNullOrEmpty(putin.LOCATIONCODE))
				{
					db.Deleteable<wms_bill_putinbindingbody>().Where(it => it.HEADERID == ID).ExecuteCommand();
					db.Deleteable<wms_bill_putinbindingheader>().Where(it => it.ID == ID).ExecuteCommand();
					result = "true";
				}
				else
				{
					var taskcount = db.Queryable<wms_stocker_task>().Where(t => t.SOURCEORDER == putin.SOURCEORDER & t.CONTAINER == putin.CONTAINER).Count();
					if (taskcount > 0)
					{
						result = "WCS已接收任务，堆垛机任务正在执行，无法删除";
					}
					else
					{
						db.Deleteable<wms_bill_putinbindingbody>().Where(it => it.HEADERID == ID).ExecuteCommand();
						db.Deleteable<wms_bill_putinbindingheader>().Where(it => it.ID == ID).ExecuteCommand();
						db.Deleteable<wms_stocker_task>().Where(t => t.SOURCEORDER == putin.SOURCEORDER & t.CONTAINER == putin.CONTAINER).ExecuteCommand();
						db.Updateable<wms_base_location>().UpdateColumns(t => new wms_base_location() { ISAVIALABLE = 1 }).Where(it => it.ZONECODE==putin.TARGET&it.LOCATIONCODE == putin.LOCATIONCODE).ExecuteCommand();

						result = "true";
					}
				}
				db.Ado.CommitTran();
				
			}
			catch (Exception ex)
			{
				db.Ado.RollbackTran();
			}

			return result;
		}

		//public List<wms_bill_putinbindingheader> Getlist(
		// Expression<Func<wms_bill_putinbindingheader, wms_bill_putinbindingbody, bool>> whereExpression,
		//	 //Expression<Func<wms_bill_putinbindingbody, bool>> whereExpressionbody,
		//	 ref int intTotalCount,
		//	 int intPageIndex = 0,
		//	 int intPageSize = 20,
		//	 string strOrderByFileds = null)
		//{
		//	//b.ITEMCODE, b.LOT
		//	//(" SELECT  a.ID,a.SOURCEORDER,a.QUESTFLAG,a.CONTAINER,a.TASKTYPE,a.CONTAINERTYPE,a.LOCATIONCODE,a.TARGET,a.STATE,a.REMARK,a.QUESTTIME,a.FINISHTIME,a.ENABLED,a.CREATOR, a.CREATETIME, a.LASTEDITOR, a.LASTEDITTIME ,b.ITEMCODE, b.LOT  from wms_bill_putinbindingheader a     FORCE  INDEX(PRIMARY) left JOIN  wms_bill_putinbindingbody  b  on  a.ID = b.HEADERID   ")
		//	return db.Queryable<wms_bill_putinbindingheader, wms_bill_putinbindingbody>((a, b) => new object[] {
		//JoinType.Left,a.ID==b.HEADERID})

		//		.OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
		//	   .WhereIF(whereExpression != null, whereExpression)
		//		  .Select<wms_bill_putinbindingheader>()
		//	   .Distinct()
		//	   //.WhereIF(whereExpressionbody != null, whereExpressionbody)
		//	   .ToPageList(intPageIndex, intPageSize, ref intTotalCount);
		//}

	}
}
