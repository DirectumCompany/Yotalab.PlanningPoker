using System;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services.FilesStoraging
{
  public class TransientAvatarDescriptor : AvatarDescriptor
  {
    public TransientAvatarDescriptor(string relativeFileName, string targetRelativeFileName)
      : base(relativeFileName)
    {
      this.TargetRelativeFileName = RemovePathQuery(
        ConvertToWebPathSeparator(
          targetRelativeFileName ?? throw new ArgumentNullException(nameof(targetRelativeFileName))));
    }

    public string TargetRelativeFileName { get; }
  }
}
