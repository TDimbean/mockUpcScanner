using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL
{
    public interface IProductScanService
    {
        Product Get(int id);
        IEnumerable<Product> GetAll();
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetAsync(int id);
        (bool? isValid, string errorList) ValidateCodeFormat(string barCode);
        bool CodeExists(string code);
    }
}