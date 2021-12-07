using System;
using System.Linq;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services.FilesStoraging
{
  public class AvatarDescriptor
  {
    public AvatarDescriptor(string relativeFileName)
    {
      this.RelativeFileName = RemovePathQuery(
        ConvertToWebPathSeparator(
          relativeFileName ?? throw new ArgumentNullException(nameof(relativeFileName))));
    }

    public string RelativeFileName { get; }

    protected static string ConvertToWebPathSeparator(string url)
    {
      return url.Replace('\\', '/');
    }

    protected static string RemovePathQuery(string url)
    {
      var queryIndex = url.IndexOf("?");
      return queryIndex > 0 ? url.Substring(0, queryIndex) : url;
    }

    public static string UniqueUrl(string avatarUrl)
    {
      return AvatarStorage.DefaultAvatarCollection.Contains(avatarUrl) ? avatarUrl : $"{avatarUrl}?{DateTime.UtcNow.Ticks}";
    }
  }
}
