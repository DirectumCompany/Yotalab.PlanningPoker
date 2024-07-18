using System;
using System.Buffers;
using System.Collections.Generic;
using Orleans.Serialization.Buffers;
using Orleans.Serialization.Serializers;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Serializers
{
  /// <summary>
  /// Заглушка сериалайзера для базового класса бюллетени.
  /// </summary>
  public sealed class BulletinSerializer : IBaseCodec<HashSet<BulletinItem>>
  {
    public void Serialize<TBufferWriter>(ref Writer<TBufferWriter> writer, HashSet<BulletinItem> value)
      where TBufferWriter : IBufferWriter<byte>
    {
      throw new NotImplementedException();
    }

    public void Deserialize<TInput>(ref Reader<TInput> reader, HashSet<BulletinItem> value)
    {
      throw new NotImplementedException();
    }
  }
}
