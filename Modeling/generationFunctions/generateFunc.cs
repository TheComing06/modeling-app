using Kompas6API5;
using Kompas6Constants;
using Kompas6Constants3D;
using KompasAPI7;
using Modeling.Class;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Modeling.generationFunctions
{
    internal class generateFunc
    {
        public KompasObject kompas { get; set; }
        public string selectedMaterial { get; set; }
        public double selectedMaterialDensity { get; set; }

        public bool isAssembly { get; set; }

        public int countSpar { get; set; }
        public double stepMissing { get; set; }

        public static string[] nameFiles;


        private string assemblyPath;

        public ksDocument3D createDocuments(KompasObject kompas, bool isAssembly)
        {
            ksDocument3D doc3D = kompas.Document3D();
            if (isAssembly)
                doc3D.Create(false, false);
            else
                doc3D.Create();
            return doc3D;
        }

        public string returnPath(int typeGenerator) 
        {
            string directoryPath;
            string fileName;
            int countFiles = 0;

            if (typeGenerator == 0) //временный лонжерон
            {
                directoryPath = "NoEditor/3dDocuments/Temp/";
                fileName = "ВременныйЛонжерон№";
            }
            else if (typeGenerator == 1) //лонжерон
            {
                directoryPath = "NoEditor/3dDocuments/";
                fileName = "Лонжерон№";
                if (Directory.Exists("NoEditor/3dDocuments/Temp/"))
                    countFiles -= 1;
            }
            else // сборка
            {
                directoryPath = "NoEditor/3dDocuments/Assembly/";
                fileName = "Сборка№";
            }

            checkDirectory(directoryPath);

            string resultPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            int ind = resultPath.Length - 12;
            resultPath = resultPath.Remove(ind);
            resultPath += directoryPath;
            countFiles += new DirectoryInfo(resultPath).GetFiles().Length + 1;
            resultPath += typeGenerator == 2 ? $"{fileName}{countFiles}.a3d" : $"{fileName}{countFiles}.m3d";

            return resultPath;
        }

   

        public void clearDirectory()
        {
            cmdCommand("taskkill /PID KOMPAS.exe", false);

            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string tempPath = path.Remove(path.Length - 12) + @"NoEditor\3dDocuments\Temp\";

            cmdCommand("rd /s /q " + tempPath, false);

            assemblyPath = path.Remove(path.Length - 12) + @"NoEditor\3dDocuments\Assembly\";

            cmdCommand("rd /s /q " + assemblyPath, false);
        }

        public void cmdCommand(string command, bool shellExecute){

            var proc = new ProcessStartInfo()
            {
                UseShellExecute = shellExecute,
                WorkingDirectory = @"C:\Windows\System32",
                FileName = @"C:\Windows\System32\cmd.exe",
                Arguments = "/c " + command,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(proc);
        }


        private void checkDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public void buildMode(double sparInnerWidth)
        {
            if(isAssembly)
            {
                try
                {
                    string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    path = path.Remove(path.Length - 12) + @"NoEditor\3dDocuments\Temp\";
                    nameFiles = Directory.GetFiles(path).Where(name => name.Contains("ВременныйЛонжерон")).ToArray();

                    kompas.ActivateControllerAPI();

                    ksDocument3D doc3D = createDocuments(kompas, true);

                    for (int i = 0; i < countSpar; i++)
                    {
                        ksPart part = (ksPart)doc3D.GetPart((short)Part_Type.pTop_Part);

                        part.SetMaterial(selectedMaterial, selectedMaterialDensity);

                        ksPlacement placement = (ksPlacement)part.GetPlacement();

                        placement.SetPlacement((short)Obj3dType.o3d_placement);

                        placement.SetOrigin(0, i == 0 ? 0 : (stepMissing + sparInnerWidth) * i, 0);

                        doc3D.SetPartFromFile(nameFiles[0], part, false);

                        part.Update();
                    }

                    checkDirectory(assemblyPath);
                    assemblyPath = returnPath(2);
                    doc3D.SaveAs(assemblyPath);
                }
                catch(Exception ex)
                {
                   loggingActions loggingActions = new loggingActions();
                    loggingActions.loggingFunction(ex.Message, true);
                }
            }
            kompas.Visible = true;

            kompas.ksMessage($"Сборка сохранена по пути - {assemblyPath.Replace("/", @"\")}\nЕсли сборка необходима для дальнейшей работы, то скопируйте ее в другую директорию.");
        }
    }
}
