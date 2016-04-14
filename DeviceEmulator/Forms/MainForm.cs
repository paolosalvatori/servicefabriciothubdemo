// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#region Using Directives



#endregion

namespace Microsoft.AzureCat.Samples.DeviceEmulator
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Message = Microsoft.Azure.Devices.Client.Message;

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
        private const string LogFileNameFormat = "DeviceEmulatorLog-{0}.txt";

        //***************************
        // Constants
        //***************************
        private const string SaveAsTitle = "Save Log As";
        private const string SaveAsExtension = "txt";
        private const string SaveAsFilter = "Text Documents (*.txt)|*.txt";
        private const string Start = "Start";
        private const string Stop = "Stop";
        private const string Source = "source";
        private const string SourceValue = "Device Emulator";

        //***************************
        // Configuration Parameters
        //***************************
        private const string UrlParameter = "url";
        private const string ConnectionStringParameter = "connectionString";
        private const string DeviceCountParameter = "deviceCount";
        private const string EventIntervalParameter = "eventInterval";
        private const string MinValueParameter = "minValue";
        private const string MaxValueParameter = "maxValue";
        private const string MinOffsetParameter = "minOffset";
        private const string MaxOffsetParameter = "maxOffset";
        private const string SpikePercentageParameter = "spikePercentage";

        //***************************
        // Configuration Parameters
        //***************************
        private const string DefaultStatus = "Ok";
        private const int DefaultDeviceNumber = 10;
        private const int DefaultMinValue = 20;
        private const int DefaultMaxValue = 50;
        private const int DefaultMinOffset = 20;
        private const int DefaultMaxOffset = 50;
        private const int DefaultSpikePercentage = 10;
        private const int DefaultEventIntervalInMilliseconds = 100;


        //***************************
        // Messages
        //***************************
        private const string ConnectionStringCannotBeNull = "The IoT connection string cannot be null.";
        private const string ConnectionStringIsWrong = "The IoT connection string does not contain the HostName.";
        private const string InitializingDeviceActors = "Initializing device actors...";
        private const string DeviceActorsInitialized = "Device actors initialized.";
        private const string CreatingDevices = "Creating devices...";
        private const string DevicesCreated = "Devices created.";
        private const string RemovingDevices = "Removing devices...";
        private const string DevicesRemoved = "Devices removed.";

        #endregion

        #region Private Fields

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private string ioTHubUrl;
        private readonly Random random = new Random((int) DateTime.Now.Ticks);
        private readonly List<Device> deviceList = new List<Device>();

        #endregion

        #region Public Methods

        public void ConfigureComponent()
        {
            this.txtConnectionString.AutoSize = false;
            this.txtConnectionString.Size = new Size(this.txtConnectionString.Size.Width, 24);
            this.txtDeviceCount.AutoSize = false;
            this.txtDeviceCount.Size = new Size(this.txtDeviceCount.Size.Width, 24);
            this.txtEventIntervalInMilliseconds.AutoSize = false;
            this.txtEventIntervalInMilliseconds.Size = new Size(this.txtEventIntervalInMilliseconds.Size.Width, 24);
            this.txtMinValue.AutoSize = false;
            this.txtMinValue.Size = new Size(this.txtMinValue.Size.Width, 24);
            this.txtMaxValue.AutoSize = false;
            this.txtMaxValue.Size = new Size(this.txtMaxValue.Size.Width, 24);
            this.txtMinOffset.AutoSize = false;
            this.txtMinOffset.Size = new Size(this.txtMinOffset.Size.Width, 24);
            this.txtMaxOffset.AutoSize = false;
            this.txtMaxOffset.Size = new Size(this.txtMinOffset.Size.Width, 24);
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
                string urlValue = ConfigurationManager.AppSettings[UrlParameter];
                string[] urls = urlValue.Split(',', ';');
                foreach (string url in urls)
                {
                    this.cboDeviceManagementServiceUrl.Items.Add(url);
                }
                this.cboDeviceManagementServiceUrl.SelectedIndex = 0;

                this.txtConnectionString.Text = ConfigurationManager.AppSettings[ConnectionStringParameter];

                int value;
                string setting = ConfigurationManager.AppSettings[DeviceCountParameter];
                this.txtDeviceCount.Text = int.TryParse(setting, out value)
                    ? value.ToString(CultureInfo.InvariantCulture)
                    : DefaultDeviceNumber.ToString(CultureInfo.InvariantCulture);
                setting = ConfigurationManager.AppSettings[EventIntervalParameter];
                this.txtEventIntervalInMilliseconds.Text = int.TryParse(setting, out value)
                    ? value.ToString(CultureInfo.InvariantCulture)
                    : DefaultEventIntervalInMilliseconds.ToString(CultureInfo.InvariantCulture);
                setting = ConfigurationManager.AppSettings[MinValueParameter];
                this.txtMinValue.Text = int.TryParse(setting, out value)
                    ? value.ToString(CultureInfo.InvariantCulture)
                    : DefaultMinValue.ToString(CultureInfo.InvariantCulture);
                setting = ConfigurationManager.AppSettings[MaxValueParameter];
                this.txtMaxValue.Text = int.TryParse(setting, out value)
                    ? value.ToString(CultureInfo.InvariantCulture)
                    : DefaultMaxValue.ToString(CultureInfo.InvariantCulture);
                setting = ConfigurationManager.AppSettings[MinOffsetParameter];
                this.txtMinOffset.Text = int.TryParse(setting, out value)
                    ? value.ToString(CultureInfo.InvariantCulture)
                    : DefaultMinOffset.ToString(CultureInfo.InvariantCulture);
                setting = ConfigurationManager.AppSettings[MaxOffsetParameter];
                this.txtMaxOffset.Text = int.TryParse(setting, out value)
                    ? value.ToString(CultureInfo.InvariantCulture)
                    : DefaultMaxOffset.ToString(CultureInfo.InvariantCulture);
                setting = ConfigurationManager.AppSettings[SpikePercentageParameter];
                this.trackbarSpikePercentage.Value = int.TryParse(setting, out value)
                    ? value
                    : DefaultSpikePercentage;
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
            int width = (this.mainHeaderPanel.Size.Width - 80)/2;
            int halfWidth = (width - 16)/2;

            this.txtMinOffset.Size = new Size(halfWidth, this.txtMinOffset.Size.Height);
            this.txtMaxOffset.Size = new Size(halfWidth, this.txtMaxOffset.Size.Height);
            this.txtDeviceCount.Size = new Size(halfWidth, this.txtDeviceCount.Size.Height);
            this.txtEventIntervalInMilliseconds.Size = new Size(halfWidth, this.txtEventIntervalInMilliseconds.Size.Height);
            this.txtMinValue.Size = new Size(halfWidth, this.txtMinValue.Size.Height);
            this.txtMaxValue.Size = new Size(halfWidth, this.txtMaxValue.Size.Height);
            this.trackbarSpikePercentage.Size = new Size(width, this.trackbarSpikePercentage.Size.Height);

            this.txtMaxValue.Location = new Point(32 + halfWidth, this.txtMaxValue.Location.Y);
            this.txtMinOffset.Location = new Point(32 + width, this.txtMaxOffset.Location.Y);
            this.txtMaxOffset.Location = new Point(48 + +width + halfWidth, this.txtMaxOffset.Location.Y);
            this.txtEventIntervalInMilliseconds.Location = new Point(32 + halfWidth, this.txtEventIntervalInMilliseconds.Location.Y);
            this.trackbarSpikePercentage.Location = new Point(32 + width, this.trackbarSpikePercentage.Location.Y);

            this.lblMaxValue.Location = new Point(32 + halfWidth, this.lblMaxValue.Location.Y);
            this.lblMinOffset.Location = new Point(32 + width, this.lblMaxOffset.Location.Y);
            this.lblMaxOffset.Location = new Point(48 + +width + halfWidth, this.lblMaxOffset.Location.Y);
            this.lblEventIntervalInMilliseconds.Location = new Point(32 + halfWidth, this.lblEventIntervalInMilliseconds.Location.Y);
            this.lblSpikePercentage.Location = new Point(32 + width, this.lblSpikePercentage.Location.Y);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            this.txtConnectionString.SelectionLength = 0;
        }

        // ReSharper disable once FunctionComplexityOverflow
        private async void btnStart_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            if (string.Compare(this.btnStart.Text, Start, StringComparison.OrdinalIgnoreCase) == 0)
            {
                try
                {
                    // Validate parameters
                    if (!this.ValidateParameters())
                    {
                        return;
                    }
                    // Change button text
                    this.btnStart.Text = Stop;

                    // Create cancellation token source
                    this.cancellationTokenSource = new CancellationTokenSource();

                    // Initialize Devices
                    if (this.deviceList.Count != this.txtDeviceCount.IntegerValue)
                    {
                        await this.CreateDevicesOnIoTHubIdentiyRegistryAsync();
                        await this.InitializeDevicesAsync();
                    }

                    // Start Devices
                    this.StartDevices();
                }
                catch (Exception ex)
                {
                    this.HandleException(ex);

                    // Change button text
                    this.btnStart.Text = Start;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
            else
            {
                try
                {
                    // Stop Devices
                    this.StopDevices();

                    // Change button text
                    this.btnStart.Text = Start;
                }
                catch (Exception ex)
                {
                    this.HandleException(ex);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private async void btnCreateDevices_Click(object sender, EventArgs e)
        {
            await this.CreateDevicesAsync();
        }

        private async void btnRemoveDevices_Click(object sender, EventArgs e)
        {
            await this.RemoveDevicesAsync();
        }

        private bool ValidateParameters()
        {
            if (string.IsNullOrWhiteSpace(this.txtConnectionString.Text))
            {
                this.WriteToLog(ConnectionStringCannotBeNull);
                return false;
            }
            Match match = Regex.Match(this.txtConnectionString.Text, @"HostName=([A-Za-z0-9_.-]+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                this.ioTHubUrl = match.Groups[1].Value;
            }
            else
            {
                this.WriteToLog(ConnectionStringIsWrong);
                return false;
            }
            return true;
        }

        private int GetValue(
            int minValue,
            int maxValue,
            int minOffset,
            int maxOffset,
            int spikePercentage)
        {
            int value = this.random.Next(0, 100);
            if (value >= spikePercentage)
            {
                return this.random.Next(minValue, maxValue + 1);
            }
            int sign = this.random.Next(0, 2);
            int offset = this.random.Next(minOffset, maxOffset + 1);
            offset = sign == 0 ? -offset : offset;
            return this.random.Next(minValue, maxValue + 1) + offset;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.lstLog.Items.Clear();
        }

        private void StartDevices()
        {
            const string status = DefaultStatus;
            int eventInterval = this.txtEventIntervalInMilliseconds.IntegerValue;
            int minValue = this.txtMinValue.IntegerValue;
            int maxValue = this.txtMaxValue.IntegerValue;
            int minOffset = this.txtMinOffset.IntegerValue;
            int maxOffset = this.txtMaxOffset.IntegerValue;
            int spikePercentage = this.trackbarSpikePercentage.Value;
            CancellationToken cancellationToken = this.cancellationTokenSource.Token;

            // Create one task for each device
            for (int i = 0; i < this.txtDeviceCount.IntegerValue; i++)
            {
                int deviceId = i;
#pragma warning disable 4014
                Task.Run(
                    async () =>
                    {
                        string deviceName = $"device{deviceId:000}";
                        DeviceClient deviceClient = DeviceClient.Create(
                            this.ioTHubUrl,
                            new DeviceAuthenticationWithRegistrySymmetricKey(
                                this.deviceList[deviceId].Id,
                                this.deviceList[deviceId].Authentication.SymmetricKey.PrimaryKey));
                        this.WriteToLog($"DeviceClient [{deviceName}] successfully created.");
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            // Create random value
                            int value = this.GetValue(minValue, maxValue, minOffset, maxOffset, spikePercentage);
                            DateTime timestamp = DateTime.Now;

                            // Create EventData object with the payload serialized in JSON format 
                            Payload payload = new Payload
                            {
                                DeviceId = deviceId,
                                Name = deviceName,
                                Status = status,
                                Value = value,
                                Timestamp = timestamp
                            };
                            string json = JsonConvert.SerializeObject(payload);
                            using (Message message = new Message(Encoding.UTF8.GetBytes(json)))
                            {
                                // Create custom properties
                                message.Properties.Add(Source, SourceValue);

                                // Send the event to the event hub
                                await deviceClient.SendEventAsync(message);
                                this.WriteToLog(
                                    $"[Event] DeviceId=[{payload.DeviceId:000}] " +
                                    $"Value=[{payload.Value:000}] " +
                                    $"Timestamp=[{payload.Timestamp}]");
                            }

                            // Wait for the event time interval
                            Thread.Sleep(eventInterval);
                        }
                    },
                    cancellationToken).ContinueWith(
                        t =>
                        {
                            if (t.IsFaulted && t.Exception != null)
                            {
                                this.HandleException(t.Exception);
                            }
                        },
                        cancellationToken);
            }
        }

        private void StopDevices()
        {
            this.cancellationTokenSource?.Cancel();
        }

        public static string Combine(string uri1, string uri2)
        {
            uri1 = uri1.TrimEnd('/');
            uri2 = uri2.TrimStart('/');
            return $"{uri1}/{uri2}";
        }

        private void UpdateProgressBar(int value)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int>(this.InternalUpdateProgressBar), value);
            }
            else
            {
                this.InternalUpdateProgressBar(value);
            }
        }

        private void InternalUpdateProgressBar(int value)
        {
            this.toolStripProgressBar.Value = value;
        }

        private async Task CreateDevicesOnIoTHubIdentiyRegistryAsync()
        {
            // Clear device list
            this.deviceList.Clear();

            // Create device identity registry manager from the connection string
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(this.txtConnectionString.Text);
            this.toolStripProgressBar.Minimum = 0;
            this.toolStripProgressBar.Maximum = this.txtDeviceCount.IntegerValue;
            this.toolStripProgressBar.Value = 0;
            int added = 0;

            this.WriteToLog(InitializingDeviceActors);
            List<Task> taskList = new List<Task>();
            for (int i = 1; i <= this.txtDeviceCount.IntegerValue; i++)
            {
                Device device;
                string deviceId = $"device{i:000}";
                taskList.Add(
                    Task.Run(
                        async () =>
                        {
                            // if the device already exists in the device identity registry, retrieve it
                            device = await registryManager.GetDeviceAsync(deviceId);
                            if (device == null)
                            {
                                // try to create the device in the device identity registry
                                device = await registryManager.AddDeviceAsync(new Device(deviceId));
                                this.WriteToLog(
                                    $"Device [{deviceId}] successfully created in the device identity registry with key [{device.Authentication.SymmetricKey.PrimaryKey}]");
                            }
                            else
                            {
                                this.WriteToLog(
                                    $"Device [{deviceId}] already exists in the device identity registry with key [{device.Authentication.SymmetricKey.PrimaryKey}]");
                            }
                            this.UpdateProgressBar(++added);
                            this.deviceList.Add(device);
                        },
                        this.cancellationTokenSource.Token));
                await Task.WhenAll(taskList);
            }
            if (!this.cancellationTokenSource.Token.IsCancellationRequested)
            {
                this.WriteToLog(DeviceActorsInitialized);
            }
        }

        private async Task CreateDevicesAsync()
        {
            try
            {
                // Clear device list
                this.deviceList.Clear();

                // Create device identity registry manager from the connection string
                this.cancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = this.cancellationTokenSource.Token;
                RegistryManager registryManager = RegistryManager.CreateFromConnectionString(this.txtConnectionString.Text);
                this.toolStripProgressBar.Minimum = 0;
                this.toolStripProgressBar.Maximum = this.txtDeviceCount.IntegerValue;
                this.toolStripProgressBar.Value = 0;
                int added = 0;

                this.WriteToLog(CreatingDevices);
                List<Task> taskList = new List<Task>();
                for (int i = 1; i <= this.txtDeviceCount.IntegerValue; i++)
                {
                    Device device;
                    string deviceId = $"device{i:000}";
                    taskList.Add(
                        Task.Run(
                            async () =>
                            {
                                try
                                {
                                    device = await registryManager.AddDeviceAsync(new Device(deviceId), cancellationToken);
                                    this.WriteToLog(
                                        $"Device [{deviceId}] successfully created in the device identity registry with key [{device.Authentication.SymmetricKey.PrimaryKey}]");
                                }
                                catch (DeviceAlreadyExistsException)
                                {
                                    // if the device already exists in the device identity registry, retrieve it
                                    device = await registryManager.GetDeviceAsync(deviceId, cancellationToken);
                                    this.WriteToLog(
                                        $"Device [{deviceId}] already exists in the device identity registry with key [{device.Authentication.SymmetricKey.PrimaryKey}]");
                                }
                                this.UpdateProgressBar(++added);
                                this.deviceList.Add(device);
                            },
                            cancellationToken).ContinueWith(
                                t =>
                                {
                                    if (t.IsFaulted && t.Exception != null)
                                    {
                                        this.HandleException(t.Exception);
                                    }
                                },
                                cancellationToken));
                    await Task.WhenAll(taskList);
                }
                if (!cancellationToken.IsCancellationRequested)
                {
                    this.WriteToLog(DevicesCreated);
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private async Task RemoveDevicesAsync()
        {
            try
            {
                // Create device identity registry manager from the connection string
                this.cancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = this.cancellationTokenSource.Token;
                RegistryManager registryManager = RegistryManager.CreateFromConnectionString(this.txtConnectionString.Text);
                this.toolStripProgressBar.Minimum = 0;
                this.toolStripProgressBar.Maximum = this.txtDeviceCount.IntegerValue;
                this.toolStripProgressBar.Value = 0;
                int removed = 0;
                List<Task> taskList = new List<Task>();

                this.WriteToLog(RemovingDevices);
                for (int i = 1; i <= this.txtDeviceCount.IntegerValue; i++)
                {
                    string deviceId = $"device{i:000}";
                    taskList.Add(
                        Task.Run(
                            async () =>
                            {
                                try
                                {
                                    await registryManager.RemoveDeviceAsync(deviceId, cancellationToken);
                                    this.WriteToLog($"Device [{deviceId}] successfully removed from the device identity registry.");
                                }
                                catch (DeviceNotFoundException)
                                {
                                    this.WriteToLog($"Device [{deviceId}] does not exist in the device identity registry.");
                                }
                                this.UpdateProgressBar(++removed);
                            },
                            cancellationToken).ContinueWith(
                                t =>
                                {
                                    if (t.IsFaulted && t.Exception != null)
                                    {
                                        this.HandleException(t.Exception);
                                    }
                                },
                                cancellationToken));
                }
                await Task.WhenAll(taskList);
                if (!cancellationToken.IsCancellationRequested)
                {
                    this.WriteToLog(DevicesRemoved);
                }
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions != null && ex.InnerExceptions.Any())
                {
                    foreach (Exception exception in ex.InnerExceptions)
                    {
                        this.HandleException(exception);
                    }
                }

                // Change button text
                this.btnStart.Text = Start;
            }
            catch (Exception ex)
            {
                this.HandleException(ex);

                // Change button text
                this.btnStart.Text = Start;
            }
        }

        private async Task InitializeDevicesAsync()
        {
            Dictionary<string, List<Tuple<string, string>>> manufacturerDictionary = new Dictionary<string, List<Tuple<string, string>>>
            {
                {
                    "Contoso", new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>("TS1", "Temperature Sensor"),
                        new Tuple<string, string>("TS2", "Temperature Sensor")
                    }
                }
                ,
                {
                    "Fabrikam", new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>("HS1", "Humidity Sensor"),
                        new Tuple<string, string>("HS2", "Humidity Sensor")
                    }
                }
            };

            Dictionary<string, List<string>> siteDictionary = new Dictionary<string, List<string>>
            {
                {
                    "Italy",
                    new List<string> {"Milan", "Rome", "Turin"}
                },
                {
                    "Germany",
                    new List<string> {"Munich", "Berlin", "Amburg"}
                },
                {
                    "UK",
                    new List<string> {"London", "Manchester", "Liverpool"}
                },
                {
                    "France",
                    new List<string> {"Paris", "Lion", "Nice"}
                }
            };
            List<PayloadEntities.Device> list = new List<PayloadEntities.Device>();

            // Prepare device data
            for (int i = 1; i <= this.txtDeviceCount.IntegerValue; i++)
            {
                int m = this.random.Next(0, manufacturerDictionary.Count);
                int d = this.random.Next(0, manufacturerDictionary.Values.ElementAt(m).Count);
                string model = manufacturerDictionary.Values.ElementAt(m)[d].Item1;
                string type = manufacturerDictionary.Values.ElementAt(m)[d].Item2;
                int s = this.random.Next(0, siteDictionary.Count);
                int c = this.random.Next(0, siteDictionary.Values.ElementAt(s).Count);

                list.Add(
                    new PayloadEntities.Device
                    {
                        DeviceId = i,
                        Name = $"Device {i}",
                        MinThreshold = this.txtMinValue.IntegerValue,
                        MaxThreshold = this.txtMaxValue.IntegerValue,
                        Manufacturer = manufacturerDictionary.Keys.ElementAt(m),
                        Model = model,
                        Type = type,
                        City = siteDictionary.Values.ElementAt(s)[c],
                        Country = siteDictionary.Keys.ElementAt(s)
                    });
            }

            // Create HttpClient object used to send events to the event hub.
            HttpClient httpClient = new HttpClient
            {
                BaseAddress = new Uri(this.cboDeviceManagementServiceUrl.Text)
            };
            httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string json = JsonConvert.SerializeObject(list);

            // Create HttpContent
            StringContent postContent = new StringContent(json, Encoding.UTF8, "application/json");
            this.WriteToLog(InitializingDeviceActors);
            HttpResponseMessage response = await httpClient.PostAsync(Combine(httpClient.BaseAddress.AbsoluteUri, "api/devices/set"), postContent);
            response.EnsureSuccessStatusCode();
            this.WriteToLog(DeviceActorsInitialized);
        }

        private void grouperManagementService_CustomPaint(PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(
                new Pen(SystemColors.ActiveBorder, 1),
                this.cboDeviceManagementServiceUrl.Location.X - 1,
                this.cboDeviceManagementServiceUrl.Location.Y - 1,
                this.cboDeviceManagementServiceUrl.Size.Width + 1,
                this.cboDeviceManagementServiceUrl.Size.Height + 1);
        }

        #endregion
    }
}