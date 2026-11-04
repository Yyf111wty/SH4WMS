namespace TRWMS_BillApi.Models.Response
{
    public class MesIssueDrillBitResponse
    {
        public bool IsSuccess { get; set; }

        public string ErrorMsg { get; set; }

        public string Userld { get; set; }

        public int ExceptionType { get; set; }

        public object Data { get; set; }

        public string Message { get; set; }
    }
}
