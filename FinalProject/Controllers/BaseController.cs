using Domain;
using FinalProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FinalProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public partial class BaseController<TEntity> : ControllerBase where TEntity : class, new()
    {
        protected readonly AppDbContext _context;
        protected DbSet<TEntity> _dbSet { get; set; }

        public BaseController(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        [HttpGet]
        public virtual async Task<IEnumerable<TEntity>> SearchAsync([FromQuery] SearchParam param)
        {
            return await _dbSet.Where(BuidSearchExpression(param.Filter, param.Search)).OrderBy(BuidOrderByExpression(param.Filter)).ToListAsync();
        }

        [HttpGet("id")]
        public virtual async Task<TEntity> GetEntityByIdAsync(string Id)
        {
            return await _dbSet.Where(BuidSearchExpression("Id", Id.ToString())).FirstOrDefaultAsync();
        }

        [HttpPost]
        public virtual async Task<IActionResult> CreateEntityAsync([FromBody] TEntity entity)
        {
            _context.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity); 
        }

        /// <summary>
        /// Generate expression =  (T entity) => entity|Filter| === Value;
        /// </summary>
        /// <param name="Filter"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        private Expression<Func<TEntity, bool>> BuidSearchExpression(string Filter, string Value)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity));
            var searchValue = Expression.Constant(Value);
            var property = Expression.Property(parameterExpression,
                Filter);

            var expression = Expression.Equal(property, searchValue);

            return Expression.Lambda<Func<TEntity,
             bool>>(expression, parameterExpression);
        }

        private Expression<Func<TEntity, object>> BuidOrderByExpression(string OrderBy)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity));
            return Expression.Lambda<Func<TEntity, object>>(Expression.Property(parameterExpression, OrderBy), parameterExpression);
        }
    }
}