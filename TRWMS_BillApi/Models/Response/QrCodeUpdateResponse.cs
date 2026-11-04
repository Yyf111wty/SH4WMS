namespace TRWMS_BillApi.Models.Response
{
    /// <summary>
    /// 备刀机 ResponseModel 包装。
    /// </summary>
    public class QrCodeUpdateResponse
    {
        public bool Success { get; set; }

        public int Code { get; set; }

        public string Message { get; set; }

        public object Result { get; set; }
    }
}
