using Aspose.Gis.Rendering;
using DotSpatial;
using DotSpatial.Data;
using DotSpatial.Topology;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
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
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Rectangle = System.Windows.Shapes.Rectangle;


namespace SmallGISApp
{
    /// <summary>
    /// MouseDraw.xaml 的交互逻辑
    /// </summary>
    
    public partial class MouseDraw : Window
    {       
        //平移
        int Temp;
        private bool isDragging = false;
        Point lastPosition = new Point();
        bool isDrawingEnabled = false;//是否开始画图

        //交互
        double LimitDistance = 10;
        int InteractionVal = 0;

        //画线 面
        Point currentPoint = new Point();//记录目前最后一个点

        private bool isDrawingLine = false; // 是否处于绘制线条的模式
        Line currentLine = new Line();//记录每条线的轨迹

        private bool isDrawingPolygon = false; // 是否处于绘制面的模式
        Polygon currentPolygon = new Polygon();//记录每个面的点轨迹

        //橡皮筋 线
        System.Windows.Shapes.Path temPath = new System.Windows.Shapes.Path();//在鼠标划动过程 线的橡皮筋

        //图层 记录画好的点、线、面
        MultiPoint save_Point =new MultiPoint();
        MultiLine save_Line = new MultiLine();
        MultiPolygon save_Polygon = new MultiPolygon();

        System.Windows.Vector delta = new System.Windows.Vector();

