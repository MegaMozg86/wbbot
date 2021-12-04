using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using wbbot.ViewModel;
using wbbot.Model;
using Newtonsoft.Json;

namespace wbbot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string file = @"./settings.json";
        Settings settings;
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(file));
            }
            catch
            {
                settings = new Settings();
            }

            DataContext = new MainViewModel(settings);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = (DataContext as MainViewModel);
            settings.PhoneNumber = vm.PhoneNumber;
            settings.Limit = vm.Limit;
            settings.Freq = vm.Freq;
            settings.StartTime = vm.StartTime;
            settings.EndTime = vm.EndTime;
            settings.Link = vm.Link;

            if(vm.driver != null)
                vm.driver.Close();

            try
            {
                string json = JsonConvert.SerializeObject(settings);
                File.WriteAllText(file, json);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
