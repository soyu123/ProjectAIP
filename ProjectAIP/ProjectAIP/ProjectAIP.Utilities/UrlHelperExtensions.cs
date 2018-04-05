using Microsoft.AspNetCore.Mvc;

namespace ProjectAIP.Utilities
{
    public static class UrlHelperExtensions
    {
        public static string EmailConfirmationLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return urlHelper.Action(
                action: "ConfirmEmail", //nameof(AccountController.ConfirmEmail),
                controller: "Account",
                values: new { userId, code },
                protocol: scheme);
        }

        public static string ResetPasswordCallbackLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return urlHelper.Action(
                action: "ResetPassword", //nameof(AccountController.ResetPassword),
                controller: "Account",
                values: new { userId, code },
                protocol: scheme);
        }
    }
}
