using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TR.IDAL;
using TR.Model;
using TR.ORM.Model;

namespace TR.DAL
{
	public partial class wms_bill_putoutheaderDal : BaseDal<wms_bill_putoutheader>, Iwms_bill_putoutheaderDal
	{
      
        public List<wms_bill_putoutheader> Getlist(
		 Expression<Func<PutOutModel, bool>> whereExpression,		 
			 ref int intTotalCount,
			 int intPageIndex = 0,
			 int intPageSize = 20,
			 string strOrderByFileds = null)
		{
			//b.ITEMCODE, b.LOT
			//(" SELECT  a.ID,a.SOURCEORDER,a.QUESTFLAG,a.CONTAINER,a.TASKTYPE,a.CONTAINERTYPE,a.LOCATIONCODE,a.TARGET,a.STATE,a.REMARK,a.QUESTTIME,a.FINISHTIME,a.ENABLED,a.CREATOR, a.CREATETIME, a.LASTEDITOR, a.LASTEDITTIME ,b.ITEMCODE, b.LOT  from wms_bill_putinbindingheader a     FORCE  INDEX(PRIMARY) left JOIN  wms_bill_putinbindingbody  b  on  a.ID = b.HEADERID   ")
			// 先按明细条件匹配单头 ID，再读取单头，保证一张单据仅返回一次。
			var matchingHeaders = db.SqlQueryable<PutOutModel>(" SELECT  a.*,b.ITEMCODE,b.LOT from  wms_bill_putoutheader   a     left JOIN  wms_bill_putoutbody  b   on  a.ID = b.HEADERID   ")
			   .WhereIF(whereExpression != null, whereExpression)
			   .Select(a => a.ID).ToSql();
			return db.Queryable<wms_bill_putoutheader>()
			   .Where("ID IN (" + matchingHeaders.Key + ")", matchingHeaders.Value.ToArray())
			   .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
			   .ToPageList(intPageIndex, intPageSize, ref intTotalCount)
				
			   ;
		}

	}
}
