using AOTWebApi.Controllers;
using Quartz;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
var mvcBuilder = builder.Services.AddControllers();
/*
 * ����Controller��Task<T> ����������AOT�е� ����
 */
mvcBuilder.AddJsonOptions(options => options.JsonSerializerOptions.AddContext<WeatherForecastJsonContext>());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Logging.AddLog4Net();
builder.Host.UseSerilog((ctx, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
        .Enrich.WithProperty("Environment", ctx.HostingEnvironment);

});
builder.Logging.AddSerilog();

// base configuration from appsettings.json
builder.Services.Configure<QuartzOptions>(x =>
{
    x.SchedulerId = "NativeAot";
    x.SchedulerName = "NativeAotScheduler";
});
/*
* ����Quartz
* job��ȡ��quartz_jobs.xml
*/
builder.Services.AddQuartz(qz =>
{
    // as of 3.3.2 this also injects scoped services (like EF DbContext) without problems
    qz.UseMicrosoftDependencyInjectionJobFactory();
    // or for scoped service support like EF Core DbContext
    // q.UseMicrosoftDependencyInjectionScopedJobFactory();
    // these are the defaults
    //qz.UseSimpleTypeLoader();
    //qz.UseInMemoryStore();
    qz.UseDefaultThreadPool(tp =>
    {
        tp.MaxConcurrency = 10;
    });
    //ʹ�������ļ�
    qz.UseXmlSchedulingConfiguration(x => x.Files = new string[] {
        "quartz_jobs.xml"
    });
    #region �ֶ����Job

    //qz.ScheduleJob<HelthCheckJob>(trigger =>
    //{
    //    //ÿ1����ִ��һ��
    //    string time = configuration.GetValue<string>("HelthCheckJobTriggerTime") ?? "0 0/1 * * * ?";

    //    trigger
    //     .WithIdentity("Tr_HelthCheckJob", "HelthCheckJobJ_Group")
    //     .WithDescription("���Job���״̬")
    //     .WithCronSchedule(time)
    //     .UsingJobData("trig_taskName", "HelthCheckJob")
    //     .UsingJobData("trig_taskNameCN", "���Job���״̬")
    //     .UsingJobData("testRecoveryData", "���Ը���Trigger")
    //     .StartNow();
    //}, jobdtl =>
    //{
    //    jobdtl
    //     .WithIdentity("HelthCheckJob", "HelthCheckJobJ_Group")
    //     .UsingJobData("taskName", "HelthCheckJob")
    //     .UsingJobData("taskNameCN", "���Job���״̬")
    //     .UsingJobData("testRecoveryData", "���Ը���Job");
    //});

    #endregion
});
builder.Services.AddQuartzHostedService(qz =>
{
    // when shutting down we want jobs to complete gracefully
    qz.WaitForJobsToComplete = true;
});

var app = builder.Build();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
