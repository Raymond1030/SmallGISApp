using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;

namespace SmallGISApp
{
    internal class Geometry
    {
        public int id { get; set; }
        public string color { get; set; }
    }
    //点类 point class
    internal class Point:Geometry
    {
        public double x { get; set; }
        public double y { get; set; }
        public Point(double p_x, double p_y)
        {
            this.x = p_x;
            this.y = p_y;
        }
        public void DrawPoint(Canvas canvas, double radius)
        {
            Ellipse Drawpoint = new Ellipse
            {
                Fill = Brushes.Black,
                Width = radius * 2,
                Height = radius * 2
            };

            Canvas.SetLeft(Drawpoint, this.x - radius);
            Canvas.SetTop(Drawpoint, this.y - radius);

            canvas.Children.Add(Drawpoint);
        }
    }
    //多点类 multi point class
    internal class MultiPoint : Geometry
    {

        public List<Point> m_multiPoint { get; set; }
        public MultiPoint()
        {
            m_multiPoint = new List<Point>();
        }
        public void PushPoint(Point P)
        {
            m_multiPoint.Add(P);
        }
        public void DrawPoint(Point P)
        {
        }
    }
    //线类 line class
    internal class Line:Geometry
    {
        //线 起点终点
        public List<Point> m_Line  { get; set; }
        public Line()
        {
           m_Line = new List<Point>();
        }
        public void DrawLine(Canvas canvas)
        {
            for (int i = 0; i+ 1< m_Line.Count; i++)
            {
                LineGeometry myLineGeometry = new LineGeometry();
                myLineGeometry.StartPoint = new System.Windows.Point(m_Line[i].x, m_Line[i].y);
                myLineGeometry.EndPoint = new System.Windows.Point(m_Line[i+1].x, m_Line[i+1].y);

                Path myPath = new Path();
                myPath.Stroke = Brushes.Black;
                myPath.StrokeThickness = 1;
                myPath.Data = myLineGeometry;

                canvas.Children.Add(myPath);
            }
        
        }
    }
    //多线类 multi line class
    class MultiLine :Geometry
    {
        public List<Line> m_multiLine { get; set; }
        public MultiLine()
        {
            m_multiLine = new List<Line>();
        }
        public void PushLine(Line L)
        {
            m_multiLine.Add(L);
        }
    }
    //面类 polygon class
    internal class Polygon:Geometry
    { 
        public List<Point> m_polygon { get; set; }
        public Polygon()
        {
            m_polygon = new List<Point>();
        }
        public void PushRingPoint(Point P)
        //添加顶点
        {
            m_polygon.Add(P);
        }
    }
    //多面类 multi polygon class
    internal class MultiPolygon:Geometry
    {
        public List<Polygon> m_multiPolygon { get; set; }
        public MultiPolygon()
        {
            m_multiPolygon = new List<Polygon>();
        }
        public void PushPolygon(Polygon P)
        //添加面
        {

            m_multiPolygon.Add(P);
        }
    }

}
