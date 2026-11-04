namespace TRWMS_BillApi.Models.Request
{
    /// <summary>
    /// 备刀机二维码批量更新请求体（与 WorkQRCodeBindingInfo 字段一致）。
    /// </summary>
    public class QrCodeInfoListRequest
    {
        public string[] QRCodes { get; set; }

        public string[] Contents { get; set; }
    }
}
