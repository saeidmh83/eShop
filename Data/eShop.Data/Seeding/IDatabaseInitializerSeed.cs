using System.Threading.Tasks;

namespace eShop.Data.Seeding
{
    public interface IDatabaseInitializerSeed
    {
        Task SeedAsync();
    }
}