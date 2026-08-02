using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Categories.ValueObjects;
using eMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Infrastructure.Persistence.Repositories
{
    public sealed class CategoryRepository : Repository<Category, CategoryId>, ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
        {
            await _context.Categories.AddAsync(category, cancellationToken);
        }

        public async Task<bool> ExistsAsync(
     CategoryName name,
     CategoryId? excludedId = null,
     CancellationToken cancellationToken = default)
        {
            var query = _context.Categories.AsQueryable();

            query = query.Where(c => c.Name.Value == name.Value);

            if (excludedId is not null)
            {
                query = query.Where(c => c.Id != excludedId);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<Category?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken = default)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Category?> GetByNameAsync(CategoryName name, CancellationToken cancellationToken = default)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Name.Value == name.Value, cancellationToken);  
        }
        public async Task<List<Category>> GetAllAsync(
      CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(x => x.Name.Value)
                .ToListAsync(cancellationToken);
        }
    }
}
