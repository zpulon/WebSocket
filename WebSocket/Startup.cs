using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Linq;

namespace WebSocket
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebScoket.Api", Version = "v1" });

                DirectoryInfo dirs = new DirectoryInfo(AppContext.BaseDirectory);
                FileInfo[] files = dirs.GetFiles("*.xml");
                foreach (var path in files)
                {
                    c.IncludeXmlComments(path.FullName);
                }
            });
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.MimeTypes =
                    ResponseCompressionDefaults.MimeTypes.Concat(
                        new[] { "image/svg+xml" });
            });
            #region 注册Redis
            //redis缓存
            var section = Configuration.GetSection("Redis:Default");
            //连接字符串
            var _connectionString = section.GetSection("Connection").Value;
            var _passWord = section.GetSection("PassWord").Value;
            //实例名称
            var _instanceName = section.GetSection("InstanceName").Value;
            //默认数据库 
            int _defaultDB = int.Parse(section.GetSection("DefaultDB").Value ?? "0");
            RedisHelper redisHelper = new RedisHelper(_connectionString, _passWord, _instanceName, _defaultDB);
            services.AddSingleton(redisHelper);

            #endregion

            services.AddTransient<IChatSessionService, ChatSessionService>();
            services.AddTransient<IJsonHelper, JsonHelper>();
            services.AddTransient<IAsyncJsonHelper, JsonHelper>();

            services.AddSocketManager();


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="serviceProvider"></param>
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            #region 压缩
            app.UseResponseCompression();
            #endregion
            //app.UseMiddleware<SocketsCommitFilterMiddleWare>(serviceProvider.GetService<RedisHelper>(), serviceProvider.GetService<IJsonHelper>());
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebSocket.Api v1"));
            //var webSocketOptions = new WebSocketOptions()
            //{
            //    KeepAliveInterval = TimeSpan.FromSeconds(120),  //向客户端发送“ping”帧的频率，以确保代理保持连接处于打开状态
            //};
            ////webSocketOptions.AllowedOrigins.Add("https://client.com");
            ////webSocketOptions.AllowedOrigins.Add("https://www.client.com");
            ////注册websocket
            app.UseWebSockets();
            app.MapSocket("/websocket/chat", serviceProvider.GetService<WebSocketMessageHandler>());
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
