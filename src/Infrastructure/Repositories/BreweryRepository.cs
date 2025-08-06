using AutoMapper;
using BoldareBrewery.Application.Interfaces;
using BoldareBrewery.Application.Models.External;
using BoldareBrewery.Domain.Data.Entities;
using BoldareBrewery.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoldareBrewery.Infrastructure.Repositories
{
    public class BreweryRepository : IBreweryRepository
    {
        private readonly BreweryDbContext _context;
        private readonly IMapper _mapper;

        public BreweryRepository(BreweryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ExternalBrewery>> GetAllAsync()
        {
            var entities = await _context.Breweries.ToListAsync();
            return _mapper.Map<IEnumerable<ExternalBrewery>>(entities);
        }

        public async Task SaveBreweriesAsync(IEnumerable<ExternalBrewery> breweries)
        {
            var entities = _mapper.Map<IEnumerable<BreweryEntity>>(breweries);

            // Clear existing data
            _context.Breweries.RemoveRange(_context.Breweries);

            // Add new data
            _context.Breweries.AddRange(entities);

            await _context.SaveChangesAsync();
        }

        public async Task<DateTime?> GetLastSyncTimeAsync()
        {
            var lastBrewery = await _context.Breweries
                .OrderByDescending(b => b.UpdatedAt)
                .FirstOrDefaultAsync();

            return lastBrewery?.UpdatedAt;
        }

        public async Task UpdateSyncTimeAsync()
        {
            await _context.Breweries
                .ExecuteUpdateAsync(b => b.SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
        }

        public async Task<IEnumerable<ExternalBrewery>> SearchAsync(ISpecification<BreweryEntity> specification)
        {
            var query = ApplySpecification(_context.Breweries, specification);
            var entities = await query.ToListAsync();
            return _mapper.Map<IEnumerable<ExternalBrewery>>(entities);
        }

        public async Task<int> CountAsync(ISpecification<BreweryEntity> specification)
        {
            var query = ApplySpecification(_context.Breweries, specification);
            return await query.CountAsync();
        }

        private static IQueryable<BreweryEntity> ApplySpecification(
            IQueryable<BreweryEntity> inputQuery,
            ISpecification<BreweryEntity> specification)
        {
            var query = inputQuery;

            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            if (specification.IsPagingEnabled)
            {
                query = query.Skip(specification.Skip).Take(specification.Take);
            }

            return query;
        }
    }
}