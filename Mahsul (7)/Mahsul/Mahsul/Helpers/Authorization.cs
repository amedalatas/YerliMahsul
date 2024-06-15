using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class FarmerAuthorizationAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // Kullanıcı oturum açmış mı kontrol et
        if (!user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Kullanıcının rolünü kontrol et (örneğin, çiftçi)
        if (!user.IsInRole("Farmer"))
        {
            context.Result = new ForbidResult(); // Yetkisiz erişim
            return;
        }
    }
}