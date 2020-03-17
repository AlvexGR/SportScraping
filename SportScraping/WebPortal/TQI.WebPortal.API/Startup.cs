using System.Data;
using Dapper.FluentMap;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MySql.Data.MySqlClient;
using TQI.Infrastructure.Entity.Database;
using TQI.Infrastructure.Entity.Database.Mapping;
using TQI.Infrastructure.Entity.Database.Mapping.Metrics;
using TQI.WebPortal.Service.IServices;
using TQI.WebPortal.Service.Services;
using TQI.WebPortal.Service.UnitOfWork;

namespace TQI.WebPortal.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Allow headers
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddControllers();

            services.AddScoped<IMasterDataService, MasterDataService>();
            services.AddScoped<IMetricDataService, MetricDataService>();
            services.AddScoped<ISportService, SportService>();
            services.AddScoped<IScrapingService, ScrapingService>();
            services.AddScoped<ISimulationService, SimulationService>();

            //services.AddSingleton(Helper.GetLoggerConfig($@"{Constants.BaseLoggerPath}\WebPortal\webportal-.txt"));
            services.AddSingleton<IDbConnection, MySqlConnection>();
            services.AddSingleton<IWebPortalUnitOfWork, WebPortalUnitOfWork>(
                provider => new WebPortalUnitOfWork(
                    new DbConnectionString(Configuration.GetConnectionString("SportScrapingDb"))));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TQI.SportScraping", Version = "v1" });
            });

            DatabaseColumnsMapping();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowAll");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TQI.SportScraping");
            });
        }

        private static void DatabaseColumnsMapping()
        {
            FluentMapper.Initialize(config =>
            {
                config.AddMap(new MatchMap());
                config.AddMap(new TeamMap());
                config.AddMap(new PlayerMap());
                config.AddMap(new ScrapingInformationMap());
                config.AddMap(new MetricMap());
                config.AddMap(new PlayerHeadToHeadMap());
                config.AddMap(new PlayerOverUnderMap());
                config.AddMap(new SportMap());
                config.AddMap(new ProviderMap());
                config.AddMap(new TempTableToTestMap());
            });
        }
    }
}
