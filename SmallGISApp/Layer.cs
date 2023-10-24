using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallGISApp
{
            internal class Layer
            {
                public string Name { get; set; }
                public List<Point> Points { get; set; }
                public List<Line> Lines { get; set; }
                public List<Polygon> Polygons { get; set; }

                public bool is_visible { get; set; }

                public Layer(string name)
                {
                    Name = name;
                    is_visible = true;
                    Points = new List<Point>();
                    Lines = new List<Line>();
                    Polygons = new List<Polygon>();
                }

                public void AddPoint(Point point)
                {
                    Points.Add(point);
                }

                public void AddLine(Line line)
                {
                    Lines.Add(line);
                }

                public void AddPolygon(Polygon polygon)
                {
                    Polygons.Add(polygon);
                }
                public void Clear()
                {
                    Points.Clear();
                    Lines.Clear();
                    Polygons.Clear();
                }

    }
        }


