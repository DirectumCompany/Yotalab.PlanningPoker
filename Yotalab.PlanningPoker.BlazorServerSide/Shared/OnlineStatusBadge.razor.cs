using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Orleans.Streams;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications;

namespace Yotalab.PlanningPoker.BlazorServerSide.Shared
{
  public partial class OnlineStatusBadge : IAsyncDisposable
  {
    private StreamSubscriptionHandle<UserOnlineStatusChanged> userOnlineTrackingSubscription;

    private bool isOnline = false;

    [Inject]
    private UsersActivityService ActivityService { get; set; }

    [Parameter]
    public Size BadgeSize { get; set; } = Size.Medium;

    [Parameter]
    public RenderFragment BadgeContent { get; set; }

    [Parameter]
    public Guid ParticipantId { get; set; }

    protected override async Task OnInitializedAsync()
    {
      if (!Guid.Empty.Equals(this.ParticipantId))
      {
        // Обработка изменения состояния "в сети" для пользователя.
        this.userOnlineTrackingSubscription = await this.ActivityService.SubscribeAsync(this.ParticipantId,
          notification => this.InvokeAsync(() => this.HandleNotification(notification)));

        this.isOnline = await this.ActivityService.GetOnlineStatus(this.ParticipantId);
        this.StateHasChanged();
      }
    }

    private Task HandleNotification(UserOnlineStatusChanged notification)
    {
      if (this.ParticipantId == notification.UserId)
      {
        this.isOnline = notification.IsOnline;
        this.StateHasChanged();
      }

      return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
      try
      {
        await this.userOnlineTrackingSubscription?.UnsubscribeAsync();
      }
      catch
      {
        // Игнорируем.
      }
    }
  }
}
