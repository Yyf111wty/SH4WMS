using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TR.IService;
using TR.Model;
using TR.ORM.Model;

namespace TR.Service
{
    public partial class wms_bill_inventorybookService : BaseService<wms_bill_inventorybook>, Iwms_bill_inventorybookService
    {
        //public List<InventorystatisticsModel> GetInventoryList(
        // Expression<Func<InventorystatisticsModel, bool>> whereExpression,
        //     ref int intTotalCount,
        //     int intPageIndex = 0,
        //     int intPageSize = 20,
        //     string strOrderByFileds = null)
        //{

        //    return dbSession.Getwms_bill_inventorybookDal().GetInventoryList(whereExpression, ref intTotalCount, intPageIndex, intPageSize, strOrderByFileds);



        //}

        public List<InventorystatisticsModel> Getlist(
		 Expression<Func<InventorystatisticsModel, bool>> whereExpression,
			 ref int intTotalCount,
			 int intPageIndex = 0,
			 int intPageSize = 20,
			 string strOrderByFileds = null)
		{

            return dbSession.Getwms_bill_inventorybookDal().Getlist(whereExpression, ref intTotalCount, intPageIndex, intPageSize, strOrderByFileds);



        }
        public List<InventoryGGModel> GetGGlist(
      string sourceOrder,
          ref int intTotalCount,
          int intPageIndex = 0,
          int intPageSize = 20,
          string strOrderByFileds = null,
          InventoryGGModel summary = null)
        {

            return dbSession.Getwms_bill_inventorybookDal().GetGGlist(sourceOrder, ref intTotalCount, intPageIndex, intPageSize, strOrderByFileds, summary);



        }
        public List<InventoryKCModel> GetKClist(
   Expression<Func<InventoryKCModel, bool>> whereExpression,
       ref int intTotalCount,
       int intPageIndex = 0,
       int intPageSize = 20,
       string strOrderByFileds = null)
        {

            return dbSession.Getwms_bill_inventorybookDal().GetKClist(whereExpression, ref intTotalCount, intPageIndex, intPageSize, strOrderByFileds);



        }
    }
}
