using Kompas6API5;
using Kompas6Constants3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Modeling.Class
{
    //не сильно та работающий класс для заклепок
    class RivetGenerators
    {
        private void CreateRivet(ksPart part, double x, double y, double z, double diameter, double height, string rivetType, double skinThickness, bool isUpper)
        {
            try
            {
                // Корректировка z-координаты для потайных заклёпок
                double adjustedZ = z;
                if (rivetType == "Потайные")
                {
                    adjustedZ += isUpper ? -skinThickness : skinThickness;
                }

                // Создание плоскости для заклёпки на внешней поверхности
                ksEntity plane = (ksEntity)part.NewEntity((short)Obj3dType.o3d_planeOffset);
                ksPlaneOffsetDefinition planeDef = (ksPlaneOffsetDefinition)plane.GetDefinition();
                planeDef.SetPlane(part.GetDefaultEntity((short)Obj3dType.o3d_planeXOZ));
                planeDef.direction = true;
                planeDef.offset = adjustedZ;
                plane.Create();

                // Создание эскиза для заклёпки
                ksEntity rivetSketch = (ksEntity)part.NewEntity((short)Obj3dType.o3d_sketch);
                ksSketchDefinition rivetSketchDef = (ksSketchDefinition)rivetSketch.GetDefinition();
                rivetSketchDef.SetPlane(plane);
                rivetSketch.Create();
                ksDocument2D rivetSketchEdit = (ksDocument2D)rivetSketchDef.BeginEdit();
                rivetSketchEdit.ksCircle(x, z, diameter / 2, 1);
                rivetSketchDef.EndEdit();

                if (rivetType == "Потайные")
                {
                    // Потайная заклёпка - вырез внутрь
                    ksEntity rivetCut = (ksEntity)part.NewEntity((short)Obj3dType.o3d_cutExtrusion);
                    ksCutExtrusionDefinition cutDef = (ksCutExtrusionDefinition)rivetCut.GetDefinition();
                    cutDef.directionType = isUpper ? (short)Direction_Type.dtReverse : (short)Direction_Type.dtNormal;
                    cutDef.SetSideParam(false, (short)End_Type.etBlind, height);
                    cutDef.SetSketch(rivetSketch);
                    rivetCut.Create();
                }
                else
                {
                    // Обычная заклёпка - выдавливание наружу
                    ksEntity rivetExtrude = (ksEntity)part.NewEntity((short)Obj3dType.o3d_bossExtrusion);
                    ksBossExtrusionDefinition extrudeDef = (ksBossExtrusionDefinition)rivetExtrude.GetDefinition();
                    extrudeDef.directionType = isUpper ? (short)Direction_Type.dtNormal : (short)Direction_Type.dtReverse;
                    extrudeDef.SetSideParam(true, (short)End_Type.etBlind, height);
                    extrudeDef.SetSketch(rivetSketch);
                    rivetExtrude.Create();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании заклёпки (x={x}, y={y}, z={z}): {ex.Message}");
            }
        }

        private void CreateRivetPair(ksPart part, double x, double y, double z, double diameter, double height,
                                   string rivetType, double skinThickness, bool isUpper, double offset, bool isStaggered, int rowIndex)
        {
            // Основная заклёпка
            CreateRivet(part, x, y, z, diameter, height, rivetType, skinThickness, isUpper);
            // Вторая заклёпка в паре
            double secondX = x;
            double secondY = y;

            if (isStaggered)
            {
                // Шахматный узор: чередование направления смещения по строкам
                double staggerSign = (rowIndex % 2 == 0) ? 1 : -1;
                secondX += staggerSign * offset;
            }
            else
            {
                // Двухрядный узор: фиксированное смещение
                secondX += offset;
            }

            CreateRivet(part, secondX, secondY, z, diameter, height, rivetType, skinThickness, isUpper);
        }

        private (double upperZ, double lowerZ) GetAirfoilHeightAtPosition(double x, double[] xUpper, double[] yUpper, double[] xLower, double[] yLower)
        {
            try
            {
                if (xUpper == null || yUpper == null || xLower == null || yLower == null ||
                    xUpper.Length < 2 || yUpper.Length < 2 || xLower.Length < 2 || yLower.Length < 2)
                {
                    throw new ArgumentException("Недостаточно данных профиля крыла");
                }

                // Поиск ближайшей точки на верхней поверхности
                int upperIdx = FindNearestIndex(x, xUpper);
                double upperT = (x - xUpper[upperIdx]) / (xUpper[upperIdx + 1] - xUpper[upperIdx]);
                double upperZ = yUpper[upperIdx] + upperT * (yUpper[upperIdx + 1] - yUpper[upperIdx]);

                // Поиск ближайшей точки на нижней поверхности
                int lowerIdx = FindNearestIndex(x, xLower);
                double lowerT = (x - xLower[lowerIdx]) / (xLower[lowerIdx + 1] - xLower[lowerIdx]);
                double lowerZ = yLower[lowerIdx] + lowerT * (yLower[lowerIdx + 1] - yLower[lowerIdx]);

                return (upperZ, lowerZ);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при вычислении высоты профиля для x={x}: {ex.Message}");
            }
        }

        private int FindNearestIndex(double x, double[] xCoords)
        {
            if (xCoords == null || xCoords.Length < 2)
                throw new ArgumentException("Недостаточно координат для поиска индекса");

            for (int i = 0; i < xCoords.Length - 1; i++)
            {
                if (x >= xCoords[i] && x <= xCoords[i + 1])
                {
                    return i;
                }
            }
            return xCoords.Length - 2;
        }

        public void GenerateRivets(
            ksPart part,
            List<double> sparPositions,
            List<double> ribPositions,
            double chordLength,
            double wingSpan,
            double[] xUpper,
            double[] yUpper,
            double[] xLower,
            double[] yLower,
            string cbConnectionType,
            TextBox tbRivetDiameter,
            TextBox tbRivetSpacing,
            TextBox tbRivetHeight,
            TextBox tbSkinThickness,
            ComboBox cbRivetPattern,
            ComboBox cbRivetLocation,
            ComboBox cbRivetType)
        {
            try
            {
                if (cbConnectionType != "Riveting")
                {
                    MessageBox.Show("Выбран неподходящий тип соединения");
                    return;
                }

                if (!double.TryParse(tbRivetDiameter.Text, out double rivetDiameter) || rivetDiameter <= 0)
                {
                    MessageBox.Show("Неверный диаметр заклёпок");
                    return;
                }

                if (!double.TryParse(tbRivetSpacing.Text, out double rivetSpacing) || rivetSpacing <= 0)
                {
                    MessageBox.Show("Неверный шаг заклёпок");
                    return;
                }

                if (!double.TryParse(tbRivetHeight.Text, out double rivetHeight) || rivetHeight <= 0)
                {
                    MessageBox.Show("Неверная высота заклёпок");
                    return;
                }

                if (!double.TryParse(tbSkinThickness.Text, out double skinThickness) || skinThickness <= 0)
                {
                    MessageBox.Show("Неверная толщина обшивки");
                    return;
                }

                if (part == null)
                {
                    MessageBox.Show("Деталь не инициализирована");
                    return;
                }

                string rivetPattern = (cbRivetPattern.SelectedItem as ComboBoxItem)?.Content.ToString();
                string rivetLocation = (cbRivetLocation.SelectedItem as ComboBoxItem)?.Content.ToString();
                string rivetType = (cbRivetType.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (string.IsNullOrEmpty(rivetPattern) || string.IsNullOrEmpty(rivetLocation) || string.IsNullOrEmpty(rivetType))
                {
                    MessageBox.Show("Не выбраны параметры узора, расположения или типа заклёпок");
                    return;
                }

                // Расчёт расстояния между рядами заклёпок
                double rowOffset = rivetDiameter * 2.5;

                // Заклёпки вдоль лонжеронов
                if (rivetLocation == "Лонжероны" || rivetLocation == "Лонжероны и нервюры")
                {
                    if (sparPositions == null || !sparPositions.Any())
                    {
                        MessageBox.Show("Список позиций лонжеронов пуст");
                        return;
                    }

                    foreach (double sparPos in sparPositions)
                    {
                        var (upperZ, lowerZ) = GetAirfoilHeightAtPosition(sparPos, xUpper, yUpper, xLower, yLower);

                        for (double y = 0; y <= wingSpan; y += rivetSpacing)
                        {
                            int rowIndex = (int)(y / rivetSpacing);
                            if (rivetPattern == "Однорядная")
                            {
                                CreateRivet(part, sparPos, y, upperZ, rivetDiameter, rivetHeight, rivetType, skinThickness, true);
                                CreateRivet(part, sparPos, y, lowerZ, rivetDiameter, rivetHeight, rivetType, skinThickness, false);
                            }
                            else
                            {
                                bool isStaggered = rivetPattern == "Шахматная";
                                CreateRivetPair(part, sparPos, y, upperZ, rivetDiameter, rivetHeight,
                                               rivetType, skinThickness, true, rowOffset, isStaggered, rowIndex);
                                CreateRivetPair(part, sparPos, y, lowerZ, rivetDiameter, rivetHeight,
                                               rivetType, skinThickness, false, rowOffset, isStaggered, rowIndex);
                            }
                        }
                    }
                }

                // Заклёпки вдоль нервюр
                if (rivetLocation == "Нервюры" || rivetLocation == "Лонжероны и нервюры")
                {
                    if (ribPositions == null || !ribPositions.Any())
                    {
                        MessageBox.Show("Список позиций нервюр пуст");
                        return;
                    }

                    if (xUpper == null || xUpper.Length < 2 || yUpper == null || yUpper.Length < 2 ||
                        xLower == null || xLower.Length < 2 || yLower == null || yLower.Length < 2)
                    {
                        MessageBox.Show("Недостаточно данных профиля крыла для нервюр");
                        return;
                    }

                    double xMin = xUpper.Min();
                    double xMax = xUpper.Max();

                    if (xMin >= xMax)
                    {
                        MessageBox.Show($"Некорректные границы профиля: xMin={xMin}, xMax={xMax}");
                        return;
                    }

                    foreach (double ribPos in ribPositions)
                    {
                        if (ribPos < 0 || ribPos > wingSpan)
                        {
                            MessageBox.Show($"Некорректная позиция нервюры: {ribPos}");
                            continue;
                        }

                        for (double x = xMin; x <= xMax; x += rivetSpacing)
                        {
                            try
                            {
                                var (upperZ, lowerZ) = GetAirfoilHeightAtPosition(x, xUpper, yUpper, xLower, yLower);

                                int rowIndex = (int)(x / rivetSpacing);
                                if (rivetPattern == "Однорядная")
                                {
                                    CreateRivet(part, x, ribPos, upperZ, rivetDiameter, rivetHeight, rivetType, skinThickness, true);
                                    CreateRivet(part, x, ribPos, lowerZ, rivetDiameter, rivetHeight, rivetType, skinThickness, false);
                                }
                                else
                                {
                                    bool isStaggered = rivetPattern == "Шахматная";
                                    CreateRivetPair(part, x, ribPos, upperZ, rivetDiameter, rivetHeight,
                                                   rivetType, skinThickness, true, rowOffset, isStaggered, rowIndex);
                                    CreateRivetPair(part, x, ribPos, lowerZ, rivetDiameter, rivetHeight,
                                                   rivetType, skinThickness, false, rowOffset, isStaggered, rowIndex);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка при создании заклёпки на нервюре (x={x}, ribPos={ribPos}): {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Общая ошибка при генерации заклёпок: {ex.Message}");
            }
        }
    }
}