        //定义一个回调或事件来接收GeoJSON数据。
        // 修改委托为包含两个参数
        public delegate void GeoJsonReceivedHandler(object sender, string geoJsonData);
        public event GeoJsonReceivedHandler OnGeoJsonReceived;
        public MouseDraw()
        {
            InitializeComponent();
            OnGeoJsonReceived += HandleGeoJsonData;
        }
        // 定义一个保护的方法来触发此事件
        public virtual void RaiseGeoJsonReceived(string geoJsonData)
        {
            OnGeoJsonReceived?.Invoke(this,geoJsonData);
        }
        private void HandleGeoJsonData(object sender, string geoJsonData)
        {

            var obj=JObject.Parse(geoJsonData);
            // 这里处理和绘制您的GeoJSON数据
            //var type = obj["type"].ToString();
            var features = obj["features"] as Newtonsoft.Json.Linq.JArray;
            if (features != null && features.Count > 0)
            {
                // 遍历所有的特征
                foreach (var feature in features)
                {
                    // 获取geometry的类型
                    var type = feature["geometry"]["type"].ToString();

                    switch (type)
                    {
                        case "Point":
                            // 处理点
                            var coordinates = feature["geometry"]["coordinates"];
                            var lon = coordinates[0];
                            var lat = coordinates[1];
                            Point point = new Point(lon.Value<double>(), lat.Value<double>());
                            //遍历一下box
                            if (point.x > CLayer.layertest.MaxX)
                                CLayer.layertest.MaxX = point.x;
                            if (point.y > CLayer.layertest.MaxY)
                                CLayer.layertest.MaxY = point.y;
                            if (point.x < CLayer.layertest.MinX)
                                CLayer.layertest.MinX = point.x;
                            if (point.y < CLayer.layertest.MinY)
                                CLayer.layertest.MinY = point.y;
                            // 可以设置其它属性
                            break;
                        case "Line":
                            Line myLine = null;
                            myLine = new Line();
                            foreach (var coord in feature["geometry"]["coordinates"])
                            {
                                
                                Point p = new Point(coord[0].Value<double>(), coord[1].Value<double>());
                                //遍历一下box
                                myLine.m_Line.Add(p);
                                if (p.x > CLayer.layertest.MaxX)
                                    CLayer.layertest.MaxX = p.x;
                                if (p.y > CLayer.layertest.MaxY)
                                    CLayer.layertest.MaxY = p.y;
                                if (p.x < CLayer.layertest.MinX)
                                    CLayer.layertest.MinX = p.x;
                                if (p.y < CLayer.layertest.MinY)
                                    CLayer.layertest.MinY =p.y;
                            }
                            break;
                        case "MultiLineString":
                            foreach (var singleLineString in feature["geometry"]["coordinates"])
                            {
                                myLine = new Line();
                                foreach (var coord in singleLineString)
                                {
                                    Point p = new Point(coord[0].Value<double>(), coord[1].Value<double>());
                                    //遍历一下box
                                    myLine.m_Line.Add(p);
                                    if (p.x > CLayer.layertest.MaxX)
                                        CLayer.layertest.MaxX = p.x;
                                    if (p.y > CLayer.layertest.MaxY)
                                        CLayer.layertest.MaxY = p.y;
                                    if (p.x < CLayer.layertest.MinX)
                                        CLayer.layertest.MinX = p.x;
                                    if (p.y < CLayer.layertest.MinY)
                                        CLayer.layertest.MinY = p.y;
                                }
                            }
                            break;
                        case "Polygon":
                            Polygon myPolygon = null;
                            myPolygon = new Polygon();
                            foreach (var coord in feature["geometry"]["coordinates"][0])
                            {
                                Point p = new Point(coord[0].Value<double>(), coord[1].Value<double>());
                                //遍历一下box
                                myPolygon.m_polygon.Add(p);
                                if (p.x > CLayer.layertest.MaxX)
                                    CLayer.layertest.MaxX = p.x;
                                if (p.y > CLayer.layertest.MaxY)
                                    CLayer.layertest.MaxY = p.y;
                                if (p.x < CLayer.layertest.MinX)
                                    CLayer.layertest.MinX = p.x;
                                if (p.y < CLayer.layertest.MinY)
                                    CLayer.layertest.MinY = p.y;
                            }
                            break;
                        case"MultiPolygon":
                            foreach (var singlePolygon in feature["geometry"]["coordinates"])
                            {
                                myPolygon = new Polygon();
                                foreach (var coord in singlePolygon[0])
                                {
                                    Point p = new Point(coord[0].Value<double>(), coord[1].Value<double>());
                                    //遍历一下box
                                    myPolygon.m_polygon.Add(p);
                                    if (p.x > CLayer.layertest.MaxX)
                                        CLayer.layertest.MaxX = p.x;
                                    if (p.y > CLayer.layertest.MaxY)
                                        CLayer.layertest.MaxY = p.y;
                                    if (p.x < CLayer.layertest.MinX)
                                        CLayer.layertest.MinX = p.x;
                                    if (p.y < CLayer.layertest.MinY)
                                        CLayer.layertest.MinY = p.y;
                                }
                            }
                            break;
                            //switch (feature.Geometry)
                            //{
                            //    case NetTopologySuite.Geometries.LineString lineString:
                            //        // 处理线
                                   

                            //    case NetTopologySuite.Geometries.MultiLineString multiLineString:
                            //        // 处理多线
                            //        foreach (var singleLineString in multiLineString.Geometries)
                            //        {
                            //            myLine = new Line();
                            //            foreach (NetTopologySuite.Geometries.Coordinate coord in singleLineString.Coordinates)
                            //            {
                            //                Point p = new Point(coord.X, coord.Y);
                            //                if (coord.X > CLayer.layertest.MaxX)
                            //                    CLayer.layertest.MaxX = coord.X;
                            //                if (coord.Y > CLayer.layertest.MaxY)
                            //                    CLayer.layertest.MaxY = coord.Y;
                            //                if (coord.X < CLayer.layertest.MinX)
                            //                    CLayer.layertest.MinX = coord.X;
                            //                if (coord.Y < CLayer.layertest.MinY)
                            //                    CLayer.layertest.MinY = coord.Y;
                            //                myLine.m_Line.Add(p);
                            //            }
                            //        }
                            //        break;

                            //    default:
                            //        // 其他几何类型
                            //        break;
                            //}
                            //break;
                            //        case DotSpatial.Data.FeatureType.Polygon:
                            //            Polygon myPolygon = null;
                            //            switch (feature.Geometry)
                            //            {

                            //                case NetTopologySuite.Geometries.Polygon polygon:
                            //                    // 处理面
                            //                    myPolygon = new Polygon();
                            //                    foreach (NetTopologySuite.Geometries.Coordinate coord in polygon.Shell.Coordinates)
                            //                    {
                            //                        Point p = new Point(coord.X, coord.Y);
                            //                        if (coord.X > CLayer.layertest.MaxX)
                            //                            CLayer.layertest.MaxX = coord.X;
                            //                        if (coord.Y > CLayer.layertest.MaxY)
                            //                            CLayer.layertest.MaxY = coord.Y;
                            //                        if (coord.X < CLayer.layertest.MinX)
                            //                            CLayer.layertest.MinX = coord.X;
                            //                        if (coord.Y < CLayer.layertest.MinY)
                            //                            CLayer.layertest.MinY = coord.Y;
                            //                        myPolygon.m_polygon.Add(p);
                            //                    }
                            //                    break;

                            //                case NetTopologySuite.Geometries.MultiPolygon multiPolygon:
                            //                    // 处理多面
                            //                    foreach (var singlePolygon in multiPolygon.Geometries)
                            //                    {
                            //                        myPolygon = new Polygon();
                            //                        foreach (NetTopologySuite.Geometries.Coordinate coord in singlePolygon.Coordinates)
                            //                        {
                            //                            Point p = new Point(coord.X, coord.Y);
                            //                            if (coord.X > CLayer.layertest.MaxX)
                            //                                CLayer.layertest.MaxX = coord.X;
                            //                            if (coord.Y > CLayer.layertest.MaxY)
                            //                                CLayer.layertest.MaxY = coord.Y;
                            //                            if (coord.X < CLayer.layertest.MinX)
                            //                                CLayer.layertest.MinX = coord.X;
                            //                            if (coord.Y < CLayer.layertest.MinY)
                            //                                CLayer.layertest.MinY = coord.Y;
                            //                            myPolygon.m_polygon.Add(p);
                            //                        }
                            //                    }
                            //                    break;

                            //                default:
                            //                    // 其他几何类型
                            //                    break;
                            //            }
                            //            break;
                            //        default:
                            //            MessageBox.Show("Unsupported geometry type.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            //            break;
                            //    }
                    }
                    CLayer.layertest.SetFrame(ref CLayer.layertest, MousedrawingCanvas);
                }

                foreach (var feature in features)
                {
                    var type = feature["geometry"]["type"].ToString();
                    switch (type)
                    {
                        case "Point":
                            // 处理点
                            var coordinates = feature["geometry"]["coordinates"];
                            var lon = coordinates[0];
                            var lat = coordinates[1];
                            Point myPoint = new Point(lon.Value<double>(), lat.Value<double>());
                            // 可以设置其它属性
                            myPoint.Draw(MousedrawingCanvas); // 绘制点
                            save_Point.PushPoint(myPoint);
                            break;
                        case "Line":
                            Line myLine = null;
                            myLine = new Line();
                            foreach (var coord in feature["geometry"]["coordinates"])
                            {
                                Point p = new Point(coord[0].Value<double>(), coord[1].Value<double>());
                                myLine.m_Line.Add(p);
                            }
                            myLine.Draw(MousedrawingCanvas);
                            save_Line.PushLine(myLine);
                            break;
                        case "MultiLineString":
                            foreach (var singleLineString in feature["geometry"]["coordinates"])
                            {
                                myLine = new Line();
                                foreach (var coord in singleLineString)
                                {
                                    Point p = new Point(coord[0].Value<double>(), coord[1].Value<double>());
                                    myLine.m_Line.Add(p);
                                }
                                myLine.Draw(MousedrawingCanvas);
                                save_Line.PushLine(myLine);
                            }
                            break;
                        case "Polygon":
                            Polygon myPolygon = null;
                            myPolygon = new Polygon();
                            foreach (var coord in feature["geometry"]["coordinates"][0])
                            {
                                Point p = new Point(coord[0].Value<double>(), coord[1].Value<double>());
                                //遍历一下box
                                myPolygon.m_polygon.Add(p);

                            }
                            myPolygon.Draw(MousedrawingCanvas, true);
                            save_Polygon.PushPolygon(myPolygon);
                            break;
                        case "MultiPolygon":
                            foreach (var singlePolygon in feature["geometry"]["coordinates"])
                            {
                                myPolygon = new Polygon();
                                foreach (var coord in singlePolygon[0])
                                {
                                    Point p = new Point(coord[0].Value<double>(), coord[1].Value<double>());
                                    //遍历一下box
                                    myPolygon.m_polygon.Add(p);

                                }
                                myPolygon.Draw(MousedrawingCanvas, true);
                                save_Polygon.PushPolygon(myPolygon);
                            }
                            break;


                    }
                }
                
            }
            string formattedText = $"1: {1 / ((23.8 * 0.01) / (CLayer.layertest.scale * 1080 * 111 * 1000))}";
            ScaleBox.Text = formattedText;

            //var coordinates = obj["geometry"]["coordinates"];
            //var lon = coordinates[0];
            //var lat = coordinates[1];

            //MessageBox.Show(geoJsonData);
        }


