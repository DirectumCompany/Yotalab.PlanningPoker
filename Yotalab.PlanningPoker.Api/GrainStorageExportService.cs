using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Newtonsoft.Json;
using Yotalab.PlanningPoker.Grains;
using Yotalab.PlanningPoker.Hosting;

namespace Yotalab.PlanningPoker.Api
{
  public class GrainStorageExportService : IHostedService
  {
    private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
    {
      ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
      ContractResolver = new PrivateSetterContractResolver(),
      TypeNameHandling = TypeNameHandling.Auto
    };

    private readonly MySqlConnection connection;
    private readonly IHostApplicationLifetime lifetime;
    private readonly IConfiguration configuration;
    private readonly ILogger<GrainStorageExportService> logger;

    public GrainStorageExportService(
      MySqlConnection connection,
      IHostApplicationLifetime lifetime,
      IConfiguration configuration,
      ILogger<GrainStorageExportService> logger)
    {
      this.connection = connection;
      this.lifetime = lifetime;
      this.configuration = configuration;
      this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      await this.connection.OpenAsync();

      var sessionMap = new Dictionary<Guid, SessionGrainState>();
      var participantMap = new Dictionary<Guid, ParticipantGrainState>();

      using var command = new MySqlCommand("SELECT GrainIdN0, GrainIdN1, GrainTypeString, PayloadJson FROM orleansstorage;", connection);
      using var reader = await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        var grainIdN0 = reader.GetInt64("GrainIdN0");
        var grainIdN1 = reader.GetInt64("GrainIdN1");
        var grainTypeString = reader.GetValue("GrainTypeString");
        var payloadJson = Convert.IsDBNull(reader["PayloadJson"]) ? null : reader.GetString("PayloadJson");

        var grainGuidId = ToGuidKey(grainIdN0, grainIdN1);

        if ("Yotalab.PlanningPoker.Grains.ParticipantGrain,Yotalab.PlanningPoker.Grains.Participant".Equals(grainTypeString))
        {
          if (payloadJson == null)
          {
            this.logger.LogInformation("Participant '{ParticipantId}' deleted.", grainGuidId);
            participantMap.Add(grainGuidId, null);
          }
          else
          {
            ParticipantGrainState participant = DeserializeParticipant(payloadJson);

            participantMap.Add(grainGuidId, participant);
            this.logger.LogInformation("Export participant '{ParticipantId}' '{ParticipantName}'.", grainGuidId, participant.Name);
          }
        }
        else if ("Yotalab.PlanningPoker.Grains.SessionGrain,Yotalab.PlanningPoker.Grains.Session".Equals(grainTypeString))
        {
          if (payloadJson == null)
          {
            this.logger.LogInformation("Session '{SessionId}' deleted.", grainGuidId);
            sessionMap.Add(grainGuidId, null);
          }
          else
          {
            SessionGrainState session = DeserializeSession(payloadJson);

            sessionMap.Add(grainGuidId, session);
            this.logger.LogInformation("Export session '{SessionId}' '{SessionName}'.", grainGuidId, session.Name);
          }
        }
        else
          this.logger.LogInformation("Unknown grain '{GrainId}', type: {GrainType}.", grainGuidId, grainTypeString);
      }

      await ExportToOutput(sessionMap, participantMap);

      this.logger.LogInformation("Export finished.");
      this.lifetime.StopApplication();
    }

    private async Task ExportToOutput(Dictionary<Guid, SessionGrainState> sessionMap, Dictionary<Guid, ParticipantGrainState> participantMap)
    {
      var sessionsJson = JsonConvert.SerializeObject(sessionMap);
      var participantsJson = JsonConvert.SerializeObject(participantMap);

      var exportOutput = this.configuration.GetValue("Orleans:GrainStorageExportFolder", Environment.CurrentDirectory);
      var sessionsOutputPath = Path.Combine(exportOutput, "sessions");
      var participantsOutputPath = Path.Combine(exportOutput, "participants");
      await File.WriteAllTextAsync(sessionsOutputPath, sessionsJson);
      await File.WriteAllTextAsync(participantsOutputPath, participantsJson);
    }

    private static ParticipantGrainState DeserializeParticipant(string payloadJson)
    {
      var participant = JsonConvert.DeserializeObject<ParticipantGrainState>(payloadJson, JsonSettings);

      if (!string.IsNullOrEmpty(participant.AvatarUrl))
      {
        var queryIndex = participant.AvatarUrl.IndexOf("?");
        if (queryIndex > 0)
          participant.AvatarUrl = participant.AvatarUrl.Substring(0, queryIndex);
      }

      return participant;
    }

    private static SessionGrainState DeserializeSession(string payloadJson)
    {
      var session = JsonConvert.DeserializeObject<SessionGrainState>(payloadJson, JsonSettings);

      if (session.ModeratorId != Guid.Empty)
      {
        var moderatorIds = session.ModeratorIds ?? new HashSet<Guid>();
        moderatorIds.Add(session.ModeratorId);
        session.ModeratorId = Guid.Empty;
        session.ModeratorIds = moderatorIds;
      }

      return session;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      return Task.CompletedTask;
    }

    // см. https://github.com/dotnet/orleans/blob/54382a15b653f80784520c9055614cbf429a1b16/src/AdoNet/Orleans.Persistence.AdoNet/Storage/Provider/AdoGrainKey.cs#L122
    private static Guid ToGuidKey(long n0Key, long n1Key)
    {
      return new Guid((uint)(n0Key & 0xffffffff), (ushort)(n0Key >> 32), (ushort)(n0Key >> 48), (byte)n1Key, (byte)(n1Key >> 8), (byte)(n1Key >> 16), (byte)(n1Key >> 24), (byte)(n1Key >> 32), (byte)(n1Key >> 40), (byte)(n1Key >> 48), (byte)(n1Key >> 56));
    }
  }
}
