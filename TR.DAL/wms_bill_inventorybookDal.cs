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

        public List<InventoryGGModel> GetGGlist(
        Expression<Func<InventoryGGModel, bool>> whereExpression,
            ref int intTotalCount,
            int intPageIndex = 0,
            int intPageSize = 20,
            string strOrderByFileds = null)
        {
            return db.SqlQueryable<InventoryGGModel>("SELECT  ZONECODE, SOURCEORDER, COUNT(*) AS NUM, SUM(CASE WHEN OUTENABLED = 0 THEN 1 ELSE 0 END) AS OCCUPYNUM  from wms_base_location   WHERE CONTAINER!=''   GROUP BY SOURCEORDER    ")
               .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
               .WhereIF(whereExpression != null, whereExpression)
               .ToPageList(intPageIndex, intPageSize, ref intTotalCount);
        }


        public List<InventoryKCModel> GetKClist(
        Expression<Func<InventoryKCModel, bool>> whereExpression,
            ref int intTotalCount,
            int intPageIndex = 0,
            int intPageSize = 20,
            string strOrderByFileds = null)
        {
            return db.SqlQueryable<InventoryKCModel>("SELECT a.ID,  a.ITEMCODE, COUNT(b.SOURCEORDER) AS NUM, a.MINSTORAGE, CASE  WHEN a.MINSTORAGE <= COUNT(b.SOURCEORDER) THEN '库存充足'  ELSE '库存不足'  END AS KC FROM wms_base_master a LEFT JOIN wms_base_location b    ON a.ITEMCODE =b.SOURCEORDER   AND b.CONTAINER != ''  GROUP BY     a.ITEMCODE,     a.MINSTORAGE  ")
               .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
               .WhereIF(whereExpression != null, whereExpression)
               .ToPageList(intPageIndex, intPageSize, ref intTotalCount);
        }
    }
}
