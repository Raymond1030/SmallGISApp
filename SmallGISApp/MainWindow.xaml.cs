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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmallGISApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MouseDraw mDraw = new MouseDraw();
            mDraw.Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //画点按钮
            //获取控件的X、Y值
            double Draw_point_x = Convert.ToDouble(Point_Text_X.Text);
            double Draw_point_y = Convert.ToDouble(Point_Text_Y.Text);
            //画点
            Point P = new Point(Draw_point_x, Draw_point_y);
            P.Draw(drawingCanvas);
        }

    }
}
