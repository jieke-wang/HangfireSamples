using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cronos;

using Hangfire;
using Hangfire.Server;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MongoDB.Bson.IO;

namespace CoreWebApiSample.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HangfireJobWithPerformContextController : ControllerBase
    {
        // /api/hangfirejobwithperformcontext/addjob
        [HttpGet]
        public IActionResult AddJob()
        {
            //string cronExpression = "* 0/1 * * * ?";
            string cronExpression = "* 0/1 * * *";
            BackgroundJob.Enqueue<LongJob>(job => job.Run((PerformContext)null, cronExpression, 1, 2.0, true, "string", DateTime.Now));
            return Content("AddJob");
        }

        // /api/hangfirejobwithperformcontext/addrecurringjob
        [HttpGet]
        public IActionResult AddRecurringJob()
        {
            //string cronExpression = "* 0/1 * * * ?";
            string cronExpression = "* 0/1 * * *";
            RecurringJob.AddOrUpdate<LongJob2>("RecurringJobID", job => job.Run((PerformContext)null, cronExpression, 1, 2.0, true, "string", DateTime.Now), cronExpression);
            return Content("AddRecurringJob");
        }

        // /api/hangfirejobwithperformcontext/removerecurringjob
        [HttpGet]
        public IActionResult RemoveRecurringJob()
        {
            RecurringJob.RemoveIfExists("RecurringJobID");
            return Content("RemoveRecurringJob");
        }

        // /api/hangfirejobwithperformcontext/addrecurringobjectjob
        [HttpGet]
        public IActionResult AddRecurringObjectJob()
        {
            ObjectParam objectParam = new ObjectParam();
            RecurringJob.AddOrUpdate<ObjectJob>("ObjectJobID", job => job.Run((PerformContext)null, objectParam), objectParam.Cron);
            return Content("AddRecurringObjectJob");
        }
    }

    public class LongJob
    {
        public void Run(PerformContext context, string cronExp, params object[] arges)
        {
            Console.WriteLine(string.Join(Environment.NewLine, arges));

            //CronExpression cronExpression = CronExpression.Parse(cronExp, CronFormat.IncludeSeconds);
            CronExpression cronExpression = CronExpression.Parse(cronExp, CronFormat.Standard);

            DateTimeOffset nowTime = DateTimeOffset.Now;
            DateTimeOffset? nextTime = cronExpression.GetNextOccurrence(nowTime, TimeZoneInfo.Local);
            Console.WriteLine($"{nameof(nowTime)}: {nowTime}; {nameof(nextTime)}:{nextTime}");
            DateTimeOffset? next2Time = cronExpression.GetNextOccurrence(nextTime.Value, TimeZoneInfo.Local);
            Console.WriteLine($"{nameof(nowTime)}: {nowTime}; {nameof(nextTime)}:{nextTime}; {nameof(next2Time)}:{next2Time}");
            TimeSpan? cycleInterval = next2Time?.DateTime - nextTime?.DateTime;
            DateTime? currentCycleTime = nextTime?.DateTime - cycleInterval;
            Console.WriteLine($"{nameof(cycleInterval)}: {cycleInterval}; {nameof(currentCycleTime)}:{currentCycleTime}");
        }
    }

    public class LongJob2
    {
        public void Run(PerformContext context, string cronExp, params object[] arges)
        {
            Console.WriteLine("***********************************");
            Console.WriteLine(string.Join(Environment.NewLine, arges));

            //CronExpression cronExpression = CronExpression.Parse(cronExp, CronFormat.IncludeSeconds);
            CronExpression cronExpression = CronExpression.Parse(cronExp, CronFormat.Standard);

            DateTimeOffset nowTime = DateTimeOffset.Now;
            DateTimeOffset? nextTime = cronExpression.GetNextOccurrence(nowTime, TimeZoneInfo.Local);
            Console.WriteLine($"{nameof(nowTime)}: {nowTime}; {nameof(nextTime)}:{nextTime}");
            DateTimeOffset? next2Time = cronExpression.GetNextOccurrence(nextTime.Value, TimeZoneInfo.Local);
            Console.WriteLine($"{nameof(nowTime)}: {nowTime}; {nameof(nextTime)}:{nextTime}; {nameof(next2Time)}:{next2Time}");
            TimeSpan? cycleInterval = next2Time?.DateTime - nextTime?.DateTime;
            DateTime? currentCycleTime = nextTime?.DateTime - cycleInterval;
            Console.WriteLine($"{nameof(cycleInterval)}: {cycleInterval}; {nameof(currentCycleTime)}:{currentCycleTime}");
        }
    }

    public class ObjectJob
    {
        public void Run(PerformContext context, ObjectParam param)
        {
            Console.WriteLine("***********************************");
            Console.WriteLine($"{nameof(param)}: {Newtonsoft.Json.JsonConvert.SerializeObject(param)}");

            CronExpression cronExpression = CronExpression.Parse(param.Cron, CronFormat.Standard);
            DateTimeOffset nowTime = DateTimeOffset.Now;
            DateTimeOffset? nextTime = cronExpression.GetNextOccurrence(nowTime, TimeZoneInfo.Local);
            Console.WriteLine($"{nameof(nowTime)}: {nowTime}; {nameof(nextTime)}:{nextTime}");
            DateTimeOffset? next2Time = cronExpression.GetNextOccurrence(nextTime.Value, TimeZoneInfo.Local);
            Console.WriteLine($"{nameof(nowTime)}: {nowTime}; {nameof(nextTime)}:{nextTime}; {nameof(next2Time)}:{next2Time}");
            TimeSpan? cycleInterval = next2Time?.DateTime - nextTime?.DateTime;
            DateTime? currentCycleTime = nextTime?.DateTime - cycleInterval;
            Console.WriteLine($"{nameof(cycleInterval)}: {cycleInterval}; {nameof(currentCycleTime)}:{currentCycleTime}");
        }
    }

    public class ObjectParam
    {
        public string Cron { get; set; } = "* 0/1 * * *";
        public string JobID { get; set; } = "ObjectJob";
        public string Str { get; set; } = "任务描述";
        public bool B { get; set; } = true;
        public int I { get; set; } = int.MaxValue;
        public double D { get; set; } = double.NaN;
        public DateTime DT { get; set; } = DateTime.Now;
    }
}
