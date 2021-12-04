using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using wbbot.Helper;
using wbbot.View;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.PhantomJS;

namespace wbbot.ViewModel
{
    public class CodeViewModel : INotifyPropertyChanged
    {
        IWebDriver driver;
        CodeWindow window;

        public CodeViewModel(IWebDriver _driver, CodeWindow _window)
        {
            driver = _driver;
            window = _window;
        }

        string code;
        public string Code
        {
            get
            {
                return code;
            }
            set
            {
                code = value;
                OnPropertyChanged("Code");
            }
        }

        private RelayCommand sendCommand;
        public RelayCommand SendCommand
        {
            get
            {
                return sendCommand ??
                    (sendCommand = new RelayCommand(obj =>
                    {
                        IWebElement query = driver.FindElement(By.Name("notifyCode"));
                        query.SendKeys(Code);
                        window.Close();

                    }));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
