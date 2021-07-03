using System;
using System.Collections.Generic;

#nullable disable

namespace TestBankGuaranteeAPI.DatabaseModels
{
    public partial class TelegramUserData
    {
        public long TelegramId { get; set; }
        public long? UserId { get; set; }
        public string Stage { get; set; }
        public string NotificationNumber { get; set; }
        public string GuaranteeType { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Sum { get; set; }
        public string Link { get; set; }
    }
}
