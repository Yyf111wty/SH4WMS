﻿using System;
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

		/// <summary>
		/// 单头分页查询：默认只查单头表，物料/批次条件走 EXISTS，避免 JOIN 明细表造成行放大。
		/// </summary>
		List<wms_bill_putoutheader> GetHeaderPage(
			PutOutModel filter,
			bool useFinishTimeRange,
			DateTime? createdFrom,
			DateTime? createdTo,
			ref int intTotalCount,
			int intPageIndex = 1,
			int intPageSize = 30,
			string strOrderByFileds = null);

	}
}
