using System;
using System.Collections.Generic;

#nullable disable

namespace TestBankGuaranteeAPI.DatabaseModels
{
    public partial class Notification
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public long UserId { get; set; }
    }
}
