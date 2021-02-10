using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Yotalab.PlanningPoker.Grains.Interfaces;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.Api.Controllers
{
  // Пример контроллера для построение RESTful апи.
  [Route("api/[controller]")]
  [ApiController]
  public class SessionsController : ControllerBase
  {
    private readonly IGrainFactory grainFactory;

    public SessionsController(IGrainFactory grainFactory)
    {
      this.grainFactory = grainFactory;
    }

    public Task<SessionInfo> GetAsync(Guid sessionId)
    {
      var sessionGrain = this.grainFactory.GetGrain<ISessionGrain>(sessionId);
      return sessionGrain.StatusAsync();
    }

    public async Task<IEnumerable<SessionInfo>> ListAsync(Guid participantId)
    {
      var participant = this.grainFactory.GetGrain<IParticipantGrain>(participantId);
      var sessionsGrainList = await participant.Sessions();
      return await Task.WhenAll(sessionsGrainList.Select(sessionGrain => sessionGrain.StatusAsync()));
    }

    public Task JoinAsync(Guid sessionId)
    {
      var participantGrain = this.grainFactory.GetGrain<IParticipantGrain>(Guid.NewGuid());
      return participantGrain.Join(sessionId);
    }
  }
}
