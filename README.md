# StrEnum.Dapper

Lets you use [StrEnum](https://github.com/StrEnum/StrEnum/) string enums with Dapper.

Supports Dapper 2.0.4+.

## Installation

Install [StrEnum.Dapper](https://www.nuget.org/packages/StrEnum.Dapper/) via the .NET CLI:

```
dotnet add package StrEnum.Dapper
```

## Usage

### Defining a string enum and an entity

```csharp
public class Sport: StringEnum<Sport>
{
    public static readonly Sport RoadCycling = Define("ROAD_CYCLING");
    public static readonly Sport MountainBiking = Define("MTB");
    public static readonly Sport TrailRunning = Define("TRAIL_RUNNING");
}

public class Race
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Sport Sport { get; set; }
}
```

### Initialization

Call `StrEnumDapper.UseStringEnums()` before the first use of Dapper:

```csharp
StrEnumDapper.UseStringEnums();
```

`UseStringEnums()` searches all *loaded* assemblies for string enums and registers each with Dapper. Since .NET loads assemblies lazily, make sure the assemblies containing your string enums are loaded before this call — either implicitly, by referencing a type from them, or explicitly:

```csharp
Assembly.Load(typeof(Sport).Assembly.GetName());
```

### Executing commands

```csharp
connection.Execute(@"INSERT Races(Id, Name, Sport) values (@id, @name, @sport)",
    new[]
    {
        new { id = Guid.NewGuid(), name = "Chornohora Sky Marathon", sport = Sport.TrailRunning  },
        new { id = Guid.NewGuid(), name = "Cape Town Cycle Tour",    sport = Sport.RoadCycling   },
        new { id = Guid.NewGuid(), name = "Cape Epic",               sport = Sport.MountainBiking }
    });
```

### Running queries

Pass string enums as query parameters and map them onto your entities:

```csharp
var trailRuns = connection.Query(
    @"SELECT Id, Name, Sport FROM Races WHERE Sport = @sport",
    new { sport = Sport.TrailRunning });
```

Dapper [doesn't yet support](https://github.com/DapperLib/Dapper/issues/1134) string enums in `IN` clauses, so this won't work:

```csharp
var cyclingRaces = connection.Query(
    @"SELECT Id, Name, Sport FROM Races WHERE Sport in @sports",
    new { sports = new[] { Sport.TrailRunning, Sport.MountainBiking } });
```

Cast each member to `string` as a workaround:

```csharp
var cyclingRaces = connection.Query(
    @"SELECT Id, Name, Sport FROM Races WHERE Sport in @sports",
    new { sports = new[] { (string)Sport.TrailRunning, (string)Sport.MountainBiking } });
```

## Postgres native enum columns

`StrEnum.Dapper` flattens `StringEnum<T>` to its underlying `string` value at the Dapper layer, which Postgres accepts straight into a `text` column. Native Postgres enum columns (`CREATE TYPE ... AS ENUM`) are a different story: Postgres has no implicit cast from `text` to a user-defined enum, so you'll see:

```
42804: column "sport" is of type sport but expression is of type text
```

`StrEnum.Dapper` does not currently bind the parameter to the enum's OID for you. Two practical workarounds:

* **Cast in SQL:** add `::your_enum` to the parameter site. Works against existing tables without ceremony.

  ```csharp
  connection.Execute(
      "INSERT INTO races (id, name, sport) VALUES (@id, @name, @sport::sport)",
      new { id = Guid.NewGuid(), name = "Chornohora Sky Marathon", sport = Sport.TrailRunning });
  ```

* **Bypass the Dapper handler for that parameter** with a hand-rolled `NpgsqlParameter` that sets `DataTypeName`:

  ```csharp
  var p = new DynamicParameters();
  p.Add("id", Guid.NewGuid());
  p.Add("name", "Chornohora Sky Marathon");
  p.Add(new NpgsqlParameter
  {
      ParameterName = "sport",
      DataTypeName = "sport",
      Value = (string)Sport.TrailRunning,
  });
  connection.Execute("INSERT INTO races (id, name, sport) VALUES (@id, @name, @sport)", p);
  ```

If you only need raw Npgsql wire support (no Dapper), use [StrEnum.Npgsql](https://github.com/StrEnum/StrEnum.Npgsql/) — `NpgsqlDataSourceBuilder.MapStringEnum<T>()` binds `Sport` instances directly to the enum OID. There's no first-class `StrEnum.Npgsql.Dapper` package today; if you'd find one useful, please open an issue.

## License

Copyright &copy; 2026 [Dmytro Khmara](https://dmytrokhmara.com).

StrEnum is licensed under the [MIT license](LICENSE.txt).
