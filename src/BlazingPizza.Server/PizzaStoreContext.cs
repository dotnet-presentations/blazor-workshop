using Microsoft.EntityFrameworkCore;

namespace BlazingPizza.Server
{
    public class PizzaStoreContext : DbContext
    {
        public PizzaStoreContext()
        {
        }

        public PizzaStoreContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<PizzaSpecial> Specials { get; set; }

        public DbSet<Topping> Toppings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuring a many-to-many special -> topping relationship that is friendly for serialisation
            modelBuilder.Entity<PizzaSpecialTopping>().HasKey(pst => new { pst.PizzaSpecialId, pst.ToppingId });
            modelBuilder.Entity<PizzaSpecialTopping>().HasOne<PizzaSpecial>().WithMany(ps => ps.Toppings);
            modelBuilder.Entity<PizzaSpecialTopping>().HasOne(pst => pst.Topping).WithMany();
        }
    }
}
