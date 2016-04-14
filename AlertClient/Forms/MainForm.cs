// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#region Using Directives



#endregion

namespace Microsoft.AzureCat.Samples.AlertClient
{
    using System;
    using System.Configuration;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Microsoft.AzureCat.Samples.PayloadEntities;
    using Microsoft.ServiceBus.Messaging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public partial class MainForm : Form
    {
        #region Public Constructor

        /// <summary>
        /// Initializes a new instance of the MainForm class.
        /// </summary>
        public MainForm()
        {
            this.InitializeComponent();
            this.ConfigureComponent();
            this.ReadConfiguration();
        }

        #endregion

        #region Private Constants

        //***************************
        // Formats
        //***************************
        private const string DateFormat = "<{0,2:00}:{1,2:00}:{2,2:00}> {3}";
        private const string ExceptionFormat = "Exception: {0}";
        private const string InnerExceptionFormat = "InnerException: {0}";
        private const string LogFileNameFormat = "AlertClientLog-{0}.txt";

        //***************************
        // Constants
        //***************************
        private const string SaveAsTitle = "Save Log As";
        private const string SaveAsExtension = "txt";
        private const string SaveAsFilter = "Text Documents (*.txt)|*.txt";
        private const string Start = "Start";
        private const string Stop = "Stop";

        //***************************
        // Columns
        //***************************
        private const string DeviceId = "DeviceId";
        private const string DeviceName = "Name";
        private const string Value = "Value";
        private const string Model = "Model";
        private const string Manufacturer = "Manufacturer";
        private const string Type = "Type";
        private const string City = "City";
        private const string Country = "Country";

        //***************************
        // Configuration Parameters
        //***************************
        private const string DefaultEventHubName = "DeviceDemoOutputHub";
        private const string DefaultConsumerGroupName = "AlertClient";
        private const string StorageAccountConnectionStringParameter = "storageAccountConnectionString";
        private const string ServiceBusConnectionStringParameter = "serviceBusConnectionString";
        private const string EventHubParameter = "eventHub";
        private const string ConsumerGroupParameter = "consumerGroup";

        //***************************
        // Messages
        //***************************
        private const string StorageAccountConnectionStringCannotBeNull = "The Storage Account connection string cannot be null.";
        private const string ServiceBusConnectionStringCannotBeNull = "The Service Bus connection string cannot be null.";
        private const string EventHubNameCannotBeNull = "The Event Hub name cannot be null.";
        private const string ConsumerGroupCannotBeNull = "The Consumer Group name cannot be null.";
        private const string RegisteringEventProcessor = "Registering Event Processor [EventProcessor]... ";
        private const string EventProcessorRegistered = "Event Processor [EventProcessor] successfully registered. ";

        #endregion

        #region Private Fields

        private readonly SortableBindingList<Alert> alertBindingList = new SortableBindingList<Alert> {AllowNew = false, AllowEdit = false, AllowRemove = false};
        private EventProcessorHost eventProcessorHost;

        #endregion

        #region Public Methods

        public void ConfigureComponent()
        {
            this.txtStorageAccountConnectionString.AutoSize = false;
            this.txtStorageAccountConnectionString.Size = new Size(this.txtStorageAccountConnectionString.Size.Width, 24);
            this.txtServiceBusConnectionString.AutoSize = false;
            this.txtServiceBusConnectionString.Size = new Size(this.txtServiceBusConnectionString.Size.Width, 24);
            this.cboEventHub.AutoSize = false;
            this.cboEventHub.Size = new Size(this.cboEventHub.Size.Width, 24);
            this.txtConsumerGroup.AutoSize = false;
            this.txtConsumerGroup.Size = new Size(this.txtConsumerGroup.Size.Width, 24);
            // Set Grid style
            this.alertDataGridView.EnableHeadersVisualStyles = false;
            this.alertDataGridView.AutoGenerateColumns = false;
            this.alertDataGridView.AutoSize = true;

            // Create the DeviceId column
            DataGridViewTextBoxColumn textBoxColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = DeviceId,
                Name = DeviceId
            };
            this.alertDataGridView.Columns.Add(textBoxColumn);

            // Create the Name column
            textBoxColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = DeviceName,
                Name = DeviceName
            };
            this.alertDataGridView.Columns.Add(textBoxColumn);

