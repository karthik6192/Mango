using Mango.Services.RewardAPI.Messaging;

namespace Mango.Services.RewardAPI.Extension
{
    public static class ApplicationBuilderExtensions
    {
        private static IAzureServiceBusConsumer serviceBusConsumer { get; set; }
        public static IApplicationBuilder UseAzureServiceBusConsumer(this IApplicationBuilder app)
        {
            serviceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumer>();
            var hostApplicationLife = app.ApplicationServices.GetService<IHostApplicationLifetime>();

            hostApplicationLife.ApplicationStarted.Register(OnStart);

            hostApplicationLife.ApplicationStopped.Register(OnStop);

            return app;
        }


        private static void OnStart()
        {
            serviceBusConsumer.Start();
        }
        private static void OnStop()
        {
            serviceBusConsumer.Stop();
        }

        
    }
}
