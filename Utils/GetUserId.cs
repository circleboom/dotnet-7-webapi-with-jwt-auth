        using System.Security.Claims;
        using Microsoft.AspNetCore.Http;

        namespace JwtAuth
        {
            public static class Utils
            {
                public static string GetUserId(this HttpContext httpContext)
                {
                    return httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                }
            }
        }