using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreWebApiSample {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {

            services.AddHangfire (configuration => {
                configuration.SetDataCompatibilityLevel (CompatibilityLevel.Version_170);
                configuration.UseSimpleAssemblyNameTypeSerializer ();
                configuration.UseRecommendedSerializerSettings ();

                // mongodb://用户名:密码@IP或host:端口/数据库名
                configuration.UseMongoStorage ("mongodb://HangfireStorage:123456@192.168.199.117:27017/HangfireStorage", new MongoStorageOptions {
                    MigrationOptions = new MongoMigrationOptions {
                            MigrationStrategy = new MigrateMongoMigrationStrategy (),
                                BackupStrategy = new CollectionMongoBackupStrategy ()
                        },
                        Prefix = "hangfire.mongo",
                        CheckConnection = true,
                        QueuePollInterval = TimeSpan.FromSeconds (5),
                        InvisibilityTimeout = TimeSpan.FromSeconds (5),
                        DistributedLockLifetime = TimeSpan.FromSeconds (5),
                });
            });

            services.AddHangfireServer (options => {
                options.ServerName = "Hangfire.Mongo server 1";
                options.SchedulePollingInterval = TimeSpan.FromSeconds (5);
                options.ServerCheckInterval = TimeSpan.FromSeconds (5);
                options.HeartbeatInterval = TimeSpan.FromSeconds (5);
            });

            services.AddControllers ();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            }

            // app.UseHangfireServer ();
            app.UseHangfireDashboard ();

            app.UseHttpsRedirection ();

            app.UseRouting ();

            app.UseAuthorization ();

            app.UseEndpoints (endpoints => {
                endpoints.MapControllers ();
            });

            app.Run(async context => 
            {
                await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes("路由没有命中"));
            });
        }
    }
}