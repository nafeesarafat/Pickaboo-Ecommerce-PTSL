using Ecommerce.Api.Data;
using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Services;

internal static class CategoryProductQuery
{
    public static IQueryable<Product> GetActiveProducts(AppDbContext context, int categoryId)
    {
        return context.Products
            .AsNoTracking()
            .Where(product => product.CategoryId == categoryId && product.IsActive);
    }
}
