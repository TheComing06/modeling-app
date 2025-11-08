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
using Modeling.Constructors;
using Modeling.generationFunctions;
using Modeling.Pages;

namespace Modeling.Windows
{
    /// <summary>
    /// Логика взаимодействия для searchMaterial.xaml
    /// </summary>
    public partial class searchMaterial : Window
    {
        private allConstruct allConstruct;
        private generateFunc generateFunc;
        private string[] nameMaterial = new string[95];
        private string[] densityMaterial = new string[95];
        private readonly string _component; // Идентификатор компонента
        private readonly generateWholeSpar _parentPage; // Ссылка на страницу generateWholeSpar

        public searchMaterial(string component, generateWholeSpar parentPage)
        {
            InitializeComponent();
            clearComboBox();

            _component = component; // Сохраняем компонент (например, "Lonj", "Skin", "Rivets", "Ribs")
            _parentPage = parentPage; // Сохраняем ссылку на родительскую страницу

            allConstruct = MainWindow.allConstruct;
            generateFunc = MainWindow.generateFunc;

            for (int i = 0; i < nameMaterial.Length; i++)
            {
                nameMaterial[i] = allConstruct.material[i, 0];
                densityMaterial[i] = allConstruct.material[i, 1];
            }

            cmb_material.ItemsSource = nameMaterial;
        }

        private void cmb_material_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно добавить предварительную обработку выбора
        }

        public void clearComboBox()
        {
            cmb_material.ItemsSource = null;
            cmb_material.Items.Clear();
        }

        private void tb_searchMaterial_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tb_searchMaterial.Text.Trim().Length <= 1)
            {
                clearComboBox();
                cmb_material.ItemsSource = nameMaterial;
            }
            else
            {
                var searchedItems = nameMaterial.Where(material => material.Contains(tb_searchMaterial.Text)).ToArray();
                clearComboBox();
                cmb_material.ItemsSource = searchedItems;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cmb_material.SelectedItem != null)
            {
                string selectedMaterial = cmb_material.SelectedItem as string;
                double selectedMaterialDensity = 0;

                for (int i = 0; i < nameMaterial.Length; i++)
                {
                    if (nameMaterial[i] == selectedMaterial)
                    {
                        selectedMaterialDensity = double.Parse(densityMaterial[i].Replace(".", ","));
                        break;
                    }
                }

                // Передача выбранного материала в родительскую страницу
                switch (_component)
                {
                    case "Skin":
                        _parentPage.SelectedMaterialSkin = selectedMaterial;
                        break;
                    case "Lonj":
                        _parentPage.SelectedMaterialLonj = selectedMaterial;
                        break;
                    case "Rivets":
                        _parentPage.SelectedMaterialRivets = selectedMaterial;
                        break;
                    case "Ribs":
                        _parentPage.SelectedMaterialRibs = selectedMaterial;
                        break;
                }

                this.Close(); // Закрываем окно
            }
        }
    }
}