using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Domain
{
    public static class ModelBuilderExtensions
    {
        public static void RegisterAllEntities(this ModelBuilder modelBuilder, params Assembly[] assemblies)
        {
            IEnumerable<Type> types = assemblies.SelectMany(a => a.GetTypes()).Where(c => c.IsClass && !c.IsAbstract && c.IsPublic);
            foreach (Type type in types)
                modelBuilder.Entity(type);
        }
    }
}