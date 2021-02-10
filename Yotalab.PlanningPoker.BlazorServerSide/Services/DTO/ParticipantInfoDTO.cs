using System;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services.DTO
{
  public class ParticipantInfoDTO
  {
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string AvatarUrl { get; set; }

    public Vote Vote { get; set; }

    public ParticipantInfo ToInfo() => new ParticipantInfo()
    {
      Id = this.Id,
      Name = this.Name,
      AvatarUrl = this.AvatarUrl
    };
  }
}
