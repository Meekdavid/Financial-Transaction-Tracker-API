using MerchantTransactionCore.Helpers.Common;
using MerchantTransactionCore.Helpers.Dtos.Global;
using MerchantTransactionCore.Helpers.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Helpers.ActionFilters
{
    public class ValidationActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new ObjectResult(new FilterResponse()
                {
                    ResponseCode = Utils.StatusCode_BadRequest.ToString(),
                    ResponseDescription = context.ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary(kvp => kvp.Key, kvp => string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))).ToDictionaryString(),
                })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
            //base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            base.OnResultExecuted(context);
        }
    }
}
