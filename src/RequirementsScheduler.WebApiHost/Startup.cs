using System;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Quartz.Spi;
using RequirementsScheduler.BLL;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.Core.Service;
using RequirementsScheduler.DAL;
using RequirementsScheduler.DAL.Model;
using RequirementsScheduler.DAL.Repository;
using RequirementsScheduler.Library.Worker;
using RequirementsScheduler.WebApiHost.Extensions;
using RequirementsScheduler.WebApiHost.Identity;

namespace RequirementsScheduler.WebApiHost
{
    public class Startup
    {
        // secretKey contains a secret passphrase only your server knows
        private static readonly string secretKey = "501FC5DD-4268-4CC5-A791-44A6CEA41A43";
        private readonly IWebHostEnvironment _hostingEnvironment;

        private readonly SymmetricSecurityKey
            _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;

            _hostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureOAuth(services);

            // Add framework services.
            var builder = services.AddControllers().AddNewtonsoftJson();

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
            services.AddSingleton<IRepository<User, int>, UsersInMemoryRepository>();
            services.AddSingleton<IRepository<Experiment, Guid>, ExperimentsInMemoryRepository>();
            services.AddSingleton<IRepository<ExperimentResult, int>, ExperimentReportsInMemoryRepository>();
        }

        private void ConfigureRequirementsServices(IServiceCollection services)
        {
            services.AddSingleton<Database, Database>();
            //todo change to azure blob storage
            services.AddSingleton<IExperimentTestResultService, ExperimentTestResultFileService>();

            services.AddSingleton<IRepository<User, int>, Repository<User, int>>();
            services.AddSingleton<IRepository<Experiment, Guid>, Repository<Experiment, Guid>>();
            services
                .AddSingleton<IRepository<ExperimentResult, int>, Repository<ExperimentResult, int>
                >();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseMiddleware<CustomExceptionHandlerMiddleware>();

            if (_hostingEnvironment.IsDevelopment())
                app.UseDeveloperExceptionPage();
            //app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
            //{
            //    HotModuleReplacement = true
            //});
            else
                app.UseExceptionHandler("/Home/Error");

            app.UseStaticFiles();

            app.UseRouting();

            // Add JWT generation endpoint:
            var options = new TokenProviderOptions
            {
                Path = "/api/token",
                Audience = "ExampleAudience",
                Issuer = "RequirementsScheduler",
                Expiration = TimeSpan.FromDays(1),
                SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
            };

            app.UseMiddleware<TokenProviderMiddleware>(Options.Create(options));

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseQuartz(c =>
                c.AddJob<ExperimentWorker>("experimentJob", "experimentGroup",
                    (int) TimeSpan.FromMinutes(1).TotalSeconds));
        }

        private void ConfigureOAuth(IServiceCollection services)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match!
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

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
                            // Console.WriteLine("OnAuthenticationFailed: " +
                            //                   context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            // Console.WriteLine("OnTokenValidated: " +
                            //                   context.SecurityToken);
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