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
    public partial interface Ijc_bidding_detailDal : IBaseDal<jc_bidding_detail>
    {
		List<BiddingProjectDetailModel> Getlist(
		  Expression<Func<BiddingProjectDetailModel, bool>> whereExpression,
			  ref int intTotalCount,
			  int intPageIndex = 0,
			  int intPageSize = 20,
			  string strOrderByFileds = null);
		List<BiddingProjectDetailModel> GetDetaillist(
	  Expression<Func<BiddingProjectDetailModel, bool>> whereExpression,
		  ref int intTotalCount,
		  int intPageIndex = 0,
		  int intPageSize = 20,
		  string strOrderByFileds = null);
		bool DeleteProject(string PROJECTCODE);
        bool DBiNIT();
    }
}
