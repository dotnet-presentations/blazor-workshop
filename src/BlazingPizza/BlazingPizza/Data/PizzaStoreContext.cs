// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza.Data;

public class PizzaStoreContext(DbContextOptions<PizzaStoreContext> options)
    : IdentityDbContext<PizzaStoreUser>(options)
{
    public DbSet<Order> Orders { get; set; }

    public DbSet<Pizza> Pizzas { get; set; }

    public DbSet<PizzaSpecial> Specials { get; set; }

    public DbSet<Topping> Toppings { get; set; }

    public DbSet<NotificationSubscription> NotificationSubscriptions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var dbPath = Path.Join(path, "pizza.db");

        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuring a many-to-many special -> topping relationship that is friendly for serialization
        builder.Entity<PizzaTopping>().HasKey(pst => new { pst.PizzaId, pst.ToppingId });
        builder.Entity<PizzaTopping>().HasOne<Pizza>().WithMany(ps => ps.Toppings);
        builder.Entity<PizzaTopping>().HasOne(pst => pst.Topping).WithMany();

        // Inline the Lat-Long pairs in Order rather than having a FK to another table
        builder.Entity<Order>().OwnsOne(o => o.DeliveryLocation);
    }
}
