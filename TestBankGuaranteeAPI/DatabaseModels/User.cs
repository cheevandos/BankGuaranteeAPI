using System;
using System.Collections.Generic;

#nullable disable

namespace TestBankGuaranteeAPI.DatabaseModels
{
    public partial class User
    {
        public long UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string Inn { get; set; }
        public string CompanyName { get; set; }
    }
}
