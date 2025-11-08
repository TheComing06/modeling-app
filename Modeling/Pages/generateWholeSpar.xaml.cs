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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Modeling.Constructors;
using Modeling.Windows;
using Kompas6API5;
using Kompas6Constants3D;
using Modeling.generationFunctions;
using Modeling.Class;
using System.ComponentModel;

namespace Modeling.Pages
{

    public partial class generateWholeSpar : Page, INotifyPropertyChanged
    {
        private KompasObject _kompas;
        private ksDocument3D doc3D;
        private allConstruct allConstruct;
        private generateFunc generateFunc;
        private loggingActions loggingActions = new loggingActions();

        private double extraction;
        private double sparUpperParallel;
        private double sparLowerParallel;
        private double sparInnerWidth;
        private double sparLowerHeight;
        private double sparUpperHeight;

        private readonly WingPreviewRenderer _previewRenderer;
        private readonly RivetGenerators _rivetGenerator;
        private readonly RibGenerator _ribGenerator;
        private readonly WingtipGenerator _wingtipGenerator;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static readonly Dictionary<int, (double m, double k1)> Naca5Series = new Dictionary<int, (double, double)>
        {
            { 210, (0.0580, 361.400) },
            { 220, (0.1260, 51.640) },
            { 230, (0.1260, 15.957) },
            { 240, (0.1300, 6.643) },
            { 250, (0.1300, 3.230) }
        };

        private string _selectedMaterialLonj;
        private string _selectedMaterialSkin;
        private string _selectedMaterialRivets;
        private string _selectedMaterialRibs;
        public string SelectedMaterialLonj
        {
            get => _selectedMaterialLonj;
            set
            {
                _selectedMaterialLonj = value;
                OnPropertyChanged(nameof(SelectedMaterialLonj));
                tbMaterialLonj.Text = value;
            }
        }
        public string SelectedMaterialSkin
        {
            get => _selectedMaterialSkin;
            set
            {
                _selectedMaterialSkin = value;
                OnPropertyChanged(nameof(SelectedMaterialSkin));
                tbMaterialSkin.Text = value;
            }
        }
        public string SelectedMaterialRivets
        {
            get => _selectedMaterialRivets;
            set
            {
                _selectedMaterialRivets = value;
                OnPropertyChanged(nameof(SelectedMaterialRivets));
                tbMaterialRivets.Text = value;
            }
        }
        public string SelectedMaterialRibs
        {
            get => _selectedMaterialRibs;
            set
            {
                _selectedMaterialRibs = value;
                OnPropertyChanged(nameof(SelectedMaterialRibs));
                tbMaterialRibs.Text = value;
            }
        }

        private readonly StringBuilder _errorMessages = new StringBuilder();

        public generateWholeSpar()
        {
            InitializeComponent();
            InitializeKompas();
            _previewRenderer = new WingPreviewRenderer();
            _rivetGenerator = new RivetGenerators();
            _ribGenerator = new RibGenerator(CreateOffsetPlane);
            _wingtipGenerator = new WingtipGenerator();
        }

        private void InitializeKompas()
        {
            try
            {
                _kompas = (KompasObject)Activator.CreateInstance(Type.GetTypeFromProgID("KOMPAS.Application.5"));
                _kompas.Visible = true;
            }
            catch
            {
                MessageBox.Show("Ошибка инициализации Kompas-3D");
            }
        }

