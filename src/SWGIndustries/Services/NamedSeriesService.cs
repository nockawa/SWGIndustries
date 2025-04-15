using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using SWGIndustries.Data;

namespace SWGIndustries.Services;

[PublicAPI]
public class NamedSeriesService
{
    private readonly ApplicationDbContext _context;

    public NamedSeriesService(ApplicationDbContext context)
    {
        _context = context;
    }

    private const int MaxRetries = 100;
    public async Task<int> GetNextValueAsync(string name)
    {
        var maxRetries = MaxRetries;
        do
        {
            var namedSeries = await _context.Set<NamedSeriesEntity>()
                .FirstOrDefaultAsync(ns => ns.Name == name);

            if (namedSeries == null)
            {
                namedSeries = new NamedSeriesEntity
                {
                    Name = name,
                    Counter = 1
                };
                _context.Set<NamedSeriesEntity>().Add(namedSeries);
            }
            else
            {
                namedSeries.Counter++;
            }

            try
            {
                await _context.SaveChangesAsync();
                return namedSeries.Counter;
            }
            catch (DbUpdateConcurrencyException)
            {
            }
        } while (maxRetries-- > 0);

        throw new InvalidOperationException($"Failed to get next value for the series '{name}' after {MaxRetries} attempts.");
    }
}