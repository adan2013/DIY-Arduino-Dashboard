using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ArduinoDashboardInterpreter
{
    /// <summary>
    /// Interaction logic for ChangeScreenType.xaml
    /// </summary>
    public partial class ChangeScreenType : Window
    {
        public ScreenController.ScreenType SelectedType;

        public ChangeScreenType()
        {
            InitializeComponent();
            LoadValuesToList();
        }

        private void LoadValuesToList()
        {
            foreach (ScreenController.ScreenType type in (ScreenController.ScreenType[])Enum.GetValues(typeof(ScreenController.ScreenType)))
            {
                TypeList.Items.Add(type);
            }
            if (TypeList.Items.Count > 0) TypeList.SelectedIndex = 0;
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if(TypeList.SelectedIndex >= 0)
            {
                SelectedType = (ScreenController.ScreenType)TypeList.SelectedItem;
                DialogResult = true;
            }
        }
    }
}
