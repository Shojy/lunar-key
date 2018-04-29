using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Lunar.Auth.TagHelpers
{
    public class AdministratorTagHelper : TagHelper
    {
        private IHttpContextAccessor HttpContextAccessor { get; }

        public AdministratorTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            this.HttpContextAccessor = httpContextAccessor;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var user = this.HttpContextAccessor.HttpContext.User;
            if (null == user || !user.IsInRole("Administrator"))
            {
                output.SuppressOutput();
                return;
            }

            output.TagName = "";
        }
    }
}
