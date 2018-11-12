using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PolygonFiller
{
    public class DrawingAreaInputHandler
    {
        private bool isLeftMouseButtonClicked;
        private bool isMiddleMouseButtonClicked;
        private Point selectedElementLastPosition;
        private IEnumerable<IPolygon> handledPolygons;

        public Action OnElementSelection { get; set; }
        public Action OnElementUnselection { get; set; }
        public Action OnSuccessfullElementMove { get; set; }

        public Rectangle ClickArea { get; set; }

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

            Point location = GetPointInsideClickArea(e.Location);

            Point offsetFromLastMove = new Point(location.X - selectedElementLastPosition.X, location.Y - selectedElementLastPosition.Y);
            selectedElementLastPosition = location;

            if (isLeftMouseButtonClicked)
            {
                if (!SelectedPolygon.IsClickableMovingPermitted(SelectedElement, offsetFromLastMove, ClickArea))
                    return;
                if (SelectedPolygon.HandleClickableMove(SelectedElement, offsetFromLastMove))
                {
                    OnSuccessfullElementMove?.Invoke();
                }
            }
            else if (isMiddleMouseButtonClicked)
            {
                if (!SelectedPolygon.IsPolygonMovingPermitted(offsetFromLastMove, ClickArea))
                    return;
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

        private Point GetPointInsideClickArea(Point original)
        {
            if (original.X < ClickArea.Left)
                original = new Point(ClickArea.Left, original.Y);
            else if (original.X > ClickArea.Right)
                original = new Point(ClickArea.Right, original.Y);

            if (original.Y < ClickArea.Top)
                original = new Point(original.X, ClickArea.Top);
            else if (original.Y > ClickArea.Bottom)
                original = new Point(original.X, ClickArea.Bottom);

            return original;
        }
    }
}
