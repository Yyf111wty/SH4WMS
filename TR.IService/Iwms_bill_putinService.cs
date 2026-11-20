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
	public partial interface Iwms_bill_putinService : IBaseService<wms_bill_putin>
	{
		List<wms_bill_putinbindingheader> Getlist(
		 Expression<Func<PutInModel, bool>> whereExpression,
			 ref int intTotalCount,
			 int intPageIndex = 0,
			 int intPageSize = 20,
			 string strOrderByFileds = null);

		List<wms_bill_putinbindingheader> GetHeaderPage(
			PutInModel filter,
			bool useFinishTimeRange,
			DateTime? createdFrom,
			DateTime? createdTo,
			ref int totalCount,
			int pageIndex = 1,
			int pageSize = 30);

		string DeletePutIn(string ID);
	}
}
