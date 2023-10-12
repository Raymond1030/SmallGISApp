using Npgsql;
using System;
using System.Collections.Generic;
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
using Npgsql;
using Color = System.Windows.Media.Color;
using Rectangle = System.Windows.Shapes.Rectangle;
using Brushes = System.Windows.Media.Brushes;

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

        //画线 面
        Point currentPoint = new Point();//记录目前最后一个点

        private bool isDrawingLine = false; // 是否处于绘制线条的模式
        Line currentLine = new Line();//记录每条线的轨迹

        private bool isDrawingPolygon = false; // 是否处于绘制面的模式
        Polygon currentPolygon = new Polygon();//记录每个面的点轨迹

        //橡皮筋 线
        Path temPath = new Path();//在鼠标划动过程 线的橡皮筋
        
        //图层 记录画好的点、线、面
        MultiPoint save_Point=new MultiPoint();
        MultiLine save_Line = new MultiLine();
        MultiPolygon save_Polygon = new MultiPolygon();


        public MouseDraw()
        {
            InitializeComponent();
            
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

        private void MousedrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
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
                //画点按钮
                Point m_p = new Point(winX, winY);
                //画点的半径 由拖动栏控制数值
                m_p.radius = Size.Value;
                //画点的颜色 由颜色选择器控制
                m_p.color = (Color)DrawcolorPicker.SelectedColor;
                m_p.Draw(MousedrawingCanvas);
                //图层记录点
                save_Point.m_multiPoint.Add(m_p);
            }
            //2. 鼠标画线
            else if (Item_Line.IsSelected == true && isDrawingEnabled == true)
            {
                // Get the mouse click position
                double winX = e.GetPosition(MousedrawingCanvas).X;
                double winY = e.GetPosition(MousedrawingCanvas).Y;
                currentPoint = new Point(winX, winY);
                if (e.LeftButton == MouseButtonState.Pressed && !isDrawingLine)
                {

                    currentLine.m_Line.Add(currentPoint);
                    isDrawingLine = true;

                }
                else if (e.LeftButton == MouseButtonState.Pressed && isDrawingLine)
                {
                    // 更新线条的结束位置
                    currentPoint.x = e.GetPosition(MousedrawingCanvas).X;
                    currentPoint.y = e.GetPosition(MousedrawingCanvas).Y;
                    // 添加到线中
                    currentLine.m_Line.Add(currentPoint);
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
                currentPoint = new Point(winX, winY);
                if (e.LeftButton == MouseButtonState.Pressed && !isDrawingPolygon)
                {

                    currentPolygon.m_polygon.Add(currentPoint);
                    isDrawingPolygon = true;

                }
                else if (e.LeftButton == MouseButtonState.Pressed && isDrawingPolygon)
                {
                    // 更新线条的结束位置
                    currentPoint.x = e.GetPosition(MousedrawingCanvas).X;
                    currentPoint.y = e.GetPosition(MousedrawingCanvas).Y;
                    // 添加到线中
                    currentPolygon.m_polygon.Add(currentPoint);
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
            // 获取鼠标的当前位置
            double newX = e.GetPosition(MousedrawingCanvas).X;
            double newY = e.GetPosition(MousedrawingCanvas).Y;

            // 更新TextBox的文本为鼠标的实时位置
            string formattedText = $"X: {newX:F4}, Y: {newY:F4}";
            MouseTextBox.Text = formattedText;

            //橡皮筋效果
            if ((Item_Polygon.IsSelected == true || Item_Line.IsSelected == true) && (isDrawingLine || isDrawingPolygon) && (currentLine != null || currentPolygon != null))
            {
                double winX = e.GetPosition(MousedrawingCanvas).X;
                double winY = e.GetPosition(MousedrawingCanvas).Y;
                LineGeometry myLineGeometry = new LineGeometry();
                myLineGeometry.StartPoint = new System.Windows.Point(currentPoint.x, currentPoint.y);
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
                Vector delta = new Vector();
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
        private void Translate(Vector translation)
        {
            MousedrawingCanvas.Children.Clear();
            foreach (Line line in save_Line.m_multiLine)
            {
                line.MaxY = -9999;
                line.MaxX = -9999;
                line.MinY = 9999;
                line.MinX = 9999;
                for (int i = 0; i < line.m_Line.Count; i++)
                {
                    line.m_Line[i].x += translation.X;
                    line.m_Line[i].y += translation.Y;
                }
                line.Draw(MousedrawingCanvas);

            }

            foreach (Polygon polygon in save_Polygon.m_multiPolygon)
            {
                polygon.MaxY = -9999;
                polygon.MaxX = -9999;
                polygon.MinY = 9999;
                polygon.MinX = 9999;
                for (int i = 0; i < polygon.m_polygon.Count; i++)
                {
                    polygon.m_polygon[i].x += translation.X;
                    polygon.m_polygon[i].y += translation.Y;
                }
                polygon.Draw(MousedrawingCanvas, true);
            }

            foreach (Point point in save_Point.m_multiPoint)
            {
                point.x += translation.X;
                point.y += translation.Y;
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
            }
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
                rectangle.Stroke = Brushes.Red;
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
        private void TranslateButton(object sender, RoutedEventArgs e)
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
            isDragging = true;
            isDrawingEnabled = false;
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {

            //画布清屏
            MousedrawingCanvas.Children.Clear();


        }

    
    }
}
