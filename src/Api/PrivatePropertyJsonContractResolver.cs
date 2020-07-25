using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace UAlbion.Api
{
    public class PrivatePropertyJsonContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProperty = base.CreateProperty(member, memberSerialization);
            if (!jsonProperty.Writable && member is PropertyInfo property)
                jsonProperty.Writable = property.GetSetMethod(true) != null;
            return jsonProperty;
        }
    }
}