        private bool ValidateInputs()
        {
            _errorMessages.Clear();
            if (!int.TryParse(tbSparCount.Text, out int sparCount) || sparCount <= 0)
                _errorMessages.AppendLine("Неверное количество лонжеронов");
            if (!double.TryParse(tbUpperFlangeThickness.Text, out double upperFlangeThickness) || upperFlangeThickness <= 0)
                _errorMessages.AppendLine("Неверная толщина верхнего пояса");
            if (!double.TryParse(tbLowerFlangeThickness.Text, out double lowerFlangeThickness) || lowerFlangeThickness <= 0)
                _errorMessages.AppendLine("Неверная толщина нижнего пояса");
            if (!double.TryParse(tbUpperFlangeHeight.Text, out double upperFlangeHeight) || upperFlangeHeight <= 0)
                _errorMessages.AppendLine("Неверная высота верхнего пояса");
            if (!double.TryParse(tbLowerFlangeHeight.Text, out double lowerFlangeHeight) || lowerFlangeHeight <= 0)
                _errorMessages.AppendLine("Неверная высота нижнего пояса");
            if (!double.TryParse(tbInternalWidth.Text, out double internalWidth) || internalWidth <= 0)
                _errorMessages.AppendLine("Неверная внутренняя ширина");
            if (!double.TryParse(tbExtrusion.Text, out double extrusion) || extrusion <= 0)
                _errorMessages.AppendLine("Неверная длина выдавливания");

            var positions = tbSparPositions.Text.Split(',').Select(s => s.Trim()).ToList();
            if (positions.Count != sparCount || !positions.All(p => double.TryParse(p, out double pos) && pos >= 0 && pos <= 100))
                _errorMessages.AppendLine("Неверные позиции лонжеронов (в % от хорды, разделены запятыми)");

            if (!int.TryParse(tbNacaCode.Text, out int nacaCode) || nacaCode < 0)
                _errorMessages.AppendLine("Неверный код NACA");
            bool is4Digit = cbNacaType.SelectedItem?.ToString().Contains("4-Digit") == true;
            if (!is4Digit && !Naca5Series.ContainsKey(nacaCode / 100))
                _errorMessages.AppendLine($"Неверный код NACA для 5-значного профиля. Поддерживаемые серии: {string.Join(", ", Naca5Series.Keys)}xx");


            if (!double.TryParse(tbChordLength.Text, out double chordLength) || chordLength <= 0)
                _errorMessages.AppendLine("Неверная длина хорды");
            if (!double.TryParse(tbWingSpan.Text, out double wingSpan) || wingSpan <= 0)
                _errorMessages.AppendLine("Неверный размах крыла");

            if (!double.TryParse(tbRivetSpacing.Text, out double rivetSpacing) || rivetSpacing <= 0)
                _errorMessages.AppendLine("Неверный шаг заклёпок");
            if (!double.TryParse(tbRivetDiameter.Text, out double rivetDiameter) || rivetDiameter <= 0)
                _errorMessages.AppendLine("Неверный диаметр заклёпок");
            if (!double.TryParse(tbRivetHeight.Text, out double rivetHeight) || rivetHeight <= 0)
                _errorMessages.AppendLine("Неверная высота заклёпок");
            if (!double.TryParse(tbSkinThickness.Text, out double skinThickness) || skinThickness <= 0)
                _errorMessages.AppendLine("Неверная толщина обшивки");

            if (_errorMessages.Length > 0)
            {
                MessageBox.Show(_errorMessages.ToString());
                return false;
            }

            return true;
        }


