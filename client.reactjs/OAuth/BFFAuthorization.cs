//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Microsoft.Extensions.Primitives;

//namespace client.reactjs.OAuth
//{
//    public class BffAuthorizationAttribute : AuthorizeAttribute, IAuthorizationFilter
//    {
//        public void OnAuthorization(AuthorizationFilterContext context)
//        {
//            if (!ValidateToken(context.HttpContext.Request.Headers["token"]))
//            {
//                context.Result = new UnauthorizedResult();
//            }
//        }

//        private bool ValidateToken(StringValues stringValues)
//        {

//            // send the reqyest to connect/useinfo
//            return true;
//            //throw new NotImplementedException();
//        }
//    }
//}
