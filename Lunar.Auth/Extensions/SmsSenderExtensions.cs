using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lunar.Auth.Services;

namespace Lunar.Auth.Extensions
{
    public static class SmsSenderExtensions
    {
        public static async Task SendAuthorizationCode(this ISmsSender sender, string number,  string code)
        {
            var message = $"Your Lunar login code is {code}.";
            await sender.SendSmsAsync(number, message);
        }
    }
}
