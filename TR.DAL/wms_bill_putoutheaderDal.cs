﻿using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TR.IDAL;
using TR.Model;
using TR.ORM.Model;

namespace TR.DAL
{
    public partial class wms_bill_putoutheaderDal : BaseDal<wms_bill_putoutheader>, Iwms_bill_putoutheaderDal
    {
        public List<wms_bill_putoutheader> GetHeaderPage(
            PutOutModel filter,
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
            AddLikeFilter(where, parameters, "h.CONTAINER", "CONTAINER", filter.CONTAINER);
            AddLikeFilter(where, parameters, "h.TARGET", "TARGET", filter.TARGET);
            AddEqualFilter(where, parameters, "h.ZONECODE", "ZONECODE", filter.ZONECODE);
            AddEqualFilter(where, parameters, "h.STATE", "STATE", filter.STATE, -10);
            AddEqualFilter(where, parameters, "h.ENABLED", "ENABLED", filter.ENABLED, -10);
            AddEqualFilter(where, parameters, "h.QUESTFLAG", "QUESTFLAG", filter.QUESTFLAG, -10);

            if (!string.IsNullOrEmpty(filter.ITEMCODE))
            {
                where.Add("EXISTS (SELECT 1 FROM wms_bill_putoutbody b WHERE b.HEADERID = h.ID AND b.ITEMCODE = @ITEMCODE)");
                parameters.Add(new SugarParameter("@ITEMCODE", filter.ITEMCODE));
            }
            if (!string.IsNullOrEmpty(filter.LOT))
            {
                where.Add("EXISTS (SELECT 1 FROM wms_bill_putoutbody b WHERE b.HEADERID = h.ID AND b.LOT = @LOT)");
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
                "SELECT COUNT(1) FROM wms_bill_putoutheader h WHERE " + whereSql,
                parameters);

            int offset = Math.Max(0, (pageIndex - 1) * pageSize);
            var pageParameters = new List<SugarParameter>(parameters)
            {
                new SugarParameter("@OFFSET", offset),
                new SugarParameter("@PAGE_SIZE", pageSize)
            };
            string sql = "SELECT h.* FROM wms_bill_putoutheader h WHERE " + whereSql
                + " ORDER BY h.CREATETIME DESC LIMIT @OFFSET, @PAGE_SIZE";
            return db.Ado.SqlQuery<wms_bill_putoutheader>(sql, pageParameters.ToArray());
        }

        private static void AddLikeFilter(
            List<string> where,
            List<SugarParameter> parameters,
            string column,
            string parameterName,
            string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            where.Add(column + " LIKE CONCAT('%', @" + parameterName + ", '%')");
            parameters.Add(new SugarParameter("@" + parameterName, value));
        }

        private static void AddEqualFilter(
            List<string> where,
            List<SugarParameter> parameters,
            string column,
            string parameterName,
            string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            where.Add(column + " = @" + parameterName);
            parameters.Add(new SugarParameter("@" + parameterName, value));
        }

        private static void AddEqualFilter(
            List<string> where,
            List<SugarParameter> parameters,
            string column,
            string parameterName,
            int value,
            int ignoredValue = int.MinValue)
        {
            if (value == ignoredValue)
            {
                return;
            }

            where.Add(column + " = @" + parameterName);
            parameters.Add(new SugarParameter("@" + parameterName, value));
        }

        public List<wms_bill_putoutheader> Getlist(
            Expression<Func<PutOutModel, bool>> whereExpression,
            ref int intTotalCount,
            int intPageIndex = 0,
            int intPageSize = 20,
            string strOrderByFileds = null)
        {
            var matchingHeaders = db.SqlQueryable<PutOutModel>(" SELECT a.*, b.ITEMCODE, b.LOT FROM wms_bill_putoutheader a LEFT JOIN wms_bill_putoutbody b ON a.ID = b.HEADERID ")
                .WhereIF(whereExpression != null, whereExpression)
                .Select(a => a.ID).ToSql();
            return db.Queryable<wms_bill_putoutheader>()
                .Where("ID IN (" + matchingHeaders.Key + ")", matchingHeaders.Value.ToArray())
                .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
                .ToPageList(intPageIndex, intPageSize, ref intTotalCount);
        }
    }
}
