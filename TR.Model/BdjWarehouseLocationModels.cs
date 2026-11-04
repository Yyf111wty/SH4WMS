using System;
using System.Collections.Generic;

namespace TR.Model
{
    public class BdjWarehouseLocationQueryModel
    {
        public int? warehouseId { get; set; }
        public string diameter { get; set; }
        public string qrCode { get; set; }
        public int? isExistBox { get; set; }
        public int? storey { get; set; }
        public int? col { get; set; }
        public int? grindCount { get; set; }
        public int? minRemain { get; set; }
        public int? maxRemain { get; set; }
        public DateTime? startTime { get; set; }
        public DateTime? endTime { get; set; }
    }

    public class BdjWarehouseLocationQueryRequest
    {
        public int? WarehouseId { get; set; }
        public string Diameter { get; set; }
        public string QRCode { get; set; }
        public int? IsExistBox { get; set; }
        public int? Storey { get; set; }
        public int? Col { get; set; }
        public int? GrindCount { get; set; }
        public int? MinRemain { get; set; }
        public int? MaxRemain { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class BdjApiResponse<T>
    {
        public bool Success { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public T Result { get; set; }
    }

    public class BdjPagedResult<T>
    {
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<T> Items { get; set; }
    }

    public class BdjWarehouseLocationDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BoxNo { get; set; }
        public int Storey { get; set; }
        public int Col { get; set; }
        public string QRCode { get; set; }
        public string Diameter { get; set; }
        public int GrindCount { get; set; }
        public int IsExistBox { get; set; }
        public string BoxStatusDescription { get; set; }
        public int Remain { get; set; }
        public DateTime? StoreTime { get; set; }
    }
}
