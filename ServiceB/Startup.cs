using System;
using System.IO;
using System.Reflection;
using Common;
using Common.Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
namespace ServiceB
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        public IHostEnvironment HostingEnvironment { get; }
        public Startup(IHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // HostingEnvironment.EnvironmentName ͨ��������������
            Configuration = new ConfigurationBuilder()
              .SetBasePath(HostingEnvironment.ContentRootPath)
              .AddJsonFile("appsettings.json", true, true)
              .AddJsonFile($"appsettings.{HostingEnvironment.EnvironmentName}.json", true, true)
              .AddEnvironmentVariables()
              .Build();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",

                });
                //Ϊ Swagger JSON and UI����xml�ĵ�ע��·��
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddOptions();
            services.Configure<ConsulOption>(Configuration.GetSection("ConsulOption"));
            services.Configure<ConfigOptions>(Configuration.GetSection("ConfigOptions"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //�����м����������Swagger��ΪJSON�ս��
            app.UseSwagger();
            //�����м�������swagger-ui��ָ��Swagger JSON�ս��
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            var consulOption = app.ApplicationServices.GetService<IOptions<ConsulOption>>().Value;
            app.RegisterConsul(lifetime, consulOption);
        }
    }
}
