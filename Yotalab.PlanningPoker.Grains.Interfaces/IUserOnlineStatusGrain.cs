using System.Threading.Tasks;
using Orleans;

namespace Yotalab.PlanningPoker.Grains.Interfaces
{
  public interface IUserOnlineStatusGrain : IGrainWithGuidKey
  {
    Task Online(string clientId);

    Task Offline(string clientId);

    Task<bool> IsOnline();
  }
}
