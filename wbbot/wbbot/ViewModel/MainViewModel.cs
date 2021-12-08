using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.IO;
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
            Link = settings.Link;
            Limit = settings.Limit;
            Freq = settings.Freq;
            StartTime = settings.StartTime;
            EndTime = settings.EndTime;
            IsHeadless = settings.IsHeadless;
        }

        public IWebDriver driver;
        DispatcherTimer timer;

        double[] places = new double[3];

        string header = "Дата;Время;Ключевая фраза;Просмотры;Частота;Клики;CTR(%);СРС;Затраты;Повышение ставки;Актуальная ставка;Ставка за первок место; Ставка за второе место; Ставка за третье место";
        string file = @"./report.csv";

        string current;
        double _current;
        bool isRaise = false;   // было повышение ставки

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

        string link;
        public string Link
        {
            get
            {
                return link;
            }
            set
            {
                link = value;
                OnPropertyChanged("Link");
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
        bool isHeadless;
        public bool IsHeadless
        {
            get
            {
                return isHeadless;
            }
            set
            {
                isHeadless = value;
                OnPropertyChanged("IsHeadless");
            }
        }

        void DoIt()
        {
            new Task(() =>
            {
                isRaise = false;
                System.Threading.Thread.Sleep(10000);

                driver.Navigate().GoToUrl(Link);

                System.Threading.Thread.Sleep(30000);
                // проверка статуса кампании
                try
                {
                    // если элемент Идут показы не найден
                    IWebElement status9 = driver.FindElement(By.ClassName("status--9"));
                }
                catch
                {
                    // то жмем кнопку запуска
                    IWebElement btnRun = driver.FindElement(By.ClassName("begin-container")).FindElement(By.ClassName("btn--primary"));
                    btnRun.Click();
                }


                // Определяем позицию
                IWebElement span = driver.FindElement(By.ClassName("card__settings__row__box--place")).FindElement(By.TagName("span"));
                var positions = span.Text.Split('-');

                // получаем ставки за три первых места
                var ms = driver.FindElements(By.ClassName("places__text"));
                for (int i = 0; i < 3; ++i)
                {
                    var m = ms[i].Text.Split(' ');
                    places[i] = double.Parse(m[0]);
                }

                // поле ввода
                IWebElement input = driver.FindElement(By.ClassName("form__input--white"));

                // определили повышение ставки
                current = input.GetAttribute("value");    // получаем текущую ставку
                _current = double.Parse(current);
                if (places[0] > _current)
                    isRaise = true;
                // -----------------------------

                if (positions[0] != "1")
                {
                    // Получаем стоимость первого места
                    if (places[0] > Limit)
                    {
                        Stop();
                        return;
                    }

                    input.Clear();
                    //  input.SendKeys(m[0]);
                    input.SendKeys(((int)places[0]).ToString());
                    span.Click();// делаем ставку

                    IWebElement btnSave = driver.FindElement(By.ClassName("begin-container")).FindElement(By.ClassName("btn--outline"));
                    btnSave.Click();
                }

                // клик на кнопке статистики
                // не обязательно перечеслять все уровни иерархии вложений
                IWebElement link = driver.FindElement(By.ClassName("well"))
                .FindElement(By.ClassName("preview-link"));
                link.Click();

                ParseTable();
            }).Start();            
        }

        void ParseTable()
        {
            string content = string.Empty;
            var columns = driver.FindElements(By.ClassName("table-nonadaptive--column"));
            content += DateTime.Now.ToString("dd.MM.yyyy;HH:mm:ss;");

            for(int i = 0; i < 7; ++i)
            {
                var text = columns[i].Text.Split('\r');
                
                if(i == 6)
                {
                    // избавляемся от знака валюты
                    content += text[1].Substring(0, text[1].IndexOf(' ')).Replace('\n', ' ').Replace('.', ',').Trim() + ";";
                }
                else
                    content += text[1].Replace('\n', ' ').Replace('.', ',').Trim() + ";";
            }


            content += string.Format("{0};{1};{2};{3};{4}{5}", isRaise? "Да" : "Нет", _current, places[0], places[1], places[2], Environment.NewLine);
            File.AppendAllText(file, content, Encoding.GetEncoding(1251));
        }

        void Check()
        {
            if (StartTime.Hour != EndTime.Hour && StartTime.Minute != EndTime.Minute)
            {
                if (DateTime.Now.TimeOfDay >= StartTime.TimeOfDay && DateTime.Now.TimeOfDay <= EndTime.TimeOfDay)
                    DoIt();
                else
                    return;
            }

            DoIt();
        }

        void Stop()
        {
            timer.Stop();

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
                        FirefoxOptions options = new FirefoxOptions();
                        
                        if(IsHeadless)
                            options.AddArgument("-headless");

                        driver = new FirefoxDriver(options);

                        driver.Navigate().GoToUrl(Link);

                        new Task(() =>
                        {

                            System.Threading.Thread.Sleep(10000);
                            IWebElement query = driver.FindElement(By.ClassName("SimpleInput--vVIag"));
                            query.SendKeys(PhoneNumber);
                            IWebElement button = driver.FindElement(By.ClassName("Button--full-width--DVZvW"));
                            button.Click();
                        }).Start();
                                                
                        CodeWindow codeWindow = new CodeWindow();
                        CodeViewModel codeViewModel = new CodeViewModel(driver, codeWindow);
                        codeWindow.DataContext = codeViewModel;
                        codeWindow.ShowDialog();

                        // создаем файл отчета и пишем шапку
                        header += Environment.NewLine;
                        File.AppendAllText(file, header, Encoding.GetEncoding(1251));

                        Check();

                        timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(Freq);
                        timer.Tick += Timer_Tick;
                        timer.Start();
                    }));
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Check();
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
                        driver.Close();
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
