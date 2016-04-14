// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#region Using Directives



#endregion

namespace Microsoft.AzureCat.Samples.DeviceEmulator
{
    using System;
    using System.Globalization;
    using System.Windows.Forms;

    public class NumericTextBox : TextBox
    {
        #region Protected Methods

        // Restricts the entry of characters to digits (including hex), the negative sign,
        // the decimal point, and editing keystrokes (backspace).
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            NumberFormatInfo numberFormatInfo = CultureInfo.CurrentCulture.NumberFormat;
            string decimalSeparator = numberFormatInfo.NumberDecimalSeparator;
            string groupSeparator = numberFormatInfo.NumberGroupSeparator;
            string negativeSign = numberFormatInfo.NegativeSign;

            string keyInput = e.KeyChar.ToString(CultureInfo.InvariantCulture);

            if (Char.IsDigit(e.KeyChar))
            {
                // Digits are OK
            }
            else if (keyInput.Equals(decimalSeparator) || keyInput.Equals(groupSeparator) ||
                     keyInput.Equals(negativeSign))
            {
                // Decimal separator is OK
            }
            else if (e.KeyChar == '\b')
            {
                // Backspace key is OK
            }
            //    else if ((ModifierKeys & (Keys.Control | Keys.Alt)) != 0)
            //    {
            //     // Let the edit control handle control and alt key combinations
            //    }
            else if (this.AllowSpace && e.KeyChar == ' ')
            {
            }
            else
            {
                // Swallow this invalid key and beep
                e.Handled = true;
            }
        }

        #endregion

        #region Private Fields

        #endregion

        #region Public Properties

        public int IntegerValue
        {
            get { return Int32.Parse(this.Text); }
        }

        public decimal DecimalValue
        {
            get { return Decimal.Parse(this.Text); }
        }

        public bool AllowSpace { set; get; }

        #endregion
    }
}