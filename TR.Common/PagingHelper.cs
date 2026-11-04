using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace TR.Common
{
    public class PagingHelper : Controller
    {
        public static Dictionary<string, object> GetPagingData<T>(List<T> list, int pageSize, int pageId) where T : class
        {
            Dictionary<string, object> json = new Dictionary<string, object>();
            json.Add("total", list.Count);
            json.Add("rows", list.Skip(pageSize * (pageId - 1)).Take(pageSize).ToList());
            return json;
        }

    }
}