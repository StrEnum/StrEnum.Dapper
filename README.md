# StrEnum.Dapper

Allows to use [StrEnum](https://github.com/StrEnum/StrEnum/) string enums with Dapper.

Supports Dapper v2.0.4+

## Installation

You can install [StrEnum.Dapper](https://www.nuget.org/packages/StrEnum.Dapper/) using the .NET CLI:

```
dotnet add package StrEnum.Dapper
```

## Usage

Define a string enum and an entity that uses it:

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

Invoke the `StrEnumDapper.UseStringEnums` method before the first use of Dapper:

```csharp
StrEnumDapper.UseStringEnums();
```

Note, that the `UseStringEnums` method searches for string enums in all the _loaded_ assemblies, and then registers them with Dapper. Since .NET loads assemblies in a lazy fashion, make sure that the assemblies containing string enums are loaded before calling `UseStringEnums`. You can do that either implicitly, by referencing types in such assemblies, or by explicitly loading such assemblies. For example, if `Sport` is located in a separate assembly, you can load it the following way: 

```csharp
Assembly.Load(typeof(Sport).Assembly.GetName());
```

### Executing commands

```csharp
connection.Execute(@"INSERT Races(Id, Name, Sport) values (@id, @name, @sport)",
    new[]
    {
        new 
        { 
            id = Guid.NewGuid(), 
            name = "Chornohora Sky Marathon", 
            sport = Sport.TrailRunning 
         },
         new 
         { 
             id = Guid.NewGuid(), 
             name = "Cape Town Cycle Tour", 
             sport = Sport.RoadCycling 
          },
          new 
          { 
              id = Guid.NewGuid(), 
              name = "Cape Epic", 
              sport = Sport.MountainBiking 
          }
    });
```

### Running queries

You can pass string enums in query parameters and map them to your entities:

```csharp
var trailRuns = connection.Query(@"SELECT Id, Name, Sport FROM Races WHERE Sport = @sport", new { sport = Sport.TrailRunning });
```

Note, that you cannot use string enums in the `IN` clause - Dapper [does not support](https://github.com/DapperLib/Dapper/issues/1134) it yet. The following won't work:

```csharp
var cyclingRaces = connection.Query(@"SELECT Id, Name, Sport FROM Races WHERE Sport in @sports",
    new { sports = new[]{ Sport.TrailRunning, Sport.MountainBiking }});
```

The workaround would be to case each of the string enum's members to string:

```csharp
var cyclingRaces = connection.Query(@"SELECT Id, Name, Sport FROM Races WHERE Sport in @sports",
    new { sports = new[]{ (string)Sport.TrailRunning, (string)Sport.MountainBiking }});
```

## License

Copyright &copy; 2022 [Dmitry Khmara](https://dmitrykhmara.com).

StrEnum is licensed under the [MIT license](LICENSE.txt).