using System;
using System.Collections.Generic;
using System.Data;
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
using Newtonsoft.Json.Linq;
using Npgsql;


namespace SmallGISApp
{
    /// <summary>
    /// PostGIS_Login.xaml 的交互逻辑
    /// </summary>
    public partial class PostGIS_Login : Window
    {

        public MouseDraw MouseDrawWindow { get; set; }

        public PostGIS_Login()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
           
        }

        private void PostGIS_Connect(object sender, RoutedEventArgs e)
        {
            //保存数据库名字
            string DatabaseName = Database.Text.ToString();
            string Table = TableName.Text.ToString();
            string connectionString = "HOST="+ Host.Text.ToString()+ ";PORT=5432;Username=" + Username.Text.ToString() +";PASSWORD="+PasswordBox.Password.ToString() + ";DATABASE="+Database.Text.ToString();
            string sql = @"SELECT json_build_object(
                        'type', 'FeatureCollection',
                        'features', json_agg(
                            json_build_object(
                                'type', 'Feature',
                                'geometry', ST_AsGeoJSON(geom)::json,
                                'properties', to_jsonb(row) - 'geom' 
                            )
                        )
                    )
                    FROM "+Table+" AS row;";
            NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();


            using (var conn = new NpgsqlConnection(connectionString))
            {

                try
                {

                    conn.Open();
             
                    var dt = new DataTable();

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        string geoJsonData = cmd.ExecuteScalar().ToString();

                        // Pass the data to the first window
                        MouseDrawWindow?.RaiseGeoJsonReceived(geoJsonData,Table);

                        // If you still want to display this in the current window:
                        QueryTest.Text = geoJsonData.Substring(0, 100) + "..."; // Displaying only first 100 chars as an example
                                                                                // Convert GeoJSON attributes to DataTable
                        // 将Geojson转化成属性表
                        var dataTable = ConvertGeoJsonToDataTable(geoJsonData);

                        // Showing the DataAttributeShow window and setting its data source
                        DataAttributeShow dataAttributeShow = new DataAttributeShow();
                        dataAttributeShow.dataGrid.ItemsSource = dataTable.DefaultView;
                        dataAttributeShow.Show();
                    }
                    conn.Close();
       
            
                }
                catch (Exception ex)
                {
                    QueryTest.Text += "Error: " + ex.Message + Environment.NewLine;
                }

            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
           this.Close();
        }

        private DataTable ConvertGeoJsonToDataTable(string geoJsonData)
        {
            var dataTable = new DataTable();

            // Parsing the GeoJSON string
            var jsonObject = Newtonsoft.Json.Linq.JObject.Parse(geoJsonData);

            // Assuming the GeoJSON structure contains a 'features' array
            var features = jsonObject["features"] as Newtonsoft.Json.Linq.JArray;

            if (features != null && features.Count > 0)
            {
                // Add rows for each feature
                foreach (var feature in features)
                {
                    var row = dataTable.NewRow();

                    foreach (var property in feature["properties"].Children())
                    {
                        // Extracting the property name without the full path
                        var propertyName = property.Path.Split('.').Last();

                        // Check if column exists, if not add it
                        if (!dataTable.Columns.Contains(propertyName))
                        {
                            dataTable.Columns.Add(propertyName);
                        }

                        row[propertyName] = property.First.ToString();
                    }

                    dataTable.Rows.Add(row);
                }
            }

            return dataTable;
        }

    }
}
