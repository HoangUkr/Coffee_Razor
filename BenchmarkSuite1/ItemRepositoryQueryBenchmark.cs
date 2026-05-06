using BenchmarkDotNet.Attributes;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Benchmarks
{
    [SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 5)]
    [MemoryDiagnoser]
    public class ItemRepositoryQueryBenchmark
    {
        private DbContextOptions<CoffeeDbContext> _options = null!;

        [GlobalSetup]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");

            _options = new DbContextOptionsBuilder<CoffeeDbContext>().UseSqlServer(connectionString).Options;
            // Warm up the connection pool
            using var ctx = new CoffeeDbContext(_options);
            ctx.Database.OpenConnection();
        }

        [Benchmark(Baseline = true, Description = "SplitQuery (current)")]
        public async Task<int> GetAllActive_SplitQuery()
        {
            await using var ctx = new CoffeeDbContext(_options);
            var items = await ctx.Items.AsSplitQuery().AsNoTracking().Include(i => i.Category).Include(i => i.ItemImages).Where(i => i.IsActive).OrderBy(i => i.Name).ToListAsync();
            return items.Count;
        }

        [Benchmark(Description = "SingleQuery (proposed)")]
        public async Task<int> GetAllActive_SingleQuery()
        {
            await using var ctx = new CoffeeDbContext(_options);
            var items = await ctx.Items.AsSingleQuery().AsNoTracking().Include(i => i.Category).Include(i => i.ItemImages).Where(i => i.IsActive).OrderBy(i => i.Name).ToListAsync();
            return items.Count;
        }
    }
}