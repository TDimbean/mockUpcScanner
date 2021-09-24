using DAL.Entities;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class ProductScanService : IProductScanService
    {
        private IProductScanRepository _repo;

        public ProductScanService(IProductScanRepository repo) => _repo = repo; 

        public Product Get(int id)
        =>_repo.Get(id);

        public IEnumerable<Product> GetAll()
        => _repo.GetAll();

        public bool CodeExists(string code) => !_repo.IsUniqueBarCode(code);

        public (bool? isValid, string errorList) ValidateCodeFormat(string barCode)
        {
            var isValid = true;
            var errorList = new StringBuilder();

            if (barCode.Length != 12)
            {
                errorList.AppendLine("Barcode must be precisely 12 characters long.");
                isValid = false;
            }

            if (barCode.Length>0&&!IsDigitsOnly(barCode))
            {
                errorList.AppendLine("Product Barcode must be made up entirely of digits.");
                isValid = false;
            }

            else if(barCode.Length==12)
            {
            var oddSum = 0;
            var evenSum = 0;

            for (int i = 0; i < 11; i++)
            {
                var digit = int.Parse(barCode[i].ToString());

                oddSum += i % 2 == 0 ? digit : 0;
                evenSum += i % 2 != 0 ? digit : 0;
            }

            var totalSum = oddSum * 3 + evenSum;
            var check = 0;

            while (true)
                    {
                        if ((check + totalSum) % 10 == 0) break;
                        else check++;
                    }

            var validFormat = check == int.Parse(barCode.Substring(11, 1));

            if (!validFormat)
                    {
                        errorList.AppendLine("Barcode must be in a valid format\n(X-XXXXX-YYYYY-Z,\n X=> Manufacturer Code;\n Y=> Product Code;\n Z=> Check Digit).");
                        isValid = false;
                    }
            }
                   
            return (isValid, errorList.ToString());
        }

        #region Async

        public async Task<Product> GetAsync(int id)
        => await _repo.GetAsync(id);

        public async Task<IEnumerable<Product>> GetAllAsync()
        => await _repo.GetAllAsync();

        #endregion

        public static bool IsDigitsOnly(string inp)
        {
            foreach (var c in inp) if (c < '0' || c > '9') return false;
            return true;
        }
    }
}
