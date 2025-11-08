using System;

using System.Net;

using System.Windows;

using Kompas6API5;
using Microsoft.Win32;
using System.Xml;
using Modeling.Pages;
using Modeling.Constructors;
using Modeling.Windows;
using System.Threading.Tasks;
using Modeling.generationFunctions;

namespace Modeling
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            functionalityCheck();
        }


        internal static allConstruct allConstruct = new allConstruct();
        internal static generateFunc generateFunc = new generateFunc(); 
        public async void functionalityCheck()
        {
            generateFunc.clearDirectory();

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ASCON\KOMPAS-3D");


            if (key == null)
            {
                MessageBox.Show("KOMPAS-3D не установлен!");
                return;
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("material.xml");

            XmlNodeList nameNodes = xmlDoc.SelectNodes("//name");
            XmlNodeList densityNodes = xmlDoc.SelectNodes("//density");


            string[,] names = new string[nameNodes.Count, 2];
            for (int i = 0; i < nameNodes.Count; i++)
            {
                names[i,0] = WebUtility.HtmlDecode(nameNodes[i].InnerText);
                names[i,1] = WebUtility.HtmlDecode(densityNodes[i].InnerText);
            }

            KompasObject kompasObject = new Kompas6API5.Application();
            generateFunc.kompas = kompasObject;
            allConstruct.material = names;
            Random random = new Random();
            await Task.Delay(random.Next(1000, 2750));
            int width = (int)SystemParameters.PrimaryScreenWidth;
            int height = (int)SystemParameters.PrimaryScreenHeight;
            bool resolutionSupported = false;

            for (int i = 0; i < supportedScreenSize.GetLength(0); i++)
            {
                if (supportedScreenSize[i, 0] == width.ToString() && supportedScreenSize[i, 1] == height.ToString())
                {
                    resolutionSupported = true;
                    break;
                }
            }
            allConstruct.setUserInfo(Environment.MachineName, Environment.UserName);
            allConstruct.setResolutionInfo(resolutionSupported, width, height);


            generateSparWindow generateSparWindow = new generateSparWindow();
            generateSparWindow.Show();
            this.Close();
        }

        public string[,] supportedScreenSize = new string[,]
       {
            { "800", "600" , "", "" , "580", "541"},
            { "1024", "768" , "", "" , "802", "712"},
            { "1152", "864" , "", "", "961", "803" },
            {"1176", "664", "", "", "983", "605"},
            {"1280", "720", "", "", "1088", "662"},
            {"1280", "768", "", "", "1087", "711"},
            {"1280", "800", "", "", "1087", "744"},
            {"1280", "960", "", "", "1087", "903"},
            {"1280", "1024", "", "", "1088", "967"},
            {"1360", "768", "",  "", "1167", "711"},
            {"1366", "768", "", "", "1172", "710"},
            {"1600", "900", "", "", "1408", "842"},
            {"1600", "1024", "", "", "1407", "967"},
            {"1600", "1200", "", "", "1407", "1144"},
            {"1680", "1050", "", "", "1487", "992"},
            {"1920", "1080", "1124", "572", "1726", "1022"},
            {"1920", "1200", "", "", "1728", "1143"},
            {"1920", "1440", "", "", "1730", "1383"},
            { "2560", "1440", "1444", "752", "2372", "1385" },
       };
    }
}
