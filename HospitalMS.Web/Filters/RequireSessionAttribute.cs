using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HospitalMS.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireSessionAttribute : Attribute, IActionFilter
{
    private const string TokenSessionKey = "jwt_token";

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var token = context.HttpContext.Session.GetString(TokenSessionKey);
        if (string.IsNullOrEmpty(token))
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
