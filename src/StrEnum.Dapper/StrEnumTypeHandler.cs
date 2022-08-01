using System.Data;
using Dapper;

namespace StrEnum.Dapper
{
    public class StrEnumTypeHandler<TEnum>: SqlMapper.TypeHandler<TEnum> where TEnum : StringEnum<TEnum>, new()
    {
        public override void SetValue(IDbDataParameter parameter, TEnum value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = value != null ? (string)value : null;
        }

        public override TEnum Parse(object value)
        {
            var stringValue = value?.ToString();

            if (stringValue == null)
                return null;
            
            return StringEnum<TEnum>.Parse(stringValue, matchBy: MatchBy.ValueOnly);
        }
    }
}
