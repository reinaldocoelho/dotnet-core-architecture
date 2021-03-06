﻿using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using MvcClient.Services;

namespace MvcClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication("Cookies")
                
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/account/login";
                    options.AccessDeniedPath = "/account/denied";
                })
                
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";

                    options.Authority = "https://localhost:44373";
                    options.RequireHttpsMetadata = true;
                    options.ClientId = "taskmvc";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code id_token";

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("resourcesapi");
                    options.Scope.Add("address");
                    options.Scope.Add("website");
                    options.Scope.Add("roles");
                    options.Scope.Add("subscriptionlevel");
                    options.Scope.Add("country");
                    options.Scope.Add("offline_access");

                    options.Events = new OpenIdConnectEvents
                    {
                        // Evento que ocorre após o token ser validado
                        OnTokenValidated = context =>
                        {
                            var identity = context.Principal.Identity as ClaimsIdentity;
                            var subjectClaim = identity?.Claims.FirstOrDefault(c => c.Type == "sub");

                            var newClaimsIdentity = new ClaimsIdentity(context.Scheme.Name, "given_name", "role");
                            newClaimsIdentity.AddClaim(subjectClaim);

                            context.Principal = new ClaimsPrincipal(newClaimsIdentity);

                            return Task.FromResult(0);
                        },

                        OnUserInformationReceived = context =>
                        {
                            // ADICIONANDO TODAS AS CLAIMS DO USUARIO 

                            var newClaimsIdentity = new ClaimsIdentity(context.Scheme.Name, "given_name", "role");
                            foreach (var item in context.User)
                                newClaimsIdentity.AddClaim(new Claim(item.Key, item.Value.ToString()));

                            context.Principal = new ClaimsPrincipal(newClaimsIdentity);

                            return Task.FromResult(0);
                        }

                    };
                });

            services.AddAuthorization(options =>
            {
                // Exemplo de uma policy simples

                options.AddPolicy(
                    "CanAccessPayArea",
                    builder =>
                    {
                        builder.RequireAuthenticatedUser();
                        builder.RequireClaim("subscriptionlevel", "Subscriber");
                    });

            });

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register an IResourcesHttpClient
            services.AddScoped<IResourcesHttpClient, ResourcesHttpClient>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }


            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}
