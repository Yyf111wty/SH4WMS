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
    public partial class jc_bidding_detailService : BaseService<jc_bidding_detail>, Ijc_bidding_detailService
    {
        
        public List<BiddingProjectDetailModel> Getlist(
		 Expression<Func<BiddingProjectDetailModel, bool>> whereExpression,
			 ref int intTotalCount,
			 int intPageIndex = 0,
			 int intPageSize = 20,
			 string strOrderByFileds = null)
		{

            return dbSession.Getjc_bidding_detailDal().Getlist(whereExpression, ref intTotalCount, intPageIndex, intPageSize, strOrderByFileds);



        }
		public List<BiddingProjectDetailModel> GetDetaillist(
		 Expression<Func<BiddingProjectDetailModel, bool>> whereExpression,
			 ref int intTotalCount,
			 int intPageIndex = 0,
			 int intPageSize = 20,
			 string strOrderByFileds = null)
		{

			return dbSession.Getjc_bidding_detailDal().GetDetaillist(whereExpression, ref intTotalCount, intPageIndex, intPageSize, strOrderByFileds);



		}
		public bool DeleteProject(string PROJECTCODE)
		{
			return dbSession.Getjc_bidding_detailDal().DeleteProject(PROJECTCODE);

		}
        public bool DBiNIT()
        {
            return dbSession.Getjc_bidding_detailDal().DBiNIT();

        }
        

    }
}
