    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using SmallGISApp;

namespace SmallGISApp
{
    public class CLayer
    {
        public static CLayer layertest = new CLayer();

        public double MaxY = -9999;
        public double MaxX = -9999;
        public double MinY = 9999;
        public double MinX = 9999;
        public double scale = 0;
        public double centerX = 0;
        public double centerY = 0;

        public void SetFrame(ref CLayer LayerDraw, Canvas CanvasDraw)
        {
            LayerDraw.centerX = (LayerDraw.MaxX + LayerDraw.MinX) / 2;
            LayerDraw.centerY = (LayerDraw.MaxY + LayerDraw.MinY) / 2;
            // 稍作调整使得Shp文件的边界不会与框重合，预留一定的缓冲空间
            double scaleX = (LayerDraw.MaxX - LayerDraw.MinX) / CanvasDraw.ActualWidth;
            double scaleY = (LayerDraw.MaxY - LayerDraw.MinY) / CanvasDraw.ActualHeight;
            LayerDraw.scale = (scaleX > scaleY) ? scaleX : scaleY;
        }
        public void reverseChange(Canvas CanvasDraw,ref double mapX, ref double mapY)
        {
            mapX = (mapX - CanvasDraw.ActualWidth / 2) * scale + centerX;
            mapY = (CanvasDraw.ActualHeight / 2 - mapY) * scale + centerY;
        }

        public void MapXYChange(double mapX, double mapY,Canvas CanvasDraw, ref double tranx, ref double trany)
        {
            tranx = ((mapX - centerX) / scale) + CanvasDraw.ActualWidth / 2;
            trany = CanvasDraw.ActualHeight / 2 - ((mapY - centerY) / scale);
        }


    }
}
