using System.Collections.Generic;

namespace TRWMS_BillApi.Models.Request
{
    public class MesIssueDrillBitRequest
    {
        public string FactoryInfo { get; set; }

        public string PalletQRCode { get; set; }

        public string MaterialCode { get; set; }

        public string CreateTime { get; set; }

        public List<MesDrillBitBoxInfo> BoxInfoList { get; set; }
    }

    public class MesDrillBitBoxInfo
    {
        public string QRCode { get; set; }

        public string Content { get; set; }
    }
}
