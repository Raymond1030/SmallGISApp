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
using SmallGISApp;


namespace SmallGISApp
{     
    internal class Geometry
    {
        public double MaxY = -9999;
        public double MaxX = -9999;
        public double MinY = 9999;
        public double MinX = 9999;
        

        public double _vArea = 0;
        public double _vLength = 0;
        public int _vSize = 0;

        public int IsSelected = 0;

        public int id { get; set; }
        public System.Windows.Media.Color color { get; set; }//颜色
        public string attribute { get; set; }//属性
     
        public void Draw()
        {

        }

        
    }
    //点类 point class
    internal class Point:Geometry
    {
        public double x { get; set; }
        public double y { get; set; }
        public double Transformx = 0;
        public double Transformy = 0;
        //点半径
        public double radius { get; set; }
        public Point()
        {
            
        }
        public Point(double p_x, double p_y)
        {
            this.x = p_x;
            this.y = p_y;           
            this.radius = 5; // 设置一个固定的半径
            this.color = System.Windows.Media.Colors.Black; // 设置一个固定的颜色
        }
        public void Draw(Canvas canvas, int temp = 0)//点绘画 Draw重载
        {        
            Ellipse Drawpoint = new Ellipse
            {
                //Fill = Brushes.Black,
                Fill = new SolidColorBrush(this.color), // 使用 SolidColorBrush
                Width = this.radius * 2,
                Height = this.radius * 2
            };
            if(temp!=IsSelected)
                Drawpoint.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 0));
            CLayer.layertest.MapXYChange(x, y,canvas, ref this.Transformx, ref this.Transformy);
            Canvas.SetLeft(Drawpoint, Transformx - this.radius);
            Canvas.SetTop(Drawpoint, Transformy - this.radius);

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
        public void DrawPoint(Canvas canvas)
        {
            foreach(var P in m_multiPoint)
            {
                P.Draw(canvas);
            }
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
           this.color = System.Windows.Media.Colors.Black; // 设置一个默认的颜色
           this.width = 1.0; // 设置一个默认的线宽
        }

        //线宽
        public double width { get; set; }
        public void Draw(Canvas canvas, int temp = 0)//线绘画 Draw重载
        {
            for (int i = 0; i+ 1< m_Line.Count; i++)
            {
                ////遍历一下box
                //if (m_Line[i].x > MaxX)
                //    MaxX = m_Line[i].x;
                //if (m_Line[i].y > MaxY)
                //    MaxY = m_Line[i].y;
                //if (m_Line[i].x < MinX)
                //    MinX = m_Line[i].x;
                //if (m_Line[i].y < MinY)
                //    MinY = m_Line[i].y;
                CLayer.layertest.MapXYChange(m_Line[i].x, m_Line[i].y,canvas, ref m_Line[i].Transformx, ref m_Line[i].Transformy);
                CLayer.layertest.MapXYChange(m_Line[i+1].x, m_Line[i+1].y,canvas, ref m_Line[i+1].Transformx, ref m_Line[i+1].Transformy);
                LineGeometry myLineGeometry = new LineGeometry();
                myLineGeometry.StartPoint = new System.Windows.Point(m_Line[i].Transformx, m_Line[i].Transformy);
                myLineGeometry.EndPoint = new System.Windows.Point(m_Line[i+1].Transformx, m_Line[i+1].Transformy);

                Path myPath = new Path();
                //myPath.Stroke = Brushes.Black;
                myPath.Stroke = new SolidColorBrush(this.color);
                if(temp!=IsSelected)
                    myPath.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 0));
                myPath.StrokeThickness = width;
                myPath.Data = myLineGeometry;

