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
using Modeling.Pages;
using Modeling.generationFunctions;
using System.Windows.Navigation;
using Kompas6API5;
using KompasAPI7;
using Modeling.Class;
using Modeling.Constructors;
using System.ComponentModel;
using Kompas6Constants3D;

namespace Modeling.Windows
{
    /// <summary>
    /// Логика взаимодействия для generateSparWindow.xaml
    /// </summary>
    public partial class generateSparWindow : Window
    {

        public string selectedMaterial = string.Empty;
        public bool isAssembly = false;

        private allConstruct allConstruct;
        private generateFunc generateFunc;
        public generateSparWindow()
        {
            InitializeComponent();
            FrmMain.Navigate(new generateWholeSpar());
            allConstruct = MainWindow.allConstruct;
            generateFunc = MainWindow.generateFunc;

            if (generateFunc.isAssembly)
            {
               
            }

        }

        private void FrmMain_ContentRendered(object sender, EventArgs e)
        {

        }

        //public void changeMaterial()
        //{
        //    tb_selectedMaterial.Text = selectedMaterial;
        //    this.UpdateLayout();
        //}

        //private void btn_search_Click(object sender, RoutedEventArgs e)
        //{
        //    searchMaterial searchMaterial = new searchMaterial();
        //    searchMaterial.Show();
        //    this.Close();
        //}
    
        //private void tb_stepMissing_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    char[] enteredSymbols = tb_stepMissing.Text.ToCharArray();
        //    string temp = string.Empty;

        //    for(int i = 0; i < enteredSymbols.Length; i++)
        //    {
        //        if (Convert.ToInt32(enteredSymbols[i]) >= 48 && Convert.ToInt32(enteredSymbols[i]) <= 57)
        //        {
        //            temp += enteredSymbols[i];
        //        }
        //    }
        //    tb_stepMissing.Text = temp;

        //    if (temp == string.Empty)
        //        return;

        //    generateFunc.stepMissing = double.Parse(temp);
        //}

        //private void tb_countSpar_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    char[] enteredSymbols = tb_countSpar.Text.ToCharArray();
        //    string temp = string.Empty;

        //    for (int i = 0; i < enteredSymbols.Length; i++)
        //    {
        //        if (Convert.ToInt32(enteredSymbols[i]) >= 48 && Convert.ToInt32(enteredSymbols[i]) <= 57)
        //        {
        //            temp += enteredSymbols[i];
        //        }
        //    }
        //    tb_countSpar.Text = temp;

        //    if (temp == string.Empty)
        //        return;

        //    generateFunc.countSpar = int.Parse(temp);
        //}

        //private void cb_assembly_Click(object sender, RoutedEventArgs e)
        //{

        //    if (generateFunc.isAssembly)
        //    {
        //        tb_countSpar.Visibility = Visibility.Collapsed;
        //        lb_countSpar.Visibility = Visibility.Collapsed;
        //        tb_stepMissing.Visibility = Visibility.Collapsed;
        //        lb_stepMissing.Visibility = Visibility.Collapsed;
        //        generateFunc.isAssembly = false;
        //    }
        //   else
        //    {
        //        tb_countSpar.Text = generateFunc.countSpar.ToString();
        //        tb_stepMissing.Text = generateFunc.stepMissing.ToString();
        //        tb_countSpar.Visibility = Visibility.Visible;
        //        lb_countSpar.Visibility = Visibility.Visible;
        //        tb_stepMissing.Visibility = Visibility.Visible;
        //        lb_stepMissing.Visibility = Visibility.Visible;
        //        generateFunc.isAssembly = true;
        //    }
        //}
    }
}
