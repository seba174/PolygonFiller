using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PolygonFiller
{
    public partial class PolygonFiller : Form
    {
        private InputHandler inputHandler;
        private PolygonDrawer standardPolygonDrawer;
        private PolygonDrawer selectedElementDrawer;
        private List<IPolygon> polygons;
        private ColorsForPolygonFill colorsForPolygonFill;

        public PolygonFiller()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;

            polygons = new List<IPolygon> { PolygonCreator.GetTriangle(), PolygonCreator.GetTriangle() };
            polygons[1].HandlePolygonMove(new Point(350, 0));

            Vertice.ClickRadius = 10;
            Edge.ClickDistance = 5;

            inputHandler = new DrawingAreaInputHandler
            {
                HandledPolygons = polygons,
                OnSuccessfullElementMove = drawingArea.Refresh,
                OnElementSelection = OnElementSelection,
                OnElementUnselection = OnElementUnselection
            };

            standardPolygonDrawer = new PolygonDrawer
            {
                EdgeColor = Color.Green,
                EdgeThickness = 3,
                VerticeBorderColor = Color.LightGray,
                VerticeInsideColor = Color.FromArgb(28, 28, 28),
                VerticeBorderThickness = 1,
                VerticeRadius = 10,
            };

            selectedElementDrawer = new PolygonDrawer
            {
                EdgeColor = Color.Red,
                EdgeThickness = standardPolygonDrawer.EdgeThickness,
                VerticeBorderColor = Color.Red,
                VerticeInsideColor = standardPolygonDrawer.VerticeInsideColor,
                VerticeBorderThickness = standardPolygonDrawer.VerticeBorderThickness,
                VerticeRadius = standardPolygonDrawer.VerticeRadius,
            };

            colorsForPolygonFill = new ColorsForPolygonFill
            {
                ObjectColorOption = ObjectColorOption.Constant,
                ObjectColor = ILColorBox.BackColor
            };

            drawingArea.Paint += Draw;

            drawingArea.MouseDown += inputHandler.HandleMouseDown;
            drawingArea.MouseUp += inputHandler.HandleMouseUp;
            drawingArea.MouseMove += inputHandler.HandleMouseMove;

            LightColorButton.Click += ChangeLightColor;
            ObjectColorButton.Click += ChangeObjectColor;
            ObjectTextureButton.Click += ChangeObjectTexture;
            NormalVectorTextureButton.Click += ChangeNormalMap;
            DisruptionVectorTextureButton.Click += ChangeHeightMap;
        }

        private void ChangeHeightMap(object sender, EventArgs e)
        {
            if (TextureFileDialog.ShowDialog() == DialogResult.OK)
            {
                DisruptionVectorTextureBox.BackgroundImage = Image.FromFile(TextureFileDialog.FileName);
            }
        }

        private void ChangeNormalMap(object sender, EventArgs e)
        {
            if (TextureFileDialog.ShowDialog() == DialogResult.OK)
            {
                NormalVectorTextureBox.BackgroundImage = Image.FromFile(TextureFileDialog.FileName);
            }
        }

        private void ChangeObjectTexture(object sender, EventArgs e)
        {
            if (TextureFileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap bitmap = new Bitmap(TextureFileDialog.FileName);
                IoTextureBox.BackgroundImage = bitmap;
                colorsForPolygonFill.ChangeObjectTexture(bitmap);
                colorsForPolygonFill.ObjectColorOption = ObjectColorOption.FromTexture;
                drawingArea.Refresh();
            }
        }

        private void ChangeObjectColor(object sender, EventArgs e)
        {
            if (ColorDialog.ShowDialog() == DialogResult.OK)
            {
                IoColorBox.BackColor = ColorDialog.Color;
                colorsForPolygonFill.ObjectColor = ColorDialog.Color;
                colorsForPolygonFill.ObjectColorOption = ObjectColorOption.Constant;
                drawingArea.Refresh();
            }
        }

        private void ChangeLightColor(object sender, EventArgs e)
        {
            if (ColorDialog.ShowDialog() == DialogResult.OK)
            {
                ILColorBox.BackColor = ColorDialog.Color;
            }
        }

        private void OnElementUnselection()
        {
            drawingArea.Refresh();
        }

        private void OnElementSelection()
        {
            drawingArea.Refresh();
        }

        private void Draw(object sender, PaintEventArgs e)
        {
            ConcurrentDictionary<IPolygon, DirectBitmap> directBitmaps = new ConcurrentDictionary<IPolygon, DirectBitmap>(Environment.ProcessorCount, polygons.Count);
            Parallel.ForEach(polygons, polygon =>
            {
                directBitmaps.TryAdd(polygon, PolygonFill.GetPolygonFillDirectBitmap(polygon, colorsForPolygonFill));
            });

            foreach (var polygon in polygons)
            {
                standardPolygonDrawer.FillPolygon(e.Graphics, polygon, directBitmaps[polygon]);
                directBitmaps[polygon].Dispose();

                if (inputHandler.SelectedElement is Edge selectedEdge && inputHandler.SelectedPolygon == polygon)
                {
                    Polygon polygonWithOnlySelectedEdge = new Polygon(new Vertice[0], new Edge[] { selectedEdge });
                    selectedElementDrawer.DrawPolygon(e.Graphics, polygonWithOnlySelectedEdge);
                }

                IEnumerable<Vertice> vertices = polygon.Vertices.Where(v => v != inputHandler.SelectedElement);
                IEnumerable<Edge> edges = polygon.Edges.Where(edge => edge != inputHandler.SelectedElement);
                Polygon polygonWithoutSelectedElement = new Polygon(vertices, edges);

                standardPolygonDrawer.DrawPolygon(e.Graphics, polygonWithoutSelectedElement);
            }

            if (inputHandler.SelectedElement is Vertice selectedVertice)
            {
                Polygon polygonWithOnlySelectedVertice = new Polygon(new Vertice[] { selectedVertice }, new Edge[0]);
                selectedElementDrawer.DrawPolygon(e.Graphics, polygonWithOnlySelectedVertice);
            }
        }
    }
}
