using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Ait.SockCar.Server.Core.Entities;
using Ait.SockCar.Server.Core.Services;
using Ait.SockCar.Server.Core.SocketHelpers;


namespace Ait.SockCar.Server.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public static void DoEvents()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        CarService carService;
        Socket mainSocket;
        bool serverOnline;
        int maximumConnections = 10;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartupConfig();
        }
        private void StartupConfig()
        {
            cmbIPs.ItemsSource = IPv4.GetActiveIP4s();
            for (int port = 49200; port <= 49500; port++)
            {
                cmbPorts.Items.Add(port);
            }
            Config.GetConfig(out string savedIP, out int savedPort);
            try
            {
                cmbIPs.SelectedItem = savedIP;
            }
            catch
            {
                cmbIPs.SelectedItem = "127.0.0.1";
            }
            try
            {
                cmbPorts.SelectedItem = savedPort;
            }
            catch
            {
                cmbPorts.SelectedItem = 49200;
            }
            btnStartServer.Visibility = Visibility.Visible;
            btnStopServer.Visibility = Visibility.Hidden;
            grpCarPark.Visibility = Visibility.Hidden;
        }

        private void CmbIPs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveConfig();
        }

        private void CmbPorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveConfig();
        }
        private void SaveConfig()
        {
            if (cmbIPs.SelectedItem == null) return;
            if (cmbPorts.SelectedItem == null) return;

            string ip = cmbIPs.SelectedItem.ToString();
            int port = int.Parse(cmbPorts.SelectedItem.ToString());
            Config.WriteConfig(ip, port);
        }

        private void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            btnStartServer.Visibility = Visibility.Hidden;
            btnStopServer.Visibility = Visibility.Visible;
            cmbIPs.IsEnabled = false;
            cmbPorts.IsEnabled = false;
            grpCarPark.Visibility = Visibility.Visible;

            StartTheServer();
            StartListening();
        }

        private void btnStopServer_Click(object sender, RoutedEventArgs e)
        {
            btnStartServer.Visibility = Visibility.Visible;
            btnStopServer.Visibility = Visibility.Hidden;
            grpCarPark.Visibility = Visibility.Hidden;
            cmbIPs.IsEnabled = true;
            cmbPorts.IsEnabled = true;

            CloseTheServer();
        }
        private void CloseTheServer()
        {
            serverOnline = false;
            carService = null;

            try
            {
                if (mainSocket != null)
                    mainSocket.Close();
            }
            catch
            { }
            mainSocket = null;

        }
        private void StartTheServer()
        {
            carService = new CarService();
            serverOnline = true;

        }
        private void StartListening()
        {
            IPAddress ip = IPAddress.Parse(cmbIPs.SelectedItem.ToString());
            int port = int.Parse(cmbPorts.SelectedItem.ToString());
            IPEndPoint myEndpoint = new IPEndPoint(ip, port);
            mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                mainSocket.Bind(myEndpoint);
                mainSocket.Listen(maximumConnections);
                while (serverOnline)
                {
                    DoEvents();
                    if (mainSocket != null)
                    {
                        if (mainSocket.Poll(1000000, SelectMode.SelectRead))
                        {
                            HandleClientCall(mainSocket.Accept());
                        }
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void HandleClientCall(Socket clientCall)
        {
            byte[] clientRequest = new Byte[1024];
            string instruction = null;

            while (true)
            {

                int numByte = clientCall.Receive(clientRequest);
                instruction += Encoding.ASCII.GetString(clientRequest, 0, numByte);
                if (instruction.IndexOf("##EOM") > -1)
                    break;
            }
            string serverResponseInText = HandleInstruction(instruction);
            if (serverResponseInText != "")
            {
                byte[] serverResponse = Encoding.ASCII.GetBytes(serverResponseInText);
                clientCall.Send(serverResponse);
            }
            clientCall.Shutdown(SocketShutdown.Both);
            clientCall.Close();
        }
        private string HandleInstruction(string instruction)
        {
            lstCall.Items.Insert(0, instruction);
            instruction = instruction.Replace("##EOM", "").Trim();
            instruction = instruction.ToUpper();

            if (instruction.Length > 14 && instruction.Substring(0, 14) == "IDENTIFICATION")
            {
                string[] delen = instruction.Split("=");
                Car car = new Car(delen[1]);
                carService.AddCar(car);
                string retour = car.NrPlate;
                lstResponse.Items.Insert(0, retour);
                return retour;
            }
            else if (instruction.Length > 3 && instruction.Substring(0, 3) == "ID=")
            {
                string[] delen = instruction.Split("|");
                string[] header = delen[0].Split("=");
                string id = header[1];
                Car car = carService.FindCar(id);
                if (car != null)
                {
                    string[] body = delen[1].Split("=");
                    string action = body[0];
                    if(action == "BYEBYE")
                    {
                        carService.DeleteCar(car);
                    }
                    if (action == "START")
                    {
                        car.ChangeSpeed(0);
                        string retour = $"DISTANCE ({DateTime.Now.ToString("HH:mm:ss")}) = {car.GetTotalDistance().ToString("#,##0.00")} KM \nCURRENT SPEED = {car.LastSpeed.ToString("0.00")} KM/H";
                        lstResponse.Items.Insert(0, retour);
                        return retour;
                    }
                    else if (action == "STOP")
                    {
                        car.ChangeSpeed(0);
                        string retour = $"DISTANCE ({DateTime.Now.ToString("HH:mm:ss")}) = {car.GetTotalDistance().ToString("#,##0.00")} KM \nCURRENT SPEED = {car.LastSpeed.ToString("0.00")} KM/H";
                        lstResponse.Items.Insert(0, retour);
                        return retour;
                    }
                    else if (action == "SPEED")
                    {
                        int speed = int.Parse(body[1]);
                        car.ChangeSpeed(speed);
                        string retour = $"DISTANCE ({DateTime.Now.ToString("HH:mm:ss")}) = {car.GetTotalDistance().ToString("#,##0.00")} KM \nCURRENT SPEED = {car.LastSpeed.ToString("0.00")} KM/H";
                        lstResponse.Items.Insert(0, retour);
                        return retour;
                    }
                    else if(action == "FETCH")
                    {
                        string retour = $"DISTANCE ({DateTime.Now.ToString("HH:mm:ss")}) = {car.GetTotalDistance().ToString("#,##0.00")} KM \nCURRENT SPEED = {car.LastSpeed.ToString("0.00")} KM/H";
                        lstResponse.Items.Insert(0, retour);
                        return retour;
                    }
                }
            }
            return "";
        }

    }
}