                canvas.Children.Add(myPath);
            }
            //遍历一下box
            //遍历一下box
            //if (m_Line[m_Line.Count-1].x > MaxX)
            //    MaxX = m_Line[m_Line.Count - 1].x;
            //if (m_Line[m_Line.Count - 1].y > MaxY)
            //    MaxY = m_Line[m_Line.Count - 1].y;
            //if (m_Line[m_Line.Count - 1].x < MinX)
            //    MinX = m_Line[m_Line.Count - 1].x;
            //if (m_Line[m_Line.Count - 1].y < MinY)
            //    MinY = m_Line[m_Line.Count - 1].y;

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
        public void Draw(Canvas canvas)
        {
            foreach(var L in m_multiLine)
            {
                L.Draw(canvas);
            }
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
            this.color = System.Windows.Media.Colors.Black; // 设置一个默认的颜色
            this.width = 1.0; // 设置一个默认的线宽
        }
        public void PushRingPoint(Point P)
        //添加顶点
        {
            m_polygon.Add(P);
        }
        public void Draw(Canvas canvas, bool IsFill, int temp =0)//面绘画 Draw重载
        {
            if (!IsFill)
            {
                // IsFill==false 画多边形线
                for (int i = 0; i + 1 < m_polygon.Count; i++)
                {
                    ////遍历一下box
                    //if (m_polygon[i].x > MaxX)
                    //    MaxX = m_polygon[i].x;
                    //if (m_polygon[i].y > MaxY)
                    //    MaxY = m_polygon[i].y;
                    //if (m_polygon[i].x < MinX)
                    //    MinX = m_polygon[i].x;
                    //if (m_polygon[i].y < MinY)
                    //    MinY = m_polygon[i].y;

                    LineGeometry myLineGeometry = new LineGeometry();

                    CLayer.layertest.MapXYChange(m_polygon[i].x, m_polygon[i].y,canvas, ref m_polygon[i].Transformx, ref m_polygon[i].Transformy);
                    CLayer.layertest.MapXYChange(m_polygon[i + 1].x, m_polygon[i + 1].y, canvas, ref m_polygon[i + 1].Transformx, ref m_polygon[i + 1].Transformy);
                    myLineGeometry.StartPoint = new System.Windows.Point(m_polygon[i].Transformx, m_polygon[i].Transformy);
                    myLineGeometry.EndPoint = new System.Windows.Point(m_polygon[i + 1].Transformx, m_polygon[i + 1].Transformy);
                    Path myPath = new Path();
                    myPath.Stroke = new SolidColorBrush(this.color);
                    //myPath.Stroke = Brushes.Black;
                    myPath.StrokeThickness = width;
                    myPath.Data = myLineGeometry;
                    canvas.Children.Add(myPath);
                }
                ////遍历一下box
                //if (m_polygon[m_polygon.Count-1].x > MaxX)
                //    MaxX = m_polygon[m_polygon.Count - 1].x;
                //if (m_polygon[m_polygon.Count - 1].y > MaxY)
                //    MaxY = m_polygon[m_polygon.Count - 1].y;
                //if (m_polygon[m_polygon.Count - 1].x < MinX)
                //    MinX = m_polygon[m_polygon.Count - 1].x;
                //if (m_polygon[m_polygon.Count - 1].y < MinY)
                //    MinY = m_polygon[m_polygon.Count - 1].y;

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
                    CLayer.layertest.MapXYChange(m_polygon[i].x, m_polygon[i].y,canvas, ref m_polygon[i].Transformx, ref m_polygon[i].Transformy);
                    System.Windows.Point s_P = new System.Windows.Point(m_polygon[i].Transformx, m_polygon[i].Transformy);
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
                if(temp ==IsSelected)
                    newPolygon.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 230, 230, 250)); 
                else
                    newPolygon.Fill = new SolidColorBrush(this.paintColor);
                canvas.Children.Add(newPolygon);
            }
        }
    }
    //多面类 multi polygon class
    internal class MultiPolygon:Geometry
    {
        public List<Polygon> m_multiPolygon { get; set; }
        public double width { get; set; }//线宽

        public MultiPolygon()
        {
            m_multiPolygon = new List<Polygon>();
            this.color = System.Windows.Media.Colors.Black; // 设置一个默认的颜色
            this.width = 1.0; // 设置一个默认的线宽
        }
        public void PushPolygon(Polygon P)
        //添加面
        {

            m_multiPolygon.Add(P);
        }
        public void Draw(Canvas canvas)
        {
            foreach(var Pg in m_multiPolygon)
            {
                //画线&Fill
                Pg.Draw(canvas, false);
                Pg.Draw(canvas, true);
            }
          
        }
    }

    public class DataReader
    {
        // 示例函数，从文件中读取点数据
        internal List<Point> GetPointsFromFile(string filePath, MultiPoint save_Point)
        {
            List<Point> points = new List<Point>();
           

            // 示例：假设你有一个 MultiPoint 类的实例，包含多个点
            MultiPoint multiPoint = GetMultiPoint(save_Point); // 你需要实现获取多点的逻辑

            // 将 MultiPoint 中的点添加到列表中
            points.AddRange(multiPoint.m_multiPoint);

            return points;
        }

        // 示例函数，从文件中读取线数据
        internal List<Line> GetLinesFromFile(string filePath, MultiLine save_Line)
        {
            List<Line> lines = new List<Line>();

            // 示例：假设你有一个 MultiLine 类的实例，包含多条线
            MultiLine multiLine = GetMultiLine(save_Line); // 你需要实现获取多线的逻辑

            // 将 MultiLine 中的线添加到列表中
            lines.AddRange(multiLine.m_multiLine);

            return lines;
        }

        // 示例函数，从文件中读取多边形数据
        internal List<Polygon> GetPolygonsFromFile(string filePath, MultiPolygon save_Polygon)
        {
            List<Polygon> polygons = new List<Polygon>();

            // 示例：假设你有一个 MultiPolygon 类的实例，包含多个多边形
            MultiPolygon multiPolygon = GetMultiPolygon(save_Polygon); // 你需要实现获取多面的逻辑

            // 将 MultiPolygon 中的多边形添加到列表中
            polygons.AddRange(multiPolygon.m_multiPolygon);

            return polygons;
        }

        private MultiPoint GetMultiPoint(MultiPoint save_Point)
        {
            MultiPoint multiPoint = new MultiPoint();

            return save_Point;
        }

        private MultiLine GetMultiLine(MultiLine save_Line)
        {
            MultiLine multiLine = new MultiLine();

            return save_Line;
        }

        // 示例的获取多面的逻辑，你需要根据你的应用程序实现
        private MultiPolygon GetMultiPolygon(MultiPolygon save_Polygon)
        {
            MultiPolygon multiPolygon = new MultiPolygon();

            return save_Polygon;
        }
    }
}
