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
        public double MaxY = -9999;
        public double MaxX = -9999;
        public double MinY = 9999;
        public double MinX = 9999;
        public int id { get; set; }
        public System.Windows.Media.Color color { get; set; }//颜色
        public string attribute { get; set; }//属性
     
        public void draw()
        {

        }
    }
    //点类 point class
    internal class Point:Geometry
    {
        public double x { get; set; }
        public double y { get; set; }
        //点半径
        public double radius { get; set; }
        public Point()
        {
    
        }
        public Point(double p_x, double p_y)
        {
            this.x = p_x;
            this.y = p_y;
        }
        public void Draw(Canvas canvas)//点绘画 Draw重载
        {
            Ellipse Drawpoint = new Ellipse
            {
                //Fill = Brushes.Black,
                Fill = new SolidColorBrush(this.color), // 使用 SolidColorBrush
                Width = this.radius * 2,
                Height = this.radius * 2
            };

            Canvas.SetLeft(Drawpoint, this.x - this.radius);
            Canvas.SetTop(Drawpoint, this.y - this.radius);

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

        //线宽
        public double width { get; set; }
        public void Draw(Canvas canvas)//线绘画 Draw重载
        {
            for (int i = 0; i+ 1< m_Line.Count; i++)
            {
                //遍历一下box
                if (m_Line[i].x > MaxX)
                    MaxX = m_Line[i].x;
                if (m_Line[i].y > MaxY)
                    MaxY = m_Line[i].y;
                if (m_Line[i].x < MinX)
                    MinX = m_Line[i].x;
                if (m_Line[i].y < MinY)
                    MinY = m_Line[i].y;

                LineGeometry myLineGeometry = new LineGeometry();
                myLineGeometry.StartPoint = new System.Windows.Point(m_Line[i].x, m_Line[i].y);
                myLineGeometry.EndPoint = new System.Windows.Point(m_Line[i+1].x, m_Line[i+1].y);

                Path myPath = new Path();
                //myPath.Stroke = Brushes.Black;
                myPath.Stroke = new SolidColorBrush(this.color);
                myPath.StrokeThickness = width;
                myPath.Data = myLineGeometry;

                canvas.Children.Add(myPath);
            }
            //遍历一下box
            //遍历一下box
            if (m_Line[m_Line.Count-1].x > MaxX)
                MaxX = m_Line[m_Line.Count - 1].x;
            if (m_Line[m_Line.Count - 1].y > MaxY)
                MaxY = m_Line[m_Line.Count - 1].y;
            if (m_Line[m_Line.Count - 1].x < MinX)
                MinX = m_Line[m_Line.Count - 1].x;
            if (m_Line[m_Line.Count - 1].y < MinY)
                MinY = m_Line[m_Line.Count - 1].y;

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
        public System.Windows.Media.Color paintColor { get; set; }//颜色
        public double width { get; set; }//线宽
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
                    //遍历一下box
                    if (m_polygon[i].x > MaxX)
                        MaxX = m_polygon[i].x;
                    if (m_polygon[i].y > MaxY)
                        MaxY = m_polygon[i].y;
                    if (m_polygon[i].x < MinX)
                        MinX = m_polygon[i].x;
                    if (m_polygon[i].y < MinY)
                        MinY = m_polygon[i].y;

                    LineGeometry myLineGeometry = new LineGeometry();
                    myLineGeometry.StartPoint = new System.Windows.Point(m_polygon[i].x, m_polygon[i].y);
                    myLineGeometry.EndPoint = new System.Windows.Point(m_polygon[i + 1].x, m_polygon[i + 1].y);
                    Path myPath = new Path();
                    myPath.Stroke = new SolidColorBrush(this.color);
                    //myPath.Stroke = Brushes.Black;
                    myPath.StrokeThickness = width;
                    myPath.Data = myLineGeometry;
                    canvas.Children.Add(myPath);
                }
                //遍历一下box
                if (m_polygon[m_polygon.Count-1].x > MaxX)
                    MaxX = m_polygon[m_polygon.Count - 1].x;
                if (m_polygon[m_polygon.Count - 1].y > MaxY)
                    MaxY = m_polygon[m_polygon.Count - 1].y;
                if (m_polygon[m_polygon.Count - 1].x < MinX)
                    MinX = m_polygon[m_polygon.Count - 1].x;
                if (m_polygon[m_polygon.Count - 1].y < MinY)
                    MinY = m_polygon[m_polygon.Count - 1].y;

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
                    //遍历一下box
                    if (m_polygon[i].x > MaxX)
                        MaxX = m_polygon[i].x;
                    if (m_polygon[i].y > MaxY)
                        MaxY = m_polygon[i].y;
                    if (m_polygon[i].x < MinX)
                        MinX = m_polygon[i].x;
                    if (m_polygon[i].y < MinY)
                        MinY = m_polygon[i].y;
                    System.Windows.Point s_P = new System.Windows.Point(m_polygon[i].x, m_polygon[i].y);
                    System_points.Add(s_P);
                }
                newPolygon.Points = new PointCollection(System_points);
                //newPolygon.Stroke = Brushes.Black;
                //线颜色
                newPolygon.Stroke = new SolidColorBrush(this.color);
                //线宽
                newPolygon.StrokeThickness = width;
                //newPolygon.Fill = Brushes.LightBlue;  
                //设置填充颜色
                newPolygon.Fill = new SolidColorBrush(this.paintColor);
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
