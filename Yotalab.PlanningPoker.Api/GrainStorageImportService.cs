using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans;
using Yotalab.PlanningPoker.Grains;
using Yotalab.PlanningPoker.Grains.Interfaces;
using Yotalab.PlanningPoker.Hosting;

namespace Yotalab.PlanningPoker.Api
{
  public class GrainStorageImportService : IHostedService
  {
    private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
    {
      ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
      ContractResolver = new PrivateSetterContractResolver(),
      TypeNameHandling = TypeNameHandling.Auto
    };

    private readonly IHostApplicationLifetime lifetime;
    private readonly IConfiguration configuration;
    private readonly ILogger<GrainStorageExportService> logger;
    private readonly IGrainFactory grainFactory;

    public GrainStorageImportService(
      IHostApplicationLifetime lifetime,
      IConfiguration configuration,
      ILogger<GrainStorageExportService> logger,
      IGrainFactory grainFactory)
    {
      this.lifetime = lifetime;
      this.configuration = configuration;
      this.logger = logger;
      this.grainFactory = grainFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      return this.Import(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      return Task.CompletedTask;
    }

    public async Task Import(CancellationToken cancellationToken)
    {
      var importFolder = this.configuration.GetValue("Orleans:GrainStorageImportFolder", Environment.CurrentDirectory);
      var sessionsInputPath = Path.Combine(importFolder, "sessions");
      var participantsInputPath = Path.Combine(importFolder, "participants");

      var sessionsJson = await File.ReadAllTextAsync(sessionsInputPath);
      var participantsJson = await File.ReadAllTextAsync(participantsInputPath);

      var sessionMap = JsonConvert.DeserializeObject<Dictionary<Guid, SessionGrainState>>(sessionsJson, JsonSettings);
      var participantMap = JsonConvert.DeserializeObject<Dictionary<Guid, ParticipantGrainState>>(participantsJson, JsonSettings);

      foreach (var session in sessionMap)
      {
        var sessionId = session.Key;
        var sessionState = session.Value;
        if (sessionState == null)
        {
          this.logger.LogInformation("Skip import session '{0}'. Reason: Deleted, state is null.", sessionId);
          continue;
        }

        if (sessionState.ModeratorIds?.Any() == false)
        {
          this.logger.LogInformation("Skip import session '{0}'. Reason: Moderators empty.", sessionId);
          continue;
        }

        var moderatorId = sessionState.ModeratorIds.First();
        var sessionGrain = this.grainFactory.GetGrain<ISessionGrain>(sessionId);
        var status = await sessionGrain.StatusAsync();
        if (status.IsInitialized)
        {
          this.logger.LogInformation("Skip import session '{0}'. Reason: Session already initialized.", sessionId);
          continue;
        }

        var participantGrain = this.grainFactory.GetGrain<IParticipantGrain>(moderatorId);

        await sessionGrain.CreateAsync(sessionState.Name, participantGrain, sessionState.AutoStop, sessionState.Bulletin);
        await participantGrain.Join(sessionId);
        if (sessionState.ModeratorIds.Count > 1)
        {
          for (int i = 1; i < sessionState.ModeratorIds.Count - 1; i++)
          {
            var satelliteModeratorId = sessionState.ModeratorIds.Take(new Range(i, i + 1)).First();
            participantGrain = this.grainFactory.GetGrain<IParticipantGrain>(satelliteModeratorId);
            await participantGrain.Join(sessionId);
          }
        }

        this.logger.LogInformation("Session '{SessionId}' {SessionName} imported.", sessionId, sessionState.Name);
      }

      foreach (var participant in participantMap)
      {
        var participantId = participant.Key;
        var participantState = participant.Value;

        var participantGrain = this.grainFactory.GetGrain<IParticipantGrain>(participantId);
        await participantGrain.ChangeInfo(participantState.Name, participantState.AvatarUrl);
        foreach (var joinedSession in participantState.SessionIds)
        {
          var sessionGrain = this.grainFactory.GetGrain<ISessionGrain>(joinedSession);
          var state = await sessionGrain.StatusAsync();
          if (!state.IsInitialized)
          {
            this.logger.LogInformation("Skip join to session '{0}'. Reason: Session not initialized.", joinedSession);
            continue;
          }

          await participantGrain.Join(joinedSession);
        }

        this.logger.LogInformation("Participant '{ParticipantId}' {ParticipantName} imported.", participantId, participantState.Name);
      }

      this.logger.LogInformation("Import finished.");
    }
  }
}
