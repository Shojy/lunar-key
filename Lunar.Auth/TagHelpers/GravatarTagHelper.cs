using System.Security.Claims;
using System.Threading.Tasks;
using Lunar.Auth.Extensions;
using Lunar.Auth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Lunar.Auth.TagHelpers
{
    public class GravatarTagHelper : TagHelper
    {
        public int Size { get; set; } = 64;
        public ClaimsPrincipal User { get; set; }

        protected IHttpContextAccessor HttpContextAccessor { get; set; }
        protected UserManager<ApplicationUser> UserManager { get; set; }

        public GravatarTagHelper(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            this.HttpContextAccessor = httpContextAccessor;
            this.UserManager = userManager;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (null == User)
            {
                output.SuppressOutput();
                return;
            }
            output.TagName = "img";

            var user = UserManager.GetUserAsync(this.User).Result;
            var hash = user.Email.ToLower().Md5().ToLower();
            var url =
                $"https://www.gravatar.com/avatar/{hash}?s={this.Size}&d=https%3A%2F%2Fui-avatars.com%2Fapi%2F/{user.UserName}/{this.Size}";
            output.Attributes.SetAttribute("src", url);
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (null == User)
            {
                output.SuppressOutput();
                return;
            }
            output.TagName = "img";

            var user = await UserManager.GetUserAsync(this.User);
            var hash = user.Email.ToLower().Md5().ToLower();
            var url =
                $"https://www.gravatar.com/avatar/{hash}?s={this.Size}&d=https%3A%2F%2Fui-avatars.com%2Fapi%2F/{user.UserName}/{this.Size}";
            output.Attributes.SetAttribute("src", url);
        }
    }
}