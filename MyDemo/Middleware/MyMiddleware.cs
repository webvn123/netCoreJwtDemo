using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MyDemo.Dto;
using MyDemo.Entity;
using MyDemo.InterFace;
using NLog;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MyDemo.Middleware
{
    /// <summary>
    /// 测试中间件 
    /// </summary>
    public class MyMiddleware
    {
        private readonly RequestDelegate _next;

        public MyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IMethod method, IOptions<JWTConfig> options)
        {
            Untils.TokenUntil tokenUntil = new Untils.TokenUntil(options);

            string token = context.Request.Headers.FirstOrDefault(d => d.Key == "Authorization").ToString();
            Dictionary<string, string> cl = new Dictionary<string, string>();



            if (tokenUntil.ValidateToken(token, out cl))
            {
                context.Items.Add("id", cl[ClaimTypes.NameIdentifier]);
                context.Items.Add("userName", cl[ClaimTypes.Name]);

                await _next(context);
            }
            else
            {
                VimData vimData = new VimData
                {
                    Msg = "token过期或者无效",
                    Success = false,
                    Code = 401
                };
                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.WriteAsync(Untils.JSONUntil.SerializeJSON(vimData), Encoding.UTF8);
            }

        }
    }
}
