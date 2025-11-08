using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace Modeling.Class
{
    //класс для создания обшивки, тут все делается на основе naca кода, 4 значный полностью рабочий,
    //а вот 5 значный фигня какая то, некоторые значения лдя расчета уже определены в Dictionary что не круто,
    //но работает и ладно
    class WingPreviewRenderer
    {
        private static readonly Dictionary<int, (double m, double k1)> Naca5Series = new Dictionary<int, (double, double)>
        {
            { 210, (0.0580, 361.400) },
            { 220, (0.1260, 51.640) },
            { 230, (0.1260, 15.957) },
            { 240, (0.1300, 6.643) },
            { 250, (0.1300, 3.230) }
        };

        // Метод для генерации координат профиля крыла
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
                    throw new ArgumentException($"Серия NACA {series}xx не поддерживается. Поддерживаемые серии: {string.Join(", ", Naca5Series.Keys)}xx");
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

        // Основной метод отрисовки превью
        public void DrawPreview(
            Canvas previewCanvas,
            TextBox tbChordLength,
            TextBox tbNacaCode,
            ComboBox cbNacaType,
            TextBox tbSparPositions,
            TextBox tbWingSpan,
            TextBox tbRibSpacing,
            TextBox tbRivetSpacing,
            TextBox tbRivetDiameter
            , ComboBox cbRivetLocation)
        {
            previewCanvas.Children.Clear();

            double chordLength = double.Parse(tbChordLength.Text);
            int nacaCode = int.Parse(tbNacaCode.Text);
            bool is4Digit = cbNacaType.SelectedItem?.ToString().Contains("4-Digit") == true;

            var (xUpper, yUpper, xLower, yLower) = GenerateAirfoil(nacaCode, is4Digit, chordLength);

            // Масштабирование для отображения на Canvas
            double canvasWidth = previewCanvas.ActualWidth;
            double canvasHeight = previewCanvas.ActualHeight;
            double scale = Math.Min(canvasWidth / chordLength, canvasHeight / (yUpper.Max() - yLower.Min()));

            // Отрисовка профиля крыла
            var airfoilPath = new Path
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = new Point(xLower[0] * scale, -yLower[0] * scale + canvasHeight / 2) };
            for (int i = 1; i < xLower.Length; i++)
                figure.Segments.Add(new LineSegment(new Point(xLower[i] * scale, -yLower[i] * scale + canvasHeight / 2), true));
            for (int i = xUpper.Length - 1; i >= 0; i--)
                figure.Segments.Add(new LineSegment(new Point(xUpper[i] * scale, -yUpper[i] * scale + canvasHeight / 2), true));
            geometry.Figures.Add(figure);
            airfoilPath.Data = geometry;
            previewCanvas.Children.Add(airfoilPath);

            // Отрисовка лонжеронов
            var positions = tbSparPositions.Text.Split(',').Select(double.Parse).ToList();
            foreach (var pos in positions)
            {
                double x = pos / 100 * chordLength;
                int idx = (int)(x / chordLength * (xUpper.Length - 1));
                double sparHeight = yUpper[idx] - yLower[idx];
                var sparRect = new Rectangle
                {
                    Width = 5,
                    Height = sparHeight * scale,
                    Fill = Brushes.Red,
                    Opacity = 0.5
                };
                Canvas.SetLeft(sparRect, x * scale - 2.5);
                Canvas.SetTop(sparRect, -yUpper[idx] * scale + canvasHeight / 2);
                previewCanvas.Children.Add(sparRect);
            }

            // Проверка входных данных для заклёпок и нервюр
            if (!double.TryParse(tbRivetSpacing.Text, out double rivetSpacing) || rivetSpacing <= 0 ||
                !double.TryParse(tbRivetDiameter.Text, out double rivetDiameter) || rivetDiameter <= 0)
            {
                return; // Прекращаем отрисовку заклёпок, если данные некорректны
            }

            // Определение расположения заклёпок
            string rivetLocation = (cbRivetLocation.SelectedItem as ComboBoxItem)?.Content.ToString();
            bool drawOnSpars = rivetLocation == "Лонжероны" || rivetLocation == "Лонжероны и нервюры";
            bool drawOnRibs = rivetLocation == "Нервюры" || rivetLocation == "Лонжероны и нервюры";

            // Отрисовка заклёпок на лонжеронах (только сверху и снизу)
            if (drawOnSpars)
            {
                foreach (var pos in positions)
                {
                    double x = pos / 100 * chordLength;
                    int idx = (int)(x / chordLength * (xUpper.Length - 1));
                    double scaledRivetDiameter = rivetDiameter * scale;

                    // Заклёпка на верхней поверхности
                    var upperRivet = new Ellipse
                    {
                        Width = scaledRivetDiameter,
                        Height = scaledRivetDiameter,
                        Fill = Brushes.Green,
                        Opacity = 0.7
                    };
                    Canvas.SetLeft(upperRivet, x * scale - scaledRivetDiameter / 2);
                    Canvas.SetTop(upperRivet, -yUpper[idx] * scale + canvasHeight / 2 - scaledRivetDiameter / 2);
                    previewCanvas.Children.Add(upperRivet);

                    // Заклёпка на нижней поверхности
                    var lowerRivet = new Ellipse
                    {
                        Width = scaledRivetDiameter,
                        Height = scaledRivetDiameter,
                        Fill = Brushes.Green,
                        Opacity = 0.7
                    };
                    Canvas.SetLeft(lowerRivet, x * scale - scaledRivetDiameter / 2);
                    Canvas.SetTop(lowerRivet, -yLower[idx] * scale + canvasHeight / 2 - scaledRivetDiameter / 2);
                    previewCanvas.Children.Add(lowerRivet);
                }
            }

            // Отрисовка нервюр и заклёпок на них (на верхней и нижней поверхностях)
            double wingSpan = double.Parse(tbWingSpan.Text);
            if (double.TryParse(tbRibSpacing.Text, out double ribSpacing) && ribSpacing > 0 && drawOnRibs)
            {
                for (double pos = 0; pos <= wingSpan; pos += ribSpacing)
                {
                    // Отрисовка нервюры
                    var ribLine = new Line
                    {
                        X1 = 0,
                        Y1 = -yUpper[0] * scale + canvasHeight / 2,
                        X2 = chordLength * scale,
                        Y2 = -yUpper[yUpper.Length - 1] * scale + canvasHeight / 2,
                        Stroke = Brushes.Blue,
                        StrokeThickness = 1,
                        Opacity = 0.3
                    };
                    Canvas.SetLeft(ribLine, pos * scale / wingSpan * canvasWidth);
                    previewCanvas.Children.Add(ribLine);

                    // Отрисовка заклёпок вдоль нервюры
                    double scaledRivetSpacing = rivetSpacing * scale;
                    double scaledRivetDiameter = rivetDiameter * scale;
                    for (double x = 0; x <= chordLength * scale; x += scaledRivetSpacing)
                    {
                        int idx = (int)(x / (chordLength * scale) * (xUpper.Length - 1));
                        idx = Math.Min(idx, xUpper.Length - 1);

                        // Заклёпка на верхней поверхности
                        var upperRivet = new Ellipse
                        {
                            Width = scaledRivetDiameter,
                            Height = scaledRivetDiameter,
                            Fill = Brushes.Green,
                            Opacity = 0.7
                        };
                        Canvas.SetLeft(upperRivet, x - scaledRivetDiameter / 2);
                        Canvas.SetTop(upperRivet, -yUpper[idx] * scale + canvasHeight / 2 - scaledRivetDiameter / 2);
                        previewCanvas.Children.Add(upperRivet);

                        // Заклёпка на нижней поверхности
                        var lowerRivet = new Ellipse
                        {
                            Width = scaledRivetDiameter,
                            Height = scaledRivetDiameter,
                            Fill = Brushes.Green,
                            Opacity = 0.7
                        };
                        Canvas.SetLeft(lowerRivet, x - scaledRivetDiameter / 2);
                        Canvas.SetTop(lowerRivet, -yLower[idx] * scale + canvasHeight / 2 - scaledRivetDiameter / 2);
                        previewCanvas.Children.Add(lowerRivet);
                    }
                }
            }
        }
    }
}