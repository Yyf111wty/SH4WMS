using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TR.Model;
using TR.ORM.Model;

namespace TR.IDAL
{
    public partial interface Iwms_bill_inventorybookDal : IBaseDal<wms_bill_inventorybook>
    {
        //List<InventorystatisticsModel> GetInventoryList(
        //  Expression<Func<InventorystatisticsModel, bool>> whereExpression,
        //      ref int intTotalCount,
        //      int intPageIndex = 0,
        //      int intPageSize = 20,
        //      string strOrderByFileds = null);
        List<InventorystatisticsModel> Getlist(
		  Expression<Func<InventorystatisticsModel, bool>> whereExpression,
			  ref int intTotalCount,
			  int intPageIndex = 0,
			  int intPageSize = 20,
			  string strOrderByFileds = null);
        List<InventoryGGModel> GetGGlist(
          string sourceOrder,
              ref int intTotalCount,
              int intPageIndex = 0,
              int intPageSize = 20,
              string strOrderByFileds = null,
              InventoryGGModel summary = null);
        List<InventoryKCModel> GetKClist(
        Expression<Func<InventoryKCModel, bool>> whereExpression,
            ref int intTotalCount,
            int intPageIndex = 0,
            int intPageSize = 20,
            string strOrderByFileds = null);
    }
}
