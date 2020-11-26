using System;

using Hangfire;

using Microsoft.AspNetCore.Mvc;

namespace CoreWebApiSample.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class HangfireJobCRUDController : ControllerBase
    {
        /// <summary>
        /// api/HangfireJobCRUD/AddSimpleJob
        /// 添加简单任务
        /// </summary>
        /// <returns>任务ID</returns>
        public IActionResult AddSimpleJob()
        {
            string jobID = BackgroundJob.Enqueue(() => Console.WriteLine("Simple!"));
            return Ok(jobID);
        }

        /// <summary>
        /// api/HangfireJobCRUD/AddScheduleJob
        /// 添加延时任务
        /// </summary>
        /// <returns>任务ID</returns>
        public IActionResult AddScheduleJob()
        {
            string jobID = BackgroundJob.Schedule(() => Console.WriteLine("Schedule!"), TimeSpan.FromSeconds(10));
            return Ok(jobID);
        }

        /// <summary>
        /// api/HangfireJobCRUD/RequeueJob?jobID=
        /// 任务重新入队,任务不会出现在看板中
        /// </summary>
        /// <param name="jobID">任务ID,不带字符#</param>
        /// <returns>执行结果</returns>
        public IActionResult RequeueJob(string jobID)
        {
            bool result = BackgroundJob.Requeue(jobID);
            return Ok(result);
        }

        /// <summary>
        /// api/HangfireJobCRUD/ContinueWithJob
        /// 添加依赖任务
        /// </summary>
        /// <returns>任务ID</returns>
        public IActionResult ContinueWithJob()
        {
            string stepJobID1 = BackgroundJob.Enqueue(() => Console.WriteLine("Step1!"));
            string stepJobID2 = BackgroundJob.ContinueJobWith(stepJobID1, () => Console.WriteLine("Step2!"), JobContinuationOptions.OnlyOnSucceededState);
            string stepJobID3 = BackgroundJob.ContinueJobWith(stepJobID2, () => Console.WriteLine("Step3!"), JobContinuationOptions.OnAnyFinishedState);
            return new JsonResult(new { stepJobID1, stepJobID2, stepJobID3 });
        }

        /// <summary>
        /// api/HangfireJobCRUD/DeleteJob?jobID=
        /// 删除任务
        /// </summary>
        /// <param name="jobID">任务ID,不带字符#</param>
        /// <returns>执行结果</returns>
        public IActionResult DeleteJob(string jobID)
        {
            bool result = BackgroundJob.Delete(jobID);
            return Ok(result);
        }

        /// <summary>
        /// api/HangfireJobCRUD/AddOrUpdateRecurringJob
        /// 添加或更新定时任务
        /// </summary>
        public IActionResult AddOrUpdateRecurringJob()
        {
            RecurringJob.AddOrUpdate("JobID", () => new MyJob().JobBody(), "0/10 * * * * ?");
            return Ok();
        }

        /// <summary>
        /// api/HangfireJobCRUD/RemoveRecurringJobIfExists
        /// 删除定时任务
        /// </summary>
        public IActionResult RemoveRecurringJobIfExists()
        {
            RecurringJob.RemoveIfExists("JobID");
            return Ok();
        }

        /// <summary>
        /// api/HangfireJobCRUD/TriggerRecurringJob
        /// 触发定时任务
        /// </summary>
        public IActionResult TriggerRecurringJob()
        {
            RecurringJob.Trigger("JobID");
            return Ok();
        }
    }

    [Hangfire.AutomaticRetry(Attempts = 4, DelaysInSeconds = new[] { 1, 2, 3, 4 }, LogEvents = true)]
    public class MyJob
    {
        public void JobBody()
        {
            Console.WriteLine($"RecurringJob\t{DateTime.Now:yyyy/MM/dd HH:mm:ss.fffff}");
            // https://api.hangfire.io/html/T_Hangfire_Server_PerformContext.htm
            //Hangfire.Server.PerformContext
        }
    }
}