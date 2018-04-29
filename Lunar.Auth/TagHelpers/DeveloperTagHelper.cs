using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Lunar.Auth.TagHelpers
{
    public class DeveloperTagHelper : TagHelper
    {
        private IHttpContextAccessor HttpContextAccessor { get; }

        public DeveloperTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            this.HttpContextAccessor = httpContextAccessor;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var user = this.HttpContextAccessor.HttpContext.User;
            if (null == user || !user.IsInRole("Developer"))
            {
                output.SuppressOutput();
                return;
            }

            output.TagName = "";
        }

    }
}
