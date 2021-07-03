﻿using Microsoft.AspNetCore.Mvc;
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
            TelegramUserData user = await context.TelegramUserData
                .FirstAsync(user => user.TelegramId == telegramId);

            if (user != null)
            {
                return Ok(JsonConvert.SerializeObject(new
                {
                    user.Stage
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
    }
}