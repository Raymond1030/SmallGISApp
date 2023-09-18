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

namespace SmallGISApp
{
    /// <summary>
    /// MouseDraw.xaml 的交互逻辑
    /// </summary>
    public partial class MouseDraw : Window
    {
        bool isDrawingEnabled = false;//是否开始画图

        //画线
        private bool isDrawingLine = false; // 是否处于绘制线条的模式
        Point? currentpoint = null;//记录目前最后一个点
        Line currentline = new Line();//记录每条线的轨迹
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
            if (!isDrawingEnabled)
                return; // Exit if drawing is not enabled

            //1. 鼠标画点
            if (rbPoint.IsChecked==true)         
            {   
                // Get the mouse click position
                double winX = e.GetPosition(MousedrawingCanvas).X;
                double winY = e.GetPosition(MousedrawingCanvas).Y;
                //画点按钮
                Point m_p = new Point(winX, winY);
                m_p.DrawPoint(MousedrawingCanvas, 5);
                //图层记录点
                save_Point.m_multiPoint.Add(m_p);
            }
            //2. 鼠标画线
            else if (rbLine.IsChecked==true)
            {
                // Get the mouse click position
                double winX = e.GetPosition(MousedrawingCanvas).X;
                double winY = e.GetPosition(MousedrawingCanvas).Y;
                currentpoint = new Point(winX, winY);
                if (e.LeftButton == MouseButtonState.Pressed && !isDrawingLine)
                {
                    
                    currentline.m_Line.Add(currentpoint);
                    isDrawingLine = true;

                }
                else if (e.LeftButton == MouseButtonState.Pressed && isDrawingLine)
                {
                    // 更新线条的结束位置
                    currentpoint.x = e.GetPosition(MousedrawingCanvas).X;
                    currentpoint.y = e.GetPosition(MousedrawingCanvas).Y;
                    // 添加到线中
                    currentline.m_Line.Add(currentpoint);
                    currentline.DrawLine(MousedrawingCanvas);
                }
                else if (e.RightButton == MouseButtonState.Pressed && isDrawingLine)
                {
                    // 右击鼠标结束绘制线条
                    currentline = new Line();
                    isDrawingLine = false;
                    if (temPath != null)
                    {
                        MousedrawingCanvas.Children.Remove(temPath);
                    }    
                    save_Line.m_multiLine.Add(currentline);
                 
                }

            }
            //3. 鼠标画面
            else if (rbPolygon.IsChecked == true)
            {

            }

        }
        private void MousedrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (rbLine.IsChecked == true && isDrawingLine && currentline != null)
            {
                double winX = e.GetPosition(MousedrawingCanvas).X;
                double winY = e.GetPosition(MousedrawingCanvas).Y;
                LineGeometry myLineGeometry = new LineGeometry();
                myLineGeometry.StartPoint = new System.Windows.Point(currentpoint.x, currentpoint.y);
                myLineGeometry.EndPoint = new System.Windows.Point(winX, winY);

                if (!MousedrawingCanvas.Children.Contains(temPath))
                {
                    temPath.Stroke = Brushes.Black;
                    temPath.StrokeThickness = 1;
                    MousedrawingCanvas.Children.Add(temPath);
                }

                temPath.Data = myLineGeometry;
            }
        }


    }
}
