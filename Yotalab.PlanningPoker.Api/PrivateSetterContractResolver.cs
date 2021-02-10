using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Yotalab.PlanningPoker.Hosting
{
  public class PrivateSetterContractResolver : CamelCasePropertyNamesContractResolver
  {
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
      var jsonProperty = base.CreateProperty(member, memberSerialization);
      if (!jsonProperty.Writable)
      {
        if (member is PropertyInfo propertyInfo)
        {
          jsonProperty.Writable = propertyInfo.GetSetMethod(true) != null;
        }
      }

      return jsonProperty;
    }
  }
}
