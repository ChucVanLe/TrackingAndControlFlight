// Copyright (c) LeVanChuc. All rights reserved.
//using Bing.Maps;

//Class chứa chương trình
//using DoAn;
using System;
using System.Collections;
using System.Collections.Generic;
//UART

using System.Collections.ObjectModel;
//__________________________________

using System.ComponentModel;
using System.Data;
using System.Diagnostics;
//**********************************************************************
//**********************************************************************


using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
//Khong sai duoc open cv trong universal app

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
//_____________________________________
using System.Windows.Input;
//khong the add system.drawing
//using System.Drawing;
//add win2d.uwp
//khong the add duong thang trong win2d vao map


using Windows.ApplicationModel.Activation;
using Windows.Devices.Enumeration;
using Windows.Devices.Geolocation;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
//khong mo duoc thu vien .NET
//_______________________________
//using Esri.ArcGISRuntime.Layers;
//khong co chuc nang graphic
//khong co add truc tiep string vao map
//khong ve duong tron tu do vao map

using Windows.Services.Maps;
//using OpenCvSharp;
//khong the sai opencv Sharp trong project nay


using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TrackingAndControlFlight_v1_0809
{
    //Biến các giá trị từ sensor
    public class DataFromSensor
    {
        public string Acc { get; set; }
        public string Time { get; set; }
        public string Latitude { get; set; }
        public string Longtitude { get; set; }
        public string Speed { get; set; }
        public string Altitude { get; set; }
        public string Angle { get; set; }
        public string Temp { get; set; }
        public string DataAcc { get; set; }
        public string Roll { get; set; }
        public string Pitch { get; set; }
        public string Yaw { get; set; }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //global variable

        double dDistanToTaget;  //Save distan from flight to dentination
        double dLatGol, dLonGol;      //2 biến này là biến toàn cục của Lat and Lontitude
        //Geopoint for Seattle San Bay Tan Son Nhat: 10.818442, 106.658824
        public double dLatDentination = 10.818442, dLonDentination = 106.658824;
        Int16 i16EditPosition = -60;//để canh chỉnh màn hình các sensor chỉ hiện 1/3 màn hình
        //1366 x 768 --> 1280 x 800
        //double dConvertToTabletX = 1366 - 1280, dConvertToTabletY = 768 - 800;
        double dConvertToTabletX = 0, dConvertToTabletY = 0;

        Windows.Storage.StorageFolder storageFolder;
        Windows.Storage.StorageFile sampleFile;

        //Cac bien toan cuc dung cho chuong trinh UART 
        //Var in UART
        string strDataFromSerialPort = "";
        bool bConnectOk = false;
        private SerialDevice serialPort = null;
        DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;
        private ObservableCollection<DeviceInformation> listOfDevices;
        private CancellationTokenSource ReadCancellationTokenSource;

        private DispatcherTimer timer;
        //Tạo một mảng để lưu các Acc từ hàng 1 đến hàng 10
        //Biến này lưu Data Acc thành mảng từ DataAcc[0] đến DataAcc[9]
        DataFromSensor Data = new DataFromSensor();
        //contain all position which flighr across there
        List<BasicGeoposition> positions = new List<BasicGeoposition>();

        //end of global variable

        /// <summary>
        /// fisrt scan when program start
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            //*************************************************************************
            //Ngày 29/02/2016 Software Artchitecture
            //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            Dis_Setup();
        }

        /// <summary>
        /// Test function delay
        /// </summary>
        private async void WaitForFiveSeconds()
        {
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(20));
            // do something after 5 seconds!
        }

        //*****************************************************************
        /*
        Class Set up
        */

        /// <summary>
        /// Set up Map, Timer, Screen
        /// </summary>
        public void Dis_Setup()
        {
            Dis_Setup_MapOffline();
            //C: \Users\VANCHUC - PC\AppData\Local\Packages\54fa2b45 - b04f - 4b40 - 809b - 7556c7ed473f_pq4mhrhe9d4xp\LocalState
            //Save_Setup();

            Dis_Setup_UART();

            //Dis_Setup_Timer();

            //Set up Display data of sensor, Set up position of all component
            DisplaySensor_Setup();

            //complete setup
            bSetup = true;


        }

        /// <summary>
        /// Set up MapOffline
        /// </summary>
        public void Dis_Setup_MapOffline()
        {
            myMap.Loaded += MyMap_Loaded;
            //Hien toa do luc nhan chuot trai
            myMap.MapTapped += MyMap_MapTapped;
            //change heading
            myMap.HeadingChanged += MyMap_HeadingChanged;
            myMap.ZoomLevelChanged += MyMap_ZoomLevelChanged;

        }

        /// <summary>
        /// Set up UART
        /// </summary>
        public void Dis_Setup_UART()
        {
            Init_UART();
        }

        /// <summary>
        /// Set up timer read data and show data
        /// </summary>
        public void Dis_Setup_Timer()
        {
            //Set up timer Read Data, period = 1ms
            //InitTimerReadData(2);
            //Set up timer Show Data, period = 500ms

            //InitTimerShowData(1000);//Timer này để hiển thị data lên Compass, Speed, Altitude, Roll And Pitch Angle

            //Dữ liệu cập nhật liên tục nhưng chỉ hiện thị sau mỗi 0,5s
            //Vì gia tốc thay đổi nhanh nên ta cần hiển thị sau mỗi 0.1s
            //InitTimerReadFile(1);

            //ListPortInput(3000);

        }

        /// <summary> check 02/03/16: OK
        /// Khoi tao bien cho ham save and write header
        /// </summary>
        public async void Save_Setup()
        {
            storageFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;
            sampleFile =
                await storageFolder.CreateFileAsync("dataReceive.txt",
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);
            //write header
            SaveTotxt("Data from sensor Ublox GPS + compass, baud rate: 57600" + '\n');
            SaveTotxt("Test data: 2/3/2016, Location: ... " + '\n');
        }
        //***************End of class set up********************************************
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        //*************Class inside class set up****************************************
        /// <summary>
        /// Load map offline
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyMap_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            myMap.Center =
               new Geopoint(new BasicGeoposition()
               {
                   //Geopoint for Seattle San Bay Tan Son Nhat:   dLatDentination, dLonDentination

                   Latitude = dLatDentination,
                   Longitude = dLonDentination
               });
            myMap.ZoomLevel = 12;
            myMap.Style = MapStyle.Road;


        }
        /// <summary>
        /// Hiện tọa độ tại điểm ta nhấn chuột
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MyMap_MapTapped(Windows.UI.Xaml.Controls.Maps.MapControl sender, Windows.UI.Xaml.Controls.Maps.MapInputEventArgs args)
        {
            var tappedGeoPosition = args.Location.Position;
            //string status = "MapTapped at \nLatitude:" + tappedGeoPosition.Latitude + "\nLongitude: " + tappedGeoPosition.Longitude;

            //Show  MapTap to textox
            tblock_LatAndLon.Text = Math.Round(tappedGeoPosition.Latitude, 8).ToString()//Lấy 8 chữ số thập phân
                                    + ", " + Math.Round(tappedGeoPosition.Longitude, 8).ToString();//Lấy 8 chữ số thập phân
            //NotifyUser(status, NotifyType.StatusMessage);
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Rorate needle when change heading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MyMap_HeadingChanged(Windows.UI.Xaml.Controls.Maps.MapControl sender, object args)
        {

            ///////////////////////////////////////////////
            Rotate_Needle(myMap.Heading);
        }

        /// <summary>
        /// when change zoom level, tblock_ZoomLevel will show zoom level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MyMap_ZoomLevelChanged(Windows.UI.Xaml.Controls.Maps.MapControl sender, object args)
        {

            tblock_ZoomLevel.Text = Math.Round(myMap.ZoomLevel, 3).ToString();
            img_AtLatAndLon.Height = 5 * myMap.ZoomLevel;
            img_AtLatAndLon.Width = 5 * myMap.ZoomLevel;
            myMap.Children.Remove(img_AtLatAndLon);
            myMap.Children.Add(img_AtLatAndLon);
        }

        //Ngày 03/12/2015 22h27 đã hoàn thành đọc UART từ serial port
        //Tách dữ liệu và lấy các thông số liên quan
        //chỉ lấy 1 dòng gia tốc trong 10 dòng gia tốc trong 100ms
        //Cứ 100ms có 1 mẫu dữ liệu gồm gia tốc, thơi gian, lat, long, alt, angle
        //***********************************************************************
        //*********************************************************************
        //Ngay 02/12/2015****************************************************
        //*******************Read And Display Data***************************
        //https://ms-iot.github.io/content/en-US/win10/samples/SerialSample.htm

        //https://msdn.microsoft.com/en-us/library/windows.devices.serialcommunication.serialdevice.aspx

        /// <summary>
        /// Config UART
        /// </summary>
        public void Init_UART()
        {
            //ListBox_Com.IsEnabled = false;
            //sendTextButton.IsEnabled = false;
            listOfDevices = new ObservableCollection<DeviceInformation>();
            ListAvailablePorts();
        }

        /// <summary>
        /// Timer to Show data to Compass, Alt, Speed, Roll, Pitch,.. 
        /// </summary>
        private void InitTimerShowData(double dPeriod)
        {
            // Start the polling timer.
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(dPeriod) };
            timer.Tick += TimerShowData;
            timer.Start();

        }
        //*******************************************************************
        /// <summary>
        /// Liệt kê các port đang connect với máy tính
        /// </summary>
        /// <param name="dPeriod"></param>
        private void ListPortInput(double dPeriod)
        {

            // Start the polling timer.
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(dPeriod) };
            timer.Tick += listPort;
            timer.Start();

        }

        /// <summary>
        /// list com is connecting with pc
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void listPort(object sender, object e)
        {
            ListAvailablePorts();
        }

        //*****************************************************************
        //Hàm set up tòan bộ cảm biến
        /// <summary>
        /// Set up vị trí hiển thị của cảm biến
        /// </summary>
        void DisplaySensor_Setup()
        {

            Background_Sensor(480, -80);//da can chinh 1/3 full screen

            //Image full
            //Da can chinh 1/3
            AirSpeed_Image_full_Setup(100.1, 150 - 32 + i16EditPosition, 80 + 125);//ok
            Draw_Airspeed_full_optimize(0, 150 - 32 + i16EditPosition, 205);//ok500, 120
                                                                            //Speed_Image_Setup(100, 150, 100);
                                                                            //Da can chinh 1/3
            PitchAndRoll_Setup(0, 0, 350 + i16EditPosition * 11 / 6, 210, 140, 50);//ok
            PitchAndRoll_Draw(0, 0, 350 + i16EditPosition * 11 / 6, 210, 140, 50);//ok


            //Vẽ hình Altitude
            //Altitude_Image_Setup(00, 550, 100); //đã vẽ xong lúc 1h25 13/3/2016
            //Da can chinh 1/3
            Alttitude_Image_full_Setup(100.5, 550 + 88 / 2 + i16EditPosition * 17 / 6, 80);//ok
            Draw_Alttitude_full_optimize(0, 550 + 88 / 2 + i16EditPosition * 17 / 6, 80);//ok
            //Da can chinh 1/3
            ComPass_Setup_Rotate_Out(0, 350 + i16EditPosition * 11 / 6, 500, 120);//quay phần phía ngoài ok
            //Da can chinh 1/3
            //VerticalSpeed_Setup(0, 550 + i16EditPosition * 17 / 6, 420);

            //set up quy dao
            Draw_Trajectory_And_Flight(dLatGol, dLonGol,
                        Convert.ToDouble(Data.Altitude), Convert.ToDouble("0"));//ok


            //Set up Show distan
            ShowDistance(0, 0, dDistanToTaget.ToString() + " Meter", 30 * myMap.ZoomLevel / 22, dLatGol, dLonGol, 1);//Purple

            ///////////////////////////////////////////////////////////////////
            //Add needle
            AddNeedle(screenWidth - 35, screenHeight - 50);//screenWidth

        }
        //*************End Of Class inside class set up****************************************

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        //*************Class inside inside class set up****************************************

        //UART*********************************************************************************
        /// <summary>
        /// ListAvailablePorts
        /// - Use SerialDevice.GetDeviceSelector to enumerate all serial devices
        /// - Attaches the DeviceInformation to the ListBox source so that DeviceIds are displayed
        /// </summary>
        private async void ListAvailablePorts()
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);

                //status.Text = "Select a device and connect";
                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Remove(dis[i]);
                }
                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                }

                DeviceListSource.Source = listOfDevices;
                //comPortInput.IsEnabled = true;
                ConnectDevices.SelectedIndex = -1;
            }
            catch
            {
                //status.Text = ex.Message;
            }
        }


        /// <summary>
        /// Connect with Com Serial port
        /// </summary>
        private async void Connect_To_Com()
        {
            var selection = ConnectDevices.SelectedItems;

            if (selection.Count <= 0)
            {
                //status.Text = "Select a device and connect";
                return;
            }

            DeviceInformation entry = (DeviceInformation)selection[0];

            try
            {
                serialPort = await SerialDevice.FromIdAsync(entry.Id);

                // Disable the 'Connect' button 
                //comPortInput.IsEnabled = false;

                // Configure serial settings
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1);
                serialPort.BaudRate = 57600;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;

                //Connect is successfull
                bConnectOk = true;
                // Create cancellation token object to close I/O operations when closing the device
                ReadCancellationTokenSource = new CancellationTokenSource();

                // Enable 'WRITE' button to allow sending data
                //sendTextButton.IsEnabled = true;

                bt_Connect.IsEnabled = false;
                bt_DisConnect.IsEnabled = true;
                Listen();
            }
            catch
            {
                //status.Text = ex.Message;
                bt_Connect.IsEnabled = true;
                bt_List_Com.IsEnabled = true;
            }
        }

        /// <summary>
        /// DisConnect with Com Serial port
        /// </summary>
        private void DisConnect_To_Com()
        {
            try
            {

                CancelReadTask();
                CloseDevice();
                ListAvailablePorts();

                //comPortInput.Content = "Connect";
                bt_Connect.IsEnabled = true;
                bt_DisConnect.IsEnabled = false;
                bConnectOk = false;
            }
            catch
            {
            }
        }
        //**************************************************************************
        /// <summary>
        /// sendTextButton_Click: Action to take when 'WRITE' button is clicked
        /// - Create a DataWriter object with the OutputStream of the SerialDevice
        /// - Create an async task that performs the write operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void sendTextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (serialPort != null)
                {
                    // Create the DataWriter object and attach to OutputStream
                    dataWriteObject = new DataWriter(serialPort.OutputStream);

                    //Launch the WriteAsync task to perform the write
                    await WriteAsync();
                }
                else
                {
                    //status.Text = "Select a device and connect";
                }
            }
            catch
            {
                //status.Text = "sendTextButton_Click: " + ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (dataWriteObject != null)
                {
                    dataWriteObject.DetachStream();
                    dataWriteObject = null;
                }
            }
        }
        //**************************************************************************
        /// <summary>
        /// WriteAsync: Task that asynchronously writes data from the input text box 'sendText' to the OutputStream 
        /// </summary>
        /// <returns></returns>
        private async Task WriteAsync()
        {
            Task<UInt32> storeAsyncTask;

            //if (sendText.Text.Length != 0)
            {
                // Load the text from the sendText input text box to the dataWriter object
                //dataWriteObject.WriteString(sendText.Text);

                // Launch an async task to complete the write operation
                storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

                UInt32 bytesWritten = await storeAsyncTask;
                if (bytesWritten > 0)
                {
                    //status.Text = sendText.Text + ", ";
                    //status.Text += "bytes written successfully!";
                }
                //sendText.Text = "";
            }
            //else
            {
                //status.Text = "Enter the text you want to write and then click on 'WRITE'";
            }
        }
        //**************************************************************************
        //dem so frame loi
        int errorFrame = 0;
        /// <summary>
        /// - Create a DataReader object
        /// - Create an async task to read from the SerialDevice InputStream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);

                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Name == "TaskCanceledException")
                {
                    //status.Text = "Reading task was cancelled, closing device and cleaning up";
                    CloseDevice();
                }
                else
                {
                    //status.Text = ex.Message;
                    //loi frame
                }

            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }

        /// <summary>
        /// ReadAsync: Task that waits on data and reads asynchronously from the serial device InputStream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;//savedatatoTxtFile để lưu data --> .txt
            //nếu bộ đệm lớn sẽ có thời gian trễ lớn, nhưng dữ liệu không mất
            uint ReadBufferLength = 1;//2000 char

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            // Create a task object to wait for data on the serialPort.InputStream
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            //save --> .txt
            //savedatatoTxtFile = dataReaderObject.LoadAsync(2000).AsTask(cancellationToken);

            // Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask;
            //save
            //UInt32 bytesSave = await savedatatoTxtFile;

            //if (bytesSave > 0)
            //{
            //    rcvdText.Text = dataReaderObject.ReadString(bytesRead);
            //    strDataFromSerialPort += rcvdText.Text;
            //    //dataReaderObject.r
            //    //status.Text = "bytes read successfully!";

            //    //Save data to .txt file
            //    SaveTotxt(rcvdText.Text);

            //}
            //Process and Save
            //string sTemp = "";//dung để process and save
            //byte checkerror;
            if (bytesRead > 0)
            {
                byte[] data_check_error = new byte[bytesRead];
                byte[] data_right = new byte[bytesRead];
                Int16 temp_index_data_right = 0;
                try
                {
                    //sTemp = dataReaderObject.ReadString(bytesRead);
                    //strDataFromSerialPort += dataReaderObject.ReadString(bytesRead);
                    //processDataToDrawTrajactory();                                                                                                                                                                                                    

                    //check error, char > 127 => not string
                    dataReaderObject.ReadBytes(data_check_error);
                    for (int temp_index = 0; temp_index < bytesRead; temp_index++)
                    {
                        if (data_check_error[temp_index] < 127)
                        {
                            data_right[temp_index_data_right] = data_check_error[temp_index];
                            temp_index_data_right++;
                        }
                    }
                    strDataFromSerialPort += System.Text.Encoding.UTF8.GetString(data_right);
                    processDataToDrawTrajactory();
                }
                catch (Exception ex)
                {

                    errorFrame += 1;
                    tblock_Current_Timer.Text = "frame error: " + strDataFromSerialPort + ", Error: " + ex.Message + "No Error: " + errorFrame.ToString();

                }
                //
                //SaveTotxt(sTemp);


            }
        }
        //****************************************************************************
        /// <summary>
        /// CancelReadTask:
        /// - Uses the ReadCancellationTokenSource to cancel read operations
        /// </summary>
        private void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

        /// <summary>
        /// CloseDevice:
        /// - Disposes SerialDevice object
        /// - Clears the enumerated device Id list
        /// </summary>
        private void CloseDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;

            bt_List_Com.IsEnabled = true;
            //sendTextButton.IsEnabled = false;
            //rcvdText.Text = "";
            listOfDevices.Clear();
        }
        //***************************************************************************

        //***************************************************************************
        //Receice Data
        private async void ReceiveData_Click(object sender, RoutedEventArgs e)
        {
            // read the data

            DataReader dreader = new DataReader(serialPort.InputStream);
            uint sizeFieldCount = await dreader.LoadAsync(sizeof(uint));
            if (sizeFieldCount != sizeof(uint))
            {
                return;
            }

            uint stringLength;
            uint actualStringLength;

            try
            {
                stringLength = dreader.ReadUInt32();
                actualStringLength = await dreader.LoadAsync(stringLength);

                if (stringLength != actualStringLength)
                {
                    return;
                }
                string text = dreader.ReadString(actualStringLength);

                //message.Text = text;

            }
            catch
            {
                //errorStatus.Visibility = Visibility.Visible;
                //errorStatus.Text = "Reading data from Bluetooth encountered error!" + ex.Message;
            }


        }
        //End of all function of UART**************************************************************

        /// <summary>
        /// add icon to map
        /// </summary>
        public void Add_Icon_MyHome()
        {
            MapIcon icon = new MapIcon();
            Geopoint pointCenter = new Geopoint(new BasicGeoposition()
            {
                //Tọa độ nhà Lê Văn Chức
                Latitude = 15.235057,
                Longitude = 108.742786,
                // Altitude = 200.0
            });
            //icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/MyHome.png"));
            icon.Location = pointCenter;

            icon.CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible;

            icon.Title = "My Home";

            myMap.MapElements.Add(icon);

        }

        /// <summary>
        /// convert deg to rad
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        private static double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180D);
        }
        /***********************************************/

        /*************************Ngay 21/11/2015*******************************/
        //Calculate distance in map
        /*
         * Calculate distance between two points in latitude and longitude taking
         * into account height difference. If you are not interested in height
         * difference pass 0.0. Uses Haversine method as its base.
         * 
         * lat1, lon1 Start point lat2, lon2 End point el1 Start altitude in meters
         * el2 End altitude in meters
         * @returns Distance in Meters
         */
        /// <summary>
        /// caculator between 2 point
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lon1"></param>
        /// <param name="el1"></param>
        /// <param name="lat2"></param>
        /// <param name="lon2"></param>
        /// <param name="el2"></param>
        /// <returns></returns>
        public static Int32 distance(double lat1, double lon1, double el1,
        double lat2, double lon2, double el2)
        {

            //Int16 R = 6371; // Radius of the earth unit km
            //Do trai đất elip nên để chính xác lấy
            Int32 R = 6372803;

            double latDistance = ToRad(lat2 - lat1);//convert degree to radian
            double lonDistance = ToRad(lon2 - lon1);

            double a = Math.Sin(latDistance / 2) * Math.Sin(latDistance / 2)
                    + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                    * Math.Sin(lonDistance / 2) * Math.Sin(lonDistance / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            //double distance = R * c * 1000; // convert to meters
            double distance = R * c;
            double height = el1 - el2;

            distance = Math.Pow(distance, 2) + Math.Pow(height, 2);

            return (Int32)Math.Sqrt(distance);
        }
        //**************************************************************************
        //Show distance
        //*****************************************************************
        //Ngày 20/12/2015 bước đột phá tạo một mảng TextBlock Auto remove
        TextBlock[] Tb_ShowDistance = new TextBlock[2];
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// Set up init location for string
        /// index: index of string
        /// Roll góc nghiêng của string
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void ShowDistance(int index, double Roll, string drawString, double SizeOfText,
            double lat, double lon, double Opacity)
        {
            //create graphic text block design text
            //TextBlock Tb_ShowDistance[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_ShowDistance[index].Height = HeightOfBlock;
            //Tb_ShowDistance[index].Width = WidthOfBlock;
            //canh lề, left, right, center
            myMap.Children.Remove(Tb_ShowDistance[index]);
            Tb_ShowDistance[index] = new TextBlock();
            Tb_ShowDistance[index].HorizontalAlignment = HorizontalAlignment.Left;
            Tb_ShowDistance[index].VerticalAlignment = VerticalAlignment.Top;
            //Tb_ShowDistance[index].Margin = 
            //
            //đảo chữ
            Tb_ShowDistance[index].TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            Tb_ShowDistance[index].Text = drawString;
            Tb_ShowDistance[index].FontSize = SizeOfText;
            Tb_ShowDistance[index].FontFamily = new FontFamily("Arial");
            //Tb_ShowDistance[index].FontStyle = "Arial";
            //Tb_ShowDistance[index].FontStretch
            //color text có độ đục
            //Tb_ShowDistance[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            //Tb_ShowDistance[index].Foreground = Blush;
            Tb_ShowDistance[index].Foreground = new SolidColorBrush(Colors.Red);
            Tb_ShowDistance[index].Opacity = Opacity;
            //Quay Textblock để quay chữ
            Tb_ShowDistance[index].RenderTransform = new RotateTransform()
            {
                Angle = Roll + 180,
                //CenterX = 25, //The prop name maybe mistyped 
                //CenterY = 25 //The prop name maybe mistyped 
            };
            //position of text left, top, right, bottom
            //Tb_ShowDistance[index].Margin = new Windows.UI.Xaml.Thickness(100, 20, 0, 0);
            //BackgroundDisplay.Children.Add(Tb_ShowDistance[index]);

            //Đặt theo tọa độ
            //Tan Son Nhat Airport dLatDentination, dLonDentination
            Geopoint Position = new Geopoint(new BasicGeoposition()
            {
                //dLatDentination, dLonDentination
                //Latitude = dLatDentination,
                //Longitude = dLonDentination,
                //Altitude = 200.0
                //Latitude = dLatDentination + (lat - dLatDentination) * (sliderAdjSpeed.Value / 1000 + 0.9),
                //Longitude = dLonDentination + (lon - dLonDentination) * (sliderAdjSpeed.Value / 1000 + 0.9),

                //C2
                //Latitude = dLatDentination + (lat - dLatDentination) * (0.0169 * myMap.ZoomLevel + 0.6597),
                //Longitude = dLonDentination + (lon - dLonDentination) * (0.0169 * myMap.ZoomLevel + 0.6597),

                //C3: Quay chu them 180D
                Latitude = lat,
                Longitude = lon,
            });
            //myMap.Children.Add(bitmapImage);
            //Đặt đúng vị trí
            Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(Tb_ShowDistance[index], Position);
            myMap.Children.Add(Tb_ShowDistance[index]);

            //Show Zool Level
            //tb_ZoomLevel.Text = "Z Level: " + Math.Round(myMap.ZoomLevel, 3).ToString();
        }
        //**********************************************************************************************
        /// <summary>
        /// show distane, only show thing í esential
        /// </summary>
        /// <param name="index"></param>
        /// <param name="Roll"></param>
        /// <param name="drawString"></param>
        /// <param name="SizeOfText"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        public void ShowDistance_optimize(int index, double Roll, string drawString, double SizeOfText,
            double lat, double lon)
        {
            myMap.Children.Remove(Tb_ShowDistance[index]);
            Tb_ShowDistance[index].Text = drawString;
            Tb_ShowDistance[index].FontSize = SizeOfText;

            Tb_ShowDistance[index].RenderTransform = new RotateTransform()
            {
                Angle = Roll + 180,
                //CenterX = 25, //The prop name maybe mistyped 
                //CenterY = 25 //The prop name maybe mistyped 
            };

            Geopoint Position = new Geopoint(new BasicGeoposition()
            {

                Latitude = lat,
                Longitude = lon,
            });
            //myMap.Children.Add(bitmapImage);
            //Đặt đúng vị trí
            Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(Tb_ShowDistance[index], Position);

            myMap.Children.Add(Tb_ShowDistance[index]);
            //Show Zool Level
            //tb_ZoomLevel.Text = "Z Level: " + Math.Round(myMap.ZoomLevel, 3).ToString();
        }
        //**********************************************************************************************

        /// <summary>
        /// My home 15.235020N, 108.742780E 24.12m
        //wiew map 3D
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddMap3D(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //myMap.Heading = 10;
            myMap.Center =
               new Geopoint(new BasicGeoposition()
               {
                   //Geopoint for Seattle 
                   Latitude = 15.235057,
                   Longitude = 108.742786,
               });
            myMap.ZoomLevel = 10;
            myMap.Style = MapStyle.Road;
            //wiew map 3D 1000, 0, 90
            Geopoint point = new Geopoint(new BasicGeoposition()
            {
                Latitude = myMap.Center.Position.Latitude,
                Longitude = myMap.Center.Position.Longitude
            });
            await myMap.TrySetSceneAsync(MapScene.CreateFromLocationAndRadius(point, 1000, 0, 0));//0: phuong cua ban do, 0 là hướng bắc, 45 độ nghiêng
        }
        //******************Map 3D*************************
        /// <summary>
        /// Map 3D
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void display3DLocation(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (myMap.Is3DSupported)
            {
                // Set the aerial 3D view.
                myMap.Style = MapStyle.Aerial3DWithRoads;

                // Specify the location.
                BasicGeoposition hwGeoposition = new BasicGeoposition() { Latitude = 34.134, Longitude = -118.3216 };
                Geopoint hwPoint = new Geopoint(hwGeoposition);

                // Create the map scene.
                MapScene hwScene = MapScene.CreateFromLocationAndRadius(hwPoint,
                                                                                     80, /* show this many meters around */
                                                                                     0, /* looking at it to the North*/
                                                                                     60 /* degrees pitch */);
                // Set the 3D view with animation.
                await myMap.TrySetSceneAsync(hwScene, MapAnimationKind.Bow);
            }
            else
            {
                // If 3D views are not supported, display dialog.
                ContentDialog viewNotSupportedDialog = new ContentDialog()
                {
                    Title = "3D is not supported",
                    Content = "\n3D views are not supported on this device.",
                    PrimaryButtonText = "OK"
                };
                await viewNotSupportedDialog.ShowAsync();
            }
        }

        //draw line in map 2D
        //đường thẳng đưa vào là biến toàn cục
        Windows.UI.Xaml.Controls.Maps.MapPolyline mapPolyline = new Windows.UI.Xaml.Controls.Maps.MapPolyline();
        Windows.UI.Xaml.Controls.Maps.MapPolyline polylineHereToDentination = new Windows.UI.Xaml.Controls.Maps.MapPolyline();
        /// <summary>
        /// Ve duong thẳng nối (lat1, lon1) va (lat2, lon2)
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lon1"></param>
        /// <param name="lat2"></param>
        /// <param name="lon2"></param>
        void Map_DrawLine_2D(double lat1, double lon1,
        double lat2, double lon2)
        {

            //myMap.MapElements.Remove(mapPolyline);
            polylineHereToDentination.Path = new Geopath(new List<BasicGeoposition>() {
                new BasicGeoposition() {Latitude = lat1, Longitude = lon1},
                //San Bay Tan Son Nhat: dLatDentination, dLonDentination
                new BasicGeoposition() {Latitude = lat2, Longitude = lon2},
            });


        }
        //*****************************************
        /// <summary>
        /// route in 3D map
        /// </summary>
        public async void DrawRoute3DInMap()
        {
            Geopoint point1 = new Geopoint(new BasicGeoposition()
            {
                Latitude = myMap.Center.Position.Latitude,
                Longitude = myMap.Center.Position.Longitude,
                //Altitude = 200.0
            });
            Geopoint point2 = new Geopoint(new BasicGeoposition()
            {
                Latitude = myMap.Center.Position.Latitude + 0.001,
                Longitude = myMap.Center.Position.Longitude,
                //Altitude = 200.0
            });
            BasicGeoposition startLocation = new BasicGeoposition();
            startLocation.Latitude = 40.7517;
            startLocation.Longitude = -073.9766;
            Geopoint startPoint1 = new Geopoint(startLocation);

            // End at Central Park in New York City.
            BasicGeoposition endLocation1 = new BasicGeoposition();
            endLocation1.Latitude = 40.7669;
            endLocation1.Longitude = -073.9790;
            Geopoint endPoint1 = new Geopoint(endLocation1);

            // Get the route between the points.

            MapRouteFinderResult routeResult =
               await MapRouteFinder.GetDrivingRouteAsync(startPoint1, endPoint1,
                MapRouteOptimization.TimeWithTraffic,
                MapRouteRestrictions.None);
            // MapRouteFinderResult router = await MapRouteFinder.GetDrivingRouteAsync(;



            // Fit the MapControl to the route.

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                //MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Red;
                viewOfRoute.OutlineColor = Colors.Blue;

                // Add the new MapRouteView to the Routes collection
                // of the MapControl.
                myMap.Routes.Add(viewOfRoute);

                // Use the route to initialize a MapRouteView.
                viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Yellow;
                viewOfRoute.OutlineColor = Colors.Black;
                // Fit the MapControl to the route.
                await myMap.TrySetViewBoundsAsync(
                    routeResult.Route.BoundingBox,
                    null,
                    Windows.UI.Xaml.Controls.Maps.MapAnimationKind.None);

                // Add the new MapRouteView to the Routes collection
                // of the MapControl.

            }

            if (routeResult.Status == MapRouteFinderStatus.Success)


            {


                //(routeResult.Route.LengthInMeters / 1000);


            }

        }

        //****************************************************
        /// <summary>
        /// set route
        /// </summary>
        public async void SetRouteDirectionsBreda()
        {
            //ok
            string beginLocation = "ho chi minh";
            string endLocation = "Binh Duong";

            MapLocationFinderResult result = await MapLocationFinder.FindLocationsAsync(beginLocation, myMap.Center);
            MapLocation begin = result.Locations.First();

            result = await MapLocationFinder.FindLocationsAsync(endLocation, myMap.Center);
            MapLocation end = result.Locations.First();

            List<Geopoint> waypoints = new List<Geopoint>();
            waypoints.Add(begin.Point);
            // Adding more waypoints later
            waypoints.Add(end.Point);
            //Add point


            MapRouteFinderResult routeResult = await MapRouteFinder.GetDrivingRouteAsync(begin.Point, end.Point, MapRouteOptimization.Time, MapRouteRestrictions.None);

            //System.Diagnostics.Debug.WriteLine(routeResult.Status); // DEBUG

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Green;
                viewOfRoute.OutlineColor = Colors.Black;

                myMap.Routes.Add(viewOfRoute);

                await myMap.TrySetViewBoundsAsync(routeResult.Route.BoundingBox, null, MapAnimationKind.None);
            }
            else
            {
                // throw new Exception(routeResult.Status.ToString());
            }
        }
        ////****************************Ngay 22/11/2015************************
        //chỉ đường chỉ thực hiện online khi nhap vao dia diem
        //khi nhap toa do thi co the search offline
        //Chi đường chỉ làm được với 1 số địa điểm được đặt tên trên bản đồ như chi đường giữa 2 tỉnh, từ tp hcm

        /// <summary>
        /// đến sân bay nha trang "Sân Bay Nha Trang, Khanh Hoa, Vietnam"
        ///san bay tan son nhat "58 Truong Son, Ward 2, Tan Binh District Ho Chi Minh City  Ho Chi Minh City"
        ///endLocation2.Latitude = 10.772099;
        ///endLocation2.Longitude = 106.657693;
        ///San bay tan son nhat dLatDentination, dLonDentination
        ///san bay da nang 16.044040, 108.199357
        /// </summary>
        public async void SetRouteBetween2Point()
        {
            //ok
            string beginLocation = "ho chi minh";
            string endLocation = "ha noi";

            // End at Central in Binh Duong
            BasicGeoposition endLocation1 = new BasicGeoposition();
            endLocation1.Latitude = 11.216412;
            endLocation1.Longitude = 106.957936;
            Geopoint endPoint1 = new Geopoint(endLocation1);
            // End at Central in Tan Son Nhat InterNational AirPort: dLatDentination, dLonDentination
            BasicGeoposition endLocation2 = new BasicGeoposition();
            endLocation2.Latitude = 10.759860;
            endLocation2.Longitude = 106.92668;
            endLocation2.Altitude = 100;
            Geopoint endPoint2 = new Geopoint(endLocation2);
            //position: string
            MapLocationFinderResult result = await MapLocationFinder.FindLocationsAsync(beginLocation, myMap.Center);
            MapLocation begin = result.Locations.First();

            result = await MapLocationFinder.FindLocationsAsync(endLocation, myMap.Center);
            MapLocation end = result.Locations.First();

            List<Geopoint> waypoints = new List<Geopoint>();
            waypoints.Add(begin.Point);
            // Adding more waypoints later
            waypoints.Add(end.Point);

            MapRouteFinderResult routeResult = await MapRouteFinder.GetDrivingRouteAsync(begin.Point, end.Point, MapRouteOptimization.Distance, MapRouteRestrictions.Highways);
            //test show point
            //tb_ZoomLevel.Text = begin.Point.Position.Latitude.ToString() + "  "
            //                    + begin.Point.Position.Longitude.ToString();
            //System.Diagnostics.Debug.WriteLine(routeResult.Status); // DEBUG

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Green;
                viewOfRoute.OutlineColor = Colors.Black;

                myMap.Routes.Add(viewOfRoute);

                await myMap.TrySetViewBoundsAsync(routeResult.Route.BoundingBox, null, MapAnimationKind.None);
            }
            else
            {
                // throw new Exception(routeResult.Status.ToString());
            }
        }

        /// <summary>
        /// map 3D
        /// </summary>
        private async void showMap3D_Aerial3DWithRoads()
        {
            myMap.Style = MapStyle.Aerial3DWithRoads;
            //wiew map 3D 1000, 0, 90
            Geopoint point = new Geopoint(new BasicGeoposition()
            {
                Latitude = myMap.Center.Position.Latitude,
                Longitude = myMap.Center.Position.Longitude
            });
            await myMap.TrySetSceneAsync(MapScene.CreateFromLocationAndRadius(point, 1000, 0, 80));//0: phuong cua ban do, 0 là hướng bắc, 45 độ nghiêng
                                                                                                   // DrawRoute3DInMap();
        }
        //***********************************************
        /// <summary>
        /// show 3D map
        /// </summary>
        private async void showMap3D_Roads()
        {
            //myMap.Style = MapStyle.Aerial3DWithRoads;
            ////wiew map 3D 1000, 0, 90
            //Geopoint point = new Geopoint(new BasicGeoposition()
            //{
            //    Latitude = myMap.Center.Position.Latitude,
            //    Longitude = myMap.Center.Position.Longitude
            //});
            //await myMap.TrySetSceneAsync(MapScene.CreateFromLocationAndRadius(point, 1000, 0, 50));//0: phuong cua ban do, 0 là hướng bắc, 45 độ nghiêng
            //                                                                                       // DrawRoute3DInMap();

            if (myMap.Is3DSupported)
            {
                this.myMap.Style = MapStyle.Aerial3DWithRoads;

                BasicGeoposition spaceNeedlePosition = new BasicGeoposition();
                spaceNeedlePosition.Latitude = 47.6204;
                spaceNeedlePosition.Longitude = -122.3491;

                Geopoint spaceNeedlePoint = new Geopoint(spaceNeedlePosition);

                MapScene spaceNeedleScene = MapScene.CreateFromLocationAndRadius(spaceNeedlePoint,
                                                                                    400, /* show this many meters around */
                                                                                    0, /* looking at it to the south east*/
                                                                                    60 /* degrees pitch */);

                await myMap.TrySetSceneAsync(spaceNeedleScene);
            }
            else
            {
                //string status = "3D views are not supported on this device.";
                //rootPage.NotifyUser(status, NotifyType.ErrorMessage);
            }
        }
        bool Map3D = true;

        /// <summary>
        /// Show map 3d
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Map3D_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //showStreetsideView();
            //showMap3D_Roads();
            //Route();
            int Donghieng = 0;
            if (Map3D)
            {
                Map3D = false;
                //BtMap3D.Content = "2D_Mode";
                Donghieng = 60;
            }
            else
            {
                //BtMap3D.Content = "3D_Mode";
                Map3D = true;
                Donghieng = 0;
                //myMap.Style = MapStyle.Road;
            }
            if (myMap.Is3DSupported)
            {
                this.myMap.Style = MapStyle.Road;

                BasicGeoposition spaceNeedlePosition = new BasicGeoposition();
                spaceNeedlePosition.Latitude = dLatDentination;
                spaceNeedlePosition.Longitude = dLonDentination;

                Geopoint spaceNeedlePoint = new Geopoint(spaceNeedlePosition);


                MapScene spaceNeedleScene = MapScene.CreateFromLocationAndRadius(spaceNeedlePoint,
                                                                                    400, /* show this many meters around */
                                                                                    0, /* looking at it to the south east*/
                                                                                    Donghieng /* degrees pitch */);

                await myMap.TrySetSceneAsync(spaceNeedleScene);
            }
            else
            {
                //string status = "3D views are not supported on this device.";
                //rootPage.NotifyUser(status, NotifyType.ErrorMessage);
            }


        }

        //test map3D
        private async void showStreetsideView()
        {
            // Check if Streetside is supported.
            if (myMap.IsStreetsideSupported)
            {
                // Find a panorama near Avenue Gustave Eiffel.
                BasicGeoposition cityPosition = new BasicGeoposition() { Latitude = 48.858, Longitude = 2.295 };
                Geopoint cityCenter = new Geopoint(cityPosition);
                StreetsidePanorama panoramaNearCity = await StreetsidePanorama.FindNearbyAsync(cityCenter);

                // Set the Streetside view if a panorama exists.
                if (panoramaNearCity != null)
                {
                    // Create the Streetside view.
                    StreetsideExperience ssView = new StreetsideExperience(panoramaNearCity);
                    ssView.OverviewMapVisible = true;
                    myMap.CustomExperience = ssView;
                }
            }
            else
            {
                // If Streetside is not supported
                ContentDialog viewNotSupportedDialog = new ContentDialog()
                {
                    Title = "Streetside is not supported",
                    Content = "\nStreetside views are not supported on this device.",
                    PrimaryButtonText = "OK"
                };
                await viewNotSupportedDialog.ShowAsync();
            }
        }


        //ngày 28/11/2015 sử dụng Win2D.uwp
        //Chú ý muốn nhận cổng com của cường thì phải cài driver cho nó

        //******************************************************************************

        //xử lý data không dùng timer
        //optimize in 14/5/2016
        public void processDataToDrawTrajactory()
        {
            if (bConnectOk)
            {
                if (strDataFromSerialPort.IndexOf('\r') != -1)//Bắt ký tự $
                {
                    Data.Temp = FindTextInStr(strDataFromSerialPort, '\r');
                    processDataFull();
                }
            }
            else if (setupReadfile)//xu ly voi com
            {
                Data.Temp = strDataFromSerialPort;
                processDataFull();
            }
        }
        //******************************************************************************
        /// <summary>
        /// Process data full and draw data is optimize
        /// </summary>
        public void processDataFull()
        {
            {
                //Data.Temp = FindTextInStr(strDataFromSerialPort, "\r\n");
                //if (Data.Temp.IndexOf('$') != -1)
                try
                {

                    //cắt bỏ ký tự '$'
                    Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf('$') + 1, Data.Temp.Length - (Data.Temp.IndexOf('$') + 1));
                    //data acc
                    if ((Data.Temp[0] != 'G') && (Data.Temp.Length >= 18)) //Do có lúc chỉ có 1 hàng trắng
                    {
                        //Mặc định không lưu dữ liệu không hiểu thị các giá trị ra textbox
                        //-0011  0084 - 0040 - 0003  0012 - 0007 - 0143 - 0016  0976  2687  0175  2041  0849

                        Data.DataAcc = Data.Temp;
                        //Lấy Roll, mỗi giá trị góc dài 6 ký tự
                        //Data của Anh Bình có xuất hiện chỗ trục trặt nôi chặn lỗi này
                        //

                        Data.Roll = Data.DataAcc.Substring(0, 6);
                        Data.Pitch = Data.DataAcc.Substring(6, 6);
                        Data.Yaw = Data.DataAcc.Substring(12, 6);

                        //vẽ luôn
                        if (bSetup)
                        {
                            //dùng các biến tạm để kiểm tra có convert được hay không
                            double temp_Roll = 0, temp_Pitch = 0, temp_Raw = 0;
                            try
                            {
                                temp_Roll = Convert.ToDouble(Data.Roll) / 10;
                                temp_Pitch = Convert.ToDouble(Data.Pitch) / 10;
                                temp_Raw = Convert.ToDouble(Data.Yaw) / 10;

                                //Vẽ sự thay đổi PitchAndRoll_Draw đã tối ưu ngày 3,4 /03/2016                       
                                PitchAndRoll_Draw(temp_Roll, temp_Pitch, 350 + i16EditPosition * 11 / 6, 210, 140, 50);
                                //optimite not user remove and add
                                Comp_Rotate_OutAndAddValue(temp_Raw);
                            }
                            catch
                            {

                            }
                        }
                    }

                    if (-1 != Data.Temp.IndexOf("GPG"))
                    {


                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        ////tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                        //tách lấy giờ, thời gian GPS chậm hơn thời gian thực 7h nên phải cộng 7
                        //Nếu time >= 240000.00 thì phải trừ đi 240000.00
                        double dTemp_Time_hour = 0, dTemp_Time;
                        string temp_time = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                        if (temp_time != "")
                        {
                            dTemp_Time_hour = Convert.ToDouble(temp_time.Substring(0, 2)) + 7;
                            if (dTemp_Time_hour >= 24) dTemp_Time_hour -= 24;
                            dTemp_Time = Convert.ToDouble(temp_time) + 70000.00;
                            if (dTemp_Time >= 240000.00) dTemp_Time_hour -= 240000.00;
                            Data.Time = dTemp_Time.ToString();
                        }
                        //show now time
                        //format hour:min:sec

                        //tblock_Current_Timer.Text = dTemp_Time_hour.ToString() + ':'+ temp_time.Substring(2, 2)
                        //    + ':' + temp_time.Substring(4, 5);
                        if (bConnectOk)//connect to Com
                        {
                            if (-1 != Data.Time.IndexOf('.'))//have '.' in Data.Time 82754.7
                                tblock_Current_Timer.Text = Data.Time.Substring(0, Data.Time.Length - 6) + ':'
                                        + Data.Time.Substring(Data.Time.Length - 6, 2) + ':' + Data.Time.Substring(Data.Time.Length - 4, 4);
                            else
                                tblock_Current_Timer.Text = Data.Time.Substring(0, Data.Time.Length - 4) + ':'
                                        + Data.Time.Substring(Data.Time.Length - 4, 2) + ':' + Data.Time.Substring(Data.Time.Length - 2, 2);
                        }

                        //tblock_CurentTime.Text = "Now: " + Data.Time;
                        //Ngày 17/12/2015 17h36 ok
                        //tbOutputText.Text += "Time: " + Data.Time + '\n';
                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        ////tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                        //tách lấy vĩ độ mặc định ở vĩ độ North
                        Data.Latitude = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                        //tbOutputText.Text += "Latitude: " + Data.Latitude + '\n';
                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        //tách lấy kinh độ
                        Data.Longtitude = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                        //tbOutputText.Text += "Longtitude: " + Data.Longtitude + '\n';
                        //tách lấy độ cao so với mực nước biển
                        //tìm ký tự M đầu tiên trong chuỗi
                        //Temp =  $GPGGA,024005.000,1043.4007,N,10641.3308,E,1,4,2.67,7.9,M,2.5,M,,*6E
                        if (Data.Temp.IndexOf('M') > 0)
                        {
                            Data.Temp = Data.Temp.Substring(0, Data.Temp.IndexOf('M') - 1);
                            //Temp = $GPGGA,024005.000,1043.4007,N,10641.3308,E,1,4,2.67,7.9
                            //Độ cao là số sao dấu phẩy cuối cùng
                            Data.Altitude = Data.Temp.Substring(Data.Temp.LastIndexOf(',') + 1);
                        }

                        ShowSpeed_Alt_Position();



                    }
                    //**********************************************************************

                    //speed and angle
                    //Tìm chuối bắt đầu với $GPVTG để tìm angle
                    if (-1 != Data.Temp.IndexOf("GPV"))
                    {
                        //Chuỗi này chứa angle
                        //Reset bộ đếm số dòng của cảm biến IMU
                        //index_dataAcc = 0;
                        //$GPVTG,350.40,T,,M,0.95,N,1.76,K,A

                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "350.40,T,,M,0.95,N,1.76,K,A";
                        //tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                        //tách lấy góc
                        Data.Angle = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                        //tbOutputText.Text += "Angle: " + Data.Angle + '\n';
                        //Tách vận tốc
                        //cut bỏ Data đến chữ N và dấu phẩy kế chữ N nên mới +2 lấy sau dấu phẩy
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf('N') + 2, Data.Temp.Length - (Data.Temp.IndexOf('N') + 2));
                        //tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                        //Data.Temp = "1.76,K,A";
                        //tách lấy speed km/h
                        //De phong khong co dau ,
                        if (Data.Temp.IndexOf(',') != -1)
                            Data.Speed = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                    }
                }
                catch
                {

                }
            }
        }
        /// <summary>
        /// Onlly get data, not show date
        /// To get start time anf end of time
        /// </summary>
        public void processDataToGetInf()
        {
            if (setupReadfile)//xu ly voi com
            {

                Data.Temp = strDataFromSerialPort;
                if (Data.Temp.IndexOf('$') != -1)
                    //cắt bỏ ký tự '$'
                    Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf('$') + 1, Data.Temp.Length - (Data.Temp.IndexOf('$') + 1));
                else
                    return;

                if (Data.Temp.IndexOf("GPG") != -1)
                {

                    //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                    Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                    //Data.Temp = "024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                    ////tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                    //tách lấy giờ, thời gian GPS chậm hơn thời gian thực 7h nên phải cộng 7
                    //Nếu time >= 240000.00 thì phải trừ đi 240000.00
                    double dTemp_Time = 0;
                    string temp_time = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                    if (temp_time != "")
                    {
                        dTemp_Time = (Convert.ToDouble(temp_time) + 70000.00);
                        if (dTemp_Time >= 240000.00) dTemp_Time -= 240000.00;
                        Data.Time = dTemp_Time.ToString();
                    }

                }
            }
        }
        //******************************************************************************
        public void processToDrawTrajactory()
        {
            try
            {
                if (setupReadfile)//xu ly voi com
                {
                    Data.Temp = strDataFromSerialPort;
                    if (Data.Temp.IndexOf("GPG") != -1)
                    {

                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        ////tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                        //tách lấy giờ, thời gian GPS chậm hơn thời gian thực 7h nên phải cộng 7
                        //Nếu time >= 240000.00 thì phải trừ đi 240000.00
                        double dTemp_Time = 0;
                        string temp_time = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                        if (temp_time != "")
                        {
                            dTemp_Time = (Convert.ToDouble(temp_time) + 70000.00);
                            if (dTemp_Time >= 240000.00) dTemp_Time -= 240000.00;
                            Data.Time = dTemp_Time.ToString();
                        }

                        //Ngày 17/12/2015 17h36 ok
                        //tbOutputText.Text += "Time: " + Data.Time + '\n';
                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        ////tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                        //tách lấy vĩ độ mặc định ở vĩ độ North
                        Data.Latitude = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                        //tbOutputText.Text += "Latitude: " + Data.Latitude + '\n';
                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        //tách lấy kinh độ
                        Data.Longtitude = Data.Temp.Substring(0, Data.Temp.IndexOf(','));

                        //only draw trajectory

                        if (Data.Latitude != null)
                        {
                            //*************************************************************************************
                            //Có Data mới nên vẽ vị trí mới máy bay
                            //Có giải thích trong file word ngày 31/12/2015
                            double DoLat, DoLon;
                            if ((Data.Latitude.IndexOf('.') - 2) >= 0)
                            {
                                DoLat = Convert.ToDouble(Data.Latitude.Substring(0, Data.Latitude.IndexOf('.') - 2));

                                DoLon = Convert.ToDouble(Data.Longtitude.Substring(0, Data.Longtitude.IndexOf('.') - 2));
                                //dLatFinal = DoLat + Convert.ToDouble(Data.Latitude.Substring(2, Data.Latitude.Length - 2)) / 60;

                                //Ngay 13/01/2016
                                //Chú ý khi Angle = null thì Convert.ToDouble(Data.Angle) tính không được
                                //tính toán giá trị Lat, lon lưu vào biến toàn cục
                                dLatGol = DoLat + Convert.ToDouble(Data.Latitude.Substring(2, Data.Latitude.Length - 2)) / 60;
                                dLonGol = DoLon + Convert.ToDouble(Data.Longtitude.Substring(Data.Longtitude.IndexOf('.') - 2, Data.Longtitude.Length -
                                                (Data.Longtitude.IndexOf('.') - 2))) / 60;
                            }
                            Draw_Trajectory_optimize(dLatGol, dLonGol,
                                                0, 0);

                        }
                        //**********************************************************************

                    }
                }
            }
            catch { }

        }
        //******************************************************************************
        /// <summary>
        /// Tìm vị trí của text trong chuỗi string
        /// Lấy phần ở trước chuỗi string
        /// Sau đó cut đi phần trước chuỗi string
        /// </summary>
        /// <param name="strIsProcess"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public string FindTextInStr(string strIsProcess, char char_cantim)
        {
            /*
                \n$  0006 -0003  1594  0006  0000 -0006  0016  0003  0986 -0161 -0058  0130  1078\r 
                \n$GPVTG,,T,,M,0.209,N,0.387,K,D*21\r
                \n$GPGGA,063621.90,1045.56915,N,10639.72723,E,2,10,1.93,37.0,M,-2.5,M,,0000*72\r
            */
            string ReturnData = "";
            ReturnData = strIsProcess.Substring(0, strIsProcess.IndexOf(char_cantim));
            //remove \r to process next line
            strDataFromSerialPort = strIsProcess.Remove(0, strIsProcess.IndexOf(char_cantim) + 1);
            return ReturnData;
        }

        //double index_Lat = 10.818345, index_Lon = 106.658897, Timer_fly = 0;
        /// <summary>
        /// 03/12/2015
        /// Muc tiêu: trong 100ms phải đọc được 
        /// 1 gia tốc sensor.Acc
        /// 1 thời gian sensor.Time
        /// 1 ....
        /// timer chu kỳ là 100ms
        /// Vẽ Quỹ đạo máy bay
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TimerShowData(object sender, object e)
        {
            ShowSpeed_Alt_Position();
        }

        /// <summary>
        /// show speed, alt, trajectory, distance,...
        /// </summary>
        void ShowSpeed_Alt_Position()
        {

            double temp_angle;

            try
            {
                if (Data.Latitude != null)
                {
                    //*************************************************************************************
                    //Có Data mới nên vẽ vị trí mới máy bay
                    //Có giải thích trong file word ngày 31/12/2015
                    double DoLat, DoLon;
                    if ((Data.Latitude.IndexOf('.') - 2) >= 0)
                    {
                        DoLat = Convert.ToDouble(Data.Latitude.Substring(0, Data.Latitude.IndexOf('.') - 2));

                        DoLon = Convert.ToDouble(Data.Longtitude.Substring(0, Data.Longtitude.IndexOf('.') - 2));
                        //dLatFinal = DoLat + Convert.ToDouble(Data.Latitude.Substring(2, Data.Latitude.Length - 2)) / 60;

                        //Ngay 13/01/2016
                        //Chú ý khi Angle = null thì Convert.ToDouble(Data.Angle) tính không được
                        //tính toán giá trị Lat, lon lưu vào biến toàn cục
                        dLatGol = DoLat + Convert.ToDouble(Data.Latitude.Substring(2, Data.Latitude.Length - 2)) / 60;
                        dLonGol = DoLon + Convert.ToDouble(Data.Longtitude.Substring(Data.Longtitude.IndexOf('.') - 2, Data.Longtitude.Length -
                                        (Data.Longtitude.IndexOf('.') - 2))) / 60;
                    }
                    if ((Data.Angle != "") && (Data.Angle != null) && (Convert.ToDouble(Data.Speed) > 3.6))
                    {

                        Draw_Trajectory_And_Flight_optimize(dLatGol, dLonGol,
                                        Convert.ToDouble(Data.Altitude), Convert.ToDouble(Data.Angle));
                    }
                    else
                        Draw_Trajectory_And_Flight_optimize(dLatGol, dLonGol,
                                        Convert.ToDouble(Data.Altitude), 0.0);

                    //**********************************************************************
                    //Ngày 20/01/2016
                    //caculate distance
                    //Tan Son Nhat Airport dLatDentination, dLonDentination
                    //Point2: 10.113574, 106.052579
                    dDistanToTaget = distance(dLatGol, dLonGol, Convert.ToDouble(Data.Altitude), dLatDentination, dLonDentination, 0);
                    temp_angle = angleFromCoordinate(dLatGol, dLonGol, dLatDentination, dLonDentination);
                    //Ta hiện khoảng cách trên đường thẳng chứ không hiện textbox nên bỏ dòng sau
                    //tbShowDis.Text = "Distance to Dentination:  " + dDistanToTaget.ToString() + "\n";
                    //Tinh goc giua 2 diem từ vị trí máy bay đến đích
                    //tbShowDis.Text = "Vect Agl:  " + temp_angle.ToString();

                    //Ngay 21/1/2016 Show Data
                    //ShowDistance(0, temp_angle + 90, dDistanToTaget.ToString() + " Meter", 30 * myMap.ZoomLevel / 22, dLatGol, dLonGol, 1);//Purple
                    //**********optimize 6/3/2016
                    ShowDistance_optimize(0, temp_angle + 90, dDistanToTaget.ToString() + " Meter", 50 * myMap.ZoomLevel / 22, dLatGol, dLonGol);//Purple

                }
                if (bSetup)
                {
                    //Neu angle la null thi convert k duoc
                    if (Data.Angle != "")
                        DisplayDataOnMap(Convert.ToDouble(Data.Roll) / 10, Convert.ToDouble(Data.Pitch) / 10, Convert.ToDouble(Data.Speed),
                            Convert.ToDouble(Data.Altitude), 0, Convert.ToDouble(Data.Angle));
                    else
                        DisplayDataOnMap(Convert.ToDouble(Data.Roll) / 10, Convert.ToDouble(Data.Pitch) / 10, Convert.ToDouble(Data.Speed),
                            Convert.ToDouble(Data.Altitude), 0, 0.0);

                    maxSpeed = Math.Max(maxSpeed, Convert.ToDouble(Data.Speed));
                    maxAltitude = Math.Max(maxAltitude, Convert.ToDouble(Data.Altitude));

                }
            }
            catch
            {

            }
        }

        //**********************************************************************
        //**********************************************************************
        /// <summary>
        /// Ngày 03/12/2015 22h38
        /// Tiếp tục test add Image đến map
        /// Ngày 04/12/2015 đã edit dduwwocj ảnh, phóng to thu nhỏ, đặt tại vĩ độ, kinh độ
        /// show two sceen
        /// </summary>
        private void TwoScreen_Click(object sender, RoutedEventArgs e)
        {
            Background_Sensor(800, -40);
        }

        //******************************************************************
        //*****************************************************************
        //**************Ngày 08/12/2015************************************
        //Vẽ hiển thị của cảm biến
        Image imgAuto = new Image();
        double screenWidth, screenHeight;
        /// <summary>
        /// Chọn background cho các cảm biến
        /// Lấy một hình vẽ bất kỳ làm background
        /// Width: Chiều rộng của hình background
        /// chỉnh lại thành 2 màn hình, khi đó bản đồ bị thu lại
        /// </summary>
        public void Background_Sensor(double Width, double top)
        {

            //Convert to tablet 1366 x 768 --> 1280 x 800;
            screenWidth = Window.Current.Bounds.Width;

            screenHeight = Window.Current.Bounds.Height;
            //if (bDevTablet)
            {

                dConvertToTabletX = 1366 - screenWidth;
                dConvertToTabletY = 696 - screenHeight;


                myMap.Width = screenWidth;
                myMap.Height = screenHeight + 2;
                //MapBackground.Height = screenHeight;
            }
            //create background left
            FillRect_BackGround(new SolidColorBrush(Colors.DimGray), 0, 00, Width,
            screenHeight, 0.7);
            //create background in bottom to write latitude and longtitude, zoom level
            DrawLine(new SolidColorBrush(Colors.White), 12, Width, screenHeight - 22, screenWidth - 16, screenHeight - 22);//y axis: left
            //create border
            //FillRect_Border(new SolidColorBrush(Colors.WhiteSmoke), 300, -300, 30,
            //760, 0.7);
            DrawLine(new SolidColorBrush(Colors.MidnightBlue), 12, 6, 0, 6, screenHeight);//y axis: left
            DrawLine(new SolidColorBrush(Colors.MidnightBlue), 16, Width, 0, Width, screenHeight);//y axis mid
            DrawLine(new SolidColorBrush(Colors.MidnightBlue), 16, screenWidth - 8, 0, screenWidth - 8, screenHeight);//y axis right
            DrawLine(new SolidColorBrush(Colors.MidnightBlue), 10, 0, 5, screenWidth, 5);//x axis top
            DrawLine(new SolidColorBrush(Colors.MidnightBlue), 16, 0, screenHeight - 8, screenWidth, screenHeight - 8);//x axis bottom
            //create background 
            //FillRect_BackGround(new SolidColorBrush(Colors.White), 1236 - dConvertToTabletX, 0, 130,
            //768 - dConvertToTabletY, 0.7);
            //Add textbox Position de co the search duoc
            BackgroundDisplay.Children.Remove(tblock_Position);
            BackgroundDisplay.Children.Remove(tb_Position);
            BackgroundDisplay.Children.Add(tblock_Position);
            BackgroundDisplay.Children.Add(tb_Position);

            //thu bản đồ lại
            myMap.Width = screenWidth - Width;
            myMap.Margin = new Windows.UI.Xaml.Thickness(Width, 0, 0, 0);
            //move tblock_LatAndLon to bottom
            //show latitude and lontitude in bottom on screen
            tblock_LatAndLon.Margin = new Windows.UI.Xaml.Thickness(screenWidth - 220, screenHeight - 38, 00, 00);
            BackgroundDisplay.Children.Remove(tblock_LatAndLon);
            BackgroundDisplay.Children.Add(tblock_LatAndLon);
            //move zoom level to bottom on screen
            tblock_ZoomLevel.Margin = new Windows.UI.Xaml.Thickness(screenWidth - 280, screenHeight - 38, 00, 00);
            BackgroundDisplay.Children.Remove(tblock_ZoomLevel);
            BackgroundDisplay.Children.Add(tblock_ZoomLevel);
            //move TimeNow to bottom on screen
            //tblock_CurentTime.Margin = new Windows.UI.Xaml.Thickness(screenWidth - 360, screenHeight - 38, 00, 00);
            //BackgroundDisplay.Children.Remove(tblock_CurentTime);
            //BackgroundDisplay.Children.Add(tblock_CurentTime);

            //move tblock_Start_Timer to bottom on screen
            tblock_Start_Timer.Margin = new Windows.UI.Xaml.Thickness(488, screenHeight - 38, 00, 00);
            BackgroundDisplay.Children.Remove(tblock_Start_Timer);
            //BackgroundDisplay.Children.Add(tblock_Start_Timer);
            //move tblock_End_Timer to bottom on screen
            tblock_End_Timer.Margin = new Windows.UI.Xaml.Thickness(853, screenHeight - 38, 00, 00);
            BackgroundDisplay.Children.Remove(tblock_End_Timer);
            //BackgroundDisplay.Children.Add(tblock_End_Timer);
            //move tblock_Current_Timer to bottom on screen
            tblock_Current_Timer.Margin = new Windows.UI.Xaml.Thickness(679, screenHeight - 38, 00, 00);
            BackgroundDisplay.Children.Remove(tblock_Current_Timer);
            BackgroundDisplay.Children.Add(tblock_Current_Timer);
            //move slider_AdjTime to bottom on screen
            slider_AdjTime.Margin = new Windows.UI.Xaml.Thickness(488, screenHeight - 58, 00, 00);
            BackgroundDisplay.Children.Remove(slider_AdjTime);
            //BackgroundDisplay.Children.Add(slider_AdjTime);
            BackgroundDisplay.Children.Remove(ShowButton);
            BackgroundDisplay.Children.Add(ShowButton);
            //Disable play, Pause, Speed Lisbox when Open_File isn't selected
            //Disable play, Pause, Speed Lisbox when Open_File isn't selected
            bt_Play.IsEnabled = false;
            bt_Pause.IsEnabled = false;
            bt_Speed.IsEnabled = false;
        }
        //--------------------------------------------------------------------------
        //--------------------------------------------------------------------------
        //ngay 9/8/2016
        //Add border

        public void FillRect_Border(SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            Rectangle TestRet_Border = new Rectangle();
            TestRet_Border.Fill = Blush;
            TestRet_Border.Height = height;
            TestRet_Border.Width = width;
            TestRet_Border.Opacity = Opacity;
            //Xac định tọa độ
            TestRet_Border.Margin = new Windows.UI.Xaml.Thickness(
                -2358 + dConvertToTabletX + TestRet_Border.Width + StartX * 2, -798 + dConvertToTabletY + TestRet_Border.Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);

        }




        //Đã hoàn thành chỉnh 2 màn hình 09/12/2015 0h23p
        //Ngày 09/12/2015 Vẽ các cảm biến
        //*******************************************************************************
        //*******************************************************************************

        //*******************************************************
        //Ngày 13/12/2015
        //int x = 210, y = 200, width = 210, height = 210, startAngle = 210, sweepAngle = 120;

        /// <summary>
        /// Hàm vẽ đường tròn, cung tròn có góc bắt đầu là startAngle, góc quét là sweepAngle
        /// đường tròn nội tiếp hình chữ nhật có điểm bắt đầu là x, y và rộng width cao height
        /// Chú ý góc quét phải nhỏ hơn 360, dung cho ve goc roll
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="startAngle"></param>
        /// <param name="sweepAngle"></param>
        void DrawArc(double x, double y, double width, double height, int startAngle, int sweepAngle)

        {

            //RingSlice TestRinslice = new RingSlice();
            ////BackgroundDisplay.Children.Remove(TestRinslice);
            //TestRinslice.StartAngle = (double)startAngle + 90;
            //TestRinslice.EndAngle = startAngle + 90 + sweepAngle;
            //TestRinslice.Fill = new SolidColorBrush(Colors.Green);
            //TestRinslice.Radius = height / 2;
            //TestRinslice.InnerRadius = height / 2 - 3;
            ////Thickness sẽ dời tâm đường tròn

            //TestRinslice.Margin = new Windows.UI.Xaml.Thickness(
            //    -2358 + dConvertToTabletX + (TestRinslice.Radius + x) * 2, -800 + dConvertToTabletY + (TestRinslice.Radius + y) * 2, 0, 0);
            //BackgroundDisplay.Children.Add(TestRinslice);

        }

        //************************************************************************
        //Ngày 19/12/2015

        /// <summary>
        /// setup roll angle
        /// </summary>
        /// <param name="Roll"></param>
        /// <param name="dBalance_mid_X"></param>
        /// <param name="dBalance_mid_Y"></param>
        /// <param name="dBalance_R"></param>
        void RollAngle_Setup(double Roll, double dBalance_mid_X, double dBalance_mid_Y, double dBalance_R)
        {
            double dArc_X_15, dArc_Y_15, dArc_X_15_Into, dArc_Y_15_Into;

            //Ve duong tron ben trong I(dmidXBalance, dBalance_mid_Y) ban kinh dRIntoBalance
            //Ghi chữ Attitude (thái độ, dáng điệu) And Director (đang đi lên hay đi xuống) của máy bay
            DrawString("Attitude And Director (degrees)", 20, new SolidColorBrush(Colors.Yellow),
                dBalance_mid_X - dBalance_R, dBalance_mid_Y - dBalance_R - 25, 1);
            double dBalance_R_Into;
            dBalance_R_Into = dBalance_R - 15;
            DrawArc((dBalance_mid_X - dBalance_R_Into), dBalance_mid_Y - dBalance_R_Into,
            2 * dBalance_R_Into, 2 * dBalance_R_Into, 210, 120);

            //Ve duong thang do chia goc 15 do co 2 diem thuoc 2 duong tron
            // double dArc_X_15, dArc_Y_15, dArc_X_15_Into, dArc_Y_15_Into;
            //Ve cac duong net do chia cho cung tron o tren
            //Cung tron 120 do
            //Ve tai 30, 60, 90, 120, 150 voi duong dai 15
            //Lay duong trong trong lam moc
            SolidColorBrush BlushOfLine1 = new SolidColorBrush(Colors.Green);
            dBalance_R_Into = dBalance_R - 15;
            for (int index = 30; index <= 150; index += 30)
            {
                dArc_X_15 = dBalance_R * (double)Math.Cos(Math.PI * index / 180) + dBalance_mid_X;
                dArc_Y_15 = -dBalance_R * (double)Math.Sin(Math.PI * index / 180) + dBalance_mid_Y;
                dArc_X_15_Into = dBalance_R_Into * (double)Math.Cos(Math.PI * index / 180) + dBalance_mid_X;
                dArc_Y_15_Into = -dBalance_R_Into * (double)Math.Sin(Math.PI * index / 180) + dBalance_mid_Y;
                DrawLine(BlushOfLine1, 2, dArc_X_15, dArc_Y_15, dArc_X_15_Into, dArc_Y_15_Into);
            }
            //Ve tai 45, 135 voi duong dai 10
            dBalance_R = dBalance_R_Into + 10;
            for (int index = 45; index <= 150; index += 90)
            {
                dArc_X_15 = dBalance_R * (double)Math.Cos(Math.PI * index / 180) + dBalance_mid_X;
                dArc_Y_15 = -dBalance_R * (double)Math.Sin(Math.PI * index / 180) + dBalance_mid_Y;
                dArc_X_15_Into = dBalance_R_Into * (double)Math.Cos(Math.PI * index / 180) + dBalance_mid_X;
                dArc_Y_15_Into = -dBalance_R_Into * (double)Math.Sin(Math.PI * index / 180) + dBalance_mid_Y;
                DrawLine(BlushOfLine1, 2, dArc_X_15, dArc_Y_15, dArc_X_15_Into, dArc_Y_15_Into);
            }
            //Ve tai 70, 80, 100, 110 voi duong dai 10
            dBalance_R = dBalance_R_Into + 10;
            for (int index = 70; index <= 110; index += 10)
            {
                dArc_X_15 = dBalance_R * (double)Math.Cos(Math.PI * index / 180) + dBalance_mid_X;
                dArc_Y_15 = -dBalance_R * (double)Math.Sin(Math.PI * index / 180) + dBalance_mid_Y;
                dArc_X_15_Into = dBalance_R_Into * (double)Math.Cos(Math.PI * index / 180) + dBalance_mid_X;
                dArc_Y_15_Into = -dBalance_R_Into * (double)Math.Sin(Math.PI * index / 180) + dBalance_mid_Y;
                DrawLine(BlushOfLine1, 2, dArc_X_15, dArc_Y_15, dArc_X_15_Into, dArc_Y_15_Into);
            }
            /************************************************************/
            //Vẽ mũi tên
            double x1, y1, x2, y2, x3, y3;
            x1 = dBalance_mid_X;
            y1 = (dBalance_mid_Y - dBalance_R_Into);
            x2 = x1 - 10;
            y2 = y1 - 10;
            x3 = x1 + 10;
            y3 = y1 - 10;
            Polygon(x1, y1, x2, y2, x3, y3);
            //Vẽ mũi tên chạy chạy
            RollAngle_PolygonAutoRemove_setup();
            RollAngle_Draw_TriAngle(Roll, dBalance_mid_X, dBalance_mid_Y, dBalance_R);

        }

        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// tam giác cũ sẽ tự remove khi tam giác mới xuất hiện
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void RollAngle_PolygonAutoRemove_setup()
        {
            SolidColorBrush BlushOfTriAngle = new SolidColorBrush(Colors.Blue);
            //myPointCollection.Add(new Point(0.025, 0.005 * sliderAdjSpeed.Value));
            //Polygon myRollAngle_PolygonAutoRemove = new Polygon();
            BackgroundDisplay.Children.Remove(myRollAngle_PolygonAutoRemove);
            //myRollAngle_PolygonAutoRemove.Points = myPointCollection;
            myRollAngle_PolygonAutoRemove.Fill = BlushOfTriAngle;
            myRollAngle_PolygonAutoRemove.Stretch = Stretch.Fill;
            myRollAngle_PolygonAutoRemove.Stroke = BlushOfTriAngle;
            myRollAngle_PolygonAutoRemove.Opacity = 0.8;
            myRollAngle_PolygonAutoRemove.StrokeThickness = 1;


        }
        //************************************************************************

        /// <summary>
        /// Vẽ đường thẳng từ (x1, y1) đến (x2, y2)
        /// Bút vẽ là ColorOfLine
        /// độ rông là SizeOfLine
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        void DrawLine(SolidColorBrush ColorOfLine, double SizeOfLine, double x1, double y1, double x2, double y2)
        {
            Line TestLine = new Line();
            //Điểm bắt đầu trên cùng có tọa độ 0, 0
            //Line TestLine = new Line();
            //BackgroundDisplay.Children.Remove(TestLine);
            //TestLine.Fill = new SolidColorBrush(Colors.Green);
            TestLine.Stroke = ColorOfLine;
            //TestLine.
            //TestLine.Height = 10;
            //TestLine.Width = 10;
            TestLine.X1 = x1;
            TestLine.Y1 = y1;
            TestLine.X2 = x2;
            TestLine.Y2 = y2;
            TestLine.StrokeThickness = SizeOfLine;

            //Xac định tọa độ
            //TestLine.Margin = new Windows.UI.Xaml.Thickness(-1500, -200, 0, 0);
            BackgroundDisplay.Children.Add(TestLine);

        }

        //************************************************************
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void Polygon(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double xmin, ymin, xmax, ymax;
            xmin = Math.Min(x1, Math.Min(x2, x3));
            ymin = Math.Min(y1, Math.Min(y2, y3));
            xmax = Math.Max(x1, Math.Max(x2, x3));
            ymax = Math.Max(y1, Math.Max(y2, y3));
            //Draw
            Polygon myPolygon = new Polygon();
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point(x1 - xmin, y1 - ymin));
            myPointCollection.Add(new Point(x2 - xmin, y2 - ymin));
            myPointCollection.Add(new Point(x3 - xmin, y3 - ymin));
            //myPointCollection.Add(new Point(0.025, 0.005 * sliderAdjSpeed.Value));

            //BackgroundDisplay.Children.Remove(myPolygon);
            myPolygon.Points = myPointCollection;
            myPolygon.Fill = new SolidColorBrush(Colors.Green);
            myPolygon.Width = xmax - xmin;
            myPolygon.Height = ymax - ymin;
            myPolygon.Stretch = Stretch.Fill;
            myPolygon.Stroke = new SolidColorBrush(Colors.Black);
            myPolygon.StrokeThickness = 1;
            //Xac định tọa độ -2060, -491
            //myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2338 + xmin * 2, -786 + 2 * ymin, 0, 0);
            //myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2158 + myPolygon.Width, -600 + myPolygon.Height, 0, 0);
            myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xmax + xmin, -800 + dConvertToTabletY + ymax + ymin, 0, 0);
            //myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2250 , -486, 0, 0);
            BackgroundDisplay.Children.Add(myPolygon);


        }
        //************************************************************************
        //************************************************************
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void PolygonHaveBrush(SolidColorBrush brush, double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double xmin, ymin, xmax, ymax;
            xmin = Math.Min(x1, Math.Min(x2, x3));
            ymin = Math.Min(y1, Math.Min(y2, y3));
            xmax = Math.Max(x1, Math.Max(x2, x3));
            ymax = Math.Max(y1, Math.Max(y2, y3));
            //Draw
            Polygon myPolygon = new Polygon();
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point(x1 - xmin, y1 - ymin));
            myPointCollection.Add(new Point(x2 - xmin, y2 - ymin));
            myPointCollection.Add(new Point(x3 - xmin, y3 - ymin));
            //myPointCollection.Add(new Point(0.025, 0.005 * sliderAdjSpeed.Value));

            //BackgroundDisplay.Children.Remove(myPolygon);
            myPolygon.Points = myPointCollection;
            myPolygon.Fill = brush;
            myPolygon.Width = xmax - xmin;
            myPolygon.Height = ymax - ymin;
            myPolygon.Stretch = Stretch.Fill;
            //màu viền
            myPolygon.Stroke = brush;
            myPolygon.StrokeThickness = 1;
            //Xac định tọa độ -2060, -491
            //myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2338 + xmin * 2, -786 + 2 * ymin, 0, 0);
            //myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2158 + myPolygon.Width, -600 + myPolygon.Height, 0, 0);
            myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xmax + xmin, -800 + dConvertToTabletY + ymax + ymin, 0, 0);
            //myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2250 , -486, 0, 0);
            BackgroundDisplay.Children.Add(myPolygon);


        }
        //************************************************************************
        //************************************************************
        //Ngày 14/1/2/2015 22h39 đã hoàn thành việc vẽ tam giác đúng vị trí
        //Hoàn thahf vẽ display cảm biến gia tốc
        Polygon myPolygonAutoRemove = new Polygon();
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// tam giác cũ sẽ tự remove khi tam giác mới xuất hiện
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void PolygonAutoRemove(SolidColorBrush BlushOfTriAngle, double x1, double y1, double x2, double y2, double x3, double y3, double Opacity)
        {
            double xmin, ymin, xmax, ymax;
            xmin = Math.Min(x1, Math.Min(x2, x3));
            ymin = Math.Min(y1, Math.Min(y2, y3));
            xmax = Math.Max(x1, Math.Max(x2, x3));
            ymax = Math.Max(y1, Math.Max(y2, y3));
            //Draw

            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point((x1 - xmin), (y1 - ymin)));
            myPointCollection.Add(new Point(x2 - xmin, y2 - ymin));
            myPointCollection.Add(new Point(x3 - xmin, y3 - ymin));
            //myPointCollection.Add(new Point(0.025, 0.005 * sliderAdjSpeed.Value));
            //Polygon myPolygonAutoRemove = new Polygon();
            BackgroundDisplay.Children.Remove(myPolygonAutoRemove);
            myPolygonAutoRemove.Points = myPointCollection;
            myPolygonAutoRemove.Fill = BlushOfTriAngle;
            myPolygonAutoRemove.Width = (xmax - xmin);
            myPolygonAutoRemove.Height = (ymax - ymin);
            myPolygonAutoRemove.Stretch = Stretch.Fill;
            myPolygonAutoRemove.Stroke = BlushOfTriAngle;
            myPolygonAutoRemove.Opacity = Opacity;
            myPolygonAutoRemove.StrokeThickness = 1;
            //Xac định tọa độ -1856, -491 là dời về 0, 0
            //quá trình khảo sát trong vở
            //myPolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2158 + myPolygonAutoRemove.Width - (200 - 2 * xmin), -600 + myPolygonAutoRemove.Height, 0, 0);
            //myPolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + xmax + xmin, -600 + myPolygonAutoRemove.Height + 4 * sliderAdjSpeed.Value, 0, 0);
            //Quá trình khảo sát y và tính sai số
            myPolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xmax + xmin, -800 + dConvertToTabletY + ymax + ymin, 0, 0);
            //myPolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2060, -491, 0, 0);
            BackgroundDisplay.Children.Add(myPolygonAutoRemove);


        }

        //************************************************************
        //Ngày 14/1/2/2015 22h39 đã hoàn thành việc vẽ tam giác đúng vị trí
        //Hoàn thahf vẽ display cảm biến gia tốc
        Polygon myPolygonAutoRemove_Alt = new Polygon();
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// tam giác cũ sẽ tự remove khi tam giác mới xuất hiện
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void Altitude_PolygonAutoRemove(SolidColorBrush BlushOfTriAngle, double x1, double y1, double x2, double y2, double x3, double y3, double Opacity)
        {
            double xmin, ymin, xmax, ymax;
            xmin = Math.Min(x1, Math.Min(x2, x3));
            ymin = Math.Min(y1, Math.Min(y2, y3));
            xmax = Math.Max(x1, Math.Max(x2, x3));
            ymax = Math.Max(y1, Math.Max(y2, y3));
            //Draw

            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point((x1 - xmin), (y1 - ymin)));
            myPointCollection.Add(new Point(x2 - xmin, y2 - ymin));
            myPointCollection.Add(new Point(x3 - xmin, y3 - ymin));
            //myPointCollection.Add(new Point(0.025, 0.005 * sliderAdjSpeed.Value));
            //Polygon myPolygonAutoRemove_Alt = new Polygon();
            BackgroundDisplay.Children.Remove(myPolygonAutoRemove_Alt);
            myPolygonAutoRemove_Alt.Points = myPointCollection;
            myPolygonAutoRemove_Alt.Fill = BlushOfTriAngle;
            myPolygonAutoRemove_Alt.Width = (xmax - xmin);
            myPolygonAutoRemove_Alt.Height = (ymax - ymin);
            myPolygonAutoRemove_Alt.Stretch = Stretch.Fill;
            myPolygonAutoRemove_Alt.Stroke = BlushOfTriAngle;
            myPolygonAutoRemove_Alt.Opacity = Opacity;
            myPolygonAutoRemove_Alt.StrokeThickness = 1;
            //Xac định tọa độ -1856, -491 là dời về 0, 0
            //quá trình khảo sát trong vở
            //myPolygonAutoRemove_Alt.Margin = new Windows.UI.Xaml.Thickness(-2158 + myPolygonAutoRemove_Alt.Width - (200 - 2 * xmin), -600 + myPolygonAutoRemove_Alt.Height, 0, 0);
            //myPolygonAutoRemove_Alt.Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + xmax + xmin, -600 + myPolygonAutoRemove_Alt.Height + 4 * sliderAdjSpeed.Value, 0, 0);
            //Quá trình khảo sát y và tính sai số
            myPolygonAutoRemove_Alt.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xmax + xmin, -800 + dConvertToTabletY + ymax + ymin, 0, 0);
            //myPolygonAutoRemove_Alt.Margin = new Windows.UI.Xaml.Thickness(-2060, -491, 0, 0);
            BackgroundDisplay.Children.Add(myPolygonAutoRemove_Alt);

        }
        //*********************************************************************************************

        //*********************************************************************************************
        //Ngày 14/12/2015
        /// <summary>
        /// Vẽ mũi tên chạy qua chạy lại của bộ cảm biến gia tốc
        /// Roll là góc quay
        /// </summary>
        void RollAngle_Draw_TriAngle(double Roll, double dBalance_mid_X, double dBalance_mid_Y, double dBalance_R)
        {
            Roll = 90 - Roll;
            //*********************************************************
            //Ngay 30/09/2015
            //bien xac dinh tam va ban kinh cung tron
            //double dBalance_mid_X = 400, dBalance_mid_Y = 200, dBalance_R = 120;
            //bien xac dinh 3 diem cua tam giac
            double dTriAngle_P1_X, dTriAngle_P1_Y, dTriAngle_P2_X, dTriAngle_P2_Y, dTriAngle_P3_X, dTriAngle_P3_Y;
            double dBalance_R_Into, temp1, temp2, temp3;//cac biến tạm để tối ưu
            //Lay duong trong trong lam moc
            //Diem point1 cua tam giac
            dBalance_R_Into = dBalance_R - 16;
            temp1 = Math.PI * Roll / 180;
            dTriAngle_P1_X = dBalance_R_Into * (double)Math.Cos(temp1) + dBalance_mid_X;
            dTriAngle_P1_Y = -dBalance_R_Into * (double)Math.Sin(temp1) + dBalance_mid_Y;
            //Diem point2 cua tam giac
            dBalance_R_Into = dBalance_R - 30;
            temp2 = Math.PI * (Roll + 10) / 180;
            dTriAngle_P2_X = dBalance_R_Into * (double)Math.Cos(temp2) + dBalance_mid_X;
            dTriAngle_P2_Y = -dBalance_R_Into * (double)Math.Sin(temp2) + dBalance_mid_Y;
            //Diem point3 cua tam giac
            dBalance_R_Into = dBalance_R - 30;
            temp3 = Math.PI * (Roll - 10) / 180;
            dTriAngle_P3_X = dBalance_R_Into * (double)Math.Cos(temp3) + dBalance_mid_X;
            dTriAngle_P3_Y = -dBalance_R_Into * (double)Math.Sin(temp3) + dBalance_mid_Y;
            //Vẽ tam giác qua 3 điểm
            RollAngle_PolygonAutoRemove(dTriAngle_P1_X, dTriAngle_P1_Y, dTriAngle_P2_X, dTriAngle_P2_Y, dTriAngle_P3_X, dTriAngle_P3_Y);
            //Point[] points = { new Point((int)dTriAngle_P1_X, (int)dTriAngle_P1_Y), new Point
            //        ((int)dTriAngle_P2_X, (int)dTriAngle_P2_Y), new Point((int)dTriAngle_P3_X, (int)dTriAngle_P3_Y) };
            //G_TriAngle.dillPolygon(Blush_TriAngle, points);
            //Ve tai 45, 135


        }
        //*********************************************************************************************
        //Hoàn thanh vẽ display cảm biến gia tốc
        Polygon myRollAngle_PolygonAutoRemove = new Polygon();
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// tam giác cũ sẽ tự remove khi tam giác mới xuất hiện
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void RollAngle_PolygonAutoRemove(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double xmin, ymin, xmax, ymax;
            xmin = Math.Min(x1, Math.Min(x2, x3));
            ymin = Math.Min(y1, Math.Min(y2, y3));
            xmax = Math.Max(x1, Math.Max(x2, x3));
            ymax = Math.Max(y1, Math.Max(y2, y3));
            //Draw

            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point((x1 - xmin), (y1 - ymin)));
            myPointCollection.Add(new Point(x2 - xmin, y2 - ymin));
            myPointCollection.Add(new Point(x3 - xmin, y3 - ymin));
            //myPointCollection.Add(new Point(0.025, 0.005 * sliderAdjSpeed.Value));
            //Polygon myRollAngle_PolygonAutoRemove = new Polygon();
            BackgroundDisplay.Children.Remove(myRollAngle_PolygonAutoRemove);
            myRollAngle_PolygonAutoRemove.Points = myPointCollection;
            myRollAngle_PolygonAutoRemove.Width = (xmax - xmin);
            myRollAngle_PolygonAutoRemove.Height = (ymax - ymin);
            //Xac định tọa độ -1856, -491 là dời về 0, 0
            //quá trình khảo sát trong vở
            //myRollAngle_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2158 + myRollAngle_PolygonAutoRemove.Width - (200 - 2 * xmin), -600 + myRollAngle_PolygonAutoRemove.Height, 0, 0);
            //myRollAngle_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + xmax + xmin, -600 + myRollAngle_PolygonAutoRemove.Height + 4 * sliderAdjSpeed.Value, 0, 0);
            //Quá trình khảo sát y và tính sai số
            myRollAngle_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xmax + xmin, -800 + dConvertToTabletY + ymax + ymin, 0, 0);
            //myRollAngle_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2060, -491, 0, 0);
            BackgroundDisplay.Children.Add(myRollAngle_PolygonAutoRemove);


        }
        //*********************************************************************************************
        //********************************************************************************
        //Vẽ những hình Chữ nhật auto remove
        Rectangle RetangleAutoRemove3 = new Rectangle();
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void FillRect_AutoRemove3(SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(RetangleAutoRemove3);
            RetangleAutoRemove3.Fill = Blush;
            RetangleAutoRemove3.Height = height;
            RetangleAutoRemove3.Width = width;
            RetangleAutoRemove3.Opacity = Opacity;
            //Xac định tọa độ
            RetangleAutoRemove3.Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + RetangleAutoRemove3.Width + StartX * 2, -798 + dConvertToTabletY + RetangleAutoRemove3.Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(RetangleAutoRemove3);
        }
        //********************************************************************************
        //********************************************************************************

        Rectangle RetangleAutoRemove = new Rectangle();
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// Rectangle sẽ Auto Remove khi có Rectangle mới
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void FillRectangleAutoRemove(SolidColorBrush Blush, double StartX, double StartY, double width, double height)
        {
            //Rectangle TestRetangle = new Rectangle();
            RetangleAutoRemove.Fill = new SolidColorBrush(Colors.Pink);
            RetangleAutoRemove.Height = height;
            RetangleAutoRemove.Width = width;
            RetangleAutoRemove.Opacity = 0.5;
            //Xac định tọa độ
            RetangleAutoRemove.Margin = new Windows.UI.Xaml.Thickness(
                -2358 + RetangleAutoRemove.Width + StartX * 2, -798 + dConvertToTabletY + RetangleAutoRemove.Height + StartY * 2, 0, 0);
            BackgroundDisplay.Children.Add(RetangleAutoRemove);
        }
        //Bước đột phá, tạo mảng hình chữ nhật auto remove biến này là biến toàn cục
        Rectangle[] Ret_AutoRemove = new Rectangle[10];

        //Set up for rectangle
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// index là số thứ tự hình chữ nhật auto remove
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Rect_Setup_AutoRemove(int index, SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(Ret_AutoRemove[index]);
            Ret_AutoRemove[index] = new Rectangle();
            Ret_AutoRemove[index].Fill = Blush;
            Ret_AutoRemove[index].Height = height;
            Ret_AutoRemove[index].Width = width;
            Ret_AutoRemove[index].Opacity = Opacity;
            //Xac định tọa độ
            Ret_AutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + Ret_AutoRemove[index].Width + StartX * 2, -798 + dConvertToTabletY + Ret_AutoRemove[index].Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(Ret_AutoRemove[index]);

        }
        //********************************************************************************

        /// <summary>
        /// Khi tác động vào hình chữ nhật thì remove cái cũ và add vị trí mới
        /// màu sắc và độ đục đã được set up rồi
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// index là số thứ tự hình chữ nhật auto remove
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Rect_Change_AutoRemove(int index, double StartX, double StartY, double width, double height)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(Ret_AutoRemove[index]);
            Ret_AutoRemove[index].Height = height;
            Ret_AutoRemove[index].Width = width;
            //Xac định tọa độ
            Ret_AutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + Ret_AutoRemove[index].Width + StartX * 2, -798 + dConvertToTabletY + Ret_AutoRemove[index].Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(Ret_AutoRemove[index]);

        }
        //********************************************************************************
        //Vẽ string trong map

        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawString(string drawString, double SizeOfText, SolidColorBrush Blush, double StartX, double StartY, double Opacity)
        {
            //create graphic text block design text
            TextBlock TxtDesign = new TextBlock();
            //chiều dài rộng của khung chứa text
            //TxtDesign.Height = HeightOfBlock;
            //TxtDesign.Width = WidthOfBlock;
            //canh lề, left, right, center
            TxtDesign.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesign.VerticalAlignment = VerticalAlignment.Top;
            //TxtDesign.Margin = 
            //
            //đảo chữ
            TxtDesign.TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            TxtDesign.Text = drawString;
            TxtDesign.FontSize = SizeOfText;
            TxtDesign.FontFamily = new FontFamily("Arial");
            //TxtDesign.FontStyle = "Arial";
            //TxtDesign.FontStretch
            //color text có độ đục
            //TxtDesign.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesign.Foreground = Blush;
            TxtDesign.Opacity = Opacity;
            //position of text left, top, right, bottom
            TxtDesign.Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesign);
        }

        //************************************************************************
        //************************************************************************
        //Vẽ string Auto remove trong map
        TextBlock TxtDesignAutoRemove = new TextBlock();

        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawStringAutoRemove(string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            BackgroundDisplay.Children.Remove(TxtDesignAutoRemove);
            //create graphic text block design text
            //TextBlock TxtDesignAutoRemove = new TextBlock();
            //chiều dài rộng của khung chứa text
            TxtDesignAutoRemove.Height = 30.0;
            TxtDesignAutoRemove.Width = 100.0;
            //canh lề, left, right, center
            //drawFormat.Alignment = StringAlignment.Center;
            //drawFormat.LineAlignment = StringAlignment.Near;
            TxtDesignAutoRemove.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesignAutoRemove.VerticalAlignment = VerticalAlignment.Top;
            //TxtDesignAutoRemove.TextAlignment = string
            //TxtDesignAutoRemove.TextLi
            //drawFormat1.Alignment = StringAlignment.Center;
            // drawFormat1.LineAlignment = StringAlignment.Far;
            //Canh lề phải
            TxtDesignAutoRemove.TextAlignment = TextAlignment.Right;
            //TxtDesignAutoRemove.LineHeight = double.NaN;
            //TxtDesignAutoRemove.
            //TxtDesignAutoRemove.li
            //TxtDesignAutoRemove.Margin = 
            //
            //đảo chữ
            TxtDesignAutoRemove.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
            TxtDesignAutoRemove.Text = drawString;
            TxtDesignAutoRemove.FontSize = SizeOfText;
            TxtDesignAutoRemove.FontFamily = new FontFamily("Arial");
            //TxtDesignAutoRemove.FontStyle = "Arial";
            //TxtDesignAutoRemove.FontStretch
            //color text có độ đục
            //TxtDesignAutoRemove.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesignAutoRemove.Foreground = Blush;
            TxtDesignAutoRemove.Opacity = Opacity;
            //position of text left, top, right, bottom
            TxtDesignAutoRemove.Margin = new Windows.UI.Xaml.Thickness(StartX - 70, StartY, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesignAutoRemove);
        }
        //*****************************************************************
        //Ngày 20/12/2015 bước đột phá tạo một mảng TextBlock Auto remove
        TextBlock[] Tb_Compass_Display_Angle = new TextBlock[1];
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// Set up init location for string
        /// index: index of string
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void Compass_Setup_Display_Angle(int index, string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity, double HeightOfBlock, double WidthOfBlock)
        {
            //create graphic text block design text
            //TextBlock Tb_Compass_Display_Angle[index] = new TextBlock();

            //canh lề, left, right, center
            BackgroundDisplay.Children.Remove(Tb_Compass_Display_Angle[index]);
            Tb_Compass_Display_Angle[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            Tb_Compass_Display_Angle[index].Height = HeightOfBlock;
            Tb_Compass_Display_Angle[index].Width = WidthOfBlock;
            Tb_Compass_Display_Angle[index].HorizontalAlignment = HorizontalAlignment.Left;
            Tb_Compass_Display_Angle[index].VerticalAlignment = VerticalAlignment.Top;
            //Tb_Compass_Display_Angle[index].Margin = 
            //
            //đảo chữ
            Tb_Compass_Display_Angle[index].TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            Tb_Compass_Display_Angle[index].Text = drawString;
            Tb_Compass_Display_Angle[index].FontSize = SizeOfText;
            Tb_Compass_Display_Angle[index].TextAlignment = TextAlignment.Center;
            Tb_Compass_Display_Angle[index].FontFamily = new FontFamily("Arial");
            //Tb_Compass_Display_Angle[index].FontStyle = "Arial";
            //Tb_Compass_Display_Angle[index].FontStretch
            //color text có độ đục
            //Tb_Compass_Display_Angle[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            Tb_Compass_Display_Angle[index].Foreground = Blush;
            Tb_Compass_Display_Angle[index].Opacity = Opacity;
            //position of text left, top, right, bottom
            Tb_Compass_Display_Angle[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_Compass_Display_Angle[index]);
        }

        //*****************************************************************
        //Ngày 20/12/2015 bước đột phá tạo một mảng TextBlock Auto remove
        TextBlock[] Tb_Alt = new TextBlock[10];
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// Set up init location for string
        /// index: index of string
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void Alt_SetupString_AutoRemove(int index, string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            //create graphic text block design text
            //TextBlock Tb_Alt[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_Alt[index].Height = HeightOfBlock;
            //Tb_Alt[index].Width = WidthOfBlock;
            //canh lề, left, right, center
            BackgroundDisplay.Children.Remove(Tb_Alt[index]);
            Tb_Alt[index] = new TextBlock();
            Tb_Alt[index].HorizontalAlignment = HorizontalAlignment.Left;
            Tb_Alt[index].VerticalAlignment = VerticalAlignment.Top;
            //Tb_Alt[index].Margin = 
            //
            //đảo chữ
            Tb_Alt[index].TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            Tb_Alt[index].Text = drawString;
            Tb_Alt[index].FontSize = SizeOfText;
            Tb_Alt[index].FontFamily = new FontFamily("Arial");
            //Tb_Alt[index].FontStyle = "Arial";
            //Tb_Alt[index].FontStretch
            //color text có độ đục
            //Tb_Alt[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            Tb_Alt[index].Foreground = Blush;
            Tb_Alt[index].Opacity = Opacity;
            //position of text left, top, right, bottom
            Tb_Alt[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_Alt[index]);
        }

        //**********************************************************************************************
        /// <summary>
        /// Remove chỗi string cũ
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// Set up init location for string
        /// index: index of string
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void Alt_ChangeString_AutoRemove(int index, string drawString,
            double StartX, double StartY)
        {


            BackgroundDisplay.Children.Remove(Tb_Alt[index]);
            Tb_Alt[index].Text = drawString;

            Tb_Alt[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_Alt[index]);
        }

        //*************************************************************************
        /// <summary>
        /// draw triangle
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void Draw_TriAngle_Var(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            //Graphics G_TriAngle = this.CreateGraphics();
            //SolidBrush Blush_TriAngle = new SolidBrush(Color.Black);
            SolidColorBrush BlushOfTriAngle = new SolidColorBrush(Colors.Black);
            //Point[] points = { new Point(x1, y1), new Point(x2, y2), new Point(x3, y3) };
            PolygonAutoRemove(BlushOfTriAngle, x1, y1, x2, y2, x3, y3, 1);
        }
        //*****************************************************************************************************
        /// <summary>
        /// draw traangle foe alt
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void Altitude_Draw_TriAngle(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            SolidColorBrush BlushOfTriAngle = new SolidColorBrush(Colors.Black);
            //Point[] points = { new Point(x1, y1), new Point(x2, y2), new Point(x3, y3) };
            Altitude_PolygonAutoRemove(BlushOfTriAngle, x1, y1, x2, y2, x3, y3, 1);
        }
        //*****************************************************************************************************
        //Ngay 09/03/ 2015
        //***************************************************
        /// <summary>
        /// Vẽ hiển thị của cảm biến Compass với tọa độ trung tâm là dComPass_mid_X, dComPass_mid_Y
        /// Bán kính là dComPass_R và nhận Angle_Flight là thông số đầu vào
        /// </summary>
        /// <param name="dComPass_mid_X"></param>
        /// <param name="dComPass_mid_Y"></param>
        /// <param name="dComPass_R"></param>
        /// <param name="Angle_Flight"></param>
        void ComPass_Setup_Rotate_Out(double Angle_Flight, double dComPass_mid_X, double dComPass_mid_Y, double dComPass_R)
        {

            double Angle_Rotate = Angle_Flight + 90;

            //Ngày 19/12/2015 Vẽ đường mũi tên chỉ hướng máy bay
            SolidColorBrush BlushOfArrow = new SolidColorBrush(Colors.Green);
            double dArrowY = dComPass_mid_Y - dComPass_R;
            double dArrowY1 = dComPass_mid_Y - dComPass_R / 3;
            double dArrowY2 = dComPass_mid_Y + dComPass_R / 3;
            double dArrowY3 = dComPass_mid_Y + dComPass_R;
            DrawLine(BlushOfArrow, 5, dComPass_mid_X, dArrowY1, dComPass_mid_X, dArrowY);
            PolygonHaveBrush(BlushOfArrow, dComPass_mid_X, dArrowY, dComPass_mid_X + 8, dArrowY + 12, dComPass_mid_X - 8, dArrowY + 12);
            PolygonHaveBrush(BlushOfArrow, dComPass_mid_X, dArrowY1, dComPass_mid_X + 4, dArrowY1 + 6, dComPass_mid_X - 4, dArrowY1 + 6);
            //đường thẳng ở phía dưới
            DrawLine(BlushOfArrow, 5, dComPass_mid_X, dArrowY2, dComPass_mid_X, dArrowY3);
            //Add Image
            Compass_AddFlightToCompass(dComPass_mid_X, dComPass_mid_Y);
            //bổ sang ngày 22/12/2015
            //*********************************************************
            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            //Vẽ hình chữ nhật ghi góc, vẽ mũi tên và ghi giá trí góc
            //dComPass_mid_X, double dComPass_mid_Y, double dComPass_R)
            Compass_Rect_Setup(0, BlushRectangle4, dComPass_mid_X - 24, dComPass_mid_Y - dComPass_R - 35, 48, 25, 1);
            //Vẽ mui tên màu đen 
            Compass_Poly_AutoRemove(BlushRectangle4, dComPass_mid_X - 5, dComPass_mid_Y - dComPass_R - 10,
                 dComPass_mid_X + 5, dComPass_mid_Y - dComPass_R - 10, dComPass_mid_X, dComPass_mid_Y - dComPass_R, 1);
            //Nếu trong Angle_Flight có dấu . thì chỉ lấy 1 ký tự sau dấu chấm thập phân
            //Làm tròn thành số int
            //Vẽ String có căn lề trung tâm và format bên trong hình chữ nhật
            //Compass_SetupString_AutoRemove(12, Angle_Flight.ToString(), 22, whitePen, dComPass_mid_X - 20, dComPass_mid_Y - dComPass_R - 35, 1);
            Compass_Setup_Display_Angle(0, ((int)Angle_Flight).ToString() + '°', 22, new SolidColorBrush(Colors.Red),
                dComPass_mid_X - 24, dComPass_mid_Y - dComPass_R - 36, 1, 25, 48);
            //ghi chữ Heading (degree) ở phía dưới
            DrawString("Heading", 24, new SolidColorBrush(Colors.Purple), dComPass_mid_X - dComPass_R + 65,
                dComPass_mid_Y + dComPass_R + 5, 1);

            DrawString("(degrees)", 24, new SolidColorBrush(Colors.Purple), dComPass_mid_X - dComPass_R + 60,
                dComPass_mid_Y + dComPass_R + 5 + 30, 1);
        }

        //*******************************************************************
        //Bước đột phá, tạo mảng hình chữ nhật auto remove biến này là biến toàn cục
        Rectangle[] Compass_Ret_AutoRemove = new Rectangle[2];

        //Set up for rectangle
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// index là số thứ tự hình chữ nhật auto remove
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Compass_Rect_Setup(int index, SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(Compass_Ret_AutoRemove[index]);
            Compass_Ret_AutoRemove[index] = new Rectangle();
            Compass_Ret_AutoRemove[index].Fill = Blush;
            Compass_Ret_AutoRemove[index].Height = height;
            Compass_Ret_AutoRemove[index].Width = width;
            Compass_Ret_AutoRemove[index].Opacity = Opacity;
            //Xac định tọa độ
            Compass_Ret_AutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + Compass_Ret_AutoRemove[index].Width + StartX * 2, -798 + dConvertToTabletY + Compass_Ret_AutoRemove[index].Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(Compass_Ret_AutoRemove[index]);

        }
        //********************************************************************************

        /// <summary>
        /// Khi tác động vào hình chữ nhật thì remove cái cũ và add vị trí mới
        /// màu sắc và độ đục đã được set up rồi
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// index là số thứ tự hình chữ nhật auto remove
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Compass_Rect_Change_AutoRemove(int index, double StartX, double StartY, double width, double height)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(Compass_Ret_AutoRemove[index]);
            Compass_Ret_AutoRemove[index].Height = height;
            Compass_Ret_AutoRemove[index].Width = width;
            //Xac định tọa độ
            Compass_Ret_AutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + Compass_Ret_AutoRemove[index].Width + StartX * 2, -798 + dConvertToTabletY + Compass_Ret_AutoRemove[index].Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(Compass_Ret_AutoRemove[index]);

        }
        //********************************************************************************
        Polygon Compass_PolygonAutoRemove = new Polygon();
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// tam giác cũ sẽ tự remove khi tam giác mới xuất hiện
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void Compass_Poly_AutoRemove(SolidColorBrush BlushOfTriAngle, double x1, double y1, double x2, double y2, double x3, double y3, double Opacity)
        {
            double xmin, ymin, xmax, ymax;
            xmin = Math.Min(x1, Math.Min(x2, x3));
            ymin = Math.Min(y1, Math.Min(y2, y3));
            xmax = Math.Max(x1, Math.Max(x2, x3));
            ymax = Math.Max(y1, Math.Max(y2, y3));
            //Draw

            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point((x1 - xmin), (y1 - ymin)));
            myPointCollection.Add(new Point(x2 - xmin, y2 - ymin));
            myPointCollection.Add(new Point(x3 - xmin, y3 - ymin));
            //myPointCollection.Add(new Point(0.025, 0.005 * sliderAdjSpeed.Value));
            //Polygon Compass_PolygonAutoRemove = new Polygon();
            BackgroundDisplay.Children.Remove(Compass_PolygonAutoRemove);
            Compass_PolygonAutoRemove.Points = myPointCollection;
            Compass_PolygonAutoRemove.Fill = BlushOfTriAngle;
            Compass_PolygonAutoRemove.Width = (xmax - xmin);
            Compass_PolygonAutoRemove.Height = (ymax - ymin);
            Compass_PolygonAutoRemove.Stretch = Stretch.Fill;
            Compass_PolygonAutoRemove.Stroke = BlushOfTriAngle;
            Compass_PolygonAutoRemove.Opacity = Opacity;
            Compass_PolygonAutoRemove.StrokeThickness = 1;
            //Xac định tọa độ -1856, -491 là dời về 0, 0
            //quá trình khảo sát trong vở
            //Compass_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2158 + Compass_PolygonAutoRemove.Width - (200 - 2 * xmin), -600 + Compass_PolygonAutoRemove.Height, 0, 0);
            //Compass_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + xmax + xmin, -600 + Compass_PolygonAutoRemove.Height + 4 * sliderAdjSpeed.Value, 0, 0);
            //Quá trình khảo sát y và tính sai số
            Compass_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xmax + xmin, -800 + dConvertToTabletY + ymax + ymin, 0, 0);
            //Compass_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2060, -491, 0, 0);
            BackgroundDisplay.Children.Add(Compass_PolygonAutoRemove);

        }
        //*********************************************************************************************
        //**************Ngày 08/12/2015************************************
        Image img_FliCom = new Image();//biến này để add flight
        Image img_FliCom_Out = new Image();//biến này để quay phía ngoài
        //Hiện ảnh ở tọa độ trung tâm x, y
        //Ngày 19/12/2015 test ok
        /// <summary>
        /// Chọn background cho các cảm biến
        /// Lấy một hình vẽ bất kỳ làm background
        /// chỉnh lại thành 2 màn hình, khi đó bản đồ bị thu lại
        /// </summary>
        public void Compass_AddFlightToCompass(double CenterX, double CenterY)
        {
            //***************************************************************************************
            //9/03/2016 Add thêm ảnh mới
            //Image img_FliCom_Out = new Image();
            BackgroundDisplay.Children.Remove(img_FliCom_Out);
            //Edit size of image
            img_FliCom_Out.Height = 250;
            img_FliCom_Out.Width = 250;

            //img_FliCom_Out.RenderTransform
            img_FliCom_Out.Opacity = 1;


            //img_FliCom_Out.Transitions.
            img_FliCom_Out.Source = new BitmapImage(new Uri("ms-appx:///Assets/Compass_Rose_English_North.png"));
            //Xoay ảnh
            //kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            //Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            //khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            //Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            //dời ảnh lên trên 1 nửa chiều dài,
            //dời ảnh sang trái 1 nửa chiều rộng
            img_FliCom_Out.RenderTransform = new RotateTransform()
            {

                //Angle = 0,
                //CenterX = 40,//là la img_FliCom_Out_FliCom.Width/2
                //CenterX = 62, //The prop name maybe mistyped 
                //CenterY = 40 //la img_FliCom_Out_FliCom.Height
            };
            //mặc định ảnh có chiều dài và chiều rộng là vô cùng
            //bitmapImage.PixelHeight
            //img_FliCom_Out.sca
            img_FliCom_Out.Stretch = Stretch.Uniform;
            img_FliCom_Out.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            img_FliCom_Out.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            img_FliCom_Out.Opacity = 0.8;
            img_FliCom_Out.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + CenterX * 2, -798 + dConvertToTabletY + CenterY * 2, 0, 0);
            BackgroundDisplay.Children.Add(img_FliCom_Out);

        }

        //**************Ngày 08/12/2015************************************
        //Hiện ảnh ở tọa độ trung tâm x, y
        //Ngày 19/12/2015 test ok
        /// <summary>
        ///Ngày 04/03/2016 xoay máy bay và thay đổi góc yaw hiển thị
        ///Remove cái cũ sau đó add cái mới
        /// </summary>
        public void Comp_Rotate_OutAndAddValue(double angle_Yaw)
        {

            //BackgroundDisplay.Children.Remove(img_FliCom_Out);
            //center width/2, height/2
            img_FliCom_Out.RenderTransform = new RotateTransform()
            {

                Angle = 360 - angle_Yaw,
                CenterX = 125,
                CenterY = 125
            };


            if (angle_Yaw < 0) angle_Yaw += 360;//chỉnh góc angle_Yaw > 0 để dễ hiển thị
            Tb_Compass_Display_Angle[0].Text = (Math.Round(angle_Yaw, 0)).ToString() + '°';
        }

        //**************************************************************************
        //Ngày 20/12/2015 Set up góc Pitch
        /// <summary>
        /// Vẽ góc Pitch, Roll của máy bay
        /// Với StartX, StartY là điểm trung tâm
        /// h1: khoảng cách giữa 2 đường
        /// </summary>
        /// <param name="Pitch"></param>
        /// <param name="StartX"></param>
        /// <param name=""></param>
        void PitchAngle_Setup(double Pitch, double Roll, double StartX, double StartY, double h1)
        {


            //*************************************************************************
            //Cách 2, giữ nguyên đường màu vàng mỗi lần góc Pitch thay đổi thì toàn bộ các đường song song 
            //chạy xuống hoặc chạy lên
            //h1 <--> 10 degree: (- Pitch * h1 / 10)
            double R1 = 40, R2;//R1, R2 là độ dài nửa đường Vẽ đường vẽ có ghi số 5, 10 và đường vẽ k ghi số
            double x1, x2, y1, y2;//các điểm của đường vẽ từ x1, y1 đến x2, y2
            int indexLine = 0;
            SolidColorBrush WhitePen = new SolidColorBrush(Colors.Green);
            for (int j_setup = 0; j_setup < 12; j_setup++)
                Pitch_LineAutoRemove_setup(j_setup, WhitePen, 2);
            for (double index = -2 * h1 + Pitch * h1 / 10; index <= 2 * h1 + Pitch * h1 / 10; index += h1)
            {
                x1 = StartX - index * Math.Sin(Math.PI * Roll / 180) - R1 * Math.Cos(Math.PI * Roll / 180);
                y1 = StartY + index * Math.Cos(Math.PI * Roll / 180) - R1 * Math.Sin(Math.PI * Roll / 180);
                x2 = StartX - index * Math.Sin(Math.PI * Roll / 180) + R1 * Math.Cos(Math.PI * Roll / 180);
                y2 = StartY + index * Math.Cos(Math.PI * Roll / 180) + R1 * Math.Sin(Math.PI * Roll / 180);
                Pitch_LineAutoRemove(indexLine, x1, y1, x2, y2);
                indexLine++;
            }
            //Vẽ các đường không ghi số
            R2 = R1 / 2;
            for (double index = -1.5 * h1 + (Pitch * h1 / 10); index <= 1.5 * h1 + (Pitch * h1 / 10); index += h1)
            {
                x1 = StartX - index * Math.Sin(Math.PI * Roll / 180) - R2 * Math.Cos(Math.PI * Roll / 180);
                y1 = StartY + index * Math.Cos(Math.PI * Roll / 180) - R2 * Math.Sin(Math.PI * Roll / 180);
                x2 = StartX - index * Math.Sin(Math.PI * Roll / 180) + R2 * Math.Cos(Math.PI * Roll / 180);
                y2 = StartY + index * Math.Cos(Math.PI * Roll / 180) + R2 * Math.Sin(Math.PI * Roll / 180);
                Pitch_LineAutoRemove(indexLine, x1, y1, x2, y2);
                indexLine++;
            }
            //Vẽ đường chân trời qua điểm 0, 0
            //Chỉ số là 9 màu xanh ngày 29/02/2016 bỏ Vẽ đường chân trời qua điểm 0, 0
            /*
            R2 = StartX;
            x1 = StartX - (Pitch * h1 / 10) * Math.Sin(Math.PI * Roll / 180) - R2 * Math.Cos(Math.PI * Roll / 180);
            y1 = StartY + (Pitch * h1 / 10) * Math.Cos(Math.PI * Roll / 180) - R2 * Math.Sin(Math.PI * Roll / 180);
            x2 = StartX - (Pitch * h1 / 10) * Math.Sin(Math.PI * Roll / 180) + R2 * Math.Cos(Math.PI * Roll / 180);
            y2 = StartY + (Pitch * h1 / 10) * Math.Cos(Math.PI * Roll / 180) + R2 * Math.Sin(Math.PI * Roll / 180);
            Pitch_LineAutoRemove(indexLine, WhitePen, 2, x1, y1, x2, y2);
            */
            indexLine++;//đổi chỉ số đường khác cho đường tiếp theo
                        //Vẽ String ở hai bên
                        //Vẽ String
                        //Ghi chữ
                        //-10 là độ dời chữ lên trên
                        //+ 22 là dời sang trái
                        //x1 = StartX - (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
                        //y1 = StartY + (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
                        //x2 = StartX - (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
                        //y2 = StartY + (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
                        //Pitch_SetupString_AutoRemove(0, Roll, "20", 16, WhitePen, x1, y1, 1);
                        //Pitch_SetupString_AutoRemove(1, Roll, "20", 16, WhitePen, x2, y2, 1);
                        ////********************
                        //x1 = StartX - (-h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
                        //y1 = StartY + (-h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
                        //x2 = StartX - (-h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
                        //y2 = StartY + (-h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
                        //Pitch_SetupString_AutoRemove(2, Roll, "10", 16, WhitePen, x1, y1, 1);
                        //Pitch_SetupString_AutoRemove(3, Roll, "10", 16, WhitePen, x2, y2, 1);
                        ////********************
                        //x1 = StartX - ((Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 15) * Math.Cos(Math.PI * Roll / 180);
                        //y1 = StartY + ((Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 15) * Math.Sin(Math.PI * Roll / 180);
                        //x2 = StartX - ((Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
                        //y2 = StartY + ((Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
                        //Pitch_SetupString_AutoRemove(4, Roll, "0", 16, WhitePen, x1, y1, 1);
                        //Pitch_SetupString_AutoRemove(5, Roll, "0", 16, WhitePen, x2, y2, 1);
                        ////********************
                        //x1 = StartX - (h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
                        //y1 = StartY + (h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
                        //x2 = StartX - (h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
                        //y2 = StartY + (h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
                        //Pitch_SetupString_AutoRemove(6, Roll, "10", 16, WhitePen, x1, y1, 1);
                        //Pitch_SetupString_AutoRemove(7, Roll, "10", 16, WhitePen, x2, y2, 1);
                        ////********************
                        //x1 = StartX - (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
                        //y1 = StartY + (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
                        //x2 = StartX - (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
                        //y2 = StartY + (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
                        //Pitch_SetupString_AutoRemove(8, Roll, "20", 16, WhitePen, x1, y1, 1);
                        //Pitch_SetupString_AutoRemove(9, Roll, "20", 16, WhitePen, x2, y2, 1);
                        //Vẽ string ở bên phải
            x1 = StartX - (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
            y1 = StartY + (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
            x2 = StartX - (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
            y2 = StartY + (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
            //Pitch_SetupString_AutoRemove(0, Roll, "20", 16, WhitePen, x1, y1, 1);
            Pitch_SetupString_AutoRemove(1, Roll, "20", 16, WhitePen, x2, y2, 1);
            //********************
            x1 = StartX - (-h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
            y1 = StartY + (-h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
            x2 = StartX - (-h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
            y2 = StartY + (-h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
            //Pitch_SetupString_AutoRemove(2, Roll, "10", 16, WhitePen, x1, y1, 1);
            Pitch_SetupString_AutoRemove(3, Roll, "10", 16, WhitePen, x2, y2, 1);
            //********************
            x1 = StartX - ((Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 15) * Math.Cos(Math.PI * Roll / 180);
            y1 = StartY + ((Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 15) * Math.Sin(Math.PI * Roll / 180);
            x2 = StartX - ((Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
            y2 = StartY + ((Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
            //Pitch_SetupString_AutoRemove(4, Roll, "0", 16, WhitePen, x1, y1, 1);
            Pitch_SetupString_AutoRemove(5, Roll, "0", 16, WhitePen, x2, y2, 1);
            //********************
            x1 = StartX - (h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
            y1 = StartY + (h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
            x2 = StartX - (h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
            y2 = StartY + (h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
            //Pitch_SetupString_AutoRemove(6, Roll, "10", 16, WhitePen, x1, y1, 1);
            Pitch_SetupString_AutoRemove(7, Roll, "10", 16, WhitePen, x2, y2, 1);
            //********************
            x1 = StartX - (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
            y1 = StartY + (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
            x2 = StartX - (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
            y2 = StartY + (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
            //Pitch_SetupString_AutoRemove(8, Roll, "20", 16, WhitePen, x1, y1, 1);
            Pitch_SetupString_AutoRemove(9, Roll, "20", 16, WhitePen, x2, y2, 1);

            //Ngày 21/12/2015 Vẽ góc Pitch
            //h1 <--> 10 degree: (- Pitch * h1 / 10)
            R2 = 2 * R1;
            x1 = StartX - R2;
            y1 = StartY;
            //Vẽ đường ngang của Pitch bằng line
            //Pitch_LineAutoRemove(indexLine, new SolidColorBrush(Colors.Yellow), 8, x1, y1, 16, WhitePen, x2, y2, 1);
            //Bằng Rectangle tốt hơn
            Pitch_Draw_Rect_AutoRemove(0, new SolidColorBrush(Colors.Yellow), x1 - 30, y1 - 3, 30, 8, 1);
            //Vẽ một chấm đỏ hình chữ nhật ngay trung tâm
            Pitch_Draw_Rect_AutoRemove(2, new SolidColorBrush(Colors.Red), StartX - 4, y1 - 3, 8, 8, 1);
            //Vẽ mũi tên hình tam giác tại đầu mũi đường cho đẹp
            Pitch_ArrowAuto_Remove(0, new SolidColorBrush(Colors.Yellow), x1, y1 - 2, x1, y1 + 6, x1 + 8, y1 + 2, 1);

            indexLine++;
            //*****************************************
            x1 = StartX + R2;
            y1 = StartY;

            //Pitch_LineAutoRemove(indexLine, new SolidColorBrush(Colors.Yellow), 8, x1, y1, x2, y2);
            //Bằng Rectangle tốt hơn
            Pitch_Draw_Rect_AutoRemove(1, new SolidColorBrush(Colors.Yellow), x1, y1 - 3, 30, 8, 1);
            //Vẽ mũi tên hình tam giác tại đầu mũi đường cho đẹp
            Pitch_ArrowAuto_Remove(1, new SolidColorBrush(Colors.Yellow), x1, y1 - 2, x1, y1 + 6, x1 - 8, y1 + 2, 1);

            //

        }
        //************************************************************************************
        //**************************************************************************
        //Ngày 20/12/2015 Vẽ góc Pitch

        /// <summary>
        /// Vẽ Line Auto Remove cho Pitch
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ColorOfLine"></param>
        /// <param name="SizeOfLine"></param>
        void Pitch_LineAutoRemove_setup(int index, SolidColorBrush ColorOfLine, double SizeOfLine)
        {
            //BackgroundDisplay.Children.Remove(LinePitchAutoRemove[index]);
            LinePitchAutoRemove[index] = new Line();
            //Điểm bắt đầu trên cùng có tọa độ 0, 0
            //Line LinePitchAutoRemove[index] = new Line();
            //BackgroundDisplay.Children.Remove(LinePitchAutoRemove[index]);
            //LinePitchAutoRemove[index].Fill = new SolidColorBrush(Colors.Green);
            LinePitchAutoRemove[index].Stroke = ColorOfLine;
            //LinePitchAutoRemove[index].
            //LinePitchAutoRemove[index].Height = 10;
            //LinePitchAutoRemove[index].Width = 10;

            LinePitchAutoRemove[index].StrokeThickness = SizeOfLine;
            //Xac định tọa độ
            //LinePitchAutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(-1500, -200, 0, 0);
            //BackgroundDisplay.Children.Add(LinePitchAutoRemove[index]);

        }
        Line[] LinePitchAutoRemove = new Line[12];
        /// <summary>
        /// Vẽ đường thẳng từ (x1, y1) đến (x2, y2)
        /// Bút vẽ là ColorOfLine
        /// độ rông là SizeOfLine
        /// index: chỉ số của đường
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        void Pitch_LineAutoRemove(int index, double x1, double y1, double x2, double y2)
        {
            BackgroundDisplay.Children.Remove(LinePitchAutoRemove[index]);
            //LinePitchAutoRemove[index] = new Line();
            //Điểm bắt đầu trên cùng có tọa độ 0, 0
            //Line LinePitchAutoRemove[index] = new Line();
            //BackgroundDisplay.Children.Remove(LinePitchAutoRemove[index]);
            //LinePitchAutoRemove[index].Fill = new SolidColorBrush(Colors.Green);
            //LinePitchAutoRemove[index].Stroke = ColorOfLine;
            //LinePitchAutoRemove[index].
            //LinePitchAutoRemove[index].Height = 10;
            //LinePitchAutoRemove[index].Width = 10;
            LinePitchAutoRemove[index].X1 = x1;
            LinePitchAutoRemove[index].Y1 = y1;
            LinePitchAutoRemove[index].X2 = x2;
            LinePitchAutoRemove[index].Y2 = y2;
            //LinePitchAutoRemove[index].StrokeThickness = SizeOfLine;
            //Xac định tọa độ
            //LinePitchAutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(-1500, -200, 0, 0);
            BackgroundDisplay.Children.Add(LinePitchAutoRemove[index]);

        }
        //**************************************************************************
        //*****************************************************************
        //Ngày 20/12/2015 bước đột phá tạo một mảng TextBlock Auto remove
        TextBlock[] Tb_Pitch = new TextBlock[10];
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// Set up init location for string
        /// index: index of string
        /// Roll góc nghiêng của string
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void Pitch_SetupString_AutoRemove(int index, double Roll, string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            //create graphic text block design text
            //TextBlock Tb_Pitch[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_Pitch[index].Height = HeightOfBlock;
            //Tb_Pitch[index].Width = WidthOfBlock;
            //canh lề, left, right, center
            BackgroundDisplay.Children.Remove(Tb_Pitch[index]);
            Tb_Pitch[index] = new TextBlock();
            Tb_Pitch[index].HorizontalAlignment = HorizontalAlignment.Left;
            Tb_Pitch[index].VerticalAlignment = VerticalAlignment.Top;
            //Tb_Pitch[index].Margin = 
            //
            //đảo chữ
            Tb_Pitch[index].TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            Tb_Pitch[index].Text = drawString;
            Tb_Pitch[index].FontSize = SizeOfText;
            Tb_Pitch[index].FontFamily = new FontFamily("Arial");
            //Tb_Pitch[index].FontStyle = "Arial";
            //Tb_Pitch[index].FontStretch
            //color text có độ đục
            //Tb_Pitch[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            Tb_Pitch[index].Foreground = Blush;
            Tb_Pitch[index].Opacity = Opacity;
            //Quay Textblock để quay chữ
            Tb_Pitch[index].RenderTransform = new RotateTransform()
            {
                Angle = Roll,
                //CenterX = 25, //The prop name maybe mistyped 
                //CenterY = 25 //The prop name maybe mistyped 
            };
            //position of text left, top, right, bottom
            Tb_Pitch[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_Pitch[index]);
        }
        //**********************************************************************************************
        /// <summary>
        /// Remove chỗi string cũ
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// Set up init location for string
        /// index: index of string
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void Pitch_ChangeString_AutoRemove(int index, double Roll, string drawString,
            double StartX, double StartY)
        {
            //create graphic text block design text
            //TextBlock Tb_Pitch[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_Pitch[index].Height = HeightOfBlock;
            //Tb_Pitch[index].Width = WidthOfBlock;
            //canh lề, left, right, center

            //Tb_Pitch[index].Margin = 
            //

            BackgroundDisplay.Children.Remove(Tb_Pitch[index]);
            //Quay Textblock để quay chữ
            Tb_Pitch[index].RenderTransform = new RotateTransform()
            {
                Angle = Roll,
                //CenterX = 25, //The prop name maybe mistyped 
                //CenterY = 25 //The prop name maybe mistyped 
            };
            Tb_Pitch[index].Text = drawString;


            //Tb_Pitch[index].FontStyle = "Arial";
            //Tb_Pitch[index].FontStretch
            //color text có độ đục
            //Tb_Pitch[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));

            //position of text left, top, right, bottom
            Tb_Pitch[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_Pitch[index]);
        }
        //*********************************************************************************************
        //************************************************************
        //Ngày 14/1/2/2015 22h39 đã hoàn thành việc vẽ tam giác đúng vị trí
        //Hoàn thahf vẽ display cảm biến gia tốc
        Polygon[] Pitch_ArrowAutoRemove = new Polygon[2];
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// tam giác cũ sẽ tự remove khi tam giác mới xuất hiện
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void Pitch_ArrowAuto_Remove(int index, SolidColorBrush BlushOfTriAngle, double x1, double y1,
            double x2, double y2, double x3, double y3, double Opacity)
        {
            double xmin, ymin, xmax, ymax;
            xmin = Math.Min(x1, Math.Min(x2, x3));
            ymin = Math.Min(y1, Math.Min(y2, y3));
            xmax = Math.Max(x1, Math.Max(x2, x3));
            ymax = Math.Max(y1, Math.Max(y2, y3));
            //Draw

            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point((x1 - xmin), (y1 - ymin)));
            myPointCollection.Add(new Point(x2 - xmin, y2 - ymin));
            myPointCollection.Add(new Point(x3 - xmin, y3 - ymin));
            //myPointCollection.Add(new Point(0.025, 0.005 * sliderAdjSpeed.Value));
            //Polygon Pitch_ArrowAutoRemove[index] = new Polygon();
            BackgroundDisplay.Children.Remove(Pitch_ArrowAutoRemove[index]);
            Pitch_ArrowAutoRemove[index] = new Polygon();
            Pitch_ArrowAutoRemove[index].Points = myPointCollection;
            Pitch_ArrowAutoRemove[index].Fill = BlushOfTriAngle;
            Pitch_ArrowAutoRemove[index].Width = (xmax - xmin);
            Pitch_ArrowAutoRemove[index].Height = (ymax - ymin);
            Pitch_ArrowAutoRemove[index].Stretch = Stretch.Fill;
            Pitch_ArrowAutoRemove[index].Stroke = BlushOfTriAngle;
            Pitch_ArrowAutoRemove[index].Opacity = Opacity;
            Pitch_ArrowAutoRemove[index].StrokeThickness = 1;
            //Xac định tọa độ -1856, -491 là dời về 0, 0
            //quá trình khảo sát trong vở
            //Pitch_ArrowAutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(-2158 + Pitch_ArrowAutoRemove[index].Width - (200 - 2 * xmin), -600 + Pitch_ArrowAutoRemove[index].Height, 0, 0);
            //Pitch_ArrowAutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + xmax + xmin, -600 + Pitch_ArrowAutoRemove[index].Height + 4 * sliderAdjSpeed.Value, 0, 0);
            //Quá trình khảo sát y và tính sai số
            Pitch_ArrowAutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xmax + xmin, -800 + dConvertToTabletY + ymax + ymin, 0, 0);
            //Pitch_ArrowAutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(-2060, -491, 0, 0);
            BackgroundDisplay.Children.Add(Pitch_ArrowAutoRemove[index]);

        }
        //Bước đột phá, tạo mảng hình chữ nhật auto remove biến này là biến toàn cục
        Rectangle[] Pitch_Ret_AutoRemove = new Rectangle[3];

        //Set up for rectangle
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// index là số thứ tự hình chữ nhật auto remove
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Pitch_Draw_Rect_AutoRemove(int index, SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(Pitch_Ret_AutoRemove[index]);
            Pitch_Ret_AutoRemove[index] = new Rectangle();
            Pitch_Ret_AutoRemove[index].Fill = Blush;
            Pitch_Ret_AutoRemove[index].Height = height;
            Pitch_Ret_AutoRemove[index].Width = width;
            Pitch_Ret_AutoRemove[index].Opacity = Opacity;
            //Xac định tọa độ
            Pitch_Ret_AutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + Pitch_Ret_AutoRemove[index].Width + StartX * 2, -798 + dConvertToTabletY + Pitch_Ret_AutoRemove[index].Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(Pitch_Ret_AutoRemove[index]);

        }
        //********************************************************************************
        //Ngày 21/12/2015 vẽ góc Pitch And Roll
        /// <summary>
        /// Set up góc Pitch and Roll
        /// Pitch: Pitch Angle Of Flight
        /// Roll: Roll Angle Of Flight
        /// CenterX, CenterY: Center Coordinate
        /// Radious: Radious of arc to Draw Roll Angle
        /// DisTwoLine: Distance between two line in display of Pitch Angle
        /// </summary>
        /// <param name="Pitch"></param>
        /// <param name="Roll"></param>
        /// <param name="CenterX"></param>
        /// <param name="CenterY"></param>
        /// <param name="Radious"></param>
        /// <param name="DisTwoLine"></param>
        void PitchAndRoll_Setup(double Pitch, double Roll, double CenterX, double CenterY, double Radious, double DisTwoLine)
        {
            RollAngle_Setup(Roll, CenterX, CenterY, Radious);
            //PitchAngle_Setup(Pitch, -Roll, CenterX, CenterY, DisTwoLine);
            Background_Pitch_Roll_Setup(0, 0, CenterX, CenterY);
        }
        //*****************************************************************
        //********************************************************************************
        //Ngày 21/12/2015 vẽ góc Pitch And Roll
        /// <summary>
        /// Set up góc Pitch and Roll
        /// Pitch: Pitch Angle Of Flight
        /// Roll: Roll Angle Of Flight
        /// CenterX, CenterY: Center Coordinate
        /// Radious: Radious of arc to Draw Roll Angle
        /// DisTwoLine: Distance between two line in display of Pitch Angle
        /// </summary>
        /// <param name="Pitch"></param>
        /// <param name="Roll"></param>
        /// <param name="CenterX"></param>
        /// <param name="CenterY"></param>
        /// <param name="Radious"></param>
        /// <param name="DisTwoLine"></param>
        void PitchAndRoll_Draw(double Roll, double Pitch, double CenterX, double CenterY, double Radious, double DisTwoLine)
        {
            RollAngle_Draw_TriAngle(Roll, CenterX, CenterY, Radious);//đã tối ưu ngày 3/3/2016
            //Pitch_Draw_Angle(Pitch, Roll, CenterX, CenterY, DisTwoLine);//đã tối ưu ngày 4/3/2016 lúc 0h 39
            //Ngày 12/3/2016 xoay hình
            Draw_RollAndPitch_optimize(Roll, Pitch, CenterX, CenterY);
        }


        //**************************************************###################################################
        //************************************************************************************
        //************************************************************************************
        //************************************************************************************
        //************************************************************************************
        //************************************************************************************
        //************************************************************************************
        //********************Đồ Án 2*********************************************************
        //Ngày 29/12/2015 Vẽ quỹ đạo bay
        //draw line in map 2D
        //Vẽ quỹ đạo là nét liền nên ta dùng 2 biến tạm để lưu giá trị cũ của Lat, Lon
        double old_Lat = 0, old_Lon = 0;
        //Vẽ quỹ đạo
        Windows.UI.Xaml.Controls.Maps.MapPolyline mapPolyline_AddFlight = new Windows.UI.Xaml.Controls.Maps.MapPolyline();
        /// <summary>
        /// Chấm điểm có màu vàng tại vị trí lat, lon, Alt
        /// Vẽ vị trí máy bay và góc quay của máy bay
        /// Vẽ đường thẳng nối tới điểm đích
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        /// <param name="dHeading"></param>
        void Draw_Trajectory_And_Flight(double lat, double lon, double alt, double dHeading)
        {

            //************************Vẽ Máy bay***********************************
            //myMap.Children.Remove(img);
            //myMap.ZoomLevel = 12;

            //Edit size of image
            imageOfFlight.Height = 10 * myMap.ZoomLevel;
            imageOfFlight.Width = 10 * myMap.ZoomLevel;
            //imageOfFlight.Height = 10 * 10;
            //imageOfFlight.Width = 10 * 10;

            //img.RenderTransform
            imageOfFlight.Opacity = 0.7;


            //img.Transitions.
            imageOfFlight.Source = new BitmapImage(new Uri("ms-appx:///Assets/airplane-icon.png"));
            //Xoay ảnh
            //kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            //Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            //khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            //Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            //dời ảnh lên trên 1 nửa chiều dài,
            //dời ảnh sang trái 1 nửa chiều rộng


            //mặc định ảnh có chiều dài và chiều rộng là vô cùng
            //bitmapImage.PixelHeight
            //img.sca
            imageOfFlight.Stretch = Stretch.Uniform;
            imageOfFlight.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            imageOfFlight.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;

            imageOfFlight.RenderTransform = new RotateTransform()
            {

                Angle = dHeading,
                CenterX = 10 * myMap.ZoomLevel / 2,
                CenterY = 10 * myMap.ZoomLevel / 2 //The prop name maybe mistyped 
                //CenterX = slider_test.Value * 10 / 2,
                //CenterY = slider_test.Value * 10 / 2 //The prop name maybe mistyped 
            };

            imageOfFlight.Margin = new Windows.UI.Xaml.Thickness(-10 * myMap.ZoomLevel / 2, -10 * myMap.ZoomLevel / 2, 0, 0);
            //imageOfFlight.Margin = new Windows.UI.Xaml.Thickness(-0, 0, 0, 0);



            Geopoint Position = new Geopoint(new BasicGeoposition()
            {
                Latitude = lat,
                Longitude = lon,
                Altitude = 00.0
            });
            //myMap.Children.Add(bitmapImage);
            //Dặ Ảnh đúng vị trí
            Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(imageOfFlight, Position);

            //myMap.TrySetViewBoundsAsync()
            //Độ dài tương đối của hình so với vị trí mong muốn new Point(0.5, 0.5) không dời
            //Windows.UI.Xaml.Controls.Maps.MapControl.SetNormalizedAnchorPoint(imageOfFlight, new Point(0.5, 0.5));
            try
            {
                if (old_Lat != 0.0)//Vì lúc đầu chưa có dữ liệu nên k hiện máy bay
                    myMap.Children.Add(imageOfFlight);
                //BackgroundDisplay.Children.Add(imageOfFlight);
            }
            catch { }


            //Vẽ quỹ đạo



            mapPolyline.StrokeColor = Colors.Red;
            mapPolyline.StrokeThickness = 2;
            mapPolyline.StrokeDashed = false;//nét liền

            //Ve duong thang den dentination
            //San bay tan son nhat:  dLatDentination, dLonDentination google map
            polylineHereToDentination.StrokeColor = Colors.Blue;
            polylineHereToDentination.StrokeThickness = 2;
            polylineHereToDentination.StrokeDashed = false;//nét liền

            Map_DrawLine_2D(lat, lon, 10.818345, 106.658897);
            if (old_Lat != 0.0)//Vì lúc đầu chưa có dữ liệu nên k hiện máy bay
            {
                //Windows.UI.Xaml.Controls.Maps.MapPolyline mapPolyline = new Windows.UI.Xaml.Controls.Maps.MapPolyline();
                mapPolyline.Path = new Geopath(new List<BasicGeoposition>() {
                new BasicGeoposition() {Latitude = old_Lat, Longitude = old_Lon, Altitude = alt + 0.00005},
                //San Bay Tan Son Nhat
                new BasicGeoposition() {Latitude = lat, Longitude = lon, Altitude = alt - 0.00005},
            });
                myMap.MapElements.Add(mapPolyline);


                myMap.MapElements.Add(polylineHereToDentination);
            }
            //Updata giá trí mới
            old_Lat = lat;
            old_Lon = lon;

        }

        /// <summary>
        /// collect point to draw trajactory
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        /// <param name="dHeading"></param>
        void Draw_Trajectory_optimize(double lat, double lon, double alt, double dHeading)
        {
            if (0 != lat)
                positions.Add(new BasicGeoposition() { Latitude = lat, Longitude = lon });   //<== this

        }
        //*****************************************
        //Ảnh này là biến toàn cục vì nó có được sử dụng trong chưa trình ngắt
        Image imageOfFlight = new Image();
        //*****************************************
        //Ngày 04/03/2016 tối ưu vẽ máy bay
        //********************Đồ Án 2*********************************************************
        //Ngày 29/12/2015 Vẽ quỹ đạo bay
        //draw line in map 2D
        //Vẽ quỹ đạo là nét liền nên ta dùng 2 biến tạm để lưu giá trị cũ của Lat, Lon
        //double old_Lat, old_Lon;
        /// <summary>
        /// Chấm điểm có màu vàng tại vị trí lat, lon, Alt
        /// Vẽ vị trí máy bay và góc quay của máy bay
        /// Vẽ đường thẳng nối tới điểm đích
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        /// <param name="dHeading"></param>
        void Draw_Trajectory_And_Flight_optimize(double lat, double lon, double alt, double dHeading)
        {

            //************************Vẽ Máy bay***********************************
            myMap.Children.Remove(imageOfFlight);


            //Edit size of image
            imageOfFlight.Height = 4 * myMap.ZoomLevel;
            imageOfFlight.Width = 4 * myMap.ZoomLevel;
            //imageOfFlight.
            imageOfFlight.RenderTransform = new RotateTransform()
            {

                Angle = dHeading,
                //Angle = 0,
                CenterX = 4 * myMap.ZoomLevel / 2,
                CenterY = 4 * myMap.ZoomLevel / 2 //The prop name maybe mistyped 
            };
            //mặc định ảnh có chiều dài và chiều rộng là vô cùng


            imageOfFlight.Margin = new Windows.UI.Xaml.Thickness(-4 * myMap.ZoomLevel / 2, -4 * myMap.ZoomLevel / 2, 0, 0);


            Geopoint Position = new Geopoint(new BasicGeoposition()
            {
                Latitude = lat,
                Longitude = lon,
                //Altitude = 200.0
            });
            //myMap.Children.Add(bitmapImage);
            Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(imageOfFlight, Position);

            //Vẽ quỹ đạo

            //if (old_Lat != 0.0)//Vì lúc đầu chưa có dữ liệu nên k hiện máy bay
            //    positions.Add(new BasicGeoposition() { Latitude = old_Lat, Longitude = old_Lon });   //<== this
            //                                                                                         // Now add your positions:
            if (0 != old_Lat)
            {
                positions.Add(new BasicGeoposition() { Latitude = lat, Longitude = lon });//to turn on auto zoom mode

                //Vẽ quỹ đạo
                MapPolyline lineToRmove = new Windows.UI.Xaml.Controls.Maps.MapPolyline();

                lineToRmove.Path = new Geopath(new List<BasicGeoposition>() {
                new BasicGeoposition() {Latitude = old_Lat, Longitude = old_Lon},
                //San Bay Tan Son Nhat
                new BasicGeoposition() {Latitude = lat, Longitude = lon}
                });

                lineToRmove.StrokeColor = Colors.Red;
                lineToRmove.StrokeThickness = 2;
                lineToRmove.StrokeDashed = false;//nét liền

                //myMap.MapElements.Remove(mapPolyline);
                myMap.MapElements.Add(lineToRmove);


                //auto zoom
                if (bAutoZoom)
                    SetMapPolyline(positions);


                myMap.Children.Add(imageOfFlight);

                //Ve duong thang den dentination
                //San bay tan son nhat:  dLatDentination, dLonDentination google map

                myMap.MapElements.Remove(polylineHereToDentination);
                Map_DrawLine_2D(lat, lon, dLatDentination, dLonDentination);

                myMap.MapElements.Add(polylineHereToDentination);
            }


            //Updata giá trí mới
            old_Lat = lat;
            old_Lon = lon;
        }

        //test remove polyline
        List<Windows.UI.Xaml.Controls.Maps.MapPolyline> polyLineToRemove = new List<Windows.UI.Xaml.Controls.Maps.MapPolyline>();
        //28/5/2016 Add auto zoom to map
        //auto zoom
        public async void SetMapPolyline(List<BasicGeoposition> geoPositions)
        {
            //var polyLine = new MapPolyline { Path = new Geopath(geoPositions), StrokeThickness = 4, StrokeColor = (Color)App.Current.Resources["StravaRedColor"] };
            //myMap.MapElements.Add(polyLine);

            await myMap.TrySetViewBoundsAsync(GeoboundingBox.TryCompute(geoPositions), null, MapAnimationKind.None);
        }

        //*******************************************************
        //Ngày 10/1/2016
        /// <summary>
        /// Hiện thị các dữ liệu lên giao diện map
        /// </summary>
        /// <param name="dPitch"></param>
        /// <param name="dRoll"></param>
        /// <param name="dSpeed"></param>
        /// <param name="dAlt"></param>
        /// <param name="dVerSpeed"></param>
        /// <param name="dAngle"></param>
        void DisplayDataOnMap(double dPitch, double dRoll, double dSpeed,
                        double dAlt, double dVerSpeed, double dAngle)
        {
            //Luôn cho vận tốc >= 0, độ cao >= 0
            if (dSpeed < 0) dSpeed = 0;
            if (dAlt < 0) dAlt = 0;
            Draw_Airspeed_full_optimize(dSpeed, 150 - 32 + i16EditPosition, 205);//ok

            //Altitude_Draw_Alt(dAlt, 550, 100);
            Draw_Alttitude_full_optimize(dAlt, 550 + 88 / 2 + i16EditPosition * 17 / 6, 80);//ok

        }

        //************************************************************************
        bool bSetup = false;
        bool bOneScreen = false;
        ///// <summary>
        ///// 1 nut thuc thi 2 chuc nang: One and Two Screen
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void OneSceen_Click(object sender, RoutedEventArgs e)
        //{
        //    //myMap.Margin = new Windows.UI.Xaml.Thickness(0, 0, 00, 00);
        //    bOneScreen = !bOneScreen;
        //    if (bOneScreen)
        //    {
        //        Background_Sensor(00, -80);

        //        btOneSceen.Content = "2 Screen";
        //    }
        //    else
        //    {
        //        //Background_Sensor(700, -80);
        //        myMap.MapElements.Remove(polylineHereToDentination);//delete polyline old before reload
        //        DisplaySensor_Setup();
        //        btOneSceen.Content = "1 Screen";
        //    }
        //}
        //*********************************************************************
        Rectangle TestRet_BackGround = new Rectangle();
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void FillRect_BackGround(SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            BackgroundDisplay.Children.Remove(TestRet_BackGround);
            TestRet_BackGround.Fill = Blush;
            TestRet_BackGround.Height = height;
            TestRet_BackGround.Width = width;
            TestRet_BackGround.Opacity = Opacity;
            //Xac định tọa độ
            TestRet_BackGround.Margin = new Windows.UI.Xaml.Thickness(
                -2358 + dConvertToTabletX + TestRet_BackGround.Width + StartX * 2, -798 + dConvertToTabletY + TestRet_BackGround.Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(TestRet_BackGround);
        }
        //********************************************************************************
        //Ngay 20/1/2016
        //Tinh goc giua 2 diem
        public double angleFromCoordinate(double lat1, double long1, double lat2,
        double long2)
        {
            //convert degrees to rad
            lat1 = lat1 * Math.PI / 180;
            long1 = long1 * Math.PI / 180;
            lat2 = lat2 * Math.PI / 180;
            long2 = long2 * Math.PI / 180;
            double dLon = (long2 - long1);
            double y = Math.Sin(dLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1)
                    * Math.Cos(lat2) * Math.Cos(dLon);
            double brng = Math.Atan2(y, x);
            //Convert to Degrees
            brng = brng * 180 / Math.PI;
            brng = (brng + 360) % 360;
            //brng = 360 - brng;
            return Math.Round(brng, 2);
        }

        bool bAutoZoom = false;//zoom in trajectory of flight


        /// <summary>
        /// save content to C:\Users\VANCHUC-PC\AppData\Local\Packages\
        /// 54fa2b45-b04f-4b40-809b-7556c7ed473f_pq4mhrhe9d4xp\LocalState\dataReceive.txt
        /// Note: Close dataReceive.txt before write
        /// </summary>
        /// <param name="content"></param>
        public async void SaveTotxt(string content)
        {
            //Windows.Storage.FileIO.ReadLinesAsync()
            //Windows.Storage.FileIO.ReadLinesAsync()
            await Windows.Storage.FileIO.AppendTextAsync(sampleFile, content);

        }

        //global variable
        FileOpenPicker openPicker;
        StreamReader streamReader;
        bool setupReadfile = false;
        double maxSpeed = 0, maxAltitude = 0;
        /// <summary>
        /// set up read file text
        /// </summary>
        async void Setup_ReadFromFile()
        {


            //c2
            openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            //dinh huong trong tuong lai
            //picker.FileTypeFilter.Add(".jpg");
            //picker.FileTypeFilter.Add(".docx");
            //picker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".txt");

            Windows.Storage.StorageFile file = await openPicker.PickSingleFileAsync();

            //inputStream = await file.OpenReadAsync();
            //using (var classicStream = inputStream.AsStreamForRead())
            Stream stream = (await file.OpenReadAsync()).AsStreamForRead();

            streamReader = new StreamReader(stream);
            setupReadfile = true;
            Int32 index = 0;

            //streamReader.BaseStream.Seek(-1, SeekOrigin.End);   //đưa con trỏ về cuối của file
            //double _position = streamReader.BaseStream.Position;//lấy số ký tự của file txt
            //streamReader.BaseStream.Seek(-2000, SeekOrigin.Current);//dịch con trỏ từ cuối file về lui khoảng 2000 ký tự
            //chúng ta sẽ đọc được thời điểm bay cuối cùng
            while (streamReader.Peek() >= 0)

            {

                strDataFromSerialPort = streamReader.ReadLine().ToString();

                {

                    processDataToDrawTrajactory();


                    index++;
                    if ((index == (5)))
                    {
                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(0.5));
                        index = 0;
                    }
                }

            }

        }
        /// <summary>
        /// return start time, End of time
        /// </summary>
        async void ReadInfOfFile()
        {

            try
            {
                openPicker = new Windows.Storage.Pickers.FileOpenPicker();
                openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                openPicker.SuggestedStartLocation =
                    Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;

                openPicker.FileTypeFilter.Add(".txt");

                Windows.Storage.StorageFile file = await openPicker.PickSingleFileAsync();

                Stream stream = (await file.OpenReadAsync()).AsStreamForRead();

                streamReader = new StreamReader(stream);
                setupReadfile = true;
                //Int32 index = 0;
                Data.Time = null;
                while (null == Data.Time)

                {
                    strDataFromSerialPort = streamReader.ReadLine();

                    processDataToGetInf();

                }
                sStartTime = Data.Time;
                //format hour:min:sec
                if (-1 != Data.Time.IndexOf('.'))//have '.' in Data.Time 82754.7
                    tblock_Start_Timer.Text = Data.Time.Substring(0, Data.Time.Length - 6) + ':'
                            + Data.Time.Substring(Data.Time.Length - 6, 2) + ':' + Data.Time.Substring(Data.Time.Length - 4, 4);
                else
                    tblock_Start_Timer.Text = Data.Time.Substring(0, Data.Time.Length - 4) + ':'
                            + Data.Time.Substring(Data.Time.Length - 4, 2) + ':' + Data.Time.Substring(Data.Time.Length - 2, 2);

                streamReader.BaseStream.Seek(-1, SeekOrigin.End);   //đưa con trỏ về cuối của file
                                                                    //double _position = streamReader.BaseStream.Position;//lấy số ký tự của file txt
                streamReader.BaseStream.Seek(-2000, SeekOrigin.Current);//dịch con trỏ từ cuối file về lui khoảng 2000 ký tự
                                                                        //chúng ta sẽ đọc được thời điểm bay cuối cùng
                while (streamReader.Peek() >= 0)
                {
                    strDataFromSerialPort = streamReader.ReadLine();

                    processDataToGetInf();

                }
                sStopTime = Data.Time;
                //format hour:min:sec
                if (-1 != Data.Time.IndexOf('.'))//have '.' in Data.Time 82754.7
                    tblock_End_Timer.Text = Data.Time.Substring(0, Data.Time.Length - 6) + ':'
                            + Data.Time.Substring(Data.Time.Length - 6, 2) + ':' + Data.Time.Substring(Data.Time.Length - 4, 4);
                else
                    tblock_End_Timer.Text = Data.Time.Substring(0, Data.Time.Length - 4) + ':'
                            + Data.Time.Substring(Data.Time.Length - 4, 2) + ':' + Data.Time.Substring(Data.Time.Length - 2, 2);
                Data.Time = sStartTime;
                streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                streamReader = new StreamReader(stream);
                editTimeWhenChangeslider();
            }
            catch { }



        }
        /// <summary>
        /// doc tai bat ky thoi diem nao, co the pause, play
        /// </summary>
        public void ReadAnyTime()
        {
            //c2

            setupReadfile = true;
            //Int32 index = 0;

            while (null == Data.Time)

            {
                strDataFromSerialPort = streamReader.ReadLine();

                processDataToDrawTrajactory();
            }
            sStartTime = Data.Time;
            streamReader.BaseStream.Seek(-1, SeekOrigin.End);   //đưa con trỏ về cuối của file
            //double _position = streamReader.BaseStream.Position;//lấy số ký tự của file txt
            streamReader.BaseStream.Seek(-2000, SeekOrigin.Current);//dịch con trỏ từ cuối file về lui khoảng 2000 ký tự
                                                                    //chúng ta sẽ đọc được thời điểm bay cuối cùng
            while (streamReader.Peek() >= 0)
            {
                strDataFromSerialPort = streamReader.ReadLine();

                processDataToDrawTrajactory();

            }
            sStopTime = Data.Time;
            //format hour:min:sec
            if (-1 != Data.Time.IndexOf('.'))//have '.' in Data.Time 82754.7
                tblock_End_Timer.Text = Data.Time.Substring(0, Data.Time.Length - 6) + ':'
                        + Data.Time.Substring(Data.Time.Length - 6, 2) + ':' + Data.Time.Substring(Data.Time.Length - 4, 4);
            else
                tblock_End_Timer.Text = Data.Time.Substring(0, Data.Time.Length - 4) + ':'
                        + Data.Time.Substring(Data.Time.Length - 4, 2) + ':' + Data.Time.Substring(Data.Time.Length - 2, 2);
        }

        //*************************************************************
        //Vẽ hiển thị của cảm biến
        Image imgAuto_test = new Image();
        /// <summary>
        /// top là độ dài lên trên hay xuốn dưới, xCenter, yCenter là trung tâm ảnh khi chưa cắt
        /// dRoll góc xoay
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="top"></param>
        public void Background_Pitch_Roll_Setup(double dRoll, double dPitch, double xCenter, double yCenter)
        {
            double top = -dPitch * 4;
            //BackgroundDisplay.Children.Remove(imgAuto_test);
            //Edit size of image
            imgAuto_test.Height = 1120;
            //muốn biết kích thước thì dùng paint, kích thước trong paint ;
            //size 350 x 1120;
            BackgroundDisplay.Children.Remove(imgAuto_test);
            imgAuto_test.Source = new BitmapImage(new Uri("ms-appx:///Assets/horizon.bmp"));
            imgAuto_test.Width = 350;//Ảnh này hình vuông nên Width = Height = min(Height, Width)

            //imgAuto_test.RenderTransform
            imgAuto_test.Opacity = 0.5;
            //tbShowDis.Text = imgAuto_test.Height.ToString() + " " + imgAuto_test.Width.ToString();
            //double cutY = 200;
            //imgAuto_test.Height = 1120;
            //top = 560 - 100;//560 là trung tâm y của ảnh
            //imgAuto_test.Clip = new RectangleGeometry()

            //    {
            //    Rect = new Rect(95, 460 + top, 160, 200)//các trên trung tâm y 100, dưới 100

            //    };

            //imgAuto_test.Transitions.

            //Xoay ảnh
            //kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            //Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            //khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            //Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            //dời ảnh lên trên 1 nửa chiều dài,
            //dời ảnh sang trái 1 nửa chiều rộng
            //imgAuto_test.RenderTransform = new RotateTransform()
            //{

            //    Angle = dRoll,
            //    CenterX = 175,
            //    //CenterX = 62, //The prop name maybe mistyped 
            //    CenterY = 560 + top //The prop name maybe mistyped 
            //};
            //mặc định ảnh có chiều dài và chiều rộng là vô cùng
            //bitmapImage.PixelHeight
            //imgAuto_test.sca



            //Geopoint PointCenterMap2 = new Geopoint(new BasicGeoposition()
            //{
            //    Latitude = myMap.Center.Position.Latitude + 0.001,
            //    Longitude = myMap.Center.Position.Longitude + 0.001,
            //    //Altitude = 200.0
            //});
            //myMap.Children.Add(bitmapImage);

            //Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(imgAuto_test, PointCenterMap);
            //myMap.TrySetViewBoundsAsync()
            //Độ dài tương đối của hình so với vị trí mong muốn new Point(0.5, 0.5) không dời
            //Windows.UI.Xaml.Controls.Maps.MapControl.SetNormalizedAnchorPoint(imgAuto_test, new Point(0.5, 0.5));
            //myMap.Children.Add(imgAuto_test);
            ////tbOutputText.Background.
            //ckground.a
            //thu bản đồ lại
            //myMap.Height = 500;
            //Delete các cổng com
            //ConnectDevices.IsEnabled = false;
            //myMap.Children.Remove(ConnectDevices);
            //Background tên là BackgroundDisplay
            //Khi nhấn nút chia 2 màn hình chia là 2 phần
            //phần trái là cảm biến phần bên phải là map
            //chỉnh lại vị trí của ảnh
            //imgAuto_test.Margin = new Windows.UI.Xaml.Thickness(1500 - 2000, -cutY, 00, 00);
            //đã kiểm tra ok
            //dời lên top đơn vị thì  - 2 * top
            imgAuto_test.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
            BackgroundDisplay.Children.Add(imgAuto_test);

            //Background background_forImage = new Background();
            //sliderAdjSpeed.ValueChanged += sliderAdjSpeed_ValueChanged;
            //Vẽ đường chân trời
            //*************************************************************************
            //Cách 2, giữ nguyên đường màu vàng mỗi lần góc Pitch thay đổi thì toàn bộ các đường song song 
            //chạy xuống hoặc chạy lên
            //h1 <--> 10 degree: (- Pitch * h1 / 10)
            double R1 = 40, R2;//R1, R2 là độ dài nửa đường Vẽ đường vẽ có ghi số 5, 10 và đường vẽ k ghi số
            double x1, y1;//các điểm của đường vẽ từ x1, y1 đến x2, y2
            int indexLine = 0;
            SolidColorBrush WhitePen = new SolidColorBrush(Colors.Green);


            //Ngày 21/12/2015 Vẽ góc Pitch
            //h1 <--> 10 degree: (- Pitch * h1 / 10)
            R2 = 2 * R1;
            x1 = xCenter - R2;
            y1 = yCenter;
            //Vẽ đường ngang của Pitch bằng line
            //Pitch_LineAutoRemove(indexLine, new SolidColorBrush(Colors.Yellow), 8, x1, y1, 16, WhitePen, x2, y2, 1);
            //Bằng Rectangle tốt hơn
            Pitch_Draw_Rect_AutoRemove(0, new SolidColorBrush(Colors.Yellow), x1 - 30, y1 - 3, 30, 8, 1);
            //Vẽ một chấm đỏ hình chữ nhật ngay trung tâm
            Pitch_Draw_Rect_AutoRemove(2, new SolidColorBrush(Colors.Red), xCenter - 4, y1 - 3, 8, 8, 1);
            //Vẽ mũi tên hình tam giác tại đầu mũi đường cho đẹp
            Pitch_ArrowAuto_Remove(0, new SolidColorBrush(Colors.Yellow), x1, y1 - 2, x1, y1 + 6, x1 + 8, y1 + 2, 1);

            indexLine++;
            //*****************************************
            x1 = xCenter + R2;
            y1 = yCenter;

            //Pitch_LineAutoRemove(indexLine, new SolidColorBrush(Colors.Yellow), 8, x1, y1, x2, y2);
            //Bằng Rectangle tốt hơn
            Pitch_Draw_Rect_AutoRemove(1, new SolidColorBrush(Colors.Yellow), x1, y1 - 3, 30, 8, 1);
            //Vẽ mũi tên hình tam giác tại đầu mũi đường cho đẹp
            Pitch_ArrowAuto_Remove(1, new SolidColorBrush(Colors.Yellow), x1, y1 - 2, x1, y1 + 6, x1 - 8, y1 + 2, 1);

            //


        }

        ///////////////////////////////////////////////////////////////////
        //optimize
        /// <summary>
        /// Vẽ Roll and Pitch bằng clip hình
        /// Đã tối ưu ngày 12/3/2016
        /// xCenter, yCenter là trung tâm hình.
        /// </summary>
        /// <param name="dRoll"></param>
        /// <param name="dPitch"></param>
        /// <param name="xCenter"></param>
        /// <param name="yCenter"></param>
        public void Draw_RollAndPitch_optimize(double dRoll, double dPitch, double xCenter, double yCenter)
        {
            double top, t_cut;
            t_cut = -dPitch * 4;
            if (dPitch > 10)
            {
                dPitch = 10 + (dPitch - 10) / 2;
                //t_cut = -(dPitch * 4;
                //top = -dPitch * 2;
            }
            //else

            top = -dPitch * 4;
            //BackgroundDisplay.Children.Remove(imgAuto_test);
            //Edit size of image
            imgAuto_test.Height = 1120;
            //muốn biết kích thước thì dùng paint, kích thước trong paint ;
            //size 350 x 1120;

            //BackgroundDisplay.Children.Remove(imgAuto_test);

            //imgAuto_test.Source = new BitmapImage(new Uri("ms-appx:///Assets/horizon.bmp"));
            imgAuto_test.Width = 350;//Ảnh này hình vuông nên Width = Height = min(Height, Width)

            //imgAuto_test.RenderTransform
            //imgAuto_test.Opacity = 0.6;
            //tbShowDis.Text = imgAuto_test.Height.ToString() + " " + imgAuto_test.Width.ToString();
            //double cutY = 200;
            //imgAuto_test.Height = 1120;
            //top = 560 - 100;//560 là trung tâm y của ảnh
            imgAuto_test.Clip = new RectangleGeometry()

            {
                Rect = new Rect(95, 460 + t_cut, 160, 200)//các trên trung tâm y 100, dưới 100

            };

            //imgAuto_test.Transitions.

            //Xoay ảnh
            //kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            //Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            //khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            //Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            //dời ảnh lên trên 1 nửa chiều dài,
            //dời ảnh sang trái 1 nửa chiều rộng
            imgAuto_test.RenderTransform = new RotateTransform()
            {

                Angle = dRoll,
                CenterX = 175,
                //CenterX = 62, //The prop name maybe mistyped 
                CenterY = 560 + t_cut //The prop name maybe mistyped 
            };

            imgAuto_test.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);

            //BackgroundDisplay.Children.Add(imgAuto_test);

        }

        //Vẽ hiển thị của cảm biến
        Image imgAuto_airSpeed = new Image();

        //Speed từ 0 đến 1000km/h
        /////////////////////////////////////////////////////////////////
        Image imSpeedFull = new Image();
        public void AirSpeed_Image_full_Setup(double dAirSpeed, double xCenter, double yCenter)
        {
            double top = -dAirSpeed;
            BackgroundDisplay.Children.Remove(imgAuto_airSpeed);
            //Edit size of image
            imSpeedFull.Height = 4934;
            //muốn biết kích thước thì dùng paint, kích thước trong paint;
            //size 85 x 601;
            BackgroundDisplay.Children.Remove(imSpeedFull);
            imSpeedFull.Source = new BitmapImage(new Uri("ms-appx:///Assets/Speed_Full_v2.png"));
            imSpeedFull.Width = 88;//Ảnh này hình vuông nên Width = Height = min(Height, Width)

            //imgAuto_airSpeed.RenderTransform
            //imSpeedFull.Opacity = 1;

            imSpeedFull.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
            BackgroundDisplay.Children.Add(imSpeedFull);
            //sliderAdjSpeed.ValueChanged += sliderAdjSpeed_ValueChanged;
            Speed_Draw_String_setup(dAirSpeed, xCenter + 32, yCenter - 125);//ok
            //Viết chữ Speed + đơn vị km/ h
            DrawString("Speed ", 30, new SolidColorBrush(Colors.Crimson), xCenter + 32 - 80, yCenter - 125 + 250 + 5, 0.8);
            DrawString("(Km/h)", 30, new SolidColorBrush(Colors.Crimson), xCenter + 32 - 80, yCenter - 125 + 250 + 40, 0.8);
        }
        ///////////////////////////////////////////////////////////////////////////////////////
        //optimize
        /// <summary>
        /// Vẽ Roll and Pitch bằng clip hình
        /// Đã tối ưu ngày 12/3/2016
        /// xCenter, yCenter là trung tâm hình.
        /// </summary>
        /// <param name="dRoll"></param>
        /// <param name="dPitch"></param>
        /// <param name="xCenter"></param>
        /// <param name="yCenter"></param>
        public void Draw_Airspeed_full_optimize(double dAirSpeed, double xCenter, double yCenter)
        {
            //làm tròn và chặn không cho nhỏ hơn 0
            if (dAirSpeed < 0) dAirSpeed = 0;
            dAirSpeed = Math.Round(dAirSpeed, 1);
            //yCenter = 225
            //dAirSpeed = 00;
            double top, dAirSpeed_original = dAirSpeed, t_cut;
            t_cut = -(-2168) - dAirSpeed * 4.16;
            //if (dAirSpeed < 90)
            {
                //dAirSpeed = dAirSpeed - 424;//Image 1
                //top = -(dAirSpeed * 40 / 9.6);

                //top = -(-1103) - dAirSpeed * 4.16;

                if (dAirSpeed < 70)
                    top = -(-2168) - dAirSpeed * 4.16;
                else top = -(-2168) - 70 * 4.167 - (dAirSpeed - 70) * 4.167 / 2;

                //top = -(-sliderAdjSpeed.Value) - 0 * 4.16;

                //top = -(-1103) - dAirSpeed * 4.16; đúng từ 0 đến 100
                //top = -(-1103) - 105.0 * 4.16 - (dAirSpeed - 105) * 4.168 / 2; chạy ok từ 105 đến hết
                //Edit size of image
                imSpeedFull.Height = 4934;
                //muốn biết kích thước thì dùng paint, kích thước trong paint ;
                //size 80 x 600;
                //BackgroundDisplay.Children.Remove(imSpeedFull);
                //imgAuto_test.Source = new BitmapImage(new Uri("ms-appx:///Assets/horizon.bmp"));
                imSpeedFull.Width = 88;//Ảnh này hình vuông nên Width = Height = min(Height, Width)


                imSpeedFull.Clip = new RectangleGeometry()

                {
                    Rect = new Rect(0, 2334 + t_cut, 88, 264)//các trên trung tâm y 100, dưới 100

                };

                imSpeedFull.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
                //BackgroundDisplay.Children.Add(imSpeedFull);
            }


            //Speed_Draw_Speed(dSpeed, 150, 100);

            Speed_Draw_String_optimize(dAirSpeed_original, xCenter + 32, 100);

        }
        ///////////////////////////////////////////////////////////////////////////////////////////
        //*************************************************************************
        void Speed_Draw_String_setup(double Air_Speed, double PoinStart_X, double PoinStart_Y)
        {

            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            SolidColorBrush BlushOfLine1 = new SolidColorBrush(Colors.White);
            SolidColorBrush BlushOfString1 = new SolidColorBrush(Colors.White);

            //Tọa độ của Hình vẽ
            //Width, Height là độ rộng và cao của vạch xanh
            double Width = 8, Height = 250;
            //Draw BackGround độ đục là 1
            //*************************************************************************
            //Ve Cho mau den de ghi Airspeed
            //Ngày 16/12/2015 15h52 đã test ok
            /* font chu 16 rong 12 cao 16
             * Config_Position = 12 de canh chinh vung mau den cho phu hop
             * SizeOfString = 16 chu cao 16 rong 12
             * chieu cao cua vùng đen 2 * Config_Position = 24
             * Số 28 là độ rông của vùng đen bên trái, vùng đen lớn: DoRongVungDen >= (2 * độ rông của 1 chữ = 24)
             * Số 15 là khoảng cách của chữ cách lề bên trái (bắt đầu đường gạch gạch)
             * Bắt đầu của chữ là: i16StartStrAxisX =(Int16)(PoinStart_X + 15)
             */
            double SizeOfString = 24;
            double I16FullScale = 60, So_Khoang_Chia = 6;
            double iDoPhanGiai = I16FullScale / So_Khoang_Chia;
            //1 ký tự rộng = SizeOfString * 3 / 4;
            double Config_Position = 12, i16StartStrAxisX;
            //double DoRongVungDen = Air_Speed.ToString().Length * (SizeOfString * 3 / 4);
            double DoRongVungDen = 26;
            //Vì độ rộng của dấu . nhỏ hơn các ký tự còn lại nên ta chia 2 trường hợp
            double DoRongVungDenRect = Air_Speed.ToString().Length * (SizeOfString * 0.6);
            if (Air_Speed.ToString().IndexOf('.') != -1)
            //Trong airspeed có dấu chấm
            {
                DoRongVungDenRect = (Air_Speed.ToString().Length - 1) * (SizeOfString * 0.6) + 5;
                //10 là độ rông dấu chấm
            }
            if (Air_Speed.ToString().Length == 1)
            //Tăng độ rộng màu đen
            {
                DoRongVungDenRect = 40;
                //4 là độ rông dấu chấm
            }
            //Hình chữ nhật được căn lề phải
            i16StartStrAxisX = (PoinStart_X - 41);
            double StartXRect = (PoinStart_X - 10);
            FillRect_AutoRemove3(BlushRectangle4, StartXRect - DoRongVungDenRect, PoinStart_Y - Config_Position - 1 +
                (30 - Air_Speed % 10) * Height / 60, DoRongVungDenRect, Config_Position * 2, 1);

            //Vẽ mũi tên
            //Ngày 16/12/2015 15h52 đã test ok
            double x1, y1, x2, y2, x3, y3;
            x1 = i16StartStrAxisX + DoRongVungDen;
            y1 = PoinStart_Y - Config_Position +
                (30 - Air_Speed % 10) * Height / 60;
            x2 = x1;
            y2 = y1 + Config_Position * 2;
            x3 = PoinStart_X + Width;
            y3 = (y1 + y2) / 2;
            Draw_TriAngle_Var(x1, y1, x2, y2, x3, y3);

            //ghi chu len mau den, ghi hang nghin va hang tram (Int16)Air_Speed / 100
            //Ngày 16/12/2015 15h56 đã test ok
            //Cỡ chữ 20 bên map là cỡ chữ 16 bên System.Drawing
            /* cỡ chữ SizeOfString = 16;
             * Số -12 để canh chỉnh số cho phù hợp
             * (Int16)Air_Speed % 100: Lấy phần chục và đơn vị tìm ra vị trí phù hợp
             * Chữ số này nằm ở nửa trên cách đầu trên cùng ((Int16)Air_Speed / 100 * 100 + 300) 1 khoảng
             * Số 300 là 1/2 của fullScale
             * (300 - (Int16)Air_Speed % 100)
             * đổi qua trục tọa độ * Height / 600, 600 la fullScale
             * Chữ bắt đầu tại Point_X - Font_X / 4, Point_Y - Font_Y / 4
             * Trung tam là PoinStart_Y + (300 - (Int16)Air_Speed % 100) * Height / I16FullScale
             * Bắt đầu là PoinY - 16/ 4 = Trung tam - 16 / 2. 16 là cỡ chữ theo Y,
             * PoiY = Trung Tâm + 12;
             */

            //Cỡ chữ 20 bên map là cỡ chữ 16 bên System.Drawing
            //drawFont = new Font("Arial", SizeOfString);
            //-2 là độ dời chữ vào trong thích hợp
            DrawStringAutoRemove(Air_Speed.ToString(), SizeOfString, BlushOfString1, i16StartStrAxisX - 0,
                                PoinStart_Y - 15 + (30 - Air_Speed % 10) * Height / I16FullScale, 1);

            //Còn bên Notepad++
            //Khó quá bỏ qua

        }
        ///////////////////////////////////////////////////////////////////////////////////////////
        //*************************************************************************
        /// <summary>
        /// draw string for speed
        /// </summary>
        /// <param name="Air_Speed"></param>
        /// <param name="PoinStart_X"></param>
        /// <param name="PoinStart_Y"></param>
        void Speed_Draw_String_optimize(double Air_Speed, double PoinStart_X, double PoinStart_Y)
        {

            if (Air_Speed.ToString().Length > 5) TxtDesignAutoRemove.Text = Air_Speed.ToString().Substring(0, 5);
            else TxtDesignAutoRemove.Text = Air_Speed.ToString();

        }
        //*********************************************************************************************
        //Vẽ độ cao máy bay
        //**************************************************************************
        //******************************************************************************
        //Ngày 19/12/2015 Vẽ Altitude

        ///////////////////////////////////////////////////////////////////
        Image imAlttitudeFull = new Image();
        /// <summary>
        /// Find position with  string input or Lat, Lon input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (tb_Position.Text != "")
            {
                Search_Offline(tb_Position.Text);
            }
            else
            {//nhap dung toa do là việc của button get
                //if(tb_Lat_Search.Text != "" && tb_Lon_Search.Text != "")
                //{
                //    dLatDentination = Convert.ToDouble(tb_Lat_Search.Text);
                //    dLonDentination = Convert.ToDouble(tb_Lon_Search.Text);
                //    //Add My home picture
                //    AddImageAtLatAndLon(dLatDentination, dLonDentination);
                //}
            }
        }

        /// <summary>
        /// nhìn thấy cả máy bay và đích
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomAll_Click(object sender, RoutedEventArgs e)
        {
            var posToZoomAll = new List<BasicGeoposition>();
            //add current position and dentination
            posToZoomAll.Add(new BasicGeoposition() { Latitude = dLatGol, Longitude = dLonGol });
            posToZoomAll.Add(new BasicGeoposition() { Latitude = dLatDentination, Longitude = dLonDentination });
            SetMapPolyline(posToZoomAll);
        }

        /// <summary>
        /// xCenter, yCenter là trung tâm ảnh
        /// </summary>
        /// <param name="dAirSpeed"></param>
        /// <param name="xCenter"></param>
        /// <param name="yCenter"></param>
        public void Alttitude_Image_full_Setup(double dAirSpeed, double xCenter, double yCenter)
        {
            //dAirSpeed = 0;
            double top = -(dAirSpeed - 2000);
            //BackgroundDisplay.Children.Remove(imgAuto_airSpeed);
            //Edit size of image
            imAlttitudeFull.Height = 4560;
            //muốn biết kích thước thì dùng paint, kích thước trong paint ;
            //size 85 x 601;
            BackgroundDisplay.Children.Remove(imAlttitudeFull);
            imAlttitudeFull.Source = new BitmapImage(new Uri("ms-appx:///Assets/AltitudeFull.png"));
            imAlttitudeFull.Width = 88;//Ảnh này hình vuông nên Width = Height = min(Height, Width)

            //imgAuto_airSpeed.RenderTransform
            imAlttitudeFull.Opacity = 1;

            imAlttitudeFull.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
            BackgroundDisplay.Children.Add(imAlttitudeFull);
            /////////////////////////////////////////////////////////////////////////////////////////////

            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            SolidColorBrush whitePen = new SolidColorBrush(Colors.White);

            double Height = 250;
            double I16FullScale = 600;
            //Vẽ màu đen và mũi tên và string
            //Khúc trên ok
            //Còn bên notepad
            double Config_Position = 12, i16StartStrAxisX;
            i16StartStrAxisX = (xCenter - 88 / 2 + 15);

            double SizeOfString = 24;
            //Thay đổi độ rộng vùng đen
            //Vì độ rộng của dấu . nhỏ hơn các ký tự còn lại nên ta chia 2 trường hợp
            double DoRongVungDenRect = dAirSpeed.ToString().Length * (SizeOfString * 0.6);
            if (dAirSpeed.ToString().IndexOf('.') != -1)
            //Trong airspeed có dấu chấm
            {
                DoRongVungDenRect = (dAirSpeed.ToString().Length - 1) * (SizeOfString * 0.6) + 10;
                //10 là độ rông dấu chấm
            }
            if (dAirSpeed.ToString().Length == 1)
            //Tăng độ rộng màu đen
            {
                DoRongVungDenRect = 40;
                //4 là độ rông dấu chấm
            }
            //Vẽ hình chữ nhật mà đen để hiện số tự động remove khi hình mới xuất hiện
            //Hình chữ nhật màu đen của Alt có chỉ số là 0
            Rect_Setup_AutoRemove(0, BlushRectangle4, i16StartStrAxisX, yCenter - Config_Position - 1 +
                    (300 - dAirSpeed % 100) * Height / 600, DoRongVungDenRect, Config_Position * 2, 1);

            //Vẽ mũi tên
            double x1, y1, x2, y2, x3, y3;
            x1 = i16StartStrAxisX;
            y1 = yCenter - Config_Position +
                (300 - dAirSpeed % 100) * Height / 600;
            x2 = x1;
            y2 = y1 + Config_Position * 2;
            x3 = xCenter - 88 / 2;
            y3 = (y1 + y2) / 2;
            Altitude_Draw_TriAngle(x1, y1, x2, y2, x3, y3);

            //ghi chu len mau den, ghi hang nghin va hang tram (Int16)fAltitude / 100
            /* cỡ chữ SizeOfString = 16;
             * Số -12 để canh chỉnh số cho phù hợp
             * (Int16)fAltitude % 100: Lấy phần chục và đơn vị tìm ra vị trí phù hợp
             * Chữ số này nằm ở nửa trên cách đầu trên cùng ((Int16)fAltitude / 100 * 100 + 300) 1 khoảng
             * Số 300 là 1/2 của fullScale
             * (300 - (Int16)fAltitude % 100)
             * đổi qua trục tọa độ * Height / 600, 600 la fullScale
             * Chữ bắt đầu tại Point_X - Font_X / 4, Point_Y - Font_Y / 4
             * Trung tam là PoinStart_Y + (300 - (Int16)fAltitude % 100) * Height / I16FullScale
             * Bắt đầu là PoinY - 16/ 4 = Trung tam - 16 / 2. 16 là cỡ chữ theo Y,
             * PoiY = Trung Tâm + 12;
             */

            //drawFont = new Font("Arial", SizeOfString);
            //DrawStringAutoRemove(Air_Speed.ToString(), SizeOfString, BlushOfString1, i16StartStrAxisX - 0,
            //PoinStart_Y - 15 + (30 - Air_Speed % 10) * Height / I16FullScale, 1);
            //Chữ trong màu đen có chỉ số là 0
            Alt_SetupString_AutoRemove(0, dAirSpeed.ToString(), SizeOfString, whitePen, xCenter - 88 / 2 + 15,
                                yCenter - 15 + (300 - dAirSpeed % 100) * Height / I16FullScale, 1);

            //Viết chữ Altitude
            DrawString("Altitude", 30, new SolidColorBrush(Colors.Crimson), xCenter - 88 / 2 - 10, yCenter + Height + 5, 0.8);
            DrawString("  (m)", 30, new SolidColorBrush(Colors.Crimson), xCenter - 88 / 2 - 10, yCenter + Height + 35, 0.8);
            //sliderAdjSpeed.ValueChanged += sliderAdjSpeed_ValueChanged;
            //test
            //Speed_Draw_String_setup(130.6, 150, 100);
            //tb_ZoomLevel.Text = "";
        }
        public void Draw_Alttitude_full_optimize(double dAlttitude, double xCenter, double yCenter)
        {
            //làm tròn và chặn không cho nhỏ hơn 0
            if (dAlttitude < 0) dAlttitude = 0;
            dAlttitude = Math.Round(dAlttitude, 1);

            //yCenter = 225
            //dAlttitude = 4000;
            //if(TestText.Text != "")
            //dAlttitude = Convert.ToDouble(TestText.Text);
            double top, dAirSpeed_original = dAlttitude, t_cut;
            t_cut = -(-1980.45) - dAlttitude * 0.416;
            {

                if (dAlttitude < 1006)
                    top = -(-1980.45) - dAlttitude * 0.416;
                else
                {
                    top = -(-1980.45) - 1006 * 0.416 - (dAlttitude - 1006) * 0.416 / 2;
                    //t_cut = -(-1980.3) - dAlttitude * 0.4167;
                }

                //else top = -(-1103) - 105.0 * 4.16 - (dAlttitude - 105) * 4.168 / 2;

                //top = -(-1103) - dAirSpeed * 4.16; đúng từ 0 đến 100
                //top = -(-1103) - 105.0 * 4.16 - (dAirSpeed - 105) * 4.168 / 2; chạy ok từ 105 đến hết
                //Edit size of image
                imAlttitudeFull.Height = 4560;
                //muốn biết kích thước thì dùng paint, kích thước trong paint ;
                //size 80 x 600;
                //BackgroundDisplay.Children.Remove(imAlttitudeFull);
                //imgAuto_test.Source = new BitmapImage(new Uri("ms-appx:///Assets/horizon.bmp"));
                imAlttitudeFull.Width = 88;//Ảnh này hình vuông nên Width = Height = min(Height, Width)


                imAlttitudeFull.Clip = new RectangleGeometry()

                {
                    Rect = new Rect(0, 2272 + t_cut, 88, 264)//các trên trung tâm y 100, dưới 100

                };

                imAlttitudeFull.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
                //BackgroundDisplay.Children.Add(imAlttitudeFull);
            }

            Alttitude_Draw_String_optimize(dAlttitude);
            //Speed_Draw_Speed(dSpeed, 150, 100);

            //Speed_Draw_String_optimize(dAirSpeed_original, 150, 100);
            ///////////////////////////////////////////////////////////

        }
        ///////////////////////////////////////////////////////////////////////////////////////////
        //*************************************************************************
        void Alttitude_Draw_String_optimize(double Alttitude)
        {

            if ((Alttitude < 1000) && (Alttitude.ToString().IndexOf('.') == -1))
                Tb_Alt[0].Text = Alttitude.ToString() + ".0";
            else
                Tb_Alt[0].Text = Alttitude.ToString();


        }

        //15/3/2016
        //search offline
        //khi nhap toa do thi co the search offline
        //Chi đường chỉ làm được với 1 số địa điểm được đặt tên trên bản đồ như chi đường giữa 2 tỉnh, từ tp hcm
        //đến sân bay nha trang "Sân Bay Nha Trang, Khanh Hoa, Vietnam"
        //san bay tan son nhat "58 Truong Son, Ward 2, Tan Binh District Ho Chi Minh City  Ho Chi Minh City"
        //          endLocation2.Latitude = 10.772099;
        //          endLocation2.Longitude = 106.657693;
        //San bay tan son nhat dLatDentination, dLonDentination
        //san bay da nang 16.044040, 108.199357
        MapLocationFinderResult result_position;
        MapLocation dentination_pos;

        private void Mouse_Click(object sender, TappedRoutedEventArgs e)
        {
            NotifyUser(String.Empty, NotifyType.StatusMessage);
        }

        public async void Search_Offline(string StrDentination)
        {

            //position: string

            try
            {
                result_position = await MapLocationFinder.FindLocationsAsync(StrDentination, myMap.Center);
                dentination_pos = result_position.Locations.First();
                ////show result
                //tb_Lat_Search.Text = dentination_pos.Point.Position.Latitude.ToString();
                //tb_Lon_Search.Text = dentination_pos.Point.Position.Longitude.ToString();
                tblock_LatAndLon.Text = Math.Round(dentination_pos.Point.Position.Latitude, 8).ToString() + ", "
                                        + Math.Round(dentination_pos.Point.Position.Longitude, 8).ToString();
                //Update Lat and Lon
                dLatDentination = dentination_pos.Point.Position.Latitude;
                dLonDentination = dentination_pos.Point.Position.Longitude;
                //On map
                myMap.Center =
                   new Geopoint(new BasicGeoposition()
                   {
                       //Geopoint for Seattle San Bay Tan Son Nhat:   dLatDentination, dLonDentination

                       Latitude = dentination_pos.Point.Position.Latitude,
                       Longitude = dentination_pos.Point.Position.Longitude
                   });
                myMap.ZoomLevel = 18;
                //Add My home picture
                AddImageAtLatAndLon(dentination_pos.Point.Position.Latitude, dentination_pos.Point.Position.Longitude);

            }
            catch (Exception ex)//bat loi
            {
                tb_Position.Text = ex.Message;
            }
            //if(result.Status == MapLocationFinder.)
            //MapLocation begin = result_position.Locations.First();

            //test show point
            //tb_ZoomLevel.Text = begin.Point.Position.Latitude.ToString() + "  "
            //                    + begin.Point.Position.Longitude.ToString();
            //System.Diagnostics.Debug.WriteLine(routeResult.Status); // DEBUG

        }
        Image img_AtLatAndLon = new Image();
        bool bPlay = true;
        UInt16 limitSpeed = 2;

        /// <summary>
        /// adjust time start draw sénor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void adjTime_Click(object sender, RangeBaseValueChangedEventArgs e)
        {
            //bPlay = false;
            editTimeWhenChangeslider();
            //Play_When_ReadFile();
        }

        /// <summary>
        /// edit time start draw
        /// </summary>
        public void editTimeWhenChangeslider()
        {
            try
            {
                sDisplayTimeNotFormat = Math.Round((slider_AdjTime.Value *
                    (Convert.ToDouble(sStopTime) - Convert.ToDouble(sStartTime)) / 100 +
                    Convert.ToDouble(sStartTime)), 1).ToString();
                //edit 83994.2 --> 84034.2
                double temp_edit_number = Convert.ToDouble(sDisplayTimeNotFormat);
                if ((Int32)temp_edit_number % 100 > 59)
                {
                    temp_edit_number = ((Int32)temp_edit_number / 100 + 1) * 100 + (temp_edit_number -
                        (((Int32)temp_edit_number / 100) * 100 + 60));
                }
                if ((Int32)temp_edit_number % 10000 > 5900)
                {
                    temp_edit_number = ((Int32)temp_edit_number / 10000 + 1) * 10000 + (temp_edit_number -
                        (((Int32)temp_edit_number / 10000) * 10000 + 6000));
                }
                sDisplayTimeNotFormat = temp_edit_number.ToString();
            }
            catch
            {

            }
        }


        //enter position after press enter, system auto search this position
        private void tb_Position_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (tb_Position.Text != "")
                {
                    Search_Offline(tb_Position.Text);
                }
                else
                {//nhap dung toa do là việc của button get
                 //if(tb_Lat_Search.Text != "" && tb_Lon_Search.Text != "")
                 //{
                 //    dLatDentination = Convert.ToDouble(tb_Lat_Search.Text);
                 //    dLonDentination = Convert.ToDouble(tb_Lon_Search.Text);
                 //    //Add My home picture
                 //    AddImageAtLatAndLon(dLatDentination, dLonDentination);
                 //}
                }
            }
        }


        /// <summary>
        /// Add my home at tappped pos on map
        /// </summary>
        /// <param name="dLat_Pos"></param>
        /// <param name="dLon_Pos"></param>
        public void AddImageAtLatAndLon(double dLat_Pos, double dLon_Pos)
        {

            myMap.Children.Remove(img_AtLatAndLon);

            //Edit size of image
            img_AtLatAndLon.Height = 5 * myMap.ZoomLevel;
            img_AtLatAndLon.Width = 5 * myMap.ZoomLevel;

            //img_rotate.RenderTransform
            img_AtLatAndLon.Opacity = 0.7;


            //img_rotate.Transitions.
            img_AtLatAndLon.Source = new BitmapImage(new Uri("ms-appx:///Assets/MyHome.png"));

            img_AtLatAndLon.RenderTransform = new RotateTransform()
            {


            };
            //mặc định ảnh có chiều dài và chiều rộng là vô cùng
            //bitmapImage.PixelHeight
            //img_rotate.sca
            img_AtLatAndLon.Stretch = Stretch.Uniform;
            img_AtLatAndLon.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            img_AtLatAndLon.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;

            img_AtLatAndLon.Margin = new Windows.UI.Xaml.Thickness(-5 * myMap.ZoomLevel / 2, -5 * myMap.ZoomLevel / 2, 0, 0);
            //tbOutputText.Text = "Latitude: " + myMap.Center.Position.Latitude.ToString() + '\n';
            //tbOutputText.Text += "Longitude: " + myMap.Center.Position.Longitude.ToString() + '\n';
            ////tbOutputText.Text += "Timer Fly: " + Timer_fly.ToString();



            Geopoint PointCenterMap = new Geopoint(new BasicGeoposition()
            {
                Latitude = dLat_Pos,
                Longitude = dLon_Pos,
                //Altitude = 200.0
            });
            //myMap.Children.Add(bitmapImage);

            Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(img_AtLatAndLon, PointCenterMap);
            //myMap.TrySetViewBoundsAsync()
            //Độ dài tương đối của hình so với vị trí mong muốn new Point(0.5, 0.5) không dời
            //Windows.UI.Xaml.Controls.Maps.MapControl.SetNormalizedAnchorPoint(img_rotate, new Point(0.5, 0.5));
            myMap.Children.Add(img_AtLatAndLon);

            //Vẽ quỹ đạo
            //Draw_Trajectory(Convert.ToDouble(Data.Latitude), Convert.ToDouble(Data.Longtitude), Convert.ToDouble(Data.Altitude));

        }

        //Hien toa do khi nhap chuot
        /// <summary>
        /// Used to display messages to the user
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        public void NotifyUser(string strMessage, NotifyType type)
        {
            //switch (type)
            //{
            //    case NotifyType.StatusMessage:
            //        tb_Lat_Search.Background = new SolidColorBrush(Windows.UI.Colors.Green);
            //        tb_Lon_Search.Background = new SolidColorBrush(Windows.UI.Colors.Green);
            //        break;
            //    case NotifyType.ErrorMessage:
            //        tb_Lat_Search.Background = new SolidColorBrush(Windows.UI.Colors.Red);
            //        tb_Lon_Search.Background = new SolidColorBrush(Windows.UI.Colors.Red);
            //        break;
            //}
            //tb_Lat_Search.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
        }

        //23/02/2016
        /// <summary>
        /// Add needle when change heading
        /// </summary>
        /// <param name="CenterX"></param>
        /// <param name="CenterY"></param>
        Image Img_Needle = new Image();
        public void AddNeedle(double CenterX, double CenterY)
        {

            //***************************************************************************************
            //9/03/2016 Add thêm ảnh mới
            //Image Img_Needle = new Image();
            BackgroundDisplay.Children.Remove(Img_Needle);
            //Edit size of image
            Img_Needle.Height = 40;
            Img_Needle.Width = 40;

            //Img_Needle.RenderTransform
            Img_Needle.Opacity = 1;


            //Img_Needle.Transitions.
            Img_Needle.Source = new BitmapImage(new Uri("ms-appx:///Assets/Needle.png"));
            //Xoay ảnh
            //kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            //Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            //khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            //Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            //dời ảnh lên trên 1 nửa chiều dài,
            //dời ảnh sang trái 1 nửa chiều rộng
            Img_Needle.RenderTransform = new RotateTransform()
            {

                //Angle = 0,
                //CenterX = 40,//là la Img_Needle_FliCom.Width/2
                //CenterX = 62, //The prop name maybe mistyped 
                //CenterY = 40 //la Img_Needle_FliCom.Height
            };
            //mặc định ảnh có chiều dài và chiều rộng là vô cùng
            //bitmapImage.PixelHeight
            //Img_Needle.sca
            Img_Needle.Stretch = Stretch.Uniform;
            Img_Needle.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            Img_Needle.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            //Img_Needle.Opacity = 0.8;
            Img_Needle.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + CenterX * 2, -798 + dConvertToTabletY + CenterY * 2, 0, 0);
            BackgroundDisplay.Children.Add(Img_Needle);

        }
        /// <summary>
        /// chỉnh tốc độ baud
        /// comPortInput_Click: Action to take when 'Connect' button is clicked
        /// - Get the selected device index and use Id to create the SerialDevice object
        /// - Configure default settings for the serial port
        /// - Create the ReadCancellationTokenSource token
        /// - Start listening on the serial port input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        //////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// dHeading: góc quay của bản đồ
        /// </summary>
        /// <param name="dAngle"></param>
        public void Rotate_Needle(double dHeading)
        {

            BackgroundDisplay.Children.Remove(Img_Needle);
            //center width/2, height/2
            Img_Needle.RenderTransform = new RotateTransform()
            {

                Angle = 360 - dHeading,
                CenterX = 20,
                //CenterX = 62, //The prop name maybe mistyped 
                CenterY = 20
            };

            BackgroundDisplay.Children.Add(Img_Needle);


        }

        /// <summary>
        /// when user press play button, this function is called
        /// It will continues simulate process
        /// </summary>
        private async void Play_When_ReadFile()
        {
            streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
            Data.Time = sStartTime;
            UInt16 index = 0;
            bPlay = true;
            //when time sample = 0.2s, we add Convert.ToDouble( Data.Time) != (Convert.ToDouble(tblock_Current_Timer.Text) - 0.1)
            while (Convert.ToDouble(Data.Time) > (Convert.ToDouble(sDisplayTimeNotFormat))
                || Convert.ToDouble(Data.Time) < (Math.Round(Convert.ToDouble(sDisplayTimeNotFormat), 1) - 0.1))//find time to start
            {
                strDataFromSerialPort = streamReader.ReadLine();
                //processDataToGetInf();
                processToDrawTrajactory();//both process and collect point

                if (streamReader.Peek() <= 0)
                {
                    positions.Clear();
                    positions = new List<BasicGeoposition>();
                    streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    while (Convert.ToDouble(Data.Time) > (Convert.ToDouble(sDisplayTimeNotFormat))
                        || Convert.ToDouble(Data.Time) < (Math.Round(Convert.ToDouble(sDisplayTimeNotFormat), 1) - 0.1))//find time to start
                    {
                        strDataFromSerialPort = streamReader.ReadLine();
                        //processDataToGetInf();
                        processToDrawTrajactory();//both process and collect point
                    }
                    //dang chon thoi gian nho hon hien tai
                    break;
                }

            }
            //reset index;
            index = 0;
            //update old_lat and old_lon because program will draw 1 line connect before point and current point
            old_Lat = dLatGol;
            old_Lon = dLonGol;

            while (streamReader.Peek() >= 0)
            {
                strDataFromSerialPort = streamReader.ReadLine();

                processDataToDrawTrajactory();

                //tblock_Current_Timer.Text = Data.Time;
                sDisplayTimeNotFormat = Data.Time;
                //format hour:min:sec
                if (-1 != Data.Time.IndexOf('.'))//have '.' in Data.Time 82754.7
                    tblock_Current_Timer.Text = Data.Time.Substring(0, Data.Time.Length - 6) + ':'
                            + Data.Time.Substring(Data.Time.Length - 6, 2) + ':' + Data.Time.Substring(Data.Time.Length - 4, 4);
                else
                    tblock_Current_Timer.Text = Data.Time.Substring(0, Data.Time.Length - 4) + ':'
                            + Data.Time.Substring(Data.Time.Length - 4, 2) + ':' + Data.Time.Substring(Data.Time.Length - 2, 2);

                slider_AdjTime.Value = 100 * (Convert.ToDouble(sDisplayTimeNotFormat) - Convert.ToDouble(sStartTime)) /
                    (Convert.ToDouble(sStopTime) - Convert.ToDouble(sStartTime));

                index++;
                if (index == limitSpeed)
                {
                    //ProcessData();
                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(0.5));
                    index = 0;

                }
                if (!bPlay) break;

            }

        }

        /// <summary>
        /// when user press pause button, this function is called
        /// It will pause simulate process
        /// </summary>
        private void Pause_When_ReadFile()
        {
            bPlay = false;
            myMap.MapElements.Clear();
            foreach (MapPolyline polyLines in polyLineToRemove)
            {
                myMap.Children.Remove(polyLines);
                myMap.MapElements.Remove(polyLines);
            }
            positions.Clear();
            streamReader.BaseStream.Seek(0, SeekOrigin.Current);
        }
        //-----------------------------------------------------------------------------
        //---thesis-------------------------------------------------------------------
        string sDisplayTimeNotFormat, sStartTime, sStopTime;//save value of time don't format

        /// <summary>
        /// increase speed when program is offline mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_speed_inc_click(object sender, RoutedEventArgs e)
        {
            limitSpeed += 5;
        }

        /// <summary>
        /// decrease speed of simulation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_speed_dec_click(object sender, RoutedEventArgs e)
        {
            if (limitSpeed > 5)
                limitSpeed -= 5;
        }

        /// <summary>
        /// open file .txt simulation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_open_file_click(object sender, RoutedEventArgs e)
        {
            ConnectDevices.Opacity = 0;// don't dispay ConnectDevices
            myMap.Children.Clear();
            myMap.MapElements.Clear();
            positions.Clear();
            positions = new List<BasicGeoposition>();
            ReadInfOfFile();
            //add tblock_Start_Timer, tblock_End_Timer, slider_AdjTime
            BackgroundDisplay.Children.Remove(tblock_Start_Timer);
            BackgroundDisplay.Children.Remove(tblock_End_Timer);
            BackgroundDisplay.Children.Remove(slider_AdjTime);
            BackgroundDisplay.Children.Add(tblock_Start_Timer);
            BackgroundDisplay.Children.Add(tblock_End_Timer);
            BackgroundDisplay.Children.Add(slider_AdjTime);
            //Enable play, Pause, Speed Lisbox when Open_File is selected
            bt_Play.IsEnabled = true;
            bt_Pause.IsEnabled = true;
            bt_Speed.IsEnabled = true;
        }

        /// <summary>
        /// when this button is pressed, map will be full screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_one_sceen_click(object sender, RoutedEventArgs e)
        {
            Background_Sensor(00, -80);
        }

        /// <summary>
        /// two screen, 1 screen for sensor, 1 screen for map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_two_sceen_click(object sender, RoutedEventArgs e)
        {
            myMap.MapElements.Remove(polylineHereToDentination);//delete polyline old before reload
            DisplaySensor_Setup();
        }

        /// <summary>
        /// get position at point is tapped with mouse
        /// this position will be dentination of flight
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_get_position_click(object sender, RoutedEventArgs e)
        {
            if (tblock_LatAndLon.Text != "")
            {
                //cut string in tblock_LatAndLon because it include lat and lon
                dLatDentination = Convert.ToDouble(tblock_LatAndLon.Text.Substring(0, tblock_LatAndLon.Text.IndexOf(',')));
                dLonDentination = Convert.ToDouble(tblock_LatAndLon.Text.Substring(tblock_LatAndLon.Text.IndexOf(',') + 2, tblock_LatAndLon.Text.Length - 2 - tblock_LatAndLon.Text.IndexOf(',')));
                //Add My home picture
                AddImageAtLatAndLon(dLatDentination, dLonDentination);
            }
        }

        private void bt_list_com_click(object sender, RoutedEventArgs e)
        {
            ConnectDevices.Opacity = 1;//dispay ConnectDevices
        }

        /// <summary>
        /// when this button is pressed, maps will be show position of flight and dentination
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_zoom_nomal_click(object sender, RoutedEventArgs e)
        {
            var posToZoomAll = new List<BasicGeoposition>();
            //add current position and dentination
            posToZoomAll.Add(new BasicGeoposition() { Latitude = dLatGol, Longitude = dLonGol });
            posToZoomAll.Add(new BasicGeoposition() { Latitude = dLatDentination, Longitude = dLonDentination });
            SetMapPolyline(posToZoomAll);
        }

        /// <summary>
        /// when this button is pressed, maps will show all of point which flight accross
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_autoZoom_on_click(object sender, RoutedEventArgs e)
        {
            bAutoZoom = true;
        }

        /// <summary>
        /// turn off auto zoom mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_autoZoom_off_click(object sender, RoutedEventArgs e)
        {
            bAutoZoom = false;
        }

        /// <summary>
        /// connect with COM which is selected in listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_device_connect_click(object sender, RoutedEventArgs e)
        {
            Connect_To_Com();
            ConnectDevices.Opacity = 0;// don't dispay ConnectDevices
            //remove tblock_Start_Timer, tblock_End_Timer, slider_AdjTime when connect Com
            BackgroundDisplay.Children.Remove(tblock_Start_Timer);
            BackgroundDisplay.Children.Remove(tblock_End_Timer);
            BackgroundDisplay.Children.Remove(slider_AdjTime);
            //Disable play, Pause, Speed Lisbox when Open_File isn't selected
            bt_Play.IsEnabled = false;
            bt_Pause.IsEnabled = false;
            bt_Speed.IsEnabled = false;
        }

        /// <summary>
        /// disconnect with COM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_device_disconnect_click(object sender, RoutedEventArgs e)
        {
            DisConnect_To_Com();
            ConnectDevices.Opacity = 1;//dispay ConnectDevices
        }

        private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //myMap.Height = screenHeight + slider.Value;
            //10.818442, 106.658824
            //Draw_Trajectory_And_Flight(10.818442, 106.658824,
            //            Convert.ToDouble(Data.Altitude), slider.Value);//ok
            //Draw_Trajectory_And_Flight_optimize(10.818442, 106.658824,
            //            Convert.ToDouble(0), slider.Value);//ok
            //Draw_Airspeed_full_optimize(slider.Value, 150 - 32 + i16EditPosition, 205);
            //Draw_Alttitude_full_optimize(500 + slider.Value, 550 + 88 / 2 + i16EditPosition * 17 / 6, 80);//ok
        }

        /// <summary>
        /// pause simulation when offline mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_pause_click(object sender, RoutedEventArgs e)
        {
            Pause_When_ReadFile();
        }

        /// <summary>
        /// play simulation when offline mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_play_Click(object sender, RoutedEventArgs e)
        {
            Play_When_ReadFile();
        }

        //*********************************************************************************************
        //end of class
    }

    //////////////////////////////////////////////////////////////////////////////////////////
    public enum NotifyType//for show mesage while not find when press search button
    {
        StatusMessage,
        ErrorMessage
    };
    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
    }
}
