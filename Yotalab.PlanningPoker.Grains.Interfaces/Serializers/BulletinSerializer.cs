using System;
using System.Collections.Immutable;
using Orleans.CodeGeneration;
using Orleans.Serialization;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Serializers
{
  [Serializer(typeof(Bulletin))]
  internal class BulletinSerializer
  {
    [CopierMethod]
    public static object DeepCopier(object original, ICopyContext context)
    {
      // No deep copy required since Yotalab.PlanningPoker.Grains.Interfaces.Models.Bulletin is marked with the [Immutable] attribute.
      return original;
    }

    [SerializerMethod]
    public static void Serializer(object untypedInput, ISerializationContext context, Type expected)
    {
      var bulletin = (Bulletin)untypedInput;
      context.SerializeInner(bulletin.ToImmutableArray(), typeof(ImmutableArray<BulletinItem>));
    }

    [DeserializerMethod]
    public static object Deserializer(Type expected, IDeserializationContext context)
    {
      var bulletin = new Bulletin();
      context.RecordObject(bulletin);
      var bulletinItems = (ImmutableArray<BulletinItem>)context.DeserializeInner(typeof(ImmutableArray<BulletinItem>));
      if (!bulletinItems.IsDefaultOrEmpty)
      {
        foreach (var item in bulletinItems)
          bulletin.Add(item);
      }
      return bulletin;
    }
  }
}
