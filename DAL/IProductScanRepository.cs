using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Entities;

namespace DAL
{
    public interface IProductScanRepository
    {
        Product Get(int id);
        IEnumerable<Product> GetAll();
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetAsync(int id);
        bool IsUniqueBarCode(string barCode, int? prodId = null);
        Task<bool> IsUniqueBarCodeAsync(string barCode, int? prodId = null);
    }
}