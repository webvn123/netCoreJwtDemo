using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyDemo.Entity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace MyDemo.Untils
{
    public class TokenUntil
    {

        private IOptions<JWTConfig> _options;
        public TokenUntil(IOptions<JWTConfig> options)
        {
            _options = options;
        }

        /// <summary>
        /// 创建token
        /// </summary>
        ///  <param name="issuer">JWT的签发者</param>
        /// <param name="audience">接收JWT的一方</param>
        /// <param name="expires">什么时候过期</param>
        /// <param name="claims">自定义用户值</param>
        /// <param name="SecurityKey">密钥</param>
        /// <param name="Algorithms">SecurityAlgorithms.HmacSha256 加密方式</param>
        /// <returns></returns>
        public string CreateToken<T>(T user) where T : BaseUser
        {
            Claim[] claims = {
                new Claim(ClaimTypes.NameIdentifier, user.ID),
                new Claim(ClaimTypes.Name, user.UserName) };

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.SecurityKey));
            //证书
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            //令牌
            JwtSecurityToken token = new JwtSecurityToken(
                    issuer: _options.Value.Issuer,
                    audience: _options.Value.Audience,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(_options.Value.Expires),
                    //expires: DateTime.Now,
                    signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        /// <summary>
        /// 刷新token
        /// </summary>
        /// <returns></returns>
        public string RefreshToken(string Atoken)
        {
            //jwt 解密token
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.ReadJwtToken(Atoken);
            token.Payload[ClaimTypes.Name].ToString();
            return CreateToken(new BaseUser {
                ID = token.Payload[ClaimTypes.NameIdentifier].ToString(),
                UserName = token.Payload[ClaimTypes.Name].ToString()
            });
        }

        public string GetIDFromToken(string token)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(token);



            return jwtSecurityToken.Payload[ClaimTypes.NameIdentifier].ToString();
        }

        public bool ValidateToken(string token, out Dictionary<string, string> Clims)
        {
            Clims = new Dictionary<string, string>();
            try
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(token);
                if (jwtSecurityToken == null)
                {
                    return false;
                }

                var validationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.SecurityKey)),
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = true,//是否验证Issuer
                    ValidateAudience = true,//是否验证Audience
                    ValidateLifetime = true,//是否验证失效时间
                    ValidateIssuerSigningKey = true,//是否验证SecurityKey
                    ValidAudience = _options.Value.Audience,
                    ValidIssuer = _options.Value.Issuer
                };

                var claimsPrincipals = handler.ValidateToken(token, validationParameters, out SecurityToken securityToken);

                foreach(var item in claimsPrincipals.Claims)
                {
                    Clims.Add(item.Type, item.Value);
                }

            }
            catch(Exception e)
            {
                return false;
            }

            return true;
        }
    }
}
