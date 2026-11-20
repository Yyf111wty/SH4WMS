﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TR.Model;
using TR.ORM.Model;

namespace TR.IService
{
	public partial interface Iwms_bill_putoutheaderService : IBaseService<wms_bill_putoutheader>
	{
		List<wms_bill_putoutheader> Getlist(
		 Expression<Func<PutOutModel, bool>> whereExpression,
			 ref int intTotalCount,
			 int intPageIndex = 0,
			 int intPageSize = 20,
			 string strOrderByFileds = null);

		List<wms_bill_putoutheader> GetHeaderPage(
			PutOutModel filter,
			bool useFinishTimeRange,
			DateTime? createdFrom,
			DateTime? createdTo,
			ref int totalCount,
			int pageIndex = 1,
			int pageSize = 30);
	}
}
