﻿using System;
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
	public partial class wms_bill_putinService : BaseService<wms_bill_putin>, Iwms_bill_putinService
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
		public List<wms_bill_putinbindingheader> Getlist(
		 Expression<Func<PutInModel, bool>> whereExpression,
			 ref int intTotalCount,
			 int intPageIndex = 0,
			 int intPageSize = 20,
			 string strOrderByFileds = null)
		{

			return dbSession.Getwms_bill_putinDal().Getlist(whereExpression, ref intTotalCount, intPageIndex, intPageSize, strOrderByFileds);



		}
		public string DeletePutIn(string ID)
		{

			return dbSession.Getwms_bill_putinDal().DeletePutIn(ID);
		}

		public List<wms_bill_putinbindingheader> GetHeaderPage(
			PutInModel filter,
			bool useFinishTimeRange,
			DateTime? createdFrom,
			DateTime? createdTo,
			ref int totalCount,
			int pageIndex = 1,
			int pageSize = 30)
		{
			return dbSession.Getwms_bill_putinDal().GetHeaderPage(
				filter, useFinishTimeRange, createdFrom, createdTo,
				ref totalCount, pageIndex, pageSize);
		}

	}
}
