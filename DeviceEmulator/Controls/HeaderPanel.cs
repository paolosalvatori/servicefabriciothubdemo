#region Copyright

// //=======================================================================================
// // Microsoft Azure Customer Advisory Team  
// //
// // This sample is supplemental to the technical guidance published on the community
// // blog at http://blogs.msdn.com/b/paolos/. 
// // 
// // Author: Paolo Salvatori
// //=======================================================================================
// // Copyright © 2016 Microsoft Corporation. All rights reserved.
// // 
// // THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
// // EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
// // MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. YOU BEAR THE RISK OF USING IT.
// //=======================================================================================

#endregion

#region Using References

#endregion

#region Using Directives

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#endregion

namespace Microsoft.AzureCat.Samples.DeviceEmulator
{
    public partial class HeaderPanel : Panel
    {
        #region Public Constructors

        public HeaderPanel()
        {
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();
            Padding = new Padding(5, headerHeight + 4, 5, 4);
        }

        #endregion

        #region Private Fields

        private int headerHeight = 24;
        private string headerText = "header title";
        private Font headerFont = new Font("Arial", 10F, FontStyle.Bold);
        private Color headerColor1 = SystemColors.InactiveCaption;
        private Color headerColor2 = SystemColors.ActiveCaption;
        private Color iconTransparentColor = Color.White;
        private Image icon;

        #endregion

        #region Public Properties

        [Browsable(true)]
        [Category("Custom")]
        public string HeaderText
        {
            get { return headerText; }
            set
            {
                headerText = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Custom")]
        public int HeaderHeight
        {
            get { return headerHeight; }
            set
            {
                headerHeight = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Custom")]
        public Font HeaderFont
        {
            get { return headerFont; }
            set
            {
                headerFont = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Custom")]
        public Color HeaderColor1
        {
            get { return headerColor1; }
            set
            {
                headerColor1 = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Custom")]
        public Color HeaderColor2
        {
            get { return headerColor2; }
            set
            {
                headerColor2 = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Custom")]
        public Image Icon
        {
            get { return icon; }
            set
            {
                icon = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Custom")]
        public Color IconTransparentColor
        {
            get { return iconTransparentColor; }
            set
            {
                iconTransparentColor = value;
                Invalidate();
            }
        }

        #endregion

        #region Private Methods

        private void OutlookPanelEx_Paint(object sender, PaintEventArgs e)
        {
            if (headerHeight > 1)
            {
                // Draw border;
                DrawBorder(e.Graphics);

                // Draw heaeder
                DrawHeader(e.Graphics);

                // Draw text
                DrawText(e.Graphics);

                // Draw Icon
                DrawIcon(e.Graphics);
            }
        }

        private void DrawBorder(Graphics graphics)
        {
            using (var pen = new Pen(headerColor2))
            {
                graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void DrawHeader(Graphics graphics)
        {
            var headerRect = new Rectangle(1, 1, Width - 2, headerHeight);
            using (
                Brush brush = new LinearGradientBrush(headerRect, headerColor1, headerColor2,
                    LinearGradientMode.Vertical))
            {
                graphics.FillRectangle(brush, headerRect);
            }
        }

        private void DrawText(Graphics graphics)
        {
            if (!string.IsNullOrEmpty(headerText))
            {
                var size = graphics.MeasureString(headerText, headerFont);
                using (Brush brush = new SolidBrush(ForeColor))
                {
                    float x;
                    if (icon != null)
                        x = icon.Width + 6;
                    else
                        x = 4;
                    graphics.DrawString(headerText, headerFont, brush, x, (headerHeight - size.Height)/2);
                }
            }
        }

        private void DrawIcon(Graphics graphics)
        {
            if (icon != null)
            {
                var point = new Point(4, (headerHeight - icon.Height)/2);
                var bitmap = new Bitmap(icon);
                bitmap.MakeTransparent(iconTransparentColor);
                graphics.DrawImage(bitmap, point);
            }
        }

        #endregion
    }
}