        //计算两点距离的平方
        private double Distance(Point p1, Point p2)
        {
            return Math.Pow(p1.Transformx - p2.Transformx, 2) + Math.Pow(p1.Transformy - p2.Transformy, 2);
        }


        //计算点到线段的距离
        private double Poin2Distance(Point p1, Point p2, Point p3)
        {
            double d12 = Distance(p1, p2);
            double d13 = Distance(p1, p3);
            double d23 = Distance(p2, p3);
            double aLessDP = d12 > d13 ? d13 : d12;
            double S123 = Math.Abs((p1.Transformx - p2.Transformx) * (p1.Transformy - p3.Transformy) - (p1.Transformx - p3.Transformx) * (p1.Transformy - p2.Transformy));
            double distanceLine = S123 / Math.Sqrt(d23);
            if (d12 + d23 > d13 && d13 + d23 > d12)
                return distanceLine;
            else
                return Math.Sqrt(aLessDP);
        }

        //计算点是否在多边形内
        private int pnpoly(Polygon _vPoint, Point test)
        {
            int i, j, c = 0;
            for (i = 0, j = _vPoint.m_polygon.Count - 2; i < _vPoint.m_polygon.Count - 1; j = i++)
            {
                if (((_vPoint.m_polygon[i].Transformy > test.Transformy) != (_vPoint.m_polygon[j].Transformy > test.Transformy)) &&
                    (test.Transformx < (_vPoint.m_polygon[j].Transformx - _vPoint.m_polygon[i].Transformx) * (test.Transformy - _vPoint.m_polygon[i].Transformy) / (_vPoint.m_polygon[j].Transformy - _vPoint.m_polygon[i].Transformy) + _vPoint.m_polygon[i].Transformx))
                    c = 1 - c;
            }
            return c;
        }

        private void MouseDrawButton_Click(object sender, RoutedEventArgs e)
        {
            isDrawingEnabled = !isDrawingEnabled; // Toggle the drawing state

            // Change the button content based on the drawing state
            if (isDrawingEnabled)
            {
                (sender as Button).Content = "Stop Drawing";
            }
            else
            {
                (sender as Button).Content = "Start Drawing";
            }
        }

        private ObservableCollection<Layer> layerslist = new ObservableCollection<Layer>();
        Layer layer1 = new Layer("user_point");
        Layer layer2 = new Layer("user_line");
        Layer layer3 = new Layer("user_polygon");



        private void MousedrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (InteractionVal != 0 && !isDragging)
            {
                MousedrawingCanvas.Children.Clear();
                // Get the mouse click position
                double winX = e.GetPosition(MousedrawingCanvas).X;
                double winY = e.GetPosition(MousedrawingCanvas).Y;
                Point m_p = new Point();
                m_p.Transformx = winX;
                m_p.Transformy = winY;

                switch (InteractionVal)
                {
                    case 3:
                        foreach (Polygon polygon in save_Polygon.m_multiPolygon)
                        {
                            polygon.IsSelected = 0;
                            if (pnpoly(polygon, m_p) == 1)
                                polygon.IsSelected = 1;
                            polygon.Draw(MousedrawingCanvas, true);
                        }
                        break;
                    case 2:
                        foreach (Line line in save_Line.m_multiLine)
                        {
                            line.IsSelected = 0;
                            for (int i = 0; i + 1 < line.m_Line.Count; i++)
                            {
                                double distanceTemporaryPoint = Poin2Distance(m_p, line.m_Line[i], line.m_Line[i + 1]);
                                if (distanceTemporaryPoint < LimitDistance)
                                {
                                    line.IsSelected = 1;
                                }                                  
                            }
                            line.Draw(MousedrawingCanvas);
                        }
                        break;
                    case 1:
                        foreach (Point point in save_Point.m_multiPoint)
                        {
                            point.IsSelected = 0;
                            if (Distance(point, m_p) < LimitDistance)
                                point.IsSelected = 1;
                            point.Draw(MousedrawingCanvas);
                        }
                        break;
                    default:
                        break;
                }
            }
            

            if (isDragging == true)
            {
                isDrawingEnabled = false;
                // 获取鼠标点击的位置，并记录下来以便后续计算平移量
                lastPosition.x = e.GetPosition(MousedrawingCanvas).X;
                lastPosition.y = e.GetPosition(MousedrawingCanvas).Y;

                // 开始捕获鼠标，以便在拖动时能够持续更新位置信息
                MousedrawingCanvas.CaptureMouse();

            }