        private (double[] xUpper, double[] yUpper, double[] xLower, double[] yLower) GenerateAirfoil(int nacaCode, bool is4Digit, double chordLength)
        {
            const int pointsCount = 100;
            double[] xUpper = new double[pointsCount];
            double[] yUpper = new double[pointsCount];
            double[] xLower = new double[pointsCount];
            double[] yLower = new double[pointsCount];

            if (is4Digit)
            {
                int m = nacaCode / 1000;
                int p = (nacaCode / 100) % 10;
                int t = nacaCode % 100;
                double m_d = m / 100.0;
                double p_d = p / 10.0;
                double t_d = t / 100.0;

                double[] x = new double[pointsCount];
                double[] y_c = new double[pointsCount];
                double[] dy_c = new double[pointsCount];
                double[] y_t = new double[pointsCount];

                for (int i = 0; i < pointsCount; i++)
                {
                    x[i] = (double)i / (pointsCount - 1);
                    if (x[i] < p_d)
                    {
                        y_c[i] = m_d / (p_d * p_d) * (2 * p_d * x[i] - x[i] * x[i]);
                        dy_c[i] = 2 * m_d / (p_d * p_d) * (p_d - x[i]);
                    }
                    else
                    {
                        y_c[i] = m_d / ((1 - p_d) * (1 - p_d)) * (1 - 2 * p_d + 2 * p_d * x[i] - x[i] * x[i]);
                        dy_c[i] = 2 * m_d / ((1 - p_d) * (1 - p_d)) * (p_d - x[i]);
                    }
                    y_t[i] = 5 * t_d * (0.2969 * Math.Sqrt(x[i]) - 0.1260 * x[i] - 0.3516 * x[i] * x[i] +
                                        0.2843 * x[i] * x[i] * x[i] - 0.1015 * x[i] * x[i] * x[i] * x[i]);
                }

                for (int i = 0; i < pointsCount; i++)
                {
                    double theta = Math.Atan(dy_c[i]);
                    xUpper[i] = (x[i] - y_t[i] * Math.Sin(theta)) * chordLength;
                    yUpper[i] = (y_c[i] + y_t[i] * Math.Cos(theta)) * chordLength;
                    xLower[i] = (x[i] + y_t[i] * Math.Sin(theta)) * chordLength;
                    yLower[i] = (y_c[i] - y_t[i] * Math.Cos(theta)) * chordLength;
                }
            }
            else
            {
                int series = nacaCode / 100;
                int cl = nacaCode / 1000;
                int p = (nacaCode / 100) % 10;
                int t = nacaCode % 100;

                if (!Naca5Series.ContainsKey(series))
                {
                    throw new ArgumentException($"Серия NACA {series}xx не поддерживается. Поддерживаемые серии: {string.Join(", ", Naca5Series.Keys)}");
                }

                var (m, k1) = Naca5Series[series];
                double t_d = t / 100.0;

                double[] x = new double[pointsCount];
                double[] y_c = new double[pointsCount];
                double[] dy_c = new double[pointsCount];
                double[] y_t = new double[pointsCount];

                for (int i = 0; i < pointsCount; i++)
                {
                    x[i] = (double)i / (pointsCount - 1);
                    if (x[i] < m)
                    {
                        y_c[i] = (k1 / 6.0) * (Math.Pow(x[i], 3) - 3 * m * x[i] * x[i] + m * m * (3 - m) * x[i]);
                        dy_c[i] = (k1 / 6.0) * (3 * Math.Pow(x[i], 2) - 6 * m * x[i] + m * m * (3 - m));
                    }
                    else
                    {
                        y_c[i] = (k1 / 6.0) * m * m * m * (1 - x[i]);
                        dy_c[i] = -(k1 / 6.0) * m * m * m;
                    }

                    y_t[i] = 5 * t_d * (0.2969 * Math.Sqrt(x[i]) - 0.1260 * x[i] - 0.3516 * x[i] * x[i] +
                                        0.2843 * x[i] * x[i] * x[i] - 0.1015 * x[i] * x[i] * x[i] * x[i]);
                }

                for (int i = 0; i < pointsCount; i++)
                {
                    double theta = Math.Atan(dy_c[i]);
                    xUpper[i] = (x[i] - y_t[i] * Math.Sin(theta)) * chordLength;
                    yUpper[i] = (y_c[i] + y_t[i] * Math.Cos(theta)) * chordLength;
                    xLower[i] = (x[i] + y_t[i] * Math.Sin(theta)) * chordLength;
                    yLower[i] = (y_c[i] - y_t[i] * Math.Cos(theta)) * chordLength;
                }
            }

            return (xUpper, yUpper, xLower, yLower);
        }

