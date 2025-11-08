using Kompas6API5;
using Kompas6Constants3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Modeling.Class
{
    //класс для создания нервюр
    class RibGenerator
    {
        private readonly Func<ksPart, double, ksEntity> _createOffsetPlane; 

        public RibGenerator(Func<ksPart, double, ksEntity> createOffsetPlane)
        {
            _createOffsetPlane = createOffsetPlane ?? throw new ArgumentNullException(nameof(createOffsetPlane));
        }

        // Метод генерации нервюр
        public void GenerateRibs(
            ksPart part,
            double chordLength,
            double wingSpan,
            double[] xUpper,
            double[] yUpper,
            double[] xLower,
            double[] yLower,
            TextBox tbRibSpacing,
            TextBox tbRibThickness,
            ComboBox cbRibArrangement)
        {
            if (!double.TryParse(tbRibSpacing.Text, out double ribSpacing) || ribSpacing <= 0)
            {
                MessageBox.Show("Неверный шаг нервюр");
                return;
            }

            if (!double.TryParse(tbRibThickness.Text, out double ribThickness) || ribThickness <= 0)
            {
                MessageBox.Show("Неверная толщина нервюры");
                return;
            }

            string arrangement = (cbRibArrangement.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Рассчёт позиций нервюр
            List<double> ribPositions = new List<double>();
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

            // Создание каждой нервюры
            foreach (double pos in ribPositions)
            {
                ksEntity planeOffset = _createOffsetPlane(part, pos);
                if (planeOffset == null)
                {
                    MessageBox.Show($"Ошибка: Не удалось создать смещённую плоскость на позиции {pos:F2}");
                    continue;
                }

                ksEntity ribSketch = (ksEntity)part.NewEntity((short)Obj3dType.o3d_sketch);
                ksSketchDefinition ribSketchDef = (ksSketchDefinition)ribSketch.GetDefinition();
                ribSketchDef.SetPlane(planeOffset);
                if (!ribSketch.Create())
                {
                    MessageBox.Show($"Ошибка: Не удалось создать эскиз нервюры на позиции {pos:F2}");
                    continue;
                }

                ksDocument2D ribSketchEdit = (ksDocument2D)ribSketchDef.BeginEdit();
                for (int i = 0; i < xUpper.Length - 1; i++)
                    ribSketchEdit.ksLineSeg(xUpper[i], yUpper[i], xUpper[i + 1], yUpper[i + 1], 1);
                for (int i = xLower.Length - 1; i > 0; i--)
                    ribSketchEdit.ksLineSeg(xLower[i], yLower[i], xLower[i - 1], yLower[i - 1], 1);
                ribSketchEdit.ksLineSeg(xLower[0], yLower[0], xUpper[0], yUpper[0], 1);
                ribSketchEdit.ksLineSeg(xUpper[xUpper.Length - 1], yUpper[yUpper.Length - 1], xLower[xLower.Length - 1], yLower[yLower.Length - 1], 1);
                ribSketchDef.EndEdit();

                ksEntity ribExtrude = (ksEntity)part.NewEntity((short)Obj3dType.o3d_bossExtrusion);
                ksBossExtrusionDefinition ribExtrudeDef = (ksBossExtrusionDefinition)ribExtrude.GetDefinition();
                ribExtrudeDef.directionType = (short)Direction_Type.dtNormal;
                ribExtrudeDef.SetSideParam(true, (short)End_Type.etBlind, ribThickness);
                ribExtrudeDef.SetSketch(ribSketch);
                if (!ribExtrude.Create())
                {
                    MessageBox.Show($"Ошибка: Не удалось выполнить выдавливание нервюры на позиции {pos:F2}. Проверьте корректность эскиза.");
                    continue;
                }

                part.Update();
            }
        }
    }
} 
