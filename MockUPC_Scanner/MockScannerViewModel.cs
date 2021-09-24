using DAL;
using DAL.Entities;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MockUPC_Scanner
{
    public class MockScannerViewModel : BindableBase
    {
        #region Transfer

        private ManualResetEvent _stopEvent;
        private Thread _loopThread;

        #endregion

        private IProductScanService _service;

        #region Fields

        private string _hostBoxText;
        private string _portBoxText;
        private string _statusLblText;
        private string _codeBoxText;
        private Visibility _formatErrorVis;
        private List<Product> _productsList;
        private Product _selectedProduct;
        private bool _submitEnabled;

        #endregion

        public MockScannerViewModel(IProductScanService service)
        {
            _service = service;

            //Init

            HostBoxText = "localhost";
            PortBoxText = "8080";
            StatusLblText = "No Response";
            CodeBoxText = "# ##### ##### X";
            FormatErrorVis = Visibility.Collapsed;
            ProductsList = _service.GetAll().ToList();
            SelectedProduct = null;
            SubmitEnabled = false;

            _stopEvent = new ManualResetEvent(false);

            //Commands

            RetryConnectionCommand = new DelegateCommand(RetryConnectionCommandExecute);
            GenRandCodeCommand = new DelegateCommand(GenRandCodeCommandExecute);
            ProdSelectCommand = new DelegateCommand(ProdSelectCommandExecute);
            SubmitCommand = new DelegateCommand(SubmitCommandExecute);
            CodeBoxTextChangedCommand = new DelegateCommand<TextChangedEventArgs>(CodeBoxTextChangedCommandExecute);
            ShutDownCommand = new DelegateCommand(ShutDownCommandExecute);
        }

        #region Properties

        #region ViewModel Props

        public string HostBoxText
        {
            get => _hostBoxText;
            set => SetProperty(ref _hostBoxText, value);
        }

        public string PortBoxText
        {
            get => _portBoxText;
            set => SetProperty(ref _portBoxText, value);
        }

        public string StatusLblText
        {
            get => _statusLblText;
            set => SetProperty(ref _statusLblText, value);
        }

        public string CodeBoxText
        {
            get => _codeBoxText;
            set => SetProperty(ref _codeBoxText, value);
        }

        public Visibility FormatErrorVis
        {
            get => _formatErrorVis;
            set => SetProperty(ref _formatErrorVis, value);
        }

        public List<Product> ProductsList
        {
            get => _productsList;
            set => SetProperty(ref _productsList, value);
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public bool SubmitEnabled
        {
            get => _submitEnabled;
            set => SetProperty(ref _submitEnabled, value);
        }

        #endregion

        // Commands

        public DelegateCommand RetryConnectionCommand { get; }
        public DelegateCommand GenRandCodeCommand { get; }
        public DelegateCommand ProdSelectCommand { get; }
        public DelegateCommand SubmitCommand { get; }
        public DelegateCommand<TextChangedEventArgs> CodeBoxTextChangedCommand { get; }
        public DelegateCommand ShutDownCommand { get; }

        #endregion

        #region Commands

        private void CodeBoxTextChangedCommandExecute(TextChangedEventArgs e)
        {
            var box = e.Source as TextBox;
            if (box.Text.Length != 0)
            {
                if (box.Text.Length > 15) box.Text = box.Text.Substring(0, 15);
                if (box.Text.Length > 1 && box.Text[1] != ' ') box.Text = box.Text.Insert(1, " ");
                if (box.Text.Length > 7 && box.Text[7] != ' ') box.Text = box.Text.Insert(7, " ");
                if (box.Text.Length > 13 && box.Text[13] != ' ') box.Text = box.Text.Insert(13, " ");

                if (e.Changes.FirstOrDefault().RemovedLength == 1
                    && box.Text.ToCharArray().LastOrDefault() == ' ')
                    box.Text = box.Text.Substring(0, box.Text.Length - 1);

                box.CaretIndex = box.Text.Length;
            }

            if (_service.ValidateCodeFormat(box.Text.Replace(" ", string.Empty)).isValid.GetValueOrDefault())
            {
                FormatErrorVis = Visibility.Collapsed;
                SubmitEnabled = true;
                return;
            }
            FormatErrorVis = Visibility.Visible;
            SubmitEnabled = false;
        }

        private void GenRandCodeCommandExecute()
        {
            var finalCode = "# ##### ##### X";
            do
            {
                var rnd = new Random();
                var barcode = rnd.Next(1, 1000000000).ToString("D9") + rnd.Next(1, 100).ToString("D2");

                var oddSum = 0;
                var evenSum = 0;

                for (int i = 0; i < 11; i++)
                {
                    var digit = int.Parse(barcode[i].ToString());

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

                finalCode = barcode + check.ToString();
            } while (_service.CodeExists(finalCode));

            CodeBoxText = finalCode;
        }

        private void ProdSelectCommandExecute()
            => CodeBoxText = SelectedProduct != null ? SelectedProduct.BarCode : CodeBoxText;

        private void RetryConnectionCommandExecute() => SendMsg(true);

        private void SubmitCommandExecute() => SendMsg(false);

        private void SendMsg(bool isPing)
        {
            _stopEvent.Set();
            _stopEvent.Reset();

            StatusLblText = "No Response";

            _loopThread = new Thread(() => Send(isPing));
            _loopThread.Start();
        }

        private void Send(bool isPing = false)
        {
            var length = isPing ? 1 : 12;

            var port = 0;
            var portParses = int.TryParse(PortBoxText, out port);
            if (!portParses) return;

            try
            {
                var client = new TcpClient(HostBoxText, port);

                StatusLblText = "Connected";

                var codeToSend = new byte[length];

                codeToSend = isPing ? Encoding.ASCII.GetBytes("!") : Encoding.ASCII.GetBytes(CodeBoxText.Replace(" ", string.Empty));

                var stream = client.GetStream();

                stream.Write(codeToSend, 0, length);

                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                StatusLblText = "No Response";
                return;
            }
        }

        private void ShutDownCommandExecute() => _stopEvent.Set();


        #endregion
    }
}
