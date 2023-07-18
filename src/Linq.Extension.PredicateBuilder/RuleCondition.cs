using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Linq.Extension.PredicateBuilder
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RuleCondition
    {
        [EnumMember(Value = "And")]
        And,

        [EnumMember(Value = "OR")]
        OR,
    }
}
