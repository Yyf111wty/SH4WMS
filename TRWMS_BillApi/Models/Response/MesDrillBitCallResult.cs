namespace TRWMS_BillApi.Models.Response
{
    public class MesDrillBitCallResult
    {
        public string RequestUrl { get; set; }

        public string RequestJson { get; set; }

        public string ResponseJson { get; set; }

        public int? HttpStatusCode { get; set; }

        public MesIssueDrillBitResponse Response { get; set; }

        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public int DurationMs { get; set; }
    }
}
