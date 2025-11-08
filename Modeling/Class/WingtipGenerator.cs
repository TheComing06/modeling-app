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
    //класс для законцовки, чета работал потом перестал поэтому нафиг его 
    class WingtipGenerator
    {
        //Метод генерации законцовки крыла
            public void GenerateWingtip(
                ksPart part,
                double wingSpan,
                double[] xUpper,
                double[] yUpper,
                double[] xLower,
                double[] yLower,
                double wingtipThickness)
        {
            //    string wingtipType = (cbWingtipType.SelectedItem as ComboBoxItem)?.Content.ToString();
            //    if (string.IsNullOrEmpty(wingtipType))
            //        return;

            //    ksEntity offsetPlane = (ksEntity)part.NewEntity((short)Obj3dType.o3d_planeOffset);
            //    ksPlaneOffsetDefinition planeDef = (ksPlaneOffsetDefinition)offsetPlane.GetDefinition();
            //    planeDef.SetPlane(part.GetDefaultEntity((short)Obj3dType.o3d_planeXOZ));
            //    planeDef.direction = true;
            //    planeDef.offset = wingSpan;
            //    offsetPlane.Create();

            //    ksEntity wingtipSketch = (ksEntity)part.NewEntity((short)Obj3dType.o3d_sketch);
            //    ksSketchDefinition wingtipSketchDef = (ksSketchDefinition)wingtipSketch.GetDefinition();
            //    wingtipSketchDef.SetPlane(offsetPlane);
            //    if (!wingtipSketch.Create())
            //    {
            //        MessageBox.Show("Ошибка: Не удалось создать эскиз законцовки");
            //        return;
            //    }
            //    ksDocument2D wingtipSketchEdit = (ksDocument2D)wingtipSketchDef.BeginEdit();
            //    if (wingtipType == "Закруглённая")
            //    {
            //        for (int i = 0; i < xUpper.Length - 1; i++)
            //        {
            //            wingtipSketchEdit.ksLineSeg(xUpper[i], yUpper[i], xUpper[i + 1], yUpper[i + 1], 1);
            //        }
            //        for (int i = xLower.Length - 1; i > 0; i--)
            //        {
            //            wingtipSketchEdit.ksLineSeg(xLower[i], yLower[i], xLower[i - 1], yLower[i - 1], 1);
            //        }
            //        double zTopFront = yUpper[0];
            //        double zBottomFront = yLower[0];
            //        wingtipSketchEdit.ksArcBy3Points(xUpper[0], zTopFront, xUpper[0], (zTopFront + zBottomFront) / 2, xLower[0], zBottomFront, 1);
            //        double zTopRear = yUpper[xUpper.Length - 1];
            //        double zBottomRear = yLower[xLower.Length - 1];
            //        wingtipSketchEdit.ksArcBy3Points(xUpper[xUpper.Length - 1], zTopRear, xUpper[xUpper.Length - 1], (zTopRear + zBottomRear) / 2, xLower[xLower.Length - 1], zBottomRear, 1);
            //    }
            //    else if (wingtipType == "Прямоугольная")
            //    {
            //        int idx = xUpper.Length - 1;
            //        double xStart = xLower[0];
            //        double xEnd = xUpper[idx];
            //        double zTop = yUpper[idx];
            //        double zBottom = yLower[0];
            //        wingtipSketchEdit.ksLineSeg(xStart, zBottom, xEnd, zBottom, 1);
            //        wingtipSketchEdit.ksLineSeg(xEnd, zBottom, xEnd, zTop, 1);
            //        wingtipSketchEdit.ksLineSeg(xEnd, zTop, xStart, zTop, 1);
            //        wingtipSketchEdit.ksLineSeg(xStart, zTop, xStart, zBottom, 1);
            //    }
            //    wingtipSketchDef.EndEdit();

            //    ksEntity wingtipExtrude = (ksEntity)part.NewEntity((short)Obj3dType.o3d_bossExtrusion);
            //    ksBossExtrusionDefinition wingtipExtrudeDef = (ksBossExtrusionDefinition)wingtipExtrude.GetDefinition();
            //    wingtipExtrudeDef.directionType = (short)Direction_Type.dtNormal;
            //    wingtipExtrudeDef.SetSideParam(true, (short)End_Type.etBlind, wingtipThickness);
            //    wingtipExtrudeDef.SetSketch(wingtipSketch);
            //    if (!wingtipExtrude.Create())
            //    {
            //        MessageBox.Show($"Ошибка: Не удалось выполнить выдавливание законцовки типа {wingtipType}");
            //        return;
            //    }
        }
    }
    }
