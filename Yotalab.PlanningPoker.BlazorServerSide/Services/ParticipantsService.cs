﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using Yotalab.PlanningPoker.Grains.Interfaces;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;
using Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services
{
  public class ParticipantsService
  {
    private readonly ILogger<ParticipantsService> logger;
    private readonly IClusterClient client;

    public ParticipantsService(ILogger<ParticipantsService> logger, IClusterClient client)
    {
      this.logger = logger;
      this.client = client;
    }

    public Task Vote(Guid sessionId, Guid participantId, Vote vote)
    {
      var participantGrain = this.client.GetGrain<IParticipantGrain>(participantId);
      return participantGrain.Vote(sessionId, vote);
    }

    public Task JoinAsync(Guid sessionId, Guid participantId)
    {
      var participantGrain = this.client.GetGrain<IParticipantGrain>(participantId);
      return participantGrain.Join(sessionId);
    }

    public Task LeaveAsync(Guid sessionId, Guid participantId)
    {
      var participantGrain = this.client.GetGrain<IParticipantGrain>(participantId);
      return participantGrain.Leave(sessionId);
    }

    public Task KickAsync(Guid sessionId, Guid participantId, Guid initiatorId)
    {
      var sessionGrain = this.client.GetGrain<ISessionGrain>(sessionId);
      return sessionGrain.Kick(participantId, initiatorId);
    }

    public Task<ParticipantInfo> GetInfoAsync(Guid participantId)
    {
      var participantGrain = this.client.GetGrain<IParticipantGrain>(participantId);
      return participantGrain.GetAsync();
    }

    public Task ChangeInfo(Guid participantId, string newName, string newAvatarUrl)
    {
      var participantGrain = this.client.GetGrain<IParticipantGrain>(participantId);
      return participantGrain.ChangeInfo(newName, newAvatarUrl);
    }

    public Task<StreamSubscriptionHandle<ParticipantChangedNotification>> SubscribeAsync(Func<ParticipantChangedNotification, Task> action) =>
        this.client.GetStreamProvider("SMS")
            .GetStream<ParticipantChangedNotification>(StreamId.Create(typeof(IParticipantGrain).FullName, Guid.Empty))
            .SubscribeAsync(new NotificationsObserver<ParticipantChangedNotification>(logger, action));
  }
}
