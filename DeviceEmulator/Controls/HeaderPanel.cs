// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#region Using References



#endregion

namespace Microsoft.AzureCat.Samples.DeviceEmulator
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    public partial class HeaderPanel : Panel
    {
        #region Public Constructors

        public HeaderPanel()
        {
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.InitializeComponent();
            this.Padding = new Padding(5, this.headerHeight + 4, 5, 4);
        }

        #endregion

        #region Private Fields

        private int headerHeight = 24;
        private string headerText = "header title";
        private Font headerFont = new Font("Arial", 10F, FontStyle.Bold);
        private Color headerColor1 = SystemColors.InactiveCaption;
        private Color headerColor2 = SystemColors.ActiveCaption;
        private Color iconTransparentColor = Color.White;
        private Image icon = null;

        #endregion

        #region Public Properties

        [Browsable(true), Category("Custom")]
        public string HeaderText
        {
            get { return this.headerText; }
            set
            {
                this.headerText = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("Custom")]
        public int HeaderHeight
        {
            get { return this.headerHeight; }
            set
            {
                this.headerHeight = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("Custom")]
        public Font HeaderFont
        {
            get { return this.headerFont; }
            set
            {
                this.headerFont = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("Custom")]
        public Color HeaderColor1
        {
            get { return this.headerColor1; }
            set
            {
                this.headerColor1 = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("Custom")]
        public Color HeaderColor2
        {
            get { return this.headerColor2; }
            set
            {
                this.headerColor2 = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("Custom")]
        public Image Icon
        {
            get { return this.icon; }
            set
            {
                this.icon = value;
                this.Invalidate();
            }
        }

        [Browsable(true), Category("Custom")]
        public Color IconTransparentColor
        {
            get { return this.iconTransparentColor; }
            set
            {
                this.iconTransparentColor = value;
                this.Invalidate();
            }
        }

        #endregion

        #region Private Methods

        private void OutlookPanelEx_Paint(object sender, PaintEventArgs e)
        {
            if (this.headerHeight > 1)
            {
                // Draw border;
                this.DrawBorder(e.Graphics);

                // Draw heaeder
                this.DrawHeader(e.Graphics);

                // Draw text
                this.DrawText(e.Graphics);

                // Draw Icon
                this.DrawIcon(e.Graphics);
            }
        }

        private void DrawBorder(Graphics graphics)
        {
            using (Pen pen = new Pen(this.headerColor2))
            {
                graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

        private void DrawHeader(Graphics graphics)
        {
            Rectangle headerRect = new Rectangle(1, 1, this.Width - 2, this.headerHeight);
            using (Brush brush = new LinearGradientBrush(headerRect, this.headerColor1, this.headerColor2, LinearGradientMode.Vertical))
            {
                graphics.FillRectangle(brush, headerRect);
            }
        }

        private void DrawText(Graphics graphics)
        {
            if (!string.IsNullOrEmpty(this.headerText))
            {
                SizeF size = graphics.MeasureString(this.headerText, this.headerFont);
                using (Brush brush = new SolidBrush(this.ForeColor))
                {
                    float x;
                    if (this.icon != null)
                    {
                        x = this.icon.Width + 6;
                    }
                    else
                    {
                        x = 4;
                    }
                    graphics.DrawString(this.headerText, this.headerFont, brush, x, (this.headerHeight - size.Height)/2);
                }
            }
        }

        private void DrawIcon(Graphics graphics)
        {
            if (this.icon != null)
            {
                Point point = new Point(4, (this.headerHeight - this.icon.Height)/2);
                Bitmap bitmap = new Bitmap(this.icon);
                bitmap.MakeTransparent(this.iconTransparentColor);
                graphics.DrawImage(bitmap, point);
            }
        }

        #endregion
    }
}