using System.Data;
using Dapper;

namespace Konta.Shared.Extensions;

/// <summary>
/// Gestionnaires de types Dapper pour DateOnly et TimeOnly (compatibilité .NET 6+).
/// </summary>
public static class DapperExtensions
{
    public static void AddDateOnlyHandlers()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
    }
}

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        parameter.DbType = DbType.Date;
    }

    public override DateOnly Parse(object value)
    {
        if (value is DateTime dt) return DateOnly.FromDateTime(dt);
        if (value is DateOnly doVal) return doVal;
        return DateOnly.FromDateTime((DateTime)value);
    }
}

public class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.Value = DateTime.Today.Add(value.ToTimeSpan());
        parameter.DbType = DbType.Time;
    }

    public override TimeOnly Parse(object value)
    {
        if (value is TimeSpan ts) return TimeOnly.FromTimeSpan(ts);
        if (value is TimeOnly toVal) return toVal;
        return TimeOnly.FromTimeSpan((TimeSpan)value);
    }
}
