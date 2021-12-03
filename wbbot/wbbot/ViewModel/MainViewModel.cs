using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using wbbot.Helper;
using wbbot.View;
using wbbot.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.PhantomJS;

namespace wbbot.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel(Settings settings)
        {
            PhoneNumber = settings.PhoneNumber;
            Limit = settings.Limit;
            Freq = settings.Freq;
            StartTime = settings.StartTime;
            EndTime = settings.EndTime;
        }

        IWebDriver driver;

        string phoneNumber;
        public string PhoneNumber
        {
            get
            {
                return phoneNumber;
            }
            set
            {
                phoneNumber = value;
                OnPropertyChanged("PhoneNumber");
            }
        }

        int limit;
        public int Limit
        {
            get
            {
                return limit;
            }
            set
            {
                limit = value;
                OnPropertyChanged("Limit");
            }
        }

        int freq;
        public int Freq
        {
            get
            {
                return freq;
            }
            set
            {
                freq = value;
                OnPropertyChanged("Freq");
            }
        }

        DateTime startTime;
        public DateTime StartTime
        {
            get
            {
                return startTime;
            }
            set
            {
                startTime = value;
                OnPropertyChanged("StartTime");
            }
        }

        DateTime endTime;
        public DateTime EndTime
        {
            get
            {
                return endTime;
            }
            set
            {
                endTime = value;
                OnPropertyChanged("EndTime");
            }
        }

        void Stop()
        {
            timer stop

            // вставляем сумму в поле ввода
            IWebElement input = driver.FindElement(By.ClassName("form__input--white"));
            input.Clear();
            input.SendKeys("50");

            // нажимаем кнопку приостановить компанию
            IWebElement btnStop = driver.FindElement(By.ClassName("begin-container")).FindElement(By.ClassName("btn--primary"));
            btnStop.Click();
        }

        private RelayCommand startCommand;
        public RelayCommand StartCommand
        {
            get
            {
                return startCommand ??
                    (startCommand = new RelayCommand(obj =>
                    {                        
                        driver = new FirefoxDriver();
                        driver.Manage().Window.Maximize();
                        //driver.Navigate().GoToUrl("https://seller.wildberries.ru/cmp/campaigns/list/pause/edit/search/502540");
                        driver.Navigate().GoToUrl("https://seller.wildberries.ru/cmp/campaigns/list/pause/edit/search/505131");

                        System.Threading.Thread.Sleep(10000);
                        IWebElement query = driver.FindElement(By.ClassName("SimpleInput--vVIag"));
                        query.SendKeys(PhoneNumber);
                        IWebElement button = driver.FindElement(By.ClassName("Button--full-width--DVZvW"));
                        button.Click();

                        CodeViewModel codeViewModel = new CodeViewModel(driver);
                        CodeWindow codeWindow = new CodeWindow();
                        codeWindow.DataContext = codeViewModel;
                        codeWindow.ShowDialog();
                        System.Threading.Thread.Sleep(20000);
                        //driver.Navigate().GoToUrl("https://seller.wildberries.ru/cmp/campaigns/list/pause/edit/search/502540");
                        driver.Navigate().GoToUrl("https://seller.wildberries.ru/cmp/campaigns/list/pause/edit/search/505131");
                        
                        
                        System.Threading.Thread.Sleep(30000);
                        // Определяем позицию
                        IWebElement span = driver.FindElement(By.ClassName("card__settings__row__box--place")).FindElement(By.TagName("span"));
                        var positions = span.Text.Split('-');
                        if(positions[0] != "1")
                        {
                            // Получаем стоимость первого места
                            IWebElement money = driver.FindElement(By.ClassName("places__text"));
                            var m = money.Text.Split(' ');
                            int s = int.Parse(m[0]);
                            if(s > Limit)
                            {
                                Stop();
                                return;
                            }
                            // вставляем сумму в поле ввода
                            IWebElement input = driver.FindElement(By.ClassName("form__input--white"));
                            input.Clear();
                            input.SendKeys(m[0]);
                            span.Click();// делаем ставку

                            // нажимаем кнопку сохранить компанию
                            //          IWebElement btnSave = driver.FindElement(By.ClassName("begin-container")).FindElement(By.ClassName("m-l-24"));
                            IWebElement btnSave = driver.FindElement(By.ClassName("begin-container")).FindElement(By.ClassName("btn--outline"));
                            btnSave.Click();
                        }
                                              
                        // клик на кнопке статистики
                        //IWebElement link = driver.FindElement(By.ClassName("m-b-48"))
                        //.FindElement(By.ClassName("m-t-24"))
                        //.FindElement(By.ClassName("preview-link"));
                        //link.Click();


                    }));
            }
        }

        private RelayCommand stopCommand;
        public RelayCommand StopCommand
        {
            get
            {
                return stopCommand ??
                    (stopCommand = new RelayCommand(obj =>
                    {
                        Stop();
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
