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
    public partial class wms_bill_inventorybookDal : BaseDal<wms_bill_inventorybook>, Iwms_bill_inventorybookDal
    {

        //public List<InventorystatisticsModel> GetInventoryList(
        // Expression<Func<InventorystatisticsModel, bool>> whereExpression,
        //     ref int intTotalCount,
        //     int intPageIndex = 0,
        //     int intPageSize = 20,
        //     string strOrderByFileds = null)
        //{
        //    return db.SqlQueryable<InventorystatisticsModel>("SELECT  a.ZONECODE, a.LOCATIONCODE, CASE  WHEN a.CONTAINER = '' and  a.ENABLED=0 and a.ISAVIALABLE=0  THEN '冻结库位'  WHEN a.CONTAINER = '' and  a.ENABLED=1   THEN '空库位'    WHEN a.CONTAINER != '' and a.SOURCEORDER!= '' THEN '有库存' ELSE '' END as LOCATIONSTATUS, a.CONTAINER, a.SOURCEORDER, a.ROWNUM,  a.COLUMNNUM,a.LAYERNUM,a.TUNNELNUM    from wms_base_location a   ")
        //       .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
        //       .WhereIF(whereExpression != null, whereExpression)
        //       .ToPageList(intPageIndex, intPageSize, ref intTotalCount);
        //}

        public List<InventorystatisticsModel> Getlist(
		 Expression<Func<InventorystatisticsModel, bool>> whereExpression,
			 ref int intTotalCount,
			 int intPageIndex = 0,
			 int intPageSize = 20,
			 string strOrderByFileds = null)
		{
			return db.SqlQueryable<InventorystatisticsModel>("SELECT  a.ZONECODE, a.LOCATIONCODE, CASE  WHEN a.CONTAINER = '' and  a.ENABLED=0 and a.ISAVIALABLE=0  THEN '冻结库位'  WHEN a.CONTAINER = '' and  a.ENABLED=1   THEN '空库位'    WHEN a.CONTAINER != '' and a.SOURCEORDER!= '' THEN '有库存' ELSE '' END as LOCATIONSTATUS, a.CONTAINER, a.SOURCEORDER, a.ROWNUM,  a.COLUMNNUM,a.LAYERNUM,a.TUNNELNUM ,a.LASTEDITTIME, a.OUTENABLED   from wms_base_location a   ")
			   .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
			   .WhereIF(whereExpression != null, whereExpression)
			   .ToPageList(intPageIndex, intPageSize, ref intTotalCount);
		}

        // 全库或宽泛查询（命中盒数较多）走单遍主键顺序扫描聚合，避免关联子查询逐盒回表超时；
        // 有历史针记录但在库针数为零时不折算。
        private const string InventoryGGFullSql = @"
SELECT ANY_VALUE(l.ZONECODE) AS ZONECODE, l.SOURCEORDER,
       COUNT(*) AS NUM,
       SUM(CASE WHEN l.OUTENABLED = 0 THEN 1 ELSE 0 END) AS OCCUPYNUM,
       SUM(CASE WHEN n.CONTAINER IS NULL THEN 50 ELSE n.NEEDLENUM END) AS NEEDLENUM
FROM wms_base_location l
LEFT JOIN (
    SELECT CONTAINER,
           SUM(CASE WHEN ENABLED = 1 AND STATUS IN
               ('INBOUND_PENDING', 'AVAILABLE', 'RESERVED', 'QUARANTINED')
               THEN 1 ELSE 0 END) AS NEEDLENUM
    FROM wms_needle_inventory FORCE INDEX (PRIMARY)
    GROUP BY CONTAINER
) n ON n.CONTAINER = l.CONTAINER
WHERE l.CONTAINER <> ''
  AND (@SourceOrder = '' OR l.SOURCEORDER LIKE CONCAT('%', @SourceOrder, '%'))
GROUP BY l.SOURCEORDER";

        // 精确/窄范围规格查询（命中盒数 ≤ 1000）走按盒关联子查询，避免扫描整张针表。
        private const string InventoryGGFilteredSql = @"
SELECT ANY_VALUE(l.ZONECODE) AS ZONECODE, l.SOURCEORDER,
       COUNT(*) AS NUM,
       SUM(CASE WHEN l.OUTENABLED = 0 THEN 1 ELSE 0 END) AS OCCUPYNUM,
       SUM(COALESCE((
           SELECT SUM(CASE WHEN n.ENABLED = 1 AND n.STATUS IN
               ('INBOUND_PENDING', 'AVAILABLE', 'RESERVED', 'QUARANTINED')
               THEN 1 ELSE 0 END)
           FROM wms_needle_inventory n
           WHERE n.CONTAINER = l.CONTAINER
       ), 50)) AS NEEDLENUM
FROM wms_base_location l
WHERE l.CONTAINER <> ''
  AND l.SOURCEORDER LIKE CONCAT('%', @SourceOrder, '%')
GROUP BY l.SOURCEORDER";

        private const int FilteredSubqueryBoxThreshold = 1000;

        public List<InventoryGGModel> GetGGlist(
            string sourceOrder,
            ref int intTotalCount,
            int intPageIndex = 0,
            int intPageSize = 20,
            string strOrderByFileds = null,
            InventoryGGModel summary = null)
        {
            string keyword = (sourceOrder ?? string.Empty).Trim();

            // 这里只返回规格汇总行（非单针明细），一次查询后分页、合计，
            // 避免 ToPageList 计数和独立总计反复执行针表聚合。
            var rows = TR.ORM.SqlSugarClientFactory.ExecuteWithRetry(() =>
            {
                bool useFilteredSubquery = false;
                if (!string.IsNullOrEmpty(keyword))
                {
                    int matchedBoxes = db.Ado.GetInt(
                        "SELECT COUNT(*) FROM wms_base_location WHERE CONTAINER <> '' AND SOURCEORDER LIKE CONCAT('%', @SourceOrder, '%')",
                        new[] { new SugarParameter("@SourceOrder", keyword) });
                    useFilteredSubquery = matchedBoxes <= FilteredSubqueryBoxThreshold;
                }

                string sql = useFilteredSubquery ? InventoryGGFilteredSql : InventoryGGFullSql;
                return db.SqlQueryable<InventoryGGModel>(sql)
                    .AddParameters(new[] { new SugarParameter("@SourceOrder", keyword) })
                    .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
                    .ToList();
            });
            intTotalCount = rows.Count;
            if (summary != null)
            {
                summary.NUM = rows.Sum(row => Convert.ToInt64(row.NUM)).ToString();
                summary.OCCUPYNUM = rows.Sum(row => Convert.ToInt64(row.OCCUPYNUM)).ToString();
                summary.NEEDLENUM = rows.Sum(row => row.NEEDLENUM);
            }

            int pageSize = intPageSize > 0 ? intPageSize : 20;
            long offset = (long)Math.Max(intPageIndex - 1, 0) * pageSize;
            return rows.Skip((int)Math.Min(offset, int.MaxValue)).Take(pageSize).ToList();
        }


        public List<InventoryKCModel> GetKClist(
        Expression<Func<InventoryKCModel, bool>> whereExpression,
            ref int intTotalCount,
            int intPageIndex = 0,
            int intPageSize = 20,
            string strOrderByFileds = null)
        {
            return db.SqlQueryable<InventoryKCModel>("SELECT any_value(a.ID) AS ID,  a.ITEMCODE, COUNT(b.SOURCEORDER) AS NUM, a.MINSTORAGE, CASE  WHEN a.MINSTORAGE <= COUNT(b.SOURCEORDER) THEN '库存充足'  ELSE '库存不足'  END AS KC FROM wms_base_master a LEFT JOIN wms_base_location b    ON a.ITEMCODE =b.SOURCEORDER   AND b.CONTAINER != ''  GROUP BY     a.ITEMCODE,     a.MINSTORAGE  ")
               .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
               .WhereIF(whereExpression != null, whereExpression)
               .ToPageList(intPageIndex, intPageSize, ref intTotalCount);
        }
    }
}
