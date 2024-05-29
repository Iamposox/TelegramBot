using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace TelegramBusinessTripBot.WebApp;

public static class Extensions
{
    public static T GetConfiguration<T>(this IServiceProvider serviceProvider)
        where T : class
    {
        var o = serviceProvider.GetService<IOptions<T>>();
        if (o is null)
            throw new ArgumentNullException(nameof(T));

        return o.Value;
    }
    public static void AddOrUpdate<TEntity>(this DbSet<TEntity> set, TEntity entity)
            where TEntity : class
    {
        _ = !set.Any(e => e == entity) ? set.Add(entity) : set.Update(entity);
    }
    public static void AddOrUpdateRange<TEntity>(this DbSet<TEntity> set, IEnumerable<TEntity> entities)
            where TEntity : class
    {
        foreach (var entity in entities)
        {
            _ = !set.Any(e => e == entity) ? set.Add(entity) : set.Update(entity);
        }
    }
}