            //1. 鼠标画点
            if (Item_Point.IsSelected == true && isDrawingEnabled == true)
            {
                // Get the mouse click position
                double winX = e.GetPosition(MousedrawingCanvas).X;
                double winY = e.GetPosition(MousedrawingCanvas).Y;

                // 先进行坐标变化
                CLayer.layertest.reverseChange(MousedrawingCanvas, ref winX, ref winY);

                //画点按钮
                Point m_p = new Point(winX, winY);                

                //画点的半径 由拖动栏控制数值
                m_p.radius = Size.Value;
                //画点的颜色 由颜色选择器控制
                m_p.color = (Color)DrawcolorPicker.SelectedColor;
                m_p.Draw(MousedrawingCanvas);
                if(layer1.Points.Count == 0)
                {
                    layer1.AddPoint(m_p);
                    layerslist.Add(layer1);
                    layersTreeView.ItemsSource = layerslist;
                }
                else
                {
                    layer1.AddPoint(m_p);
                    layersTreeView.ItemsSource = layerslist;
                }
                //图层记录点
                save_Point.m_multiPoint.Add(m_p);
            }
            //2. 鼠标画线
            else if (Item_Line.IsSelected == true && isDrawingEnabled == true)
            {
                // Get the mouse click position
                double winX = e.GetPosition(MousedrawingCanvas).X;
                double winY = e.GetPosition(MousedrawingCanvas).Y;

                // 先进行坐标变化
                CLayer.layertest.reverseChange(MousedrawingCanvas, ref winX, ref winY);

                currentPoint = new Point(winX, winY);
                if (e.LeftButton == MouseButtonState.Pressed && !isDrawingLine)
                {

                    currentLine.m_Line.Add(currentPoint);
                    isDrawingLine = true;

                }
                else if (e.LeftButton == MouseButtonState.Pressed && isDrawingLine)
                {
                    // 更新线条的结束位置
                    double Xtemp = e.GetPosition(MousedrawingCanvas).X;
                    double Ytemp = e.GetPosition(MousedrawingCanvas).Y;
                    
                    // 先进行坐标变化
                    CLayer.layertest.reverseChange(MousedrawingCanvas, ref Xtemp, ref Ytemp);

                    currentPoint.x = Xtemp;
                    currentPoint.y = Ytemp;

                    // 添加到线中
                    currentLine.m_Line.Add(currentPoint);
                    if (layer2.Lines.Count == 0)
                    {
                        layer2.AddLine(currentLine);
                        layerslist.Add(layer2);
                        layersTreeView.ItemsSource = layerslist;
                    }
                    else
                    {
                        layer2.AddLine(currentLine);
                        layersTreeView.ItemsSource = layerslist;
                    }
                    // 画线颜色
                    currentLine.color = (Color)DrawcolorPicker.SelectedColor;
                    // 画线宽
                    currentLine.width = Size.Value;
                    currentLine.Draw(MousedrawingCanvas);
                }
                else if (e.RightButton == MouseButtonState.Pressed && isDrawingLine)
                {
                    // 右击鼠标结束绘制线条
                    // 保存线条到多线图层中
                    save_Line.PushLine(currentLine);

                    // 去掉橡皮筋线条
                    if (temPath != null)
                    {
                        MousedrawingCanvas.Children.Remove(temPath);
                    }
                    //重新初始化
                    currentLine = new Line();
                    isDrawingLine = false;
                }
            }
            //3. 鼠标画面
            else if (Item_Polygon.IsSelected == true && isDrawingEnabled == true)
            {
                // Get the mouse click position
                double winX = e.GetPosition(MousedrawingCanvas).X;
                double winY = e.GetPosition(MousedrawingCanvas).Y;

                // 先进行坐标变化
                CLayer.layertest.reverseChange(MousedrawingCanvas, ref winX, ref winY);

                currentPoint = new Point(winX, winY);
                if (e.LeftButton == MouseButtonState.Pressed && !isDrawingPolygon)
                {
                    currentPolygon.m_polygon.Add(currentPoint);
                    isDrawingPolygon = true;
                }
                else if (e.LeftButton == MouseButtonState.Pressed && isDrawingPolygon)
                {
                    // 更新线条的结束位置
                    double Xtemp = e.GetPosition(MousedrawingCanvas).X;
                    double Ytemp = e.GetPosition(MousedrawingCanvas).Y;

                    // 先进行坐标变化
                    CLayer.layertest.reverseChange(MousedrawingCanvas, ref Xtemp, ref Ytemp);

                    currentPoint.x = Xtemp;
                    currentPoint.y = Ytemp;

                    // 添加到线中
                    currentPolygon.m_polygon.Add(currentPoint);
                    if (layer3.Polygons.Count == 0)
                    {
                        layer3.AddPolygon(currentPolygon);
                        layerslist.Add(layer3);
                        layersTreeView.ItemsSource = layerslist;
                    }
                    else
                    {
                        layer3.AddPolygon(currentPolygon);
                        layersTreeView.ItemsSource = layerslist;
                    }
                    // 画线的颜色
                    currentPolygon.color = (Color)DrawcolorPicker.SelectedColor;
                    // 画多边形的颜色
                    currentPolygon.paintColor = (Color)PaintcolorPicker.SelectedColor;
                    // 画多边形线宽
                    currentPolygon.width = Size.Value;
                    currentPolygon.Draw(MousedrawingCanvas, false);
                }
                else if (e.RightButton == MouseButtonState.Pressed && isDrawingPolygon)
                {
                    // 右击鼠标 开始涂色多边形
                    currentPolygon.Draw(MousedrawingCanvas, true);

                    // 重新初始化
                    save_Polygon.PushPolygon(currentPolygon);
                    currentPolygon = new Polygon();
                    isDrawingPolygon = false;
                    if (temPath != null)
                    {
                        MousedrawingCanvas.Children.Remove(temPath);
                    }
                }
            }
        }


