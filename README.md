# GodeTech.Microservices.EntityFrameworkCore

## Overview
`GodelTech.Microservices.EntityFrameworkCore` project allows users easy to use [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) with patterns Repository and Specification. 

[Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/). This ORM is recommended for new projects. It allows microservice to be run without operating system constraints. E.g. Linux and Windows operating systems can be used. 

In order to use EF Core ORM the following initializer must be added to `Startup.CreateInitializers()` method:

```csharp
yield return new DbContextInitializer<WeatherForecastDbContext>(Configuration);
```

## Quick Start

In order to use GoldeTech entity framework few simple steps are required:

1. Create ASP.NET Website application using **Visual Studio** or **dotnet cli**.
2. Reference latest versions of `GodelTech.Microservices.Core` and `GodeTech.Microservices.EntityFrameworkCore` nuget packages and optionally satellite packages you would like to use.
3. Setup the project as described in the [GodelTech.Microservices.Core](https://github.com/GodelTech/GodelTech.Microservices.Core) manual.
4. Add congiguration GodelTech Entity Framework Core to `Startup.cs` `CreateInitializers()` method:
```csharp
yield return new DbContextInitializer<WeatherForecastDbContext>(Configuration, connectionString);
```
where `WeatherForecastDbContext` is a class that was inherited from `DbContext` class

```csharp
public class WeatherForecastDbContext : DbContext

public WeatherForecastDbContext(DbContextOptions<WeatherForecastDbContext> contextOptions)
    : base(contextOptions)
{ }
```
5. Setup entities and relationships between them, folowing [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) instruction.

Please see the following snippet to configure Db context for two models with one-to-many relationship:

`WeatherForecastDbContext.cs`
```csharp
public class WeatherForecastDbContext : DbContext
{
    public DbSet<PrecipitationType> PrecipitationTypes { get; set; }

    public DbSet<Precipitation> Precipitations { get; set; }

    public WeatherForecastDbContext(DbContextOptions<WeatherForecastDbContext> contextOptions)
        : base(contextOptions)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Precipitation).Assembly);
    }
}
```

`Precipitation.cs`
```csharp
public class Precipitation
{
    public int Id { get; set; }

    public string Town { get; set; }

    public DateTime DateTime { get; set; }

    public int Temperature { get; set; }

    public string Summary { get; set; }

    public int PrecipitationTypeId { get; set; }

    public PrecipitationType PrecipitationType { get; set; }
}
```

`PrecipitationType.cs`
```csharp
public class PrecipitationType
{
    public int Id { get; set; }

    public string Name { get; set; }
}
```

`PrecipitationConfiguration.cs`
```csharp
public class PrecipitationConfiguration : IEntityTypeConfiguration<Precipitation>
{
    public void Configure(EntityTypeBuilder<Precipitation> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired();
        builder.Property(p => p.Town).IsRequired();
        builder.Property(p => p.Temperature).IsRequired();
        builder.Property(p => p.DateTime).IsRequired();
        builder.Property(p => p.PrecipitationTypeId);
        builder.HasOne(x => x.PrecipitationType).WithMany().HasForeignKey(x => x.PrecipitationTypeId);
    }
}
```

## Working with Database. Repository

For CRUD operation `GodeTech.Microservices.EntityFrameworkCore` proposes using pattern Repository.

### IRepository. Simple query

In order to use Repository for get, create, update and delete operetions see the flow below:

1. Add congiguration GodelTech Entity Framework Core to `Startup.cs` `CreateInitializers()` method:
```csharp
yield return new RepositoryInitializer<WeatherForecastDbContext, TEntity>(Configuration);
```
2. Declare a property with type `IRepository<T>` where `T` is entity
```csharp
private readonly IRepository<Precipitation> _precipitationRepository;
```
3. Resolve dependency in constructor
```csharp
public WeatherForecastController(IRepository<Precipitation> precipitationRepository)
{
    _precipitationRepository = precipitationRepository;
}
```
4. Use repository to execute query
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> Get(int id)
{
    Precipitation precipitation = await _precipitationRepository.GetByIdAsync(id);

    return Ok(precipitation);
}
```
5. Use the same way for Delete operation.
```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
    Precipitation precipitation = await _precipitationRepository.GetByIdAsync(id);

    await _precipitationRepository.DeleteAsync(precipitation);

    return Ok();
}
```
6. For Add and Update operations use samples below.
```csharp
await _precipitationRepository.AddAsync(precipitation);

await _precipitationRepository.UpdateAsync(precipitation);
```

### Specification. Query with conditions

Sometimes it is necessary to get data by conditions. In this case `GodeTech.Microservices.EntityFrameworkCore` proposes using pattern Specification.

1. Create a specification class for each entity and inherite it from `Specification<T>` where `T` is entity
```csharp
public class PrecipitationSpecification : Specification<Precipitation>
```
2. Define nullable fields for the conditions to be used.
```csharp
public string Town { get; set; }

public int? Temperature { get; set; }
```
3. Build query in override method `AddPredicates`
```csharp
public override IQueryable<Precipitation> AddPredicates(IQueryable<Precipitation> query)
{
    if (Temperature.HasValue)
    {
        query = query.Where(x => x.Temperature == Temperature);
    }

    if (!string.IsNullOrWhiteSpace(Town))
    {
        query = query.Where(x => x.Town.Contains(Town));
    }
}
```
4. Use specification class
```csharp
[HttpGet("temperature/{temperature}/town/{town}")]
public async Task<IActionResult> GetList(int temperature, string town)
{
    var spec = new PrecipitationSpecification
    {
        Temperature = temperature,
        Town = town
    };

    var precipitations = await _precipitationRepository.ListAsync(spec);

    return Ok(precipitations);
}
```
5. Override the method `AddSorting(IQueryable<Precipitation> query)` in `PrecipitationSpecification` class to set sorting rules
```csharp
public override IQueryable<Precipitation> AddSorting(IQueryable<Precipitation> query)
{
    return query.OrderBy(c => c.Town);
}
```
6. Override the method `AddEagerFetching(IQueryable<Precipitation> query)` in `PrecipitationSpecification` class to include related entities
```csharp
public override IQueryable<Precipitation> AddEagerFetching(IQueryable<Precipitation> query)
{
    if (IncludePrecipitationType.HasValue)
    {
        query = query.Include(x => x.PrecipitationType);
    }

    return query;
}
```

### Stored procedure
In order to use stored procedure see code below:
```csharp
public class PrecipitationQuery : IPrecipitationQuery
{
    private readonly WeatherForecastDbContext _dbContext;

    public PrecipitationQuery(WeatherForecastDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IList<Precipitation>> GetListAsync()
    {
        return await _dbContext.Precipitations.FromSqlRaw("GetPrecipitations").ToListAsync();
    }
}
```

**NOTE** At the service start, all migrations are applied. Database is created if it is not exist.

## Links
* [Repository implementation](https://www.infoq.com/articles/repository-implementation-strategies)
* [Specification pattern](https://enterprisecraftsmanship.com/2016/02/08/specification-pattern-c-implementation/)
* [Microsoft Example of EF Core repository](https://github.com/dotnet-architecture/eShopOnWeb/blob/b864be9265545fa78ff8fb90a4824dfa7618e676/src/Infrastructure/Data/EfRepository.cs)