using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Linq.Extension.PredicateBuilder
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OperatorComparer
    {
        [EnumMember(Value = "In")]
        In,

        [EnumMember(Value = "NotIn")]
        NotIn,

        [EnumMember(Value = "Equal")]
        Equal,

        [EnumMember(Value = "NotEqual")]
        NotEqual,

        [EnumMember(Value = "Less")]
        Less,

        [EnumMember(Value = "LessOrEqual")]
        LessOrEqual,

        [EnumMember(Value = "Greater")]
        Greater,

        [EnumMember(Value = "GreaterOrEqual")]
        GreaterOrEqual,

        [EnumMember(Value = "BeginsWith")]
        BeginsWith,

        [EnumMember(Value = "DoesNotBeginWith")]
        DoesNotBeginWith,

        [EnumMember(Value = "EndsWith")]
        EndsWith,

        [EnumMember(Value = "DoesNotEndWith")]
        DoesNotEndWith,

        [EnumMember(Value = "Contains")]
        Contains,

        [EnumMember(Value = "DoesNotContain")]
        DoesNotContain,

        [EnumMember(Value = "Between")]
        Between,

        [EnumMember(Value = "NotBetween")]
        NotBetween,

        [EnumMember(Value = "IsEmpty")]
        IsEmpty,

        [EnumMember(Value = "IsNotEmpty")]
        IsNotEmpty,

        [EnumMember(Value = "IsNull")]
        IsNull,

        [EnumMember(Value = "IsNotNull")]
        IsNotNull
    }
}
