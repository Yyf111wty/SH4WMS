﻿using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TR.IDAL;
using TR.Model;
using TR.ORM;
using TR.ORM.Model;

namespace TR.DAL
{
	public partial class wms_bill_putinDal : BaseDal<wms_bill_putin>, Iwms_bill_putinDal
	{
      
        public List<wms_bill_putinbindingheader> GetHeaderPage(
            PutInModel filter,
            bool useFinishTimeRange,
            DateTime? createdFrom,
            DateTime? createdTo,
            ref int totalCount,
            int pageIndex = 1,
            int pageSize = 30,
            string orderByFields = null)
        {
            var where = new List<string>();
            var parameters = new List<SugarParameter>();

            AddLikeFilter(where, parameters, "h.SOURCEORDER", "SOURCEORDER", filter.SOURCEORDER);
            AddEqualFilter(where, parameters, "h.TARGET", "TARGET", filter.TARGET);
            AddEqualFilter(where, parameters, "h.CONTAINER", "CONTAINER", filter.CONTAINER);
            AddEqualFilter(where, parameters, "h.LOCATIONCODE", "LOCATIONCODE", filter.LOCATIONCODE);
            AddEqualFilter(where, parameters, "h.STATE", "STATE", filter.STATE, -10);
            AddEqualFilter(where, parameters, "h.ENABLED", "ENABLED", filter.ENABLED, -10);
            AddEqualFilter(where, parameters, "h.QUESTFLAG", "QUESTFLAG", filter.QUESTFLAG, -10);

            if (!string.IsNullOrEmpty(filter.ITEMCODE))
            {
                where.Add("EXISTS (SELECT 1 FROM wms_bill_putinbindingbody b WHERE b.HEADERID = h.ID AND b.ITEMCODE = @ITEMCODE)");
                parameters.Add(new SugarParameter("@ITEMCODE", filter.ITEMCODE));
            }
            if (!string.IsNullOrEmpty(filter.LOT))
            {
                where.Add("EXISTS (SELECT 1 FROM wms_bill_putinbindingbody b WHERE b.HEADERID = h.ID AND b.LOT = @LOT)");
                parameters.Add(new SugarParameter("@LOT", filter.LOT));
            }

            if (useFinishTimeRange)
            {
                if (filter.startTime.HasValue)
                {
                    where.Add("h.FINISHTIME >= @STARTTIME");
                    parameters.Add(new SugarParameter("@STARTTIME", filter.startTime.Value));
                }
                if (filter.endTime.HasValue)
                {
                    where.Add("h.FINISHTIME <= @ENDTIME");
                    parameters.Add(new SugarParameter("@ENDTIME", filter.endTime.Value));
                }
            }
            else
            {
                where.Add("h.CREATETIME >= @CREATED_FROM AND h.CREATETIME < @CREATED_TO");
                parameters.Add(new SugarParameter("@CREATED_FROM", createdFrom.Value));
                parameters.Add(new SugarParameter("@CREATED_TO", createdTo.Value));
            }

            string whereSql = where.Count == 0 ? "1 = 1" : string.Join(" AND ", where);
            totalCount = db.Ado.GetInt(
                "SELECT COUNT(1) FROM wms_bill_putinbindingheader h WHERE " + whereSql,
                parameters);

            int offset = Math.Max(0, (pageIndex - 1) * pageSize);
            var pageParameters = new List<SugarParameter>(parameters)
            {
                new SugarParameter("@OFFSET", offset),
                new SugarParameter("@PAGE_SIZE", pageSize)
            };
            string sql = "SELECT h.* FROM wms_bill_putinbindingheader h WHERE " + whereSql
                + " ORDER BY h.CREATETIME DESC LIMIT @OFFSET, @PAGE_SIZE";
            return db.Ado.SqlQuery<wms_bill_putinbindingheader>(sql, pageParameters.ToArray());
        }

        private static void AddLikeFilter(List<string> where, List<SugarParameter> parameters, string column, string parameterName, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            where.Add(column + " LIKE CONCAT('%', @" + parameterName + ", '%')");
            parameters.Add(new SugarParameter("@" + parameterName, value));
        }

        private static void AddEqualFilter(List<string> where, List<SugarParameter> parameters, string column, string parameterName, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            where.Add(column + " = @" + parameterName);
            parameters.Add(new SugarParameter("@" + parameterName, value));
        }

        private static void AddEqualFilter(List<string> where, List<SugarParameter> parameters, string column, string parameterName, int value, int ignoredValue = int.MinValue)
        {
            if (value == ignoredValue) return;
            where.Add(column + " = @" + parameterName);
            parameters.Add(new SugarParameter("@" + parameterName, value));
        }

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
				SqlSugarClientFactory.BeginTran();
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
				SqlSugarClientFactory.CommitTran();
				
			}
			catch (Exception ex)
			{
				SqlSugarClientFactory.RollbackTran();
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