        private void GenerateWing()
        {
            try
            {
                double chordLength = double.Parse(tbChordLength.Text);
                double wingSpan = double.Parse(tbWingSpan.Text);
                int nacaCode = int.Parse(tbNacaCode.Text);
                bool is4Digit = cbNacaType.SelectedItem?.ToString().Contains("4-Digit") == true;
                var (xUpper, yUpper, xLower, yLower) = GenerateAirfoil(nacaCode, is4Digit, chordLength);

                double upperFlangeThickness = double.Parse(tbUpperFlangeThickness.Text);
                double lowerFlangeThickness = double.Parse(tbLowerFlangeThickness.Text);
                double upperFlangeHeight = double.Parse(tbUpperFlangeHeight.Text);
                double lowerFlangeHeight = double.Parse(tbLowerFlangeHeight.Text);
                double internalWidth = double.Parse(tbInternalWidth.Text);
                double extrusion = double.Parse(tbExtrusion.Text);
                var sparPositions = tbSparPositions.Text.Split(',').Select(p => double.Parse(p) / 100 * chordLength).ToList();

                double wingtipThickness = 10.0;
                double wingletHeight = chordLength * 0.2;

                ksDocument3D doc3D = (ksDocument3D)_kompas.Document3D();
                doc3D.Create(false, true);
                ksPart part = (ksPart)doc3D.GetPart((short)Part_Type.pTop_Part);

                ksEntity skinSketch = (ksEntity)part.NewEntity((short)Obj3dType.o3d_sketch);
                ksSketchDefinition skinSketchDef = (ksSketchDefinition)skinSketch.GetDefinition();
                skinSketchDef.SetPlane(part.GetDefaultEntity((short)Obj3dType.o3d_planeXOZ));
                skinSketch.Create();
                
                //Отрисовка профиля крыла в эскизе
                ksDocument2D skinSketchEdit = (ksDocument2D)skinSketchDef.BeginEdit();
                for (int i = 0; i < xUpper.Length - 1; i++)
                    skinSketchEdit.ksLineSeg(xUpper[i], yUpper[i], xUpper[i + 1], yUpper[i + 1], 1);
                
                for (int i = xLower.Length - 1; i > 0; i--)
                    skinSketchEdit.ksLineSeg(xLower[i], yLower[i], xLower[i - 1], yLower[i - 1], 1);
                
                skinSketchEdit.ksLineSeg(xUpper[0], yUpper[0], xLower[0], yLower[0], 1);
                skinSketchEdit.ksLineSeg(xUpper[xUpper.Length - 1], yUpper[yUpper.Length - 1], xLower[xLower.Length - 1], yLower[yLower.Length - 1], 1);
                skinSketchDef.EndEdit();

                //Создание 3д объекта
                ksEntity skinExtrude = (ksEntity)part.NewEntity((short)Obj3dType.o3d_bossExtrusion);
                ksBossExtrusionDefinition extDef = (ksBossExtrusionDefinition)skinExtrude.GetDefinition();
                extDef.directionType = (short)Direction_Type.dtNormal;
                extDef.SetSideParam(true, (short)End_Type.etBlind, wingSpan);
                extDef.SetSketch(skinSketch);
                skinExtrude.Create();

                foreach (double pos in sparPositions)
                {
                    int idx = (int)(pos / chordLength * (xUpper.Length - 1));
                    double sparHeight = yUpper[idx] - yLower[idx];
                    if (sparHeight < upperFlangeHeight + lowerFlangeHeight)
                    {
                        MessageBox.Show($"Лонжерон на позиции {pos:F2} не помещается в обшивку (толщина профиля {sparHeight:F2} < {upperFlangeHeight + lowerFlangeHeight:F2})");
                        return;
                    }

                    double centerY = (yUpper[idx] + yLower[idx]) / 2;

                    ksEntity sparSketch = (ksEntity)part.NewEntity((short)Obj3dType.o3d_sketch);
                    ksSketchDefinition sparSketchDef = (ksSketchDefinition)sparSketch.GetDefinition();
                    sparSketchDef.SetPlane(part.GetDefaultEntity((short)Obj3dType.o3d_planeXOZ));
                    if (!sparSketch.Create())
                    {
                        MessageBox.Show($"Ошибка: Не удалось создать эскиз лонжерона на позиции {pos:F2}");
                        continue;
                    }
                    ksDocument2D sparSketchEdit = (ksDocument2D)sparSketchDef.BeginEdit();
                    double halfHeight = (sparHeight - upperFlangeHeight - lowerFlangeHeight) / 2;
                    double halfWidth = internalWidth / 2;

                    double[] xPoints = new double[]
                    {
                        pos - lowerFlangeThickness / 2, pos - lowerFlangeThickness / 2, pos - halfWidth, pos - halfWidth,
                        pos - upperFlangeThickness / 2, pos - upperFlangeThickness / 2, pos + upperFlangeThickness / 2,
                        pos + upperFlangeThickness / 2, pos + halfWidth, pos + halfWidth, pos + lowerFlangeThickness / 2,
                        pos + lowerFlangeThickness / 2
                    };

                    double[] yPoints = new double[]
                    {
                        centerY - halfHeight - lowerFlangeHeight, centerY - halfHeight, centerY - halfHeight, centerY + halfHeight,
                        centerY + halfHeight, centerY + halfHeight + upperFlangeHeight, centerY + halfHeight + upperFlangeHeight,
                        centerY + halfHeight, centerY + halfHeight, centerY - halfHeight, centerY - halfHeight,
                        centerY - halfHeight - lowerFlangeHeight
                    };

                    for (int i = 0; i < xPoints.Length - 1; i++)
                    {
                        sparSketchEdit.ksLineSeg(xPoints[i], yPoints[i], xPoints[i + 1], yPoints[i + 1], 1);
                    }
                    sparSketchEdit.ksLineSeg(xPoints[xPoints.Length - 1], yPoints[yPoints.Length - 1], xPoints[0], yPoints[0], 1);
                    sparSketchDef.EndEdit();

                    ksEntity sparExtrude = (ksEntity)part.NewEntity((short)Obj3dType.o3d_bossExtrusion);
                    ksBossExtrusionDefinition sparExtrudeDef = (ksBossExtrusionDefinition)sparExtrude.GetDefinition();
                    sparExtrudeDef.directionType = (short)Direction_Type.dtNormal;
                    sparExtrudeDef.SetSideParam(true, (short)End_Type.etBlind, extrusion);
                    sparExtrudeDef.SetSketch(sparSketch);
                    if (!sparExtrude.Create())
                    {

                        MessageBox.Show($"Ошибка: Не удалось выполнить выдавливание лонжерона на позиции {pos:F2}");
                        continue;
                    }
                }

                List<double> ribPositions = new List<double>();
                if (!double.TryParse(tbRibSpacing.Text, out double ribSpacing) || ribSpacing <= 0)
                {
                    MessageBox.Show("Неверный шаг нервюр");
                    return;
                }

                string arrangement = (cbRibArrangement.SelectedItem as ComboBoxItem)?.Content.ToString();
                switch (arrangement)
                {
                    case "Равномерный":
                        for (double pos = 0; pos <= wingSpan; pos += ribSpacing)
                            ribPositions.Add(pos);
                        break;
                    case "Уплотненный в корне":
                        for (double pos = 0; pos <= wingSpan; pos += ribSpacing * (1 - 0.5 * pos / wingSpan))
                            ribPositions.Add(pos);
                        break;
                    case "Уплотненный на концах":
                        for (double pos = 0; pos <= wingSpan; pos += ribSpacing * (0.5 + 0.5 * pos / wingSpan))
                            ribPositions.Add(pos);
                        break;
                }

                _ribGenerator.GenerateRibs(
                    part,
                    chordLength,
                    wingSpan,
                    xUpper,
                    yUpper,
                    xLower,
                    yLower,
                    tbRibSpacing,
                    tbRibThickness,
                    cbRibArrangement);

                _rivetGenerator.GenerateRivets(
                    part,
                    sparPositions,
                    ribPositions,
                    chordLength,
                    wingSpan,
                    xUpper,
                    yUpper,
                    xLower,
                    yLower,
                    "Riveting", //доделать
                    tbRivetDiameter,
                    tbRivetSpacing,
                    tbRivetHeight,
                    tbSkinThickness,
                    cbRivetPattern,
                    cbRivetLocation,
                    cbRivetType);

                _wingtipGenerator.GenerateWingtip(
                    part,
                    wingSpan,
                    xUpper,
                    yUpper,
                    xLower,
                    yLower,
                    wingtipThickness
                    );

                MessageBox.Show("Крыло успешно сгенерировано");
                part.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации: {ex.Message}");
            }
        }

