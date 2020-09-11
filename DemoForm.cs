using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Windows.Forms;

namespace BendingDemo
{
    public partial class DemoForm : Form
    {
        private const float Radius = 100f;
        private const float PointSize = 6f;
        private const float FontEmSize = 12f;
        
        private static readonly Pen PointBrush;
        private static readonly Pen OverBrush;
        private static readonly Pen DragBrush;
        private static readonly Pen LineBrush;
        private static readonly Pen TempBrush;
        private static readonly Brush LabelBrush;
        private static readonly Font LabelFont;
        private static readonly Vector2 LabelOffset;
        
        static DemoForm()
        {
            PointBrush = new Pen(Color.DeepSkyBlue, PointSize);
            OverBrush = new Pen(Color.DodgerBlue, PointSize);
            DragBrush = new Pen(Color.MidnightBlue, PointSize);
            LineBrush = new Pen(Color.DeepPink, 1f);
            TempBrush = new Pen(Color.LightGray, 1f);
            LabelBrush = new SolidBrush(Color.SlateBlue);
            LabelFont = new Font(FontFamily.GenericSansSerif, FontEmSize);
            LabelOffset = new Vector2(10f, 30f);
        }
        
        private readonly Dictionary<char, Vector2> _points;
        private char? _over;
        private char? _drag;
        
        public DemoForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            
            _points = new Dictionary<char, Vector2> {
                {'A', new Vector2(100f, 100f)},
                {'B', new Vector2(400f, 400f)},
                {'C', new Vector2(700f, 100f)}};
            
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseMove += OnMouseMove;
            Paint += OnPaint;
        }
        
        private void OnMouseDown(object sender, MouseEventArgs mouse)
        {
            if (mouse.Button == MouseButtons.Left)
                _drag = _over;
            UpdateCursor();
            Invalidate();
        }
        
        private void OnMouseUp(object sender, MouseEventArgs mouse)
        {
            if (mouse.Button == MouseButtons.Left)
                _drag = null;
            UpdateCursor();
            Invalidate();
        }
        
        private void OnMouseMove(object sender, MouseEventArgs mouse)
        {
            var p = new Vector2(mouse.X, mouse.Y);
            if (_drag.HasValue)
            {
                _over = _drag.Value;
                _points[_drag.Value] = p;
            }
            else
            {
                _over = null;
                foreach (var pair in _points)
                {
                    if (Vector2.Distance(p, pair.Value) <= PointSize)
                    {
                        _over = pair.Key;
                        break;
                    }
                }
            }
            UpdateCursor();
            Invalidate();
        }
        
        private void UpdateCursor()
        {
            Cursor.Current = _over.HasValue ? Cursors.Hand : Cursors.Default;
        }
        
        private void OnPaint(object sender, PaintEventArgs paint)
        {
            var a = _points['A'];
            var b = _points['B'];
            var c = _points['C'];

            var nba = Vector2.Normalize(a - b);
            var nbc = Vector2.Normalize(c - b);
            var nbd = Vector2.Normalize((nba + nbc) / 2f);

            var rad = Math.Acos(Vector2.Dot(nba, nbc));
            var d = b + (float) (Radius / Math.Sin(rad / 2d)) * nbd;
            var e = b + (float) (Radius / Math.Tan(rad / 2d)) * nba;
            var f = b + (float) (Radius / Math.Tan(rad / 2d)) * nbc;

            var cross = Vector3.Cross(nba.ToVector3(), nbc.ToVector3());
            var dir = (Vector3.Dot(cross, Vector3.UnitZ) < 0 ? e : f) - d;
            var start = (float) (Math.Atan2(dir.Y, dir.X) * 180d / Math.PI);
            var angle = (float) (180d - rad * 180d / Math.PI);

            paint.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var pair in _points)
            {
                var pointBrush = _drag == pair.Key ? DragBrush : _over == pair.Key ? OverBrush : PointBrush ;
                paint.Graphics.DrawPoint(pointBrush, pair.Value, PointSize);
                paint.Graphics.DrawString(pair.Key.ToString(), LabelFont, LabelBrush, pair.Value - LabelOffset);
            }

            if (d.IsNaN())
            {
                paint.Graphics.DrawLine(LineBrush, a, b);
                paint.Graphics.DrawLine(LineBrush, b, c);
            }
            else
            {
                paint.Graphics.DrawLine(TempBrush, b, d);
                paint.Graphics.DrawLine(TempBrush, b, e);
                paint.Graphics.DrawLine(TempBrush, b, f);
                paint.Graphics.DrawLine(TempBrush, d, e);
                paint.Graphics.DrawLine(TempBrush, d, f);
                paint.Graphics.DrawCircle(TempBrush, d, Radius);
                
                paint.Graphics.DrawLine(LineBrush, a, e);
                paint.Graphics.DrawLine(LineBrush, f, c);
                paint.Graphics.DrawArc(LineBrush, d, Radius, start, angle);
            }
        }
    }
    
    internal static class Extensions
    {
        public static bool IsNaN(this Vector2 vector)
        {
            return float.IsNaN(vector.X) || float.IsNaN(vector.Y);
        }

        public static Vector3 ToVector3(this Vector2 vector)
        {
            return new Vector3(vector, 0f);
        }
        
        public static void DrawLine(this Graphics graphics, Pen pen, Vector2 pt1, Vector2 pt2)
        {
            graphics.DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);
        }
        
        public static void DrawPoint(this Graphics graphics, Pen pen, Vector2 pt, float size)
        {
            graphics.DrawRectangle(pen, pt.X - size / 2f, pt.Y - size / 2f, size, size);
        }
        
        public static void DrawCircle(this Graphics graphics, Pen pen, Vector2 center, float radius)
        {
            graphics.DrawArc(pen, center.X - radius, center.Y - radius, radius * 2f, radius * 2f, 0f, 360f);
        }

        public static void DrawArc(this Graphics graphics, Pen pen, Vector2 center, float radius, float start, float angle)
        {
            graphics.DrawArc(pen, center.X - radius, center.Y - radius, radius * 2f, radius * 2f, start, angle);
        }

        public static void DrawString(this Graphics graphics, string text, Font font, Brush brush, Vector2 pt)
        {
            graphics.DrawString(text, font, brush, pt.X, pt.Y);
        }
    }
}