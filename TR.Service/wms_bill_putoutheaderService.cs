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
	public partial class wms_bill_putoutheaderService : BaseService<wms_bill_putoutheader>, Iwms_bill_putoutheaderService
	{
        
  //      public List<PutInModel> Getlist(
		// Expression<Func<PutInModel, bool>> whereExpression,
		//	 ref int intTotalCount,
		//	 int intPageIndex = 0,
		//	 int intPageSize = 20,
		//	 string strOrderByFileds = null)
		//{

  //          return dbSession.Getwms_bill_putinDal().Getlist(whereExpression, ref intTotalCount, intPageIndex, intPageSize, strOrderByFileds);



  //      }
		public List<wms_bill_putoutheader> Getlist(
		 Expression<Func<PutOutModel, bool>> whereExpression,
			 ref int intTotalCount,
			 int intPageIndex = 0,
			 int intPageSize = 20,
			 string strOrderByFileds = null)
		{

			return dbSession.Getwms_bill_putoutheaderDal().Getlist(whereExpression, ref intTotalCount, intPageIndex, intPageSize, strOrderByFileds);



		}
		

	}
}
