using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services.FilesStoraging
{
  public class AvatarStorage
  {
    public const string DefaultAvatar = "img/default_avatar.png";

    public static readonly IReadOnlyList<string> DefaultAvatarCollection = new List<string>()
    {
        "img/img_avatar_1.png",
        "img/img_avatar_2.png",
        "img/img_avatar_3.png",
        "img/img_avatar_4.png",
        "img/img_avatar_5.png"
    };

    private readonly string webRootPath;
    private readonly string avatarsWebRelativePath;
    private readonly string transientAvatarWebRelativePath;

    public AvatarStorage(string webRootPath, string avatarsWebRelativePath, string transientAvatarsWebRelativePath)
    {
      this.webRootPath = webRootPath;
      this.avatarsWebRelativePath = avatarsWebRelativePath;
      this.transientAvatarWebRelativePath = transientAvatarsWebRelativePath;
      this.InitializeDirectories();
    }

    public void DeleteTransient(TransientAvatarDescriptor transientAvatar)
    {
      if (transientAvatar == null)
        throw new ArgumentNullException(nameof(transientAvatar));

      var fullPath = Path.Combine(this.webRootPath, transientAvatar.RelativeFileName);
      File.Delete(fullPath);
    }

    public void DeleteAvatar(AvatarDescriptor avatarDescriptor)
    {
      if (avatarDescriptor == null)
        throw new ArgumentNullException(nameof(avatarDescriptor));

      if (!DefaultAvatarCollection.Contains(avatarDescriptor.RelativeFileName))
      {
        var fullPath = Path.Combine(this.webRootPath, avatarDescriptor.RelativeFileName);
        if (File.Exists(fullPath))
          File.Delete(fullPath);
      }
    }

    public async Task<TransientAvatarDescriptor> WriteTransient(Stream stream, Guid participantId, string sourceFileName)
    {
      var fileName = $"{participantId}{Path.GetExtension(sourceFileName)}";
      var fullTransientAvatarPath = Path.Combine(this.webRootPath, this.transientAvatarWebRelativePath, fileName);
      await using FileStream fs = new(fullTransientAvatarPath, FileMode.Create);
      await stream.CopyToAsync(fs);

      var transientRelativeAvatarPath = Path.Combine(this.transientAvatarWebRelativePath, fileName);
      var relativeAvatarPath = Path.Combine(this.avatarsWebRelativePath, fileName);
      return new TransientAvatarDescriptor(transientRelativeAvatarPath, relativeAvatarPath);
    }

    public async Task CommitTransient(TransientAvatarDescriptor transientAvatar)
    {
      var fullTransientAvatarPath = Path.Combine(this.webRootPath, transientAvatar.RelativeFileName);
      if (!File.Exists(fullTransientAvatarPath))
        throw new InvalidOperationException($"Transient file '{transientAvatar.RelativeFileName} not found'");

      var fullAvatarPath = Path.Combine(this.webRootPath, transientAvatar.TargetRelativeFileName);
      using (FileStream transientStream = new(fullTransientAvatarPath, FileMode.Open))
      using (FileStream avatarStream = new(fullAvatarPath, FileMode.Create))
        await transientStream.CopyToAsync(avatarStream);

      File.Delete(fullTransientAvatarPath);
    }

    private void InitializeDirectories()
    {
      var avatarsDirectory = Path.Combine(this.webRootPath, this.avatarsWebRelativePath);
      if (!Directory.Exists(avatarsDirectory))
        Directory.CreateDirectory(avatarsDirectory);

      var transientAvatarsDirectory = Path.Combine(this.webRootPath, this.transientAvatarWebRelativePath);
      if (!Directory.Exists(transientAvatarsDirectory))
        Directory.CreateDirectory(transientAvatarsDirectory);
    }
  }
}
