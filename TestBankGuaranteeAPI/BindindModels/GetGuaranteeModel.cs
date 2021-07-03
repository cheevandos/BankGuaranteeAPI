using System;

namespace TestBankGuaranteeAPI.BindindModels
{
    public class GetGuaranteeModel
    {
        public string GuaranteeType { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        public string Sum { get; set; }
        public string Fee { get; set; }
        public string DocNumber { get; set; }
    }
}
