// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#region Using Directives



#endregion

namespace Microsoft.AzureCat.Samples.AlertClient
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;
    using Microsoft.AzureCat.Samples.AlertClient.Properties;

    public partial class AboutForm : Form
    {
        #region Public Constructor

        public AboutForm()
        {
            this.InitializeComponent();

            //This form is double buffered
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            //A random number generator for the initial setup
            Random random = new Random();
            const int count = 10;

            // We create white logo bitmaps
            for (int i = 0; i < count; i++)
            {
                Picture shape = new Picture(this.whiteLogoBitmap)
                {
                    Limits = this.ClientRectangle,
                    Location = new Point(
                        random.Next(this.ClientRectangle.Width + 16),
                        random.Next(this.ClientRectangle.Height + 16)),
                    Size = new Size(1 + random.Next(100), 1 + random.Next(100)),
                    BackColor = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)),
                    ForeColor = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)),
                    RotationDelta = random.Next(20),
                    Transparency = (float) random.NextDouble(),
                    LineThickness = random.Next(10),
                    Vector = new Size(-10 + random.Next(20), -10 + random.Next(20))
                };

                //and added to the list of shapes
                this.shapes.Add(shape);
            }

            // We create azure logo bitmaps
            for (int i = 0; i < count; i++)
            {
                Picture shape = new Picture(this.azureLogoBitmap)
                {
                    Limits = this.ClientRectangle,
                    Location = new Point(
                        random.Next(this.ClientRectangle.Width + 16),
                        random.Next(this.ClientRectangle.Height + 16)),
                    Size = new Size(1 + random.Next(100), 1 + random.Next(100)),
                    BackColor = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)),
                    ForeColor = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)),
                    RotationDelta = random.Next(20),
                    Transparency = (float) random.NextDouble(),
                    LineThickness = random.Next(10),
                    Vector = new Size(-10 + random.Next(20), -10 + random.Next(20))
                };

                //and added to the list of shapes
                this.shapes.Add(shape);
            }

            //set up the timer so that animation can take place
            this.timer.Interval = 40;
            this.timer.Tick += this.timer_Tick;
            this.timer.Enabled = true;
        }

        #endregion

        private void mailLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("mailto:paolos@microsoft.com?subject=Service%20Bus%20Explorer%20Feedback");
        }

        private void blogLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://blogs.msdn.com/paolos");
        }

        private void twitterLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://twitter.com/babosbird");
        }

        #region Private Fields

        private readonly Bitmap whiteLogoBitmap = new Bitmap(Resources.WhiteLogo);
        private readonly Bitmap azureLogoBitmap = new Bitmap(Resources.AzureLogo1);

        /// <summary>
        /// A collection of Shape based objects
        /// </summary>
        private readonly ShapeCollection shapes = new ShapeCollection();

        /// <summary>
        /// The message-driven timer 
        /// </summary>
        private readonly Timer timer = new Timer();

        #endregion

        #region Event Handlers

        private void timer_Tick(object sender, EventArgs e)
        {
            foreach (Shape shape in this.shapes)
            {
                shape.Tick();
            }
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            foreach (Shape shape in this.shapes)
            {
                shape.Draw(e.Graphics);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.timer.Enabled = false;
            this.timer.Dispose();
            base.OnClosing(e);
        }

        /// <summary>
        /// Updates the limits of all current shapes so that they don't disappear off-screen
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSizeChanged(EventArgs e)
        {
            foreach (Shape shape in this.shapes)
            {
                shape.Limits = this.ClientRectangle;
            }
            base.OnSizeChanged(e);
        }

        #endregion
    }

    public class Shape
    {
        public void Draw(Graphics g)
        {
            this.SetupTransform(g);
            this.RenderObject(g);
            this.RestoreTransform(g);
        }

        public virtual void Tick()
        {
            //ensure that the object is in the page.
            //this is in case the window was resized
            if (this.Location.X > this.Limits.Right)
            {
                this.Location = new Point(this.Limits.Right - 1, this.Location.Y);
            }
            if (this.Location.Y > this.Limits.Bottom)
            {
                this.Location = new Point(this.Location.X, this.Limits.Bottom - 1);
            }
            //Generate a new location adding in the vectors
            //check the limits and switch vector directions as needed
            int newX = this.Location.X + this.Vector.Width;
            if (newX > this.Limits.Right || newX < this.Limits.Left)
            {
                this.Vector = new Size(-1*this.Vector.Width, this.Vector.Height);
            }
            int newY = this.Location.Y + this.Vector.Height;
            if (newY > this.Limits.Bottom || newY < this.Limits.Top)
            {
                this.Vector = new Size(this.Vector.Width, -1*this.Vector.Height);
            }
            //This is the new position
            this.Location = new Point(this.Location.X + this.Vector.Width, this.Location.Y + this.Vector.Height);

            //Apply the rotation factor
            this.Rotation += this.RotationDelta;

            //Limit just to be neat
            this.Rotation = (this.Rotation < 360f ? (this.Rotation >= 0 ? this.Rotation : this.Rotation + 360f) : this.Rotation - 360f);
        }

        public virtual void RenderObject(Graphics g)
        {
        }

        /// <summary>
        /// Sets up the transform for each shape
        /// </summary>
        /// <remarks>
        /// As each shape is drawn the transform for that shape including rotation and location is made to a new Matrix object.
        /// This matrix is used to modify the graphics transform <i>For each shape</i> 
        /// </remarks>
        /// <param name="g">The Graphics being drawn on</param>
        protected void SetupTransform(Graphics g)
        {
            this.state = g.Save();
            Matrix matrix = new Matrix();
            matrix.Rotate(this.Rotation, MatrixOrder.Append);
            matrix.Translate(this.Location.X, this.Location.Y, MatrixOrder.Append);
            g.Transform = matrix;
        }

        /// <summary>
        /// Simply restores the original state of the Graphics object
        /// </summary>
        /// <param name="g">The Graphics object being drawn upon</param>
        protected void RestoreTransform(Graphics g)
        {
            g.Restore(this.state);
        }

        #region Private Fields

        private GraphicsState state;
        private float transparency;

        #endregion

        #region Public Properties

        public Point Location { get; set; }

        public Size Size { get; set; }

        public Color BackColor { get; set; }

        public Color ForeColor { get; set; }

        public int LineThickness { get; set; }

        public float Rotation { get; set; }

        public Size Vector { get; set; }

        public float RotationDelta { get; set; }

        public Rectangle Limits { get; set; }

        public float Transparency
        {
            get { return this.transparency; }
            set { this.transparency = (value >= 0 ? (value <= 1 ? value : 1) : 0); }
        }

        #endregion
    }

    public class Square : Shape
    {
        /// <summary>
        /// Draws a square. Note that the square is drawn about the origin.
        /// </summary>
        /// <param name="g">The graphics to draw on.</param>
        public override void RenderObject(Graphics g)
        {
            Pen pen = new Pen(this.ForeColor, this.LineThickness);
            SolidBrush solidBrush = new SolidBrush(Color.FromArgb((int) (255*this.Transparency), this.BackColor));
            g.FillRectangle(solidBrush, -this.Size.Width/2, -this.Size.Height/2, this.Size.Width, this.Size.Height);
            g.DrawRectangle(pen, -this.Size.Width/2, -this.Size.Height/2, this.Size.Width, this.Size.Height);
            solidBrush.Dispose();
            pen.Dispose();
        }
    }

    public class Picture : Shape
    {
        #region Private Fields

        private readonly Bitmap bitmap;

        #endregion

        public Picture(Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }

        /// <summary>
        /// Draws a bitmap. 
        /// </summary>
        /// <param name="g">The graphics to draw on.</param>
        public override void RenderObject(Graphics g)
        {
            g.DrawImage(this.bitmap, -this.Size.Width/2, -this.Size.Height/2);
        }
    }

    public class Star : Shape
    {
        /// <summary>
        /// Draws a star. Note that the star is drawn about the origin.
        /// </summary>
        /// <param name="g">The graphics to draw on.</param>
        public override void RenderObject(Graphics g)
        {
            Pen pen = new Pen(this.ForeColor, this.LineThickness);
            SolidBrush solidBrush = new SolidBrush(Color.FromArgb((int) (255*this.Transparency), this.BackColor));
            Point[] points = new Point[11];
            bool pointy = true;
            float a = 0;
            for (int i = 0; i < 10; i++)
            {
                float distance = pointy ? 1 : 0.6f;
                points[i] = new Point((int) (distance*(this.Size.Width/2)*Math.Cos(a)), (int) (distance*(this.Size.Height/2)*Math.Sin(a)));
                a += (float) Math.PI*2/10;
                pointy = !pointy;
            }
            points[10] = points[0];
            g.FillPolygon(solidBrush, points);
            g.DrawPolygon(pen, points);
            solidBrush.Dispose();
            pen.Dispose();
        }
    }

    public class Pentagon : Shape
    {
        /// <summary>
        /// Draws a pentagon. Note that the pentagon is drawn about the origin.
        /// </summary>
        /// <param name="g">The graphics to draw on.</param>
        public override void RenderObject(Graphics g)
        {
            Pen pen = new Pen(this.ForeColor, this.LineThickness);
            SolidBrush solidBrush = new SolidBrush(Color.FromArgb((int) (255*this.Transparency), this.BackColor));
            Point[] points = new Point[6];
            float a = 0;
            for (int i = 0; i < 5; i++)
            {
                points[i] = new Point((int) ((this.Size.Width/2)*Math.Cos(a)), (int) ((this.Size.Height/2)*Math.Sin(a)));
                a += (float) Math.PI*2/5;
            }
            points[5] = points[0];
            g.FillPolygon(solidBrush, points);
            g.DrawPolygon(pen, points);
            solidBrush.Dispose();
            pen.Dispose();
        }
    }

    /// <summary>
    /// Manages a collection of shape objects
    /// </summary>
    public class ShapeCollection : CollectionBase
    {
        public Shape this[int index]
        {
            get { return (Shape) this.List[index]; }
            set { this.List[index] = value; }
        }

        public void Add(Shape shape)
        {
            this.List.Add(shape);
        }

        public void Remove(Shape shape)
        {
            this.List.Remove(shape);
        }
    }
}