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
	public partial interface Iwms_bill_putoutheaderDal : IBaseDal<wms_bill_putoutheader>
	{
		List<wms_bill_putoutheader> Getlist(
		 Expression<Func<PutOutModel, bool>> whereExpression,
			 ref int intTotalCount,
			 int intPageIndex = 0,
			 int intPageSize = 20,
			 string strOrderByFileds = null);

	}
}
