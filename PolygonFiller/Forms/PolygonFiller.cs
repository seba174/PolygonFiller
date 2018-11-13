using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PolygonFiller
{
    public partial class PolygonFiller : Form
    {
        private DrawingAreaInputHandler inputHandler;
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

            ColorsForPolygonFill.ConstantDisruptionVector = new Vector3(0, 0, 0);
            ColorsForPolygonFill.ConstantNormalVector = new Vector3(0, 0, 1);
            ColorsForPolygonFill.ConstantVectorToLight = new Vector3(0, 0, 1);

            colorsForPolygonFill = new ColorsForPolygonFill
            {
                ObjectColorOption = ObjectColorOption.Constant,
                DisruptionVectorOption = DisruptionVectorOption.None,
                NormalVectorOption = NormalVectorOption.Constant,
                VectorToLightOption = VectorToLightOption.Constant,
                ObjectColor = ILColorBox.BackColor,
                LightColor = new Vector3(1, 1, 1),
                VisibleAreaDimensions = drawingArea.Size
            };

            drawingArea.Paint += Draw;
            ResizeEnd += UpdateVisibleAreaSize;

            drawingArea.MouseDown += inputHandler.HandleMouseDown;
            drawingArea.MouseUp += inputHandler.HandleMouseUp;
            drawingArea.MouseMove += inputHandler.HandleMouseMove;

            LightColorButton.Click += ChangeLightColor;
            SetObjectColorButton.Click += ChangeObjectColor;
            SetObjectTextureButton.Click += ChangeObjectTexture;
            SetNormalMapButton.Click += ChangeNormalMap;
            SetHeightMapButton.Click += ChangeHeightMap;

            ObjectColorFromTexture.Click += SetObjectColorFromTexture;
            ObjectColorSingle.Click += SetConstantObjectColor;
            NormalVectorConstant.Click += SetNormalVectorConstant;
            NormalVectorFromTexture.Click += SetNormalVectorFromTexture;
            DisruptionVectorConstant.Click += SetDisruptionVectorConstant;
            DisruptionVectorFromTexture.Click += SetDisruptionVectorFromTexture;
        }

        private void SetDisruptionVectorFromTexture(object sender, EventArgs e)
        {
            colorsForPolygonFill.DisruptionVectorOption = DisruptionVectorOption.FromHeightMap;
            drawingArea.Refresh();
        }

        private void SetDisruptionVectorConstant(object sender, EventArgs e)
        {
            colorsForPolygonFill.DisruptionVectorOption = DisruptionVectorOption.None;
            drawingArea.Refresh();
        }

        private void SetNormalVectorFromTexture(object sender, EventArgs e)
        {
            colorsForPolygonFill.NormalVectorOption = NormalVectorOption.FromNormalMap;
            drawingArea.Refresh();
        }

        private void SetNormalVectorConstant(object sender, EventArgs e)
        {
            colorsForPolygonFill.NormalVectorOption = NormalVectorOption.Constant;
            drawingArea.Refresh();
        }

        private void SetConstantObjectColor(object sender, EventArgs e)
        {
            colorsForPolygonFill.ObjectColor = IoColorBox.BackColor;
            colorsForPolygonFill.ObjectColorOption = ObjectColorOption.Constant;
            drawingArea.Refresh();
        }

        private void SetObjectColorFromTexture(object sender, EventArgs e)
        {
            colorsForPolygonFill.ObjectColorOption = ObjectColorOption.FromTexture;
            drawingArea.Refresh();
        }

        private void UpdateVisibleAreaSize(object sender, EventArgs e)
        {
            colorsForPolygonFill.VisibleAreaDimensions = drawingArea.Size;
            drawingArea.Refresh();
        }

        private void ChangeHeightMap(object sender, EventArgs e)
        {
            if (TextureFileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap bitmap = new Bitmap(TextureFileDialog.FileName);
                DisruptionVectorTextureBox.BackgroundImage = Image.FromFile(TextureFileDialog.FileName);
                colorsForPolygonFill.SetHeightMap(bitmap);
                drawingArea.Refresh();
            }
        }

        private void ChangeNormalMap(object sender, EventArgs e)
        {
            if (TextureFileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap bitmap = new Bitmap(TextureFileDialog.FileName);
                NormalVectorTextureBox.BackgroundImage = bitmap;
                colorsForPolygonFill.SetNormalMap(bitmap);
                drawingArea.Refresh();
            }
        }

        private void ChangeObjectTexture(object sender, EventArgs e)
        {
            if (TextureFileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap oldImage = IoTextureBox.BackgroundImage as Bitmap;
                Bitmap bitmap = new Bitmap(TextureFileDialog.FileName);
                IoTextureBox.BackgroundImage = bitmap;
                colorsForPolygonFill.SetObjectTexture(bitmap);
                oldImage?.Dispose();
                drawingArea.Refresh();
            }
        }

        private void ChangeObjectColor(object sender, EventArgs e)
        {
            if (ColorDialog.ShowDialog() == DialogResult.OK)
            {
                IoColorBox.BackColor = ColorDialog.Color;
                colorsForPolygonFill.ObjectColor = ColorDialog.Color;
                drawingArea.Refresh();
            }
        }

        private void ChangeLightColor(object sender, EventArgs e)
        {
            if (ColorDialog.ShowDialog() == DialogResult.OK)
            {
                ILColorBox.BackColor = ColorDialog.Color;
                colorsForPolygonFill.LightColor = new Vector3(ColorDialog.Color.R / 255f, ColorDialog.Color.G / 255f, ColorDialog.Color.B / 255f);
                drawingArea.Refresh();
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

            for (int i = polygons.Count - 1; i >= 0; i--)
            {
                IPolygon polygon = polygons[i];
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
