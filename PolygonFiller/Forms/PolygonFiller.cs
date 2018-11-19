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
        private int radius;
        private int rgbHeight;
        private int rgbCosinePower;

        private FormWindowState lastState;
        private DrawingAreaInputHandler inputHandler;
        private PolygonDrawer standardPolygonDrawer;
        private PolygonDrawer selectedElementDrawer;
        private List<IPolygon> polygons;
        private ColorsForPolygonFill colorsForPolygonFill;
        private ColorsForPolygonFill colorsForPolygonFill2;
        private LightSourceGenerator lightSourceGenerator;
        private RBGHeadlights rgbHeadlights;
        private RBGHeadlights rgbHeadlights2;

        public PolygonFiller()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            lastState = WindowState;

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
                EdgeThickness = 2,
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

            rgbHeadlights = new RBGHeadlights
            {
                CosinePower = 1000,
                HeadlightsHeight = 100
            };

            rgbHeadlights2 = new RBGHeadlights
            {
                CosinePower = rgbHeadlights.CosinePower,
                HeadlightsHeight = rgbHeadlights.HeadlightsHeight
            };

            colorsForPolygonFill = new ColorsForPolygonFill
            {
                ObjectColorOption = ObjectColorOption.Constant,
                DisruptionVectorOption = DisruptionVectorOption.None,
                NormalVectorOption = NormalVectorOption.Constant,
                VectorToLightOption = VectorToLightOption.Constant,
                ObjectColor = ILColorBox.BackColor,
                LightColor = new Vector3(1, 1, 1),
                DrawingAreaSize = drawingArea.Size,
                RBGHeadlights = rgbHeadlights
            };

            colorsForPolygonFill2 = new ColorsForPolygonFill
            {
                ObjectColorOption = colorsForPolygonFill.ObjectColorOption,
                DisruptionVectorOption = colorsForPolygonFill.DisruptionVectorOption,
                NormalVectorOption = colorsForPolygonFill.NormalVectorOption,
                VectorToLightOption = colorsForPolygonFill.VectorToLightOption,
                ObjectColor = colorsForPolygonFill.ObjectColor,
                LightColor = colorsForPolygonFill.LightColor,
                DrawingAreaSize = colorsForPolygonFill.DrawingAreaSize,
                RBGHeadlights = rgbHeadlights2
            };

            DisruptionVectorTextureBox.BackgroundImage = Images.brick_heightmap;
            colorsForPolygonFill.SetHeightMap(Images.brick_heightmap);
            colorsForPolygonFill2.SetHeightMap(Images.brick_heightmap);

            NormalVectorTextureBox.BackgroundImage = Images.brick_normalmap;
            colorsForPolygonFill.SetNormalMap(Images.brick_normalmap);
            colorsForPolygonFill2.SetNormalMap(Images.brick_normalmap);

            IoTextureBox.BackgroundImage = Images.sampleTexture;
            colorsForPolygonFill.SetObjectTexture(Images.sampleTexture);

            IoTextureBox2.BackgroundImage = Images.sampleTexture;
            colorsForPolygonFill2.SetObjectTexture(Images.sampleTexture);

            UpdatePolygonsCache();

            lightSourceGenerator = new LightSourceGenerator
            {
                AngleChange = 10,
                HeightStepChange = 10,
                MinHeight = 20,
                MaxHeight = 200,
                Radius = 300,
                Origin = new Vector2(drawingArea.Width / 2, drawingArea.Height / 2),
                StartingHeight = 100
            };

            radius = lightSourceGenerator.Radius;
            rgbCosinePower = rgbHeadlights.CosinePower;
            rgbHeight = rgbHeadlights.HeadlightsHeight;

            ResizeEnd += FormSizeChangedHandler;
            Resize += HandleWindowMaximization;

            drawingArea.Paint += Draw;

            drawingArea.MouseDown += inputHandler.HandleMouseDown;
            drawingArea.MouseUp += inputHandler.HandleMouseUp;
            drawingArea.MouseMove += inputHandler.HandleMouseMove;

            LightColorButton.Click += ChangeLightColor;
            SetObjectColorButton.Click += ChangeObjectColor;
            SetObjectColorButton2.Click += ChangeObjectColor2;
            SetObjectTextureButton.Click += ChangeObjectTexture;
            SetObjectTextureButton2.Click += ChangeObjectTexture2;
            SetNormalMapButton.Click += ChangeNormalMap;
            SetHeightMapButton.Click += ChangeHeightMap;

            ObjectColorFromTexture.Click += SetObjectColorFromTexture;
            ObjectColorFromTexture2.Click += SetObjectColorFromTexture2;
            ObjectColorSingle.Click += SetConstantObjectColor;
            ObjectColorSingle2.Click += SetConstatntObjectColor2;
            NormalVectorConstant.Click += SetNormalVectorConstant;
            NormalVectorFromTexture.Click += SetNormalVectorFromTexture;
            DisruptionVectorConstant.Click += SetDisruptionVectorConstant;
            DisruptionVectorFromTexture.Click += SetDisruptionVectorFromTexture;
            VectorToLightConstant.Click += SetVectorToLightConstant;
            VectorToLightAnimated.CheckedChanged += SetVectorToLightAnimated;
            TurnRgbHighlightsOn.Click += TurnRgbHighlightsOnHandler;
            TurnRgbHighlightsOff.Click += TurnRgbHighlightsOffHandler;

            RadiusTextBox.Text = lightSourceGenerator.Radius.ToString();
            RadiusTextBox.TextChanged += RadiusTextboxTextChangedHandler;
            RadiusButtonAccept.Click += SetNewRadius;
            CosinePowerTextBox.Text = rgbHeadlights.CosinePower.ToString();
            CosinePowerTextBox.TextChanged += CosineTextboxTextChangedHandler;
            HighlightsHeightTextBox.Text = rgbHeadlights.HeadlightsHeight.ToString();
            HighlightsHeightTextBox.TextChanged += HighlightsHeightTextboxTextChangedHandler;
            RgbHeadlightsParameterAccept.Click += SetNewRgbHeadlightsParameters;
        }

        private void HandleWindowMaximization(object sender, EventArgs e)
        {
            if (lastState != WindowState && (WindowState == FormWindowState.Maximized || WindowState == FormWindowState.Normal))
            {
                lastState = WindowState;
                FormSizeChangedHandler(sender, e);
            }
        }

        private void FormSizeChangedHandler(object sender, EventArgs e)
        {
            lightSourceGenerator.Origin = new Vector2(drawingArea.Width / 2, drawingArea.Height / 2);
            colorsForPolygonFill.DrawingAreaSize = drawingArea.Size;
            colorsForPolygonFill2.DrawingAreaSize = drawingArea.Size;
            if (colorsForPolygonFill.RGBHeadlightsEnabled)
            {
                UpdateRgbHeadlights();
            }

            UpdatePolygonsCache();
            drawingArea.Refresh();
        }

        private void TurnRgbHighlightsOffHandler(object sender, EventArgs e)
        {
            colorsForPolygonFill.DisableRgbHeadlights();
            colorsForPolygonFill2.DisableRgbHeadlights();
            UpdatePolygonsCache();
            drawingArea.Refresh();
        }

        private void TurnRgbHighlightsOnHandler(object sender, EventArgs e)
        {
            if (colorsForPolygonFill.RGBHeadlightsEnabled || colorsForPolygonFill2.RGBHeadlightsEnabled)
            {
                return;
            }

            UpdateRgbHeadlights();
            UpdatePolygonsCache();
            drawingArea.Refresh();
        }

        private void HighlightsHeightTextboxTextChangedHandler(object sender, EventArgs e)
        {
            if (uint.TryParse(HighlightsHeightTextBox.Text, out uint result) && result < int.MaxValue)
            {
                rgbHeight = (int)result;
            }
            else
            {
                HighlightsHeightTextBox.Text = rgbHeight.ToString();
            }
        }

        private void CosineTextboxTextChangedHandler(object sender, EventArgs e)
        {
            if (uint.TryParse(CosinePowerTextBox.Text, out uint result) && result < int.MaxValue)
            {
                rgbCosinePower = (int)result;
            }
            else
            {
                CosinePowerTextBox.Text = rgbCosinePower.ToString();
            }
        }

        private void SetNewRgbHeadlightsParameters(object sender, EventArgs e)
        {
            if (rgbHeadlights.HeadlightsHeight != rgbHeight || rgbHeadlights.CosinePower != rgbCosinePower)
            {
                rgbHeadlights.HeadlightsHeight = rgbHeight;
                rgbHeadlights.CosinePower = rgbCosinePower;
                rgbHeadlights2.HeadlightsHeight = rgbHeight;
                rgbHeadlights2.CosinePower = rgbCosinePower;
                if (colorsForPolygonFill.RGBHeadlightsEnabled)
                {
                    UpdateRgbHeadlights();
                    UpdatePolygonsCache();
                    drawingArea.Refresh();
                }
            }
        }

        private void RadiusTextboxTextChangedHandler(object sender, EventArgs e)
        {
            if (uint.TryParse(RadiusTextBox.Text, out uint result) && result < int.MaxValue)
            {
                radius = (int)result;
            }
            else
            {
                RadiusTextBox.Text = radius.ToString();
            }
        }

        private void SetNewRadius(object sender, EventArgs e)
        {
            if (lightSourceGenerator.Radius != radius)
            {
                lightSourceGenerator.Radius = radius;
            }
        }

        private async void SetVectorToLightAnimated(object sender, EventArgs e)
        {
            if (!VectorToLightAnimated.Checked)
            {
                return;
            }

            lightSourceGenerator.Reset();
            colorsForPolygonFill.VectorToLightOption = VectorToLightOption.Variable;
            colorsForPolygonFill2.VectorToLightOption = VectorToLightOption.Variable;

            while (true)
            {
                colorsForPolygonFill.LightSourcePosition = lightSourceGenerator.GetNextLightSourcePosition();
                colorsForPolygonFill2.LightSourcePosition = colorsForPolygonFill.LightSourcePosition;
                UpdatePolygonsCache();
                drawingArea.Refresh();

                await Task.Delay(700);

                if (colorsForPolygonFill.VectorToLightOption == VectorToLightOption.Constant)
                {
                    return;
                }
            }
        }

        private void SetVectorToLightConstant(object sender, EventArgs e)
        {
            if (colorsForPolygonFill.VectorToLightOption != VectorToLightOption.Constant)
            {
                colorsForPolygonFill.VectorToLightOption = VectorToLightOption.Constant;
                colorsForPolygonFill2.VectorToLightOption = VectorToLightOption.Constant;
                UpdatePolygonsCache();
                drawingArea.Refresh();
            }
        }

        private void SetDisruptionVectorFromTexture(object sender, EventArgs e)
        {
            if (colorsForPolygonFill.DisruptionVectorOption != DisruptionVectorOption.FromHeightMap)
            {
                colorsForPolygonFill.DisruptionVectorOption = DisruptionVectorOption.FromHeightMap;
                colorsForPolygonFill2.DisruptionVectorOption = DisruptionVectorOption.FromHeightMap;
                UpdatePolygonsCache();
                drawingArea.Refresh();
            }
        }

        private void SetDisruptionVectorConstant(object sender, EventArgs e)
        {
            if (colorsForPolygonFill.DisruptionVectorOption != DisruptionVectorOption.None)
            {
                colorsForPolygonFill.DisruptionVectorOption = DisruptionVectorOption.None;
                colorsForPolygonFill2.DisruptionVectorOption = DisruptionVectorOption.None;
                UpdatePolygonsCache();
                drawingArea.Refresh();
            }
        }

        private void SetNormalVectorFromTexture(object sender, EventArgs e)
        {
            if (colorsForPolygonFill.NormalVectorOption != NormalVectorOption.FromNormalMap)
            {
                colorsForPolygonFill.NormalVectorOption = NormalVectorOption.FromNormalMap;
                colorsForPolygonFill2.NormalVectorOption = NormalVectorOption.FromNormalMap;
                UpdatePolygonsCache();
                drawingArea.Refresh();
            }
        }

        private void SetNormalVectorConstant(object sender, EventArgs e)
        {
            if (colorsForPolygonFill.NormalVectorOption != NormalVectorOption.Constant)
            {
                colorsForPolygonFill.NormalVectorOption = NormalVectorOption.Constant;
                colorsForPolygonFill2.NormalVectorOption = NormalVectorOption.Constant;
                UpdatePolygonsCache();
                drawingArea.Refresh();
            }
        }

        private void SetConstantObjectColor(object sender, EventArgs e)
        {
            SetConstantObjectColor(colorsForPolygonFill);
        }

        private void SetConstatntObjectColor2(object sender, EventArgs e)
        {
            SetConstantObjectColor(colorsForPolygonFill2);
        }

        private void SetConstantObjectColor(ColorsForPolygonFill colForPolyFill)
        {
            if (colForPolyFill.ObjectColorOption != ObjectColorOption.Constant)
            {
                colForPolyFill.ObjectColor = IoColorBox.BackColor;
                colForPolyFill.ObjectColorOption = ObjectColorOption.Constant;
                colForPolyFill.UpdateCache();
                drawingArea.Refresh();
            }
        }

        private void SetObjectColorFromTexture(object sender, EventArgs e)
        {
            SetObjectColorFromTexture(colorsForPolygonFill);
        }

        private void SetObjectColorFromTexture2(object sender, EventArgs e)
        {
            SetObjectColorFromTexture(colorsForPolygonFill2);
        }

        private void SetObjectColorFromTexture(ColorsForPolygonFill colForPolyFill)
        {
            if (colForPolyFill.ObjectColorOption != ObjectColorOption.FromTexture)
            {
                colForPolyFill.ObjectColorOption = ObjectColorOption.FromTexture;
                colForPolyFill.UpdateCache();
                drawingArea.Refresh();
            }
        }

        private void ChangeHeightMap(object sender, EventArgs e)
        {
            if (TextureFileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap oldImage = DisruptionVectorTextureBox.BackgroundImage as Bitmap;
                Bitmap bitmap = new Bitmap(TextureFileDialog.FileName);
                DisruptionVectorTextureBox.BackgroundImage = Image.FromFile(TextureFileDialog.FileName);
                colorsForPolygonFill.SetHeightMap(bitmap);
                colorsForPolygonFill2.SetHeightMap(bitmap);
                oldImage?.Dispose();
                drawingArea.Refresh();
            }
        }

        private void ChangeNormalMap(object sender, EventArgs e)
        {
            if (TextureFileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap oldImage = NormalVectorTextureBox.BackgroundImage as Bitmap;
                Bitmap bitmap = new Bitmap(TextureFileDialog.FileName);
                NormalVectorTextureBox.BackgroundImage = bitmap;
                colorsForPolygonFill.SetNormalMap(bitmap);
                colorsForPolygonFill2.SetNormalMap(bitmap);
                oldImage?.Dispose();
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

        private void ChangeObjectTexture2(object sender, EventArgs e)
        {
            if (TextureFileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap oldImage = IoTextureBox2.BackgroundImage as Bitmap;
                Bitmap bitmap = new Bitmap(TextureFileDialog.FileName);
                IoTextureBox2.BackgroundImage = bitmap;
                colorsForPolygonFill2.SetObjectTexture(bitmap);
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
                if (colorsForPolygonFill.ObjectColorOption == ObjectColorOption.Constant)
                {
                    colorsForPolygonFill.UpdateCache();
                }

                drawingArea.Refresh();
            }
        }

        private void ChangeObjectColor2(object sender, EventArgs e)
        {
            if (ColorDialog.ShowDialog() == DialogResult.OK)
            {
                IoColorBox2.BackColor = ColorDialog.Color;
                colorsForPolygonFill2.ObjectColor = ColorDialog.Color;
                if (colorsForPolygonFill2.ObjectColorOption == ObjectColorOption.Constant)
                {
                    colorsForPolygonFill2.UpdateCache();
                }

                drawingArea.Refresh();
            }
        }

        private void ChangeLightColor(object sender, EventArgs e)
        {
            if (ColorDialog.ShowDialog() == DialogResult.OK)
            {
                ILColorBox.BackColor = ColorDialog.Color;
                colorsForPolygonFill.LightColor = new Vector3(ColorDialog.Color.R / 255f, ColorDialog.Color.G / 255f, ColorDialog.Color.B / 255f);
                colorsForPolygonFill2.LightColor = colorsForPolygonFill.LightColor;
                UpdatePolygonsCache();
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
                if (polygons[0] == polygon)
                {
                    directBitmaps.TryAdd(polygon, PolygonFill.GetPolygonFillDirectBitmap(polygon, colorsForPolygonFill));
                }
                else
                {
                    directBitmaps.TryAdd(polygon, PolygonFill.GetPolygonFillDirectBitmap(polygon, colorsForPolygonFill2));
                }
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

        private void UpdatePolygonsCache()
        {
            var t1 = Task.Run(() => colorsForPolygonFill.UpdateCache());
            var t2 = Task.Run(() => colorsForPolygonFill2.UpdateCache());
            t1.Wait();
            t2.Wait();
        }

        private void UpdateRgbHeadlights()
        {
            var t1 = Task.Run(() => colorsForPolygonFill.EnableRgbHeadlights());
            var t2 = Task.Run(() => colorsForPolygonFill2.EnableRgbHeadlights());
            t1.Wait();
            t2.Wait();
        }
    }
}
