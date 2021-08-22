using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;

namespace SNSSQSWorkerServiceSubscriber
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    services.AddHostedService<SQSWorker>();
                    services.AddHostedService<S3Worker>();
                    services.AddAWSService<IAmazonSQS>(configuration.GetAWSOptions());
                });
    }
}
