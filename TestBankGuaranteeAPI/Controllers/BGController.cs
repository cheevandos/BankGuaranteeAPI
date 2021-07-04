using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using TestBankGuaranteeAPI.BindindModels;
using TestBankGuaranteeAPI.DatabaseModels;
using TestBankGuaranteeAPI.Enums;

namespace TestBankGuaranteeAPI.Controllers
{
    [ApiController]
    public class BGController : ControllerBase
    {
        private readonly BGDatabaseContext context;

        public BGController(BGDatabaseContext context)
        {
            this.context = context;
        }

        [Route("api/[controller]/bindwithinn")]
        [HttpPost]
        public async Task<IActionResult> BindWithInn([FromBody] InnBindModel innBindModel)
        {
            try
            {
                var user = await context.Users.FirstOrDefaultAsync(user =>
                user.Inn == innBindModel.Inn);

                if (user == null)
                {
                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = "NotFound"
                    }));
                }
                else
                {
                    var telegramData = await context.TelegramUserData.FirstAsync(tgData =>
                    tgData.TelegramId == innBindModel.TelegramId);

                    telegramData.UserId = user.UserId;
                    telegramData.Stage = Enum.GetName(UserStage.Registered);

                    await context.SaveChangesAsync();

                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = user.CompanyName
                    }));
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }

        [Route("api/[controller]/binduser")]
        [HttpPost]
        public async Task<IActionResult> BindUser([FromBody] BindModel bindModel)
        {
            try
            {
                var usersCount = await context.Users.CountAsync(user =>
                user.PhoneNumber == bindModel.PhoneNumber);

                if (usersCount == 0)
                {
                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = "NotFound"
                    }));
                }
                else if (usersCount == 1)
                {
                    var userToBind = await context.Users.FirstOrDefaultAsync(user =>
                    user.PhoneNumber == bindModel.PhoneNumber);

                    var telegramData = await context.TelegramUserData.FirstAsync(tgData =>
                    tgData.TelegramId == bindModel.TelegramId);

                    telegramData.UserId = userToBind.UserId;
                    telegramData.Stage = Enum.GetName(UserStage.Registered);

                    await context.SaveChangesAsync();

                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = userToBind.CompanyName
                    }));
                }
                else
                {
                    var telegramData = await context.TelegramUserData.FirstAsync(tgData =>
                    tgData.TelegramId == bindModel.TelegramId);

                    telegramData.Stage = Enum.GetName(UserStage.DefineInn);

                    await context.SaveChangesAsync();

                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = "DefineInn"
                    }));
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }

        [Route("api/[controller]/registeruser")]
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterModel registerModel)
        {
            try
            {
                await context.TelegramUserData.AddAsync(new TelegramUserData
                {
                    TelegramId = registerModel.TelegramId,
                    Stage = Enum.GetName(UserStage.Identification)
                });

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }

            return Ok();
        }

        [Route("api/[controller]/getuserstage")]
        [HttpGet]
        public async Task<IActionResult> GetUserStage(long telegramId)
        {
            TelegramUserData userData = await context.TelegramUserData
                .FirstOrDefaultAsync(user => user.TelegramId == telegramId);

            if (userData != null)
            {
                return Ok(JsonConvert.SerializeObject(new
                {
                    userData.Stage
                }));
            }
            else
            {
                return Ok(JsonConvert.SerializeObject(new
                {
                    Stage = Enum.GetName(UserStage.NotDefined)
                }));
            }
        }        
        
        [Route("tradezone/Supplier/bgV2/bgV2Step3")]
        [HttpGet]
        public async Task<IActionResult> SignDocument(string docId)
        {
            TelegramUserData userData = await context.TelegramUserData
                .FirstOrDefaultAsync(user => user.Link == docId);

            if (userData != null)
            {
                userData.Link += "(SIGNED)";

                await context.SaveChangesAsync();

                return new JsonResult("Документ успешно подписан!");
            }
            else
            {
                return new JsonResult("Ошибка! Документ не найден");
            }
        }

        [Route("api/[controller]/checkdocsign")]
        [HttpGet]
        public async Task<IActionResult> CheckSign(long telegramId)
        {
            TelegramUserData userData = await context.TelegramUserData
                .FirstOrDefaultAsync(user => user.TelegramId == telegramId);

            if (userData != null)
            {
                if (userData.Link.Contains("(SIGNED)"))
                {
                    userData.Stage = Enum.GetName(UserStage.Registered);

                    await context.SaveChangesAsync();

                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = "Success"
                    }));
                }
                else
                {
                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = "Error"
                    }));
                }
            }
            else
            {
                return Ok(JsonConvert.SerializeObject(new
                {
                    Result = "Error"
                }));
            }
        }

        [Route("api/[controller]/savenotificationnumber")]
        [HttpPost]
        public async Task<IActionResult> SaveNotificationNumber(
            [FromBody] NotificationModel notificationModel)
        {
            try
            {
                var notification = await context.Notifications.Join(
                    context.TelegramUserData,
                    notif => notif.UserId,
                    tgData => tgData.UserId,
                    (notif, tgData) => new
                    {
                        notif.Number,
                        notif.UserId
                    }).FirstOrDefaultAsync(joinedObj =>
                    joinedObj.Number == notificationModel.NotificationNumber);

                if (notification == null)
                {
                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = "NotFound"
                    }));
                }
                else
                {
                    var userData = await context.TelegramUserData.FirstAsync(tgData =>
                    tgData.UserId == notification.UserId);

                    userData.NotificationNumber = notification.Number;
                    userData.Stage = Enum.GetName(UserStage.ChooseGuarantee);

                    await context.SaveChangesAsync();

                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = "Success"
                    }));
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }

        [Route("api/[controller]/saveguaranteetype")]
        [HttpPost]
        public async Task<IActionResult> SaveNotificationNumber(
            [FromBody] GuaranteeTypeModel guaranteeModel)
        {
            try
            {
                var userData = await context.TelegramUserData.FirstAsync(tgData =>
                tgData.TelegramId == guaranteeModel.TelegramId);

                if (userData == null)
                {
                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = "Error"
                    }));
                }

                userData.GuaranteeType = guaranteeModel.GuaranteeType;
                userData.Stage = Enum.GetName(UserStage.DefineBeginDate);

                await context.SaveChangesAsync();

                return Ok(JsonConvert.SerializeObject(new
                {
                    Result = "Success"
                }));
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }

        [Route("api/[controller]/savebegindate")]
        [HttpPost]
        public async Task<IActionResult> SaveBeginDate([FromBody] DateModel dateModel)
        {
            try
            {
                var userData = await context.TelegramUserData.FirstAsync(tgData =>
                tgData.TelegramId == dateModel.TelegramId);

                if (userData == null)
                {
                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = "Error"
                    }));
                }

                UserStage userStage = (UserStage)Enum.Parse(typeof(UserStage), userData.Stage);

                if (userStage != UserStage.DefineBeginDate)
                {
                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = "Error"
                    }));
                } 

                userData.BeginDate = dateModel.Date;
                userData.Stage = Enum.GetName(UserStage.DefineEndDate);

                await context.SaveChangesAsync();

                return Ok(JsonConvert.SerializeObject(new
                {
                    Result = "Success"
                }));
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }

        [Route("api/[controller]/saveenddate")]
        [HttpPost]
        public async Task<IActionResult> SaveEndDate([FromBody] DateModel dateModel)
        {
            try
            {
                var userData = await context.TelegramUserData.FirstAsync(tgData =>
                tgData.TelegramId == dateModel.TelegramId);

                if (userData == null)
                {
                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = "Error"
                    }));
                }

                UserStage userStage = (UserStage)Enum.Parse(typeof(UserStage), userData.Stage);

                if (userStage != UserStage.DefineEndDate)
                {
                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = "Error"
                    }));
                } 

                userData.EndDate = dateModel.Date;
                userData.Stage = Enum.GetName(UserStage.DefineSum);

                await context.SaveChangesAsync();

                return Ok(JsonConvert.SerializeObject(new
                {
                    Result = "Success"
                }));
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }

        [Route("api/[controller]/savesumgetbg")]
        [HttpPost]
        public async Task<IActionResult> SaveSumGetBG([FromBody] SumModel sumModel)
        {
            try
            {
                var userData = await context.TelegramUserData.FirstAsync(tgData =>
                tgData.TelegramId == sumModel.TelegramId);

                if (userData == null)
                {
                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = "Error"
                    }));
                }

                UserStage userStage = (UserStage)Enum.Parse(typeof(UserStage), userData.Stage);

                if (userStage != UserStage.DefineSum)
                {
                    return Ok(JsonConvert.SerializeObject(new
                    {
                        Result = "Error"
                    }));
                } 

                userData.Sum = sumModel.Sum;
                userData.Stage = Enum.GetName(UserStage.CheckSign);
                int docNumber = (new Random()).Next(2000000, 3000000);
                userData.Link = docNumber.ToString();

                await context.SaveChangesAsync();

                decimal fee = Math.Round(userData.Sum.Value * 0.02m, 2);

                return Ok(JsonConvert.SerializeObject(new GetGuaranteeModel
                {
                    GuaranteeType = userData.GuaranteeType,
                    BeginDate = userData.BeginDate.Value.ToShortDateString(),
                    EndDate = userData.EndDate.Value.ToShortDateString(),
                    Sum = userData.Sum.Value.ToString(),
                    Fee = fee.ToString(),
                    DocNumber = docNumber.ToString()
                }));
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }
    }
}
