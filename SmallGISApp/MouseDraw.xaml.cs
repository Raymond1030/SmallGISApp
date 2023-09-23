using System;
using System.Collections.Generic;
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
using Xceed.Wpf.Toolkit;

namespace SmallGISApp
{
    /// <summary>
    /// MouseDraw.xaml 的交互逻辑
    /// </summary>
    public partial class MouseDraw : Window
    {
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
                ((Button)sender).Content = "Stop Drawing";
            }
            else
            {
                ((Button)sender).Content = "Start Drawing";
            }
        }

        private void MousedrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isDrawingEnabled)
                return; // Exit if drawing is not enabled

            //1. 鼠标画点
            if (rbPoint.IsChecked == true)
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
            else if (rbLine.IsChecked == true)
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
                    currentLine.color= (Color)DrawcolorPicker.SelectedColor;
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
            else if (rbPolygon.IsChecked == true)
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
                    currentPolygon.paintColor= (Color)PaintcolorPicker.SelectedColor;
                    // 画多边形线宽
                    currentPolygon.width= Size.Value;
                    currentPolygon.Draw(MousedrawingCanvas,false);
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
            //橡皮筋效果
            if ((rbPolygon.IsChecked==true||rbLine.IsChecked == true) && (isDrawingLine||isDrawingPolygon) && (currentLine != null||currentPolygon!=null))
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
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //更改画点的半径
            currentPoint.radius = Size.Value;
            //更改画线宽
            currentLine.width = Size.Value;
            currentPolygon.width = Size.Value;
        }
        //private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color> e)
        //{
        //    Color selectedColor = e.NewValue;
        //    // 你可以在这里处理所选的颜色，例如更改背景色等。

        //}
        private void PaintColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                Color selectedColor = e.NewValue.Value;
                // 你可以在这里处理所选的颜色，例如更改背景色等。
            }
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                Color selectedColor = e.NewValue.Value;
                // 你可以在这里处理所选的颜色，例如更改背景色等。
            }
        }

        private void rbPoint_Checked(object sender, RoutedEventArgs e)
        {
            SizeName.Text = "点大小";
            Size.Value = 5;
            Size.Minimum = 1;
            Size.Maximum = 50;

        }

        private void rbLine_Checked(object sender, RoutedEventArgs e)
        {
            SizeName.Text = "线宽";
            Size.Value = 1;
            Size.Minimum = 1;
            Size.Maximum = 10;
        }

        private void rbPolygon_Checked(object sender, RoutedEventArgs e)
        {
            SizeName.Text = "线宽";
            Size.Value = 1;
            Size.Minimum = 1;
            Size.Maximum = 10;
        }
    }
}
