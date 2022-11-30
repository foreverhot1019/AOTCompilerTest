using System.Diagnostics.CodeAnalysis;
using Quartz;
using Quartz.Impl.Matchers;

namespace AOTWebApi.QuartzJobs
{
    /// <summary>
    /// 检测Job存活状态
    /// PersistJobDataAfterExecution: 执行完Job后保存 JobDataMap 当中固定数据，以便任务在重复执行的时候具有相同的 JobDataMap
    /// DisallowConcurrentExecution：不能同时运行同一作业的多个实例
    /// </summary>
    [PersistJobDataAfterExecution, DisallowConcurrentExecution]
    internal class HelthCheckJob : IJob
    {
        ILogger<HelthCheckJob> _logger;
        IConfiguration _configuration;
        public HelthCheckJob(ILogger<HelthCheckJob> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [RequiresUnreferencedCode("Calls Microsoft.Extensions.Configuration.ConfigurationBinder.GetValue<T>(String)")]
        public async Task Execute(IJobExecutionContext context)
        {
            /*
             * context.MergedJobDataMap 合并 JobDetail和Trigger中的Job-Data-Map数据
             * TriggerDataMap 会覆盖Job中的Job-Data-Map数据
             */
            context.MergedJobDataMap.TryGetValue("taskName", out object _taskName);
            context.MergedJobDataMap.TryGetValue("taskNameCN", out object _taskNameCN);
            _taskName = _taskName ?? "HelthCheckJob";
            _taskNameCN = _taskNameCN ?? "检测Job存活状态";
            var log_Type = _configuration.GetValue<string>("Logging:Log_Type") ?? "serilog";
            switch (log_Type.ToLower())
            {
                case "log4net":
                    log4net.ThreadContext.Properties["taskName"] = _taskName;
                    log4net.ThreadContext.Properties["taskNameCN"] = _taskNameCN;
                    log4net.LogicalThreadContext.Properties["taskName"] = _taskName;
                    log4net.LogicalThreadContext.Properties["taskNameCN"] = _taskNameCN;
                    break;
                case "serilog":
                    Serilog.Context.LogContext.PushProperty("taskName", _taskName);
                    Serilog.Context.LogContext.PushProperty("taskNameCN", _taskNameCN);
                    break;
                default:
                    log4net.ThreadContext.Properties["taskName"] = _taskName;
                    log4net.ThreadContext.Properties["taskNameCN"] = _taskNameCN;
                    break;
            }

            _logger.LogInformation("---Log HelthCheckJob Start-------------------------");

            var ArrJobKey = await context.Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            foreach (var key in ArrJobKey)
            {
                var jobDetail = await context.Scheduler.GetJobDetail(key);
                var triggers = await context.Scheduler.GetTriggersOfJob(key);
                var logJobStr = "";
                foreach (var trig in triggers)
                {
                    var PreviousFireTimeUtc = trig.GetPreviousFireTimeUtc();
                    var NextFireTimeUtc = trig.GetNextFireTimeUtc();
                    var _PreviousFireTimeUtc = PreviousFireTimeUtc.HasValue ? PreviousFireTimeUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : "-";
                    var _NextFireTimeUtc = NextFireTimeUtc.HasValue ? NextFireTimeUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : "-";
                    logJobStr = $"Write Job Infomation \r\nTriggerName:{trig.Key.Name} JobName:{key.Name} JobType:{jobDetail.JobType.Name} \r\nPreviousFireTimeUtc:{_PreviousFireTimeUtc} NextFireTimeUtc:{_NextFireTimeUtc} \r\nJobDataMap:{System.Text.Json.JsonSerializer.Serialize(jobDetail.JobDataMap)}\r\nTriggerJobDataMap:{System.Text.Json.JsonSerializer.Serialize(trig.JobDataMap)}";

                    _logger.LogInformation(logJobStr);
                }
            }

            _logger.LogInformation("---Log HelthCheckJob End  -------------------------");
        }
    }
}
