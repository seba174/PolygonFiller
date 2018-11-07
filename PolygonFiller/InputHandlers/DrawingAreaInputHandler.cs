using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PolygonFiller
{
    public class DrawingAreaInputHandler : InputHandler
    {
        private bool isLeftMouseButtonClicked;
        private bool isMiddleMouseButtonClicked;
        private Point selectedElementLastPosition;
        private IEnumerable<IPolygon> handledPolygons;

        public Action OnElementSelection { get; set; }
        public Action OnElementUnselection { get; set; }
        public Action OnSuccessfullElementMove { get; set; }

        public IPolygon SelectedPolygon { get; private set; }
        public IClickable SelectedElement { get; private set; }

        public IEnumerable<IPolygon> HandledPolygons
        {
            get => handledPolygons;
            set
            {
                ClearSelected();
                handledPolygons = value;
            }
        }

        public void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (SelectedElement == null || SelectedPolygon == null)
                return;

            Point offsetFromLastMove = new Point(e.X - selectedElementLastPosition.X, e.Y - selectedElementLastPosition.Y);
            selectedElementLastPosition = e.Location;

            if (isLeftMouseButtonClicked)
            {
                if (!SelectedPolygon.HandleClickableMove(SelectedElement, offsetFromLastMove))
                {
                    if (SelectedPolygon.HandlePolygonMove(offsetFromLastMove))
                    {
                        OnSuccessfullElementMove?.Invoke();
                    }
                }
                else
                {
                    OnSuccessfullElementMove?.Invoke();
                }
            }
            else if (isMiddleMouseButtonClicked)
            {
                if (SelectedPolygon.HandlePolygonMove(offsetFromLastMove))
                {
                    OnSuccessfullElementMove?.Invoke();
                }
            }
        }

        public void HandleMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isLeftMouseButtonClicked = false;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                isMiddleMouseButtonClicked = false;
            }
        }

        public void HandleMouseDown(object sender, MouseEventArgs e)
        {
            if (isLeftMouseButtonClicked || isMiddleMouseButtonClicked)
                return;

            if (e.Button == MouseButtons.Left)
            {
                isLeftMouseButtonClicked = true;
                SetSelectedElements(e.Location);
            }
            else if (e.Button == MouseButtons.Middle)
            {
                isMiddleMouseButtonClicked = true;
                SetSelectedElements(e.Location);
            }
        }

        public void ClearSelected()
        {
            SelectedElement = null;
            SelectedPolygon = null;
            OnElementUnselection?.Invoke();
        }

        private void SetSelectedElements(Point mousePosition)
        {
            if (HandledPolygons == null)
                return;

            foreach (var polygon in HandledPolygons)
            {
                var selected = polygon.Clickables.Where(c => c.IsClicked(mousePosition)).FirstOrDefault();
                if (selected != null)
                {
                    SelectedPolygon = polygon;
                    SelectedElement = selected;
                    selectedElementLastPosition = mousePosition;
                    OnElementSelection?.Invoke();
                    return;
                }
            }

            ClearSelected();
        }
    }
}
