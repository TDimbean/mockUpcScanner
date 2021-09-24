using DAL.Entities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DAL
{
    public class ProductScanRepository : IProductScanRepository
    {
        private DragonDropDB_Context _context;

        public ProductScanRepository(DragonDropDB_Context context) => _context = context;

        public Product Get(int id)
            => _context.Products.SingleOrDefault(p => p.ProductId == id);

        public IEnumerable<Product> GetAll()
            => _context.Products.ToList();

        public bool IsUniqueBarCode(string barCode, int? prodId = null) 
            => !_context.Products.Any(p => p.BarCode == barCode && p.ProductId != prodId);

        #region Async

        public async Task<Product> GetAsync(int id) => await _context.Products.FindAsync(id);

        public async Task<IEnumerable<Product>> GetAllAsync()
        => await _context.Products.ToListAsync();

        public async Task<bool> IsUniqueBarCodeAsync(string barCode, int? prodId = null)
            => (await _context.Products.Where(p =>
                p.BarCode == barCode &&
                p.ProductId == prodId)
                .ToListAsync()).Count() == 0;

        #endregion
    }
}
