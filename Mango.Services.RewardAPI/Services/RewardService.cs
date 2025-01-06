using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mango.Services.RewardAPI.Services
{
    public class RewardService : IRewardService
    {
        private readonly DbContextOptions<AppDbContext> dbOptions;

        public RewardService(DbContextOptions<AppDbContext> dbOptions)
        {
            this.dbOptions = dbOptions;
        }
        public async Task UpdateRewards(RewardsMessage rewardsMessage)
        {
            try
            {
                Rewards rewards = new()
                {
                    OrderId = rewardsMessage.OrderId,
                    RewardsActivity = rewardsMessage.RewardsActivity,
                    UserId = rewardsMessage.UserId,
                    RewardsDate = DateTime.Now,

                };

                await using var _db = new AppDbContext(dbOptions);
                await _db.Rewards.AddAsync(rewards);
                await _db.SaveChangesAsync();

                
            }
            catch (Exception ex) { }
        }
    }
}
