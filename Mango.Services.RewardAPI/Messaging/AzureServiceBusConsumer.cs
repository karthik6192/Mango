using Azure.Messaging.ServiceBus;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Services;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.RewardAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {

        private readonly string serviceBusConnectionString;
        private readonly string orderCreatedTopic;
        private readonly string orderCreatedRewardSubscription;
    
        private readonly IConfiguration configuration;
        private readonly RewardService rewardService;
        private ServiceBusProcessor rewardProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration,
                                       RewardService rewardService)
        {
            this.configuration = configuration;
            this.rewardService = rewardService;
            serviceBusConnectionString = configuration.GetValue<string>("ServiceBusConnectionString");
            orderCreatedTopic = configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
            orderCreatedRewardSubscription = configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Rewards_Subscription");
            var client = new ServiceBusClient(serviceBusConnectionString);
            rewardProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedRewardSubscription);



        }

        public async Task Start()
        {
            rewardProcessor.ProcessMessageAsync += onNewOrderRequestRecieved;
            rewardProcessor.ProcessErrorAsync += ErrorHandler;
            await rewardProcessor.StartProcessingAsync();

        }

      

        public async Task Stop()
        {
            await rewardProcessor.StopProcessingAsync();
            await rewardProcessor.DisposeAsync();


        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task onNewOrderRequestRecieved(ProcessMessageEventArgs args)
        {
           // This is where you will receive message.

            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            RewardsMessage objMessage = JsonConvert.DeserializeObject<RewardsMessage>(body); 

            try
            {
                await rewardService.UpdateRewards(objMessage);

                await args.CompleteMessageAsync(args.Message);
            }

            catch (Exception ex) { throw; }
        }

     


    }
}
