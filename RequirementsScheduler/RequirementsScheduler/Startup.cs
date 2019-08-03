using System;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Quartz.Spi;
using RequirementsScheduler.BLL;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.Core.Service;
using RequirementsScheduler2.Identity;
using RequirementsScheduler.Core.Worker;
using RequirementsScheduler.DAL;
using RequirementsScheduler.DAL.Repository;
using RequirementsScheduler.Extensions;
using RequirementsScheduler.Library.Worker;
using RequirementsScheduler.Middleware;
using RequirementsScheduler.Telemetry;

namespace RequirementsScheduler
{
    public class Startup
    {
        private IHostingEnvironment _hostingEnvironment;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            _hostingEnvironment = env;
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureOAuth(services);

            // Add framework services.
            var builder = services.AddMvc();
            services.AddNodeServices();

            services.AddApplicationInsightsTelemetry();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var physicalProvider = _hostingEnvironment.ContentRootFileProvider;

            services.AddSingleton(physicalProvider);

            services.AddAutoMapper(typeof(MappingProfile));

            Mapper.AssertConfigurationIsValid();

            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IExperimentsService, ExperimentsService>();
            services.AddSingleton<IReportsService, ReportsService>();

#if IN_MEMORY
            ConfigureInMemoryRepositories(services);
            services.AddSingleton<IExperimentTestResultService, ExperimentTestResultFileService>();
#else
            ConfigureRequirementsServices(services);
#endif

            services.AddTransient<IJobFactory, WorkerJobFactory>(provider => new WorkerJobFactory(provider));
            services.AddTransient<IExperimentPipeline, ExperimentPipeline>();
            services.AddTransient<IExperimentGenerator, ExperimentGenerator>();
            services.AddSingleton<IWorkerExperimentService, WorkerExperimentService>();
            services.AddSingleton<ExperimentWorker, ExperimentWorker>();

            services.Configure<DbSettings>(options =>
            {
                options.ConnectionString = Configuration["ConnectionStrings:RequirementsSchedulerDatabase"];
            });
        }

        private void ConfigureInMemoryRepositories(IServiceCollection services)
        {
            services.AddSingleton<IRepository<DAL.Model.User, int>, UsersInMemoryRepository>();
            services.AddSingleton<IRepository<DAL.Model.Experiment, Guid>, ExperimentsInMemoryRepository>();
            services.AddSingleton<IRepository<DAL.Model.ExperimentResult, int>, ExperimentReportsInMemoryRepository>();

        }

        private void ConfigureRequirementsServices(IServiceCollection services)
        {
            services.AddSingleton<Database, Database>();
            //todo change to azure blob storage
            services.AddSingleton<IExperimentTestResultService, ExperimentTestResultFileService>();

            services.AddSingleton<IRepository<DAL.Model.User, int>, Repository<DAL.Model.User, int>>();
            services.AddSingleton<IRepository<DAL.Model.Experiment, Guid>, Repository<DAL.Model.Experiment, Guid>>();
            services
                .AddSingleton<IRepository<DAL.Model.ExperimentResult, int>, Repository<DAL.Model.ExperimentResult, int>
                >();
        }

        // secretKey contains a secret passphrase only your server knows
        private static string secretKey = "501FC5DD-4268-4CC5-A791-44A6CEA41A43";
        private SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            loggerFactory.AddApplicationInsights(app.ApplicationServices);

            app.UseMiddleware<EnableRequestRewindMiddleware>();
            app.UseMiddleware<CustomExceptionHandlerMiddleware>();

            var initialzer =
                new RequestBodyTelemetryInitializer(app.ApplicationServices.GetService<IHttpContextAccessor>());
            var configuration = app.ApplicationServices.GetService<TelemetryConfiguration>();
            configuration.TelemetryInitializers.Add(initialzer);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                //{
                //    HotModuleReplacement = true
                //});
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            // Add JWT generation endpoint:
            var options = new TokenProviderOptions
            {
                Path = "/api/token",
                Audience = "ExampleAudience",
                Issuer = "RequirementsScheduler",
                Expiration = TimeSpan.FromDays(1),
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            };

            app.UseMiddleware<TokenProviderMiddleware>(Options.Create(options));
            app.UseQuartz(c => c.AddJob<ExperimentWorker>("experimentJob", "experimentGroup", (int) TimeSpan.FromMinutes(1).TotalSeconds));

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }

        private void ConfigureOAuth(IServiceCollection services)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match!
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuer = "RequirementsScheduler",

                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = "ExampleAudience",

                // Validate the token expiry
                ValidateLifetime = true,

                // If you want to allow a certain amount of clock drift, set that here:
                ClockSkew = TimeSpan.Zero
            };

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.ClaimsIssuer = "RequirementsScheduler";
                    options.TokenValidationParameters = tokenValidationParameters;
                    options.SaveToken = true;

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine("OnAuthenticationFailed: " +
                                              context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("OnTokenValidated: " +
                                              context.SecurityToken);
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                //options.DefaultPolicy = n
                //options.AddPolicy("ApiUser", policy => policy.RequireClaim(Constants.Strings.JwtClaimIdentifiers.Rol, Constants.Strings.JwtClaims.ApiAccess));
            });
        }
    }
}