        private ksEntity CreateOffsetPlane(ksPart part, double offset)
        {
            ksEntity plane = (ksEntity)part.NewEntity((short)Obj3dType.o3d_planeOffset);
            ksPlaneOffsetDefinition planeDef = (ksPlaneOffsetDefinition)plane.GetDefinition();
            planeDef.SetPlane(part.GetDefaultEntity((short)Obj3dType.o3d_planeXOZ));
            planeDef.direction = true;
            planeDef.offset = offset;
            plane.Create();
            return plane;
        }

        private void btn_searchSkin_Click(object sender, RoutedEventArgs e)
        {
            var searchWindow = new searchMaterial("Skin", this);
            searchWindow.ShowDialog();
        }

        private void btn_searchLonj_Click(object sender, RoutedEventArgs e)
        {
            var searchWindow = new searchMaterial("Lonj", this);
            searchWindow.ShowDialog();
        }

        private void btn_searchRivets_Click(object sender, RoutedEventArgs e)
        {
            var searchWindow = new searchMaterial("Rivets", this);
            searchWindow.ShowDialog();
        }

        private void btn_searchRibs_Click(object sender, RoutedEventArgs e)
        {
            var searchWindow = new searchMaterial("Ribs", this);
            searchWindow.ShowDialog();
        }

        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;
            _previewRenderer.DrawPreview(
                previewCanvas,
                tbChordLength,
                tbNacaCode,
                cbNacaType,
                tbSparPositions,
                tbWingSpan,
                tbRibSpacing,
                tbRivetSpacing,
                tbRivetDiameter,
                cbRivetLocation);
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;
            GenerateWing();
        }

