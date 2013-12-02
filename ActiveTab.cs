using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace CustomerPortal.Web.HtmlHelpers
{
    public static class ActiveTab
    {
        public static MvcHtmlString MenuItem(this HtmlHelper helper, string linkText, string actionName, string controllerName, string id )
        {
            string currentControllerName = (string) helper.ViewContext.RouteData.Values["controller"];
            string currentActionName = (string)helper.ViewContext.RouteData.Values["action"];

            var builder = new TagBuilder("li");
            if(currentControllerName.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase)&& 
            currentActionName.Equals(actionName,StringComparison.InvariantCultureIgnoreCase))
                builder.AddCssClass("active");
            builder.InnerHtml = helper.ActionLink(linkText, actionName, controllerName, new {id},null).ToHtmlString();
            return MvcHtmlString.Create(builder.ToString(TagRenderMode.Normal));
        }
    }
}