        private void MousedrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            double newX = 0;
            double newY = 0;
            // 获取鼠标的当前位置
            newX = e.GetPosition(MousedrawingCanvas).X;
            newY = e.GetPosition(MousedrawingCanvas).Y;           
            
            CLayer.layertest.reverseChange(MousedrawingCanvas,ref newX,ref newY);

            // 更新TextBox的文本为鼠标的实时位置
            string formattedText = $"X: {newX:F4}, Y: {newY:F4}";
            MouseTextBox.Text = formattedText;

            //橡皮筋效果
            if ((Item_Polygon.IsSelected == true || Item_Line.IsSelected == true) && (isDrawingLine || isDrawingPolygon) && (currentLine != null || currentPolygon != null))
            {
                double winX = e.GetPosition(MousedrawingCanvas).X;
                double winY = e.GetPosition(MousedrawingCanvas).Y;

                // 先进行坐标变化
                //CLayer.layertest.reverseChange(MousedrawingCanvas, ref winX, ref winY);

                double Xtemp = currentPoint.x;
                double Ytemp = currentPoint.y;

                CLayer.layertest.MapXYChange(currentPoint.x, currentPoint.y, MousedrawingCanvas,ref Xtemp,ref Ytemp);

                LineGeometry myLineGeometry = new LineGeometry();
                myLineGeometry.StartPoint = new System.Windows.Point(Xtemp, Ytemp);
                myLineGeometry.EndPoint = new System.Windows.Point(winX, winY);

                if (!MousedrawingCanvas.Children.Contains(temPath))
                {
                    temPath.Stroke = new SolidColorBrush((Color)DrawcolorPicker.SelectedColor);
                    temPath.StrokeThickness = Size.Value;
                    MousedrawingCanvas.Children.Add(temPath);
                }

                temPath.Data = myLineGeometry;
            }

            if (e.LeftButton == MouseButtonState.Pressed && isDragging)
            {
                // 计算鼠标位置的变化量
                Double newPositionX = e.GetPosition(MousedrawingCanvas).X;
                Double newPositionY = e.GetPosition(MousedrawingCanvas).Y;

                Point newPosition = new Point(newPositionX, newPositionY);
                
                delta.X = newPosition.x - lastPosition.x;
                delta.Y = newPosition.y - lastPosition.y;

                Translate(delta);

                // 更新鼠标位置
                lastPosition = newPosition;
            }
            else if (e.RightButton == MouseButtonState.Pressed && isDrawingPolygon)
            {
                // 右击鼠标 开始涂色多边形
                currentPolygon.Draw(MousedrawingCanvas, true);

                // 重新初始化
                save_Polygon.PushPolygon(currentPolygon);
                currentPolygon = new Polygon();
                isDrawingPolygon = false;
                if (temPath != null)
                {
                    MousedrawingCanvas.Children.Remove(temPath);
                }


            }
        }
        private void MousedrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                MousedrawingCanvas.ReleaseMouseCapture();
            }
        }
        private void Translate(System.Windows.Vector translation)
        {
            double deltax = translation.X * CLayer.layertest.scale;
            double deltay = translation.Y * CLayer.layertest.scale;
            CLayer.layertest.centerX -= deltax;
            CLayer.layertest.centerY += deltay;

            MousedrawingCanvas.Children.Clear();

            foreach (Polygon polygon in save_Polygon.m_multiPolygon)
            {
                polygon.Draw(MousedrawingCanvas, true);
            }
            foreach (Line line in save_Line.m_multiLine)
            {
                line.Draw(MousedrawingCanvas);
            }         
            foreach (Point point in save_Point.m_multiPoint)
            {
                point.Draw(MousedrawingCanvas);
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TwitterButton_OnClick(object sender, RoutedEventArgs e)
        {
            // 创建文件选择对话框
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".shp";
            dlg.Filter = "Shapefile Documents (.shp)|*.shp";

            // 显示文件选择对话框
            Nullable<bool> result = dlg.ShowDialog();

            // 处理文件选择对话框的结果
            if (result == true)
            {
                // 打开选择的文件
                string filename = dlg.FileName;

                // 检查是否是.shp文件
                if (System.IO.Path.GetExtension(filename).ToLower() != ".shp")
                {
                    MessageBox.Show("Selected file is not a .shp file!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 载入shapefile
                DotSpatial.Data.IFeatureSet featureSet = DotSpatial.Data.FeatureSet.OpenFile(filename);

                // 获取文件名（不包含扩展名）
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filename);

                // 创建一个新的图层对象
                Layer layer = new Layer(fileNameWithoutExtension);

                // 遍历所有的特征
                foreach (DotSpatial.Data.IFeature feature in featureSet.Features)
                {
                    // 获取geometry的类型
                    var featureType = feature.FeatureType;

                    switch (featureType)
                    {
                        case DotSpatial.Data.FeatureType.Point:
                            // 处理点
                            NetTopologySuite.Geometries.Point point = feature.Geometry as NetTopologySuite.Geometries.Point;
                            Point myPoint = new Point(point.X, point.Y);
                            layer.AddPoint(myPoint);

                            //遍历一下box
                            if (point.X > CLayer.layertest.MaxX)
                                CLayer.layertest.MaxX = point.X;
                            if (point.Y > CLayer.layertest.MaxY)
                                CLayer.layertest.MaxY = point.Y;
                            if (point.X < CLayer.layertest.MinX)
                                CLayer.layertest.MinX = point.X;
                            if (point.Y < CLayer.layertest.MinY)
                                CLayer.layertest.MinY = point.Y;
                            // 可以设置其它属性
                            break;
                        case DotSpatial.Data.FeatureType.Line:
                            Line myLine = null;
                            switch (feature.Geometry)
                            {
                                case NetTopologySuite.Geometries.LineString lineString:
                                    // 处理线
                                    myLine = new Line();

                                    foreach (NetTopologySuite.Geometries.Coordinate coord in lineString.Coordinates)
                                    {
                                        Point p = new Point(coord.X, coord.Y);
                                        myLine.m_Line.Add(p);
                                        if (coord.X > CLayer.layertest.MaxX)
                                            CLayer.layertest.MaxX = coord.X;
                                        if (coord.Y > CLayer.layertest.MaxY)
                                            CLayer.layertest.MaxY = coord.Y;
                                        if (coord.X < CLayer.layertest.MinX)
                                            CLayer.layertest.MinX = coord.X;
                                        if (coord.Y < CLayer.layertest.MinY)
                                            CLayer.layertest.MinY = coord.Y;
                                    }
                                    layer.AddLine(myLine);

                                    break;

                                case NetTopologySuite.Geometries.MultiLineString multiLineString:
                                    // 处理多线
                                    foreach (var singleLineString in multiLineString.Geometries)
                                    {
                                        myLine = new Line();

                                        foreach (NetTopologySuite.Geometries.Coordinate coord in singleLineString.Coordinates)
                                        {
                                            Point p = new Point(coord.X, coord.Y);
                                            if (coord.X > CLayer.layertest.MaxX)
                                                CLayer.layertest.MaxX = coord.X;
                                            if (coord.Y > CLayer.layertest.MaxY)
                                                CLayer.layertest.MaxY = coord.Y;
                                            if (coord.X < CLayer.layertest.MinX)
                                                CLayer.layertest.MinX = coord.X;
                                            if (coord.Y < CLayer.layertest.MinY)
                                                CLayer.layertest.MinY = coord.Y;
                                            myLine.m_Line.Add(p);
                                        }
                                        layer.AddLine(myLine);
                                    }
                                    break;

                                default:
                                    // 其他几何类型
                                    break;
                            }
                            break;
                        case DotSpatial.Data.FeatureType.Polygon:
                            Polygon myPolygon = null;
                            switch (feature.Geometry)
                            {

                                case NetTopologySuite.Geometries.Polygon polygon:
                                    // 处理面
                                    myPolygon = new Polygon();

                                    foreach (NetTopologySuite.Geometries.Coordinate coord in polygon.Shell.Coordinates)
                                    {
                                        Point p = new Point(coord.X, coord.Y);
                                        if (coord.X > CLayer.layertest.MaxX)
                                            CLayer.layertest.MaxX = coord.X;
                                        if (coord.Y > CLayer.layertest.MaxY)
                                            CLayer.layertest.MaxY = coord.Y;
                                        if (coord.X < CLayer.layertest.MinX)
                                            CLayer.layertest.MinX = coord.X;
                                        if (coord.Y < CLayer.layertest.MinY)
                                            CLayer.layertest.MinY = coord.Y;
                                        myPolygon.m_polygon.Add(p);
                                    }
                                    layer.AddPolygon(myPolygon);

                                    break;

                                case NetTopologySuite.Geometries.MultiPolygon multiPolygon:
                                    // 处理多面
                                    foreach (var singlePolygon in multiPolygon.Geometries)
                                    {
                                        myPolygon = new Polygon();

                                        foreach (NetTopologySuite.Geometries.Coordinate coord in singlePolygon.Coordinates)
                                        {
                                            Point p = new Point(coord.X, coord.Y);
                                            if (coord.X > CLayer.layertest.MaxX)
                                                CLayer.layertest.MaxX = coord.X;
                                            if (coord.Y > CLayer.layertest.MaxY)
                                                CLayer.layertest.MaxY = coord.Y;
                                            if (coord.X < CLayer.layertest.MinX)
                                                CLayer.layertest.MinX = coord.X;
                                            if (coord.Y < CLayer.layertest.MinY)
                                                CLayer.layertest.MinY = coord.Y;
                                            myPolygon.m_polygon.Add(p);
                                        }
                                        layer.AddPolygon(myPolygon);
                                    }
                                    break;

                                default:
                                    // 其他几何类型
                                    break;
                            }
                            break;
                        default:
                            MessageBox.Show("Unsupported geometry type.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
                CLayer.layertest.SetFrame(ref CLayer.layertest, MousedrawingCanvas);
                layerslist.Add(layer);
                layersTreeView.ItemsSource = layerslist;

                // 遍历所有的特征
                foreach (DotSpatial.Data.IFeature feature in featureSet.Features)
                {
                    // 获取geometry的类型
                    var featureType = feature.FeatureType;

                    switch (featureType)
                    {
                        case DotSpatial.Data.FeatureType.Point:
                            // 处理点
                            NetTopologySuite.Geometries.Point point = feature.Geometry as NetTopologySuite.Geometries.Point;
                            Point myPoint = new Point(point.X, point.Y);                         
                            // 可以设置其它属性
                            myPoint.Draw(MousedrawingCanvas); // 绘制点
                            save_Point.PushPoint(myPoint);
                            break;
                        case DotSpatial.Data.FeatureType.Line:
                            Line myLine = null;
                            switch (feature.Geometry)
                            {
                                case NetTopologySuite.Geometries.LineString lineString:
                                    // 处理线
                                    myLine = new Line();
                                    foreach (NetTopologySuite.Geometries.Coordinate coord in lineString.Coordinates)
                                    {
                                        Point p = new Point(coord.X, coord.Y);
                                        myLine.m_Line.Add(p);
                                    }
                                    myLine.Draw(MousedrawingCanvas); // 绘制线
                                    save_Line.PushLine(myLine);
                                    break;

                                case NetTopologySuite.Geometries.MultiLineString multiLineString:
                                    // 处理多线
                                    foreach (var singleLineString in multiLineString.Geometries)
                                    {
                                        myLine = new Line();
                                        foreach (NetTopologySuite.Geometries.Coordinate coord in singleLineString.Coordinates)
                                        {
                                            Point p = new Point(coord.X, coord.Y);
                                            myLine.m_Line.Add(p);
                                        }
                                        myLine.Draw(MousedrawingCanvas); // 绘制线
                                    }
                                    break;

                                default:
                                    // 其他几何类型
                                    break;
                            }
                            break;
                        case DotSpatial.Data.FeatureType.Polygon:
                            Polygon myPolygon = null;
                            switch (feature.Geometry)
                            {

                                case NetTopologySuite.Geometries.Polygon polygon:
                                    // 处理面
                                    myPolygon = new Polygon();
                                    foreach (NetTopologySuite.Geometries.Coordinate coord in polygon.Shell.Coordinates)
                                    {
                                        Point p = new Point(coord.X, coord.Y);
                                        myPolygon.m_polygon.Add(p);
                                    }
                                    myPolygon.Draw(MousedrawingCanvas, false); // 绘制面的边界
                                    myPolygon.Draw(MousedrawingCanvas, true); // 填充面
                                    save_Polygon.PushPolygon(myPolygon);
                                    break; 

                                case NetTopologySuite.Geometries.MultiPolygon multiPolygon:
                                    // 处理多面
                                    foreach (var singlePolygon in multiPolygon.Geometries)
                                    {
                                        myPolygon = new Polygon();
                                        foreach (NetTopologySuite.Geometries.Coordinate coord in singlePolygon.Coordinates)
                                        {
                                            Point p = new Point(coord.X, coord.Y);
                                            myPolygon.m_polygon.Add(p);
                                        }
                                        myPolygon.Draw(MousedrawingCanvas, false); // 绘制面的边界
                                        myPolygon.Draw(MousedrawingCanvas, true); // 填充面
                                        save_Polygon.PushPolygon(myPolygon);
                                    }
                                    //save_Polygon.Draw(MousedrawingCanvas);

                                    break;

                                default:
                                    // 其他几何类型
                                    break;
                            }
                            break;
                        default:
                            MessageBox.Show("Unsupported geometry type.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
            }
            string formattedText = $"1: {1 / ((23.8 * 0.01) / (CLayer.layertest.scale * 1080 * 111 * 1000))}";
            ScaleBox.Text = formattedText;
        }

        private void RedrawLayers()
        {
            // 清除当前画布上的所有图形
            MousedrawingCanvas.Children.Clear();

            // 遍历所有图层
            foreach (var layer in layerslist)
            {
                // 如果图层是可见的，则绘制它
                if (layer.is_visible)
                {
                    // 绘制图层中的所有点
                    foreach (var point in layer.Points)
                    {
                        point.Draw(MousedrawingCanvas);
                    }

                    // 绘制图层中的所有线
                    foreach (var line in layer.Lines)
                    {
                        line.Draw(MousedrawingCanvas);
                    }

                    // 绘制图层中的所有多边形
                    foreach (var polygon in layer.Polygons)
                    {
                        polygon.Draw(MousedrawingCanvas, false); // 绘制多边形的边界
                        polygon.Draw(MousedrawingCanvas, true);  // 填充多边形
                    }
                }
            }
        }



        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var layer = checkBox.DataContext as Layer;
            layer.is_visible = true;

            // 重新绘制图层
            RedrawLayers();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var layer = checkBox.DataContext as Layer;
            layer.is_visible = false;

            // 重新绘制图层
            RedrawLayers();
        }

        private void Checkbox_Edit_Checked(object sender, RoutedEventArgs e)
        {
            Item_Point.IsEnabled = true;
            Item_Line.IsEnabled = true;
            Item_Polygon.IsEnabled = true;
            isDrawingEnabled = true;
        }

        private void Checkbox_Edit_Unchecked(object sender, RoutedEventArgs e)
        {
            Item_Point.IsEnabled = false;
            Item_Line.IsEnabled = false;
            Item_Polygon.IsEnabled = false;
            isDrawingEnabled = false;
        }

        //private void Slider_PointSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    ComboBox_PointSize.Text = Slider_PointSize.Value.ToString();
        //}

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //更改画点的半径
            currentPoint.radius = Size.Value;
            //更改画线宽
            currentLine.width = Size.Value;
            currentPolygon.width = Size.Value;
        }
        private void ListBoxItem_Goodbye_Selected(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Item_Point_Selected(object sender, RoutedEventArgs e)

        {
            SizeName.Content = "Point Size:";
            Size.Value = 5;
            Size.Minimum = 1;
            Size.Maximum = 50;
        }
        private void Item_Line_Selected(object sender, RoutedEventArgs e)
        {
            SizeName.Content = "Line Size:";
            Size.Value = 1;
            Size.Minimum = 1;
            Size.Maximum = 10;
        }
        private void Item_Polygon_Selected(object sender, RoutedEventArgs e)
        {
            SizeName.Content = "Line Size:";
            Size.Value = 1;
            Size.Minimum = 1;
            Size.Maximum = 10;
        }
        private void WrappedRectangle(object sender, RoutedEventArgs e)
        {
            foreach (Line line in save_Line.m_multiLine)
            {
                // 获取四个角的位置
                double left = line.MinX;
                double top = line.MinY;
                double right = line.MaxX;
                double bottom = line.MaxY;

                // 计算矩形框的位置和尺寸
                double width = right - left;
                double height = bottom - top;

                // 创建一个矩形框对象
                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();

                // 设置矩形框的位置和尺寸
                rectangle.Margin = new Thickness(left, top, 0, 0);
                rectangle.Width = width;
                rectangle.Height = height;

                // 设置矩形框的边框样式和颜色
                rectangle.Stroke = System.Windows.Media.Brushes.Red;
                rectangle.StrokeThickness = 2;

                // 将矩形框添加到Canvas控件中
                MousedrawingCanvas.Children.Add(rectangle);

            }

            foreach (Polygon polygon in save_Polygon.m_multiPolygon)
            {
                // 获取四个角的位置
                double left = polygon.MinX;
                double top = polygon.MinY;
                double right = polygon.MaxX;
                double bottom = polygon.MaxY;

                // 计算矩形框的位置和尺寸
                double width = right - left;
                double height = bottom - top;

                // 创建一个矩形框对象
                Rectangle rectangle = new Rectangle();

                // 设置矩形框的位置和尺寸
                rectangle.Margin = new Thickness(left, top, 0, 0);
                rectangle.Width = width;
                rectangle.Height = height;

                // 设置矩形框的边框样式和颜色
                rectangle.Stroke = System.Windows.Media.Brushes.Red;
                rectangle.StrokeThickness = 2;

                // 将矩形框添加到Canvas控件中
                MousedrawingCanvas.Children.Add(rectangle);
            }
        }
        private void PaintColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                Color selectedColor = e.NewValue.Value;
                // 你可以在这里处理所选的颜色，例如更改背景色等。
            }
        }
        private void TranslateButton(int delta,object sender, RoutedEventArgs e)
        {
            isDrawingEnabled = false;
            Temp = 1 - Temp;
            if (Temp == 1)
            {
                (sender as Button).Content = "停止平移";
                isDragging = true;                          
            }
            else
            {
                (sender as Button).Content = "开始平移";
                isDragging = false;
            }
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                Color selectedColor = e.NewValue.Value;
                // 你可以在这里处理所选的颜色，例如更改背景色等。
            }
        }

        private void Item_Cursor_Selected(object sender, RoutedEventArgs e)
        {
            isDrawingEnabled = true;
            isDragging = false;
        }

        private void Item_Hand_Selected(object sender, RoutedEventArgs e)
        {
            InteractionVal = 0;
            isDragging = true;
            isDrawingEnabled = false;
           
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {

            //画布清屏
            MousedrawingCanvas.Children.Clear();
            layerslist.Clear(); // 清空layerslist
            layersTreeView.Items.Refresh(); // 刷新TreeView控件
            layer1.Clear();
            layer2.Clear();
            layer3.Clear();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            CLayer.layertest.scale *= 1/0.95;
            MousedrawingCanvas.Children.Clear();
            foreach (Polygon polygon in save_Polygon.m_multiPolygon)
                polygon.Draw(MousedrawingCanvas, true);
            foreach (Line line in save_Line.m_multiLine)
                line.Draw(MousedrawingCanvas);     
            foreach (Point point in save_Point.m_multiPoint)
                point.Draw(MousedrawingCanvas);
        }

        private void MapZoomController(object sender, MouseWheelEventArgs e)
        {
            // Get the mouse click position
            double winX = e.GetPosition(MousedrawingCanvas).X;
            double winY = e.GetPosition(MousedrawingCanvas).Y;
            currentPoint = new Point(winX, winY);
            if (e.Delta > 0&& isDragging)
            {
                CLayer.layertest.scale /= 0.9;
                CLayer.layertest.reverseChange(MousedrawingCanvas, ref winX, ref winY);
                CLayer.layertest.centerX += (CLayer.layertest.centerX - winX) / 10.0;
                CLayer.layertest.centerY += (CLayer.layertest.centerY - winY) / 10.0;
            }
            if (e.Delta < 0 && isDragging)
            {
                CLayer.layertest.scale *= 0.9;
                CLayer.layertest.reverseChange(MousedrawingCanvas, ref winX, ref winY);
                CLayer.layertest.centerX -= (CLayer.layertest.centerX - winX) / 9.0;
                CLayer.layertest.centerY -= (CLayer.layertest.centerY - winY) / 9.0;
            }
            MousedrawingCanvas.Children.Clear();
            string formattedText = $"1: {1 /(( 23.8 * 0.01) / (CLayer.layertest.scale * 1080 * 111 * 1000))}";
            ScaleBox.Text = formattedText;
            foreach (Line line in save_Line.m_multiLine)
                line.Draw(MousedrawingCanvas);
            foreach (Polygon polygon in save_Polygon.m_multiPolygon)
                polygon.Draw(MousedrawingCanvas, true);
            foreach (Point point in save_Point.m_multiPoint)
                point.Draw(MousedrawingCanvas);
        }

        private void ZoomInval(object sender, RoutedEventArgs e)
        {
            string formattedText = $"1: {1 / ((23.8 * 0.01) / (CLayer.layertest.scale * 1080 * 111 * 1000))}";
            ScaleBox.Text = formattedText;
            CLayer.layertest.scale *= 0.95;
            MousedrawingCanvas.Children.Clear();
            foreach (Polygon polygon in save_Polygon.m_multiPolygon)
                polygon.Draw(MousedrawingCanvas, true);
            foreach (Line line in save_Line.m_multiLine)
                line.Draw(MousedrawingCanvas);
            foreach (Point point in save_Point.m_multiPoint)
                point.Draw(MousedrawingCanvas);
        }

        private void ZoomOutval(object sender, RoutedEventArgs e)
        {
            string formattedText = $"1: {1 / ((23.8 * 0.01) / (CLayer.layertest.scale * 1080 * 111 * 1000))}";
            ScaleBox.Text = formattedText;
            CLayer.layertest.scale *= 1 / 0.95;
            MousedrawingCanvas.Children.Clear();
            foreach (Polygon polygon in save_Polygon.m_multiPolygon)
                polygon.Draw(MousedrawingCanvas, true);
            foreach (Line line in save_Line.m_multiLine)
                line.Draw(MousedrawingCanvas);
            foreach (Point point in save_Point.m_multiPoint)
                point.Draw(MousedrawingCanvas);
        }

        private void InteractionFuc(object sender, RoutedEventArgs e)
        {
            isDragging = false;
            isDrawingEnabled = false;
            if (InteractionVal != 3)
                InteractionVal = 3;
            else
                InteractionVal = 0;
        }

        private void PostGIS_Open(object sender, RoutedEventArgs e)
        {
            PostGIS_Login postGIS_Login = new PostGIS_Login();
            postGIS_Login.MouseDrawWindow = this;//设置父窗口引用
            postGIS_Login.Show();
        }


  
    }

}