        private void btnNacaCodeHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
        "Код NACA определяет форму профиля крыла:\n" +
        "- Для 4-значного кода (например, 2412):\n" +
        "  * Первая цифра (2) — максимальный изгиб (m) в процентах от хорды (2%).\n" +
        "  * Вторая цифра (4) — обозначает расстояние точки максимальной кривизны от передней кромки (p) в десятках процентов от хорды (40%).\n" +
        "  * Последние две цифры (12) — максимальная толщина профиля (t) в процентах от хорды (12%).\n" +
        "- Для 5-значного кода (например, 23012):\n" +
        "  * Первые две цифры (23) — серия профиля.\n" +
        "  * Первая цифра (2) — коэффициент подъёмной силы (cl) в десятых долях (0.2).\n" +
        "  * Вторая цифра (3) — положение максимального изгиба (p) в долях хорды (0.15).\n" +
        "  * Последние две цифры (12) — максимальная толщина профиля (t) в процентах от хорды (12%).\n" +
         "  * В данной версии реализованы серии профилей с 210хх 09 250хх.\n",
        "Пояснение: Код NACA");
        }

        private void btnLiftCoefficientHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
        "Коэффициент подъёмной силы (cl):\n" +
        "- Используется для 5-значных профилей NACA.\n" +
        "- Определяет подъёмную силу профиля крыла.\n" +
        "- Значение обычно находится в диапазоне от 0.0 до 3.0.\n" +
        "- Если оставить поле пустым, используется значение из кода NACA (например, для NACA 23012 — cl = 0.2).",
        "Пояснение: Коэффициент подъёмной силы (cl)");
        }

        private void btnCamberPositionHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
        "Положение максимального изгиба (p):\n" +
        "- Используется для 5-значных профилей NACA.\n" +
        "- Определяет, где вдоль хорды (в долях от 0 до 1) находится точка максимального изгиба профиля.\n" +
        "- Значение обычно находится в диапазоне от 0.05 до 0.3.\n" +
        "- Если оставить поле пустым, используется значение из кода NACA (например, для NACA 23012 — p = 0.15).",
        "Пояснение: Положение максимального изгиба (p)");
        }
    }
}