            // Create the Value column
            textBoxColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = Value,
                Name = Value
            };
            this.alertDataGridView.Columns.Add(textBoxColumn);

            // Create the Model column
            textBoxColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = Model,
                Name = Model
            };
            this.alertDataGridView.Columns.Add(textBoxColumn);

            // Create the Type column
            textBoxColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = Type,
                Name = Type
            };
            this.alertDataGridView.Columns.Add(textBoxColumn);

            // Create the Manufacturer column
            textBoxColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = Manufacturer,
                Name = Manufacturer
            };
            this.alertDataGridView.Columns.Add(textBoxColumn);

            // Create the City column
            textBoxColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = City,
                Name = City
            };
            this.alertDataGridView.Columns.Add(textBoxColumn);

            // Create the Country column
            textBoxColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = Country,
                Name = Country
            };
            this.alertDataGridView.Columns.Add(textBoxColumn);

            // Set the selection background color for all the cells.
            this.alertDataGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(92, 125, 150);
            this.alertDataGridView.DefaultCellStyle.SelectionForeColor = SystemColors.Window;

            // Set RowHeadersDefaultCellStyle.SelectionBackColor so that its default 
            // value won't override DataGridView.DefaultCellStyle.SelectionBackColor.
            this.alertDataGridView.RowHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(153, 180, 209);

            // Set the background color for all rows and for alternating rows.  
            // The value for alternating rows overrides the value for all rows. 
            this.alertDataGridView.RowsDefaultCellStyle.BackColor = SystemColors.Window;
            this.alertDataGridView.RowsDefaultCellStyle.ForeColor = SystemColors.ControlText;
            //alertDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
            //alertDataGridView.AlternatingRowsDefaultCellStyle.ForeColor = SystemColors.ControlText;

            // Set the row and column header styles.
            this.alertDataGridView.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(215, 228, 242);
            this.alertDataGridView.RowHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
            this.alertDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(215, 228, 242);
            this.alertDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;

            // Set DataGridView DataSource
            this.alertBindingSource.DataSource = this.alertBindingList;
            this.alertDataGridView.DataSource = this.alertBindingSource;

            this.dataGridView_Resize(this.alertDataGridView, null);
        }

        public void HandleException(Exception ex)
        {
            if (string.IsNullOrEmpty(ex?.Message))
            {
                return;
            }
            this.WriteToLog(string.Format(CultureInfo.CurrentCulture, ExceptionFormat, ex.Message));
            if (!string.IsNullOrEmpty(ex.InnerException?.Message))
            {
                this.WriteToLog(string.Format(CultureInfo.CurrentCulture, InnerExceptionFormat, ex.InnerException.Message));
            }
        }

        #endregion

        #region Private Methods

        public static bool IsJson(string item)
        {
            if (item == null)
            {
                throw new ArgumentException("The item argument cannot be null.");
            }
            try
            {
                JToken obj = JToken.Parse(item);
                return obj != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string IndentJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        private void ReadConfiguration()
        {
            try
            {
                this.txtStorageAccountConnectionString.Text = ConfigurationManager.AppSettings[StorageAccountConnectionStringParameter];
                this.txtServiceBusConnectionString.Text = ConfigurationManager.AppSettings[ServiceBusConnectionStringParameter];
                string eventHubValue = ConfigurationManager.AppSettings[EventHubParameter] ?? DefaultEventHubName;
                string[] eventHubs = eventHubValue.Split(',', ';');
                foreach (string eventHub in eventHubs)
                {
                    this.cboEventHub.Items.Add(eventHub);
                }
                this.cboEventHub.SelectedIndex = 0;
                this.txtConsumerGroup.Text = ConfigurationManager.AppSettings[ConsumerGroupParameter] ?? DefaultConsumerGroupName;
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void WriteToLog(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(this.InternalWriteToLog), message);
            }
            else
            {
                this.InternalWriteToLog(message);
            }
        }

        private void InternalWriteToLog(string message)
        {
            lock (this)
            {
                if (string.IsNullOrEmpty(message))
                {
                    return;
                }
                string[] lines = message.Split('\n');
                DateTime now = DateTime.Now;
                string space = new string(' ', 19);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (i == 0)
                    {
                        string line = string.Format(
                            DateFormat,
                            now.Hour,
                            now.Minute,
                            now.Second,
                            lines[i]);
                        this.lstLog.Items.Add(line);
                    }
                    else
                    {
                        this.lstLog.Items.Add(space + lines[i]);
                    }
                }
                this.lstLog.SelectedIndex = this.lstLog.Items.Count - 1;
                this.lstLog.SelectedIndex = -1;
            }
        }

        #endregion

        #region Event Handlers

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void clearLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.lstLog.Items.Clear();
        }

        /// <summary>
        /// Saves the log to a text file
        /// </summary>
        /// <param name="sender">MainForm object</param>
        /// <param name="e">System.EventArgs parameter</param>
        private void saveLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.lstLog.Items.Count <= 0)
                {
                    return;
                }
                this.saveFileDialog.Title = SaveAsTitle;
                this.saveFileDialog.DefaultExt = SaveAsExtension;
                this.saveFileDialog.Filter = SaveAsFilter;
                this.saveFileDialog.FileName = string.Format(
                    LogFileNameFormat,
                    DateTime.Now.ToString(CultureInfo.CurrentUICulture).Replace('/', '-').Replace(':', '-'));
                if (this.saveFileDialog.ShowDialog() != DialogResult.OK ||
                    string.IsNullOrEmpty(this.saveFileDialog.FileName))
                {
                    return;
                }
                using (StreamWriter writer = new StreamWriter(this.saveFileDialog.FileName))
                {
                    foreach (object t in this.lstLog.Items)
                    {
                        writer.WriteLine(t as string);
                    }
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void logWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.splitContainer.Panel2Collapsed = !((ToolStripMenuItem) sender).Checked;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm form = new AboutForm();
            form.ShowDialog();
        }

        private void lstLog_Leave(object sender, EventArgs e)
        {
            this.lstLog.SelectedIndex = -1;
        }

        private void button_MouseEnter(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control != null)
            {
                control.ForeColor = Color.White;
            }
        }

        private void button_MouseLeave(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control != null)
            {
                control.ForeColor = SystemColors.ControlText;
            }
        }

        // ReSharper disable once FunctionComplexityOverflow
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            int width = (this.mainHeaderPanel.Size.Width - 48)/2;

            this.cboEventHub.Size = new Size(width, this.cboEventHub.Size.Height);
            this.txtConsumerGroup.Size = new Size(width, this.txtConsumerGroup.Size.Height);
            this.txtConsumerGroup.Location = new Point(32 + width, this.txtConsumerGroup.Location.Y);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            this.txtServiceBusConnectionString.SelectionLength = 0;
        }

        // ReSharper disable once FunctionComplexityOverflow
        private async void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (string.Compare(this.btnStart.Text, Start, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // Validate parameters
                    if (!this.ValidateParameters())
                    {
                        return;
                    }
                    this.btnStart.Enabled = false;

                    EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(this.txtServiceBusConnectionString.Text, this.cboEventHub.Text);

                    // Get the default Consumer Group
                    this.eventProcessorHost = new EventProcessorHost(
                        Guid.NewGuid().ToString(),
                        eventHubClient.Path.ToLower(),
                        this.txtConsumerGroup.Text.ToLower(),
                        this.txtServiceBusConnectionString.Text,
                        this.txtStorageAccountConnectionString.Text)
                    {
                        PartitionManagerOptions = new PartitionManagerOptions
                        {
                            AcquireInterval = TimeSpan.FromSeconds(10), // Default is 10 seconds
                            RenewInterval = TimeSpan.FromSeconds(10), // Default is 10 seconds
                            LeaseInterval = TimeSpan.FromSeconds(30) // Default value is 30 seconds
                        }
                    };
                    this.WriteToLog(RegisteringEventProcessor);
                    EventProcessorOptions eventProcessorOptions = new EventProcessorOptions
                    {
                        InvokeProcessorAfterReceiveTimeout = true,
                        MaxBatchSize = 100,
                        PrefetchCount = 100,
                        ReceiveTimeOut = TimeSpan.FromSeconds(30),
                    };
                    eventProcessorOptions.ExceptionReceived += this.EventProcessorOptions_ExceptionReceived;
                    EventProcessorFactoryConfiguration eventProcessorFactoryConfiguration = new EventProcessorFactoryConfiguration
                    {
                        TrackEvent = a => this.Invoke(new Action<Alert>(i => this.alertBindingList.Add(i)), a),
                        WriteToLog = m => this.Invoke(new Action<string>(this.WriteToLog), m)
                    };
                    await this.eventProcessorHost.RegisterEventProcessorFactoryAsync(
                        new EventProcessorFactory<EventProcessor>(eventProcessorFactoryConfiguration),
                        eventProcessorOptions);
                    this.WriteToLog(EventProcessorRegistered);

                    // Change button text
                    this.btnStart.Text = Stop;
                }
                else
                {
                    // Stop Event Processor
                    if (this.eventProcessorHost != null)
                    {
                        await this.eventProcessorHost.UnregisterEventProcessorAsync();
                    }

                    // Change button text
                    this.btnStart.Text = Start;
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                this.btnStart.Enabled = true;
            }
        }

        private void EventProcessorOptions_ExceptionReceived(object sender, ExceptionReceivedEventArgs e)
        {
            if (e?.Exception != null)
            {
                this.WriteToLog(e.Exception.Message);
            }
        }

        private bool ValidateParameters()
        {
            if (string.IsNullOrWhiteSpace(this.txtServiceBusConnectionString.Text))
            {
                this.WriteToLog(StorageAccountConnectionStringCannotBeNull);
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.txtServiceBusConnectionString.Text))
            {
                this.WriteToLog(ServiceBusConnectionStringCannotBeNull);
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.cboEventHub.Text))
            {
                this.WriteToLog(EventHubNameCannotBeNull);
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.txtConsumerGroup.Text))
            {
                this.WriteToLog(ConsumerGroupCannotBeNull);
                return false;
            }
            return true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.lstLog.Items.Clear();
            this.alertBindingList.Clear();
        }

        private void logTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            this.DrawTabControlTabs(this.logTabControl, e, null);
        }

        private void DrawTabControlTabs(TabControl tabControl, DrawItemEventArgs e, ImageList images)
        {
            // Get the bounding end of tab strip rectangles.
            Rectangle tabstripEndRect = tabControl.GetTabRect(tabControl.TabPages.Count - 1);
            RectangleF tabstripEndRectF = new RectangleF(
                tabstripEndRect.X + tabstripEndRect.Width,
                tabstripEndRect.Y - 5,
                tabControl.Width - (tabstripEndRect.X + tabstripEndRect.Width),
                tabstripEndRect.Height + 5);
            RectangleF leftVerticalLineRect = new RectangleF(
                2,
                tabstripEndRect.Y + tabstripEndRect.Height + 2,
                2,
                tabControl.TabPages[tabControl.SelectedIndex].Height + 2);
            RectangleF rightVerticalLineRect = new RectangleF(
                tabControl.TabPages[tabControl.SelectedIndex].Width + 4,
                tabstripEndRect.Y + tabstripEndRect.Height + 2,
                2,
                tabControl.TabPages[tabControl.SelectedIndex].Height + 2);
            RectangleF bottomHorizontalLineRect = new RectangleF(
                2,
                tabstripEndRect.Y + tabstripEndRect.Height + tabControl.TabPages[tabControl.SelectedIndex].Height + 2,
                tabControl.TabPages[tabControl.SelectedIndex].Width + 4,
                2);
            RectangleF leftVerticalBarNearFirstTab = new Rectangle(0, 0, 2, tabstripEndRect.Height + 2);

            // First, do the end of the tab strip.
            // If we have an image use it.
            if (tabControl.Parent.BackgroundImage != null)
            {
                RectangleF src = new RectangleF(
                    tabstripEndRectF.X + tabControl.Left,
                    tabstripEndRectF.Y + tabControl.Top,
                    tabstripEndRectF.Width,
                    tabstripEndRectF.Height);
                e.Graphics.DrawImage(tabControl.Parent.BackgroundImage, tabstripEndRectF, src, GraphicsUnit.Pixel);
            }
            // If we have no image, use the background color.
            else
            {
                using (Brush backBrush = new SolidBrush(tabControl.Parent.BackColor))
                {
                    e.Graphics.FillRectangle(backBrush, tabstripEndRectF);
                    e.Graphics.FillRectangle(backBrush, leftVerticalLineRect);
                    e.Graphics.FillRectangle(backBrush, rightVerticalLineRect);
                    e.Graphics.FillRectangle(backBrush, bottomHorizontalLineRect);
                    if (tabControl.SelectedIndex != 0)
                    {
                        e.Graphics.FillRectangle(backBrush, leftVerticalBarNearFirstTab);
                    }
                }
            }

            // Set up the page and the various pieces.
            TabPage page = tabControl.TabPages[e.Index];
            using (SolidBrush backBrush = new SolidBrush(page.BackColor))
            {
                using (SolidBrush foreBrush = new SolidBrush(page.ForeColor))
                {
                    string tabName = page.Text;

                    // Set up the offset for an icon, the bounding rectangle and image size and then fill the background.
                    int iconOffset = 0;
                    Rectangle tabBackgroundRect;

                    if (e.Index == tabControl.SelectedIndex)
                    {
                        tabBackgroundRect = e.Bounds;
                        e.Graphics.FillRectangle(backBrush, tabBackgroundRect);
                    }
                    else
                    {
                        tabBackgroundRect = new Rectangle(
                            e.Bounds.X,
                            e.Bounds.Y - 2,
                            e.Bounds.Width,
                            e.Bounds.Height + 4);
                        e.Graphics.FillRectangle(backBrush, tabBackgroundRect);
                        Rectangle rect = new Rectangle(e.Bounds.X - 2, e.Bounds.Y - 2, 1, 2);
                        e.Graphics.FillRectangle(backBrush, rect);
                        rect = new Rectangle(e.Bounds.X - 1, e.Bounds.Y - 2, 1, 2);
                        e.Graphics.FillRectangle(backBrush, rect);
                        rect = new Rectangle(e.Bounds.X + e.Bounds.Width, e.Bounds.Y - 2, 1, 2);
                        e.Graphics.FillRectangle(backBrush, rect);
                        rect = new Rectangle(e.Bounds.X + e.Bounds.Width + 1, e.Bounds.Y - 2, 1, 2);
                        e.Graphics.FillRectangle(backBrush, rect);
                    }

                    // If we have images, process them.
                    if (images != null)
                    {
                        // Get sice and image.
                        Size size = images.ImageSize;
                        Image icon = null;
                        if (page.ImageIndex > -1)
                        {
                            icon = images.Images[page.ImageIndex];
                        }
                        else if (page.ImageKey != "")
                        {
                            icon = images.Images[page.ImageKey];
                        }

                        // If there is an image, use it.
                        if (icon != null)
                        {
                            Point startPoint =
                                new Point(
                                    tabBackgroundRect.X + 2 + ((tabBackgroundRect.Height - size.Height)/2),
                                    tabBackgroundRect.Y + 2 + ((tabBackgroundRect.Height - size.Height)/2));
                            e.Graphics.DrawImage(icon, new Rectangle(startPoint, size));
                            iconOffset = size.Width + 4;
                        }
                    }

                    // Draw out the label.
                    Rectangle labelRect = new Rectangle(
                        tabBackgroundRect.X + iconOffset,
                        tabBackgroundRect.Y + 5,
                        tabBackgroundRect.Width - iconOffset,
                        tabBackgroundRect.Height - 3);
                    using (StringFormat sf = new StringFormat {Alignment = StringAlignment.Center})
                    {
                        e.Graphics.DrawString(tabName, new Font(e.Font.FontFamily, 8.25F, e.Font.Style), foreBrush, labelRect, sf);
                    }
                }
            }
        }

        private void dataGridView_Resize(object sender, EventArgs e)
        {
            this.CalculateLastColumnWidth(sender);
        }

        private void dataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            this.CalculateLastColumnWidth(sender);
            DataGridView dataGridView = sender as DataGridView;
            if (dataGridView == null)
            {
                return;
            }
            dataGridView.FirstDisplayedScrollingRowIndex = dataGridView.RowCount - 1;
        }

        private void dataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            this.CalculateLastColumnWidth(sender);
        }

        private void CalculateLastColumnWidth(object sender)
        {
            DataGridView dataGridView = sender as DataGridView;
            if (dataGridView == null)
            {
                return;
            }
            try
            {
                dataGridView.SuspendLayout();
                int width = dataGridView.Width - dataGridView.RowHeadersWidth;
                VScrollBar verticalScrollbar = dataGridView.Controls.OfType<VScrollBar>().First();
                if (verticalScrollbar.Visible)
                {
                    width -= verticalScrollbar.Width;
                }
                int columnWidth = width/dataGridView.ColumnCount;
                for (int i = 0; i < dataGridView.ColumnCount; i++)
                {
                    dataGridView.Columns[i].Width = i == dataGridView.ColumnCount - 1
                        ? columnWidth + (width - (columnWidth*dataGridView.ColumnCount))
                        : columnWidth;
                }
            }
            finally
            {
                dataGridView.ResumeLayout();
            }
        }

        private void grouperEventHub_CustomPaint(PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(
                new Pen(SystemColors.ActiveBorder, 1),
                this.cboEventHub.Location.X - 1,
                this.cboEventHub.Location.Y - 1,
                this.cboEventHub.Size.Width + 1,
                this.cboEventHub.Size.Height + 1);
        }

        #endregion
    }
}