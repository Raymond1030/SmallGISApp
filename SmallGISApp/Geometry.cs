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
using System.Windows.Documents;

namespace SmallGISApp
{
    internal class Geometry
    {
        public int id { get; set; }
        public string color { get; set; }
        public void draw()
        {

        }
    }
    //点类 point class
    internal class Point:Geometry
    {
        public double x { get; set; }
        public double y { get; set; }
        public Point()
        {
    
        }
        public Point(double p_x, double p_y)
        {
            this.x = p_x;
            this.y = p_y;
        }
        public void Draw(Canvas canvas, double radius)//点绘画 Draw重载
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
        public void Draw(Canvas canvas)//线绘画 Draw重载
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
        public void Draw(Canvas canvas, bool IsFill )//面绘画 Draw重载
        {
         
            if (!IsFill)
            {
                // IsFill==false 画多边形线
                for (int i = 0; i + 1 < m_polygon.Count; i++)
                {
                    LineGeometry myLineGeometry = new LineGeometry();
                    myLineGeometry.StartPoint = new System.Windows.Point(m_polygon[i].x, m_polygon[i].y);
                    myLineGeometry.EndPoint = new System.Windows.Point(m_polygon[i + 1].x, m_polygon[i + 1].y);
                    Path myPath = new Path();
                    myPath.Stroke = Brushes.Black;
                    myPath.StrokeThickness = 1;
                    myPath.Data = myLineGeometry;
                    canvas.Children.Add(myPath);
                }
            }
            else if (IsFill)
            {
                // IsFill==true 多边形涂色
                // Create solid brush.
                //SolidBrush blueBrush = new SolidBrush(Color.Blue);

                System.Windows.Shapes.Polygon newPolygon = new System.Windows.Shapes.Polygon();
                List<System.Windows.Point>System_points= new List<System.Windows.Point>();
                for (int i = 0; i < m_polygon.Count; i++)
                {
                    System.Windows.Point s_P = new System.Windows.Point(m_polygon[i].x, m_polygon[i].y);
                    System_points.Add(s_P);
                }
                newPolygon.Points = new PointCollection(System_points);
                newPolygon.Stroke = Brushes.Black;
                newPolygon.StrokeThickness = 1;
                newPolygon.Fill = Brushes.LightBlue;  // 设置填充颜色
                canvas.Children.Add(newPolygon);
            }
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
