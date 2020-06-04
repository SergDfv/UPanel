using System;
using System.Collections.Generic;
using System.Linq;
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
using Modbus.Device;
using System.Net.Sockets;
using System.ComponentModel;
using Modbus.Utility;
using System.Windows.Threading;

namespace TestCarel
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        ModbusIpMaster master;
        ushort[] InputRegisters;
        ushort[] HoldingRegisters;
        TcpClient tcpClient;
        string _state;
        DispatcherTimer timer;

        List<string> UnitModel = new List<string> { "POOL", "DLE", "ICE" };

        public string State {
            get
            {
                return _state;
            }

            set
            {
                _state = value;
                OnPropertyChanged("State");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            //State = "Не соединен";
            InitializeComponent();
            DataContext = this;
            StatBar.Text = "Не соединен";
            cmb_UnitModel.SelectedIndex = 0;
        }


        private void Connection_Click(object sender, RoutedEventArgs e)
        {
            if (tcpClient == null)
            {
                tcpClient = new TcpClient();//Create a new TcpClientobject.
            }

            if (!tcpClient.Connected)
            {
                btn_CheckConnection.Content = "Соединение...";
                btn_CheckConnection.IsEnabled = false;
                //string ipAddress = "192.168.1.152";//use TCP master for example
                int tcpPort = 502;
                try
                {
                    tcpClient.Connect(txt_IP_address.Text, tcpPort);
                    master = ModbusIpMaster.CreateIp(tcpClient);
                    if (tcpClient.Connected)
                    {
                        btn_CheckConnection.Content = "Разъединить";
                        btn_CheckConnection.IsEnabled = true;
                        //State = "Соединен";
                        StatBar.Text = "Соединен";
                        //Grid_U.Visibility = Visibility.Visible;
                        //Grid_Y.Visibility = Visibility.Visible;
                        //Grid_DI.Visibility = Visibility.Visible;
                        //Grid_DO.Visibility = Visibility.Visible;

                        //Запускаем периодическое чтение данных 
                        timer = new DispatcherTimer();
                        timer.Tick += new EventHandler(ReadHoldReg);
                        timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                        timer.Start();
                    }
                }
                catch(Exception er)
                {
                    Dispose_Connect();
                    //State = er.Message.ToString();
                    StatBar.Text = er.Message.ToString();
                }
            }
            else
            {
                Dispose_Connect();
            }
        }

        private void Dispose_Connect()
        {
            master.Dispose();
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
            }
            //State = "Не соединен";
            StatBar.Text = "Не соединен";
            btn_CheckConnection.Content = "Соединить";
            btn_CheckConnection.IsEnabled = true;
            SetDefault_Labels();
            timer.Stop();
        }


        private void ReadHoldReg(object sender, EventArgs e)
        {
            try
            {
                HoldingRegisters = master.ReadHoldingRegisters(20002, 36);
                Refresh_DI_Labels();
                HoldingRegisters = master.ReadHoldingRegisters(20038, 12);
                Refresh_Y_Labels();
                HoldingRegisters = master.ReadHoldingRegisters(20050, 58);
                Refresh_DO_Labels();
                HoldingRegisters = master.ReadHoldingRegisters(20108, 20);
                Refresh_U_Labels();
            }
            catch (Exception er)
            {
                Dispose_Connect();
                //State = er.Message.ToString();
                StatBar.Text = er.Message.ToString();
            }
        }

        private void Refresh_U_Labels()
        {
            label_U1.Content = String.Format("{0:f1} °C",
                                            Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[0], HoldingRegisters[1]));
            label_U2.Content = String.Format("{0:f1} °C",
                                            Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[2], HoldingRegisters[3]));
            label_U3.Content = String.Format("{0:f1} °C",
                                            Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[4], HoldingRegisters[5]));
            label_U4.Content = String.Format("{0:f1} °C",
                                            Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[6], HoldingRegisters[7]));
            label_U5.Content = String.Format("{0:f0} %",
                                            Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[8], HoldingRegisters[9]));
            label_U6.Content = String.Format("{0:f0} ppm",
                                            Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[10], HoldingRegisters[11]));
            label_U7.Content = String.Format("{0:f1}",
                                            Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[12], HoldingRegisters[13]));
            label_U8.Content = String.Format("{0:f1}",
                                            Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[14], HoldingRegisters[15]));
            label_U9.Content = String.Format("{0:f1}",
                                            Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[16], HoldingRegisters[17]));
            label_U10.Content = String.Format("{0:f1}",
                                            Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[18], HoldingRegisters[19]));
        }

        private void Refresh_DI_Labels()
        {
            lamp_DI1.Fill = Convert.ToBoolean(HoldingRegisters[0]) ? Brushes.Green : Brushes.Gray;
            lamp_DI2.Fill = Convert.ToBoolean(HoldingRegisters[2]) ? Brushes.Green : Brushes.Gray;
            lamp_DI3.Fill = Convert.ToBoolean(HoldingRegisters[4]) ? Brushes.Green : Brushes.Gray;
            lamp_DI4.Fill = Convert.ToBoolean(HoldingRegisters[6]) ? Brushes.Green : Brushes.Gray;
            lamp_DI5.Fill = Convert.ToBoolean(HoldingRegisters[8]) ? Brushes.Green : Brushes.Gray;
            lamp_DI6.Fill = Convert.ToBoolean(HoldingRegisters[10]) ? Brushes.Green : Brushes.Gray;
            lamp_DI7.Fill = Convert.ToBoolean(HoldingRegisters[12]) ? Brushes.Green : Brushes.Gray;
            lamp_DI8.Fill = Convert.ToBoolean(HoldingRegisters[14]) ? Brushes.Green : Brushes.Gray;
            lamp_DI9.Fill = Convert.ToBoolean(HoldingRegisters[16]) ? Brushes.Green : Brushes.Gray;
            lamp_DI10.Fill = Convert.ToBoolean(HoldingRegisters[18]) ? Brushes.Green : Brushes.Gray;
            lamp_DI11.Fill = Convert.ToBoolean(HoldingRegisters[20]) ? Brushes.Green : Brushes.Gray;
            lamp_DI12.Fill = Convert.ToBoolean(HoldingRegisters[22]) ? Brushes.Green : Brushes.Gray;
            lamp_DI13.Fill = Convert.ToBoolean(HoldingRegisters[24]) ? Brushes.Green : Brushes.Gray;
            lamp_DI14.Fill = Convert.ToBoolean(HoldingRegisters[26]) ? Brushes.Green : Brushes.Gray;
            lamp_DI15.Fill = Convert.ToBoolean(HoldingRegisters[28]) ? Brushes.Green : Brushes.Gray;
            lamp_DI16.Fill = Convert.ToBoolean(HoldingRegisters[30]) ? Brushes.Green : Brushes.Gray;
            lamp_DI17.Fill = Convert.ToBoolean(HoldingRegisters[32]) ? Brushes.Green : Brushes.Gray;
            lamp_DI18.Fill = Convert.ToBoolean(HoldingRegisters[34]) ? Brushes.Green : Brushes.Gray;
        }

        private void Refresh_Y_Labels()
        {
            label_Y1.Content = String.Format("{0:f0} %",
                                Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[0], HoldingRegisters[1]));
            label_Y2.Content = String.Format("{0:f0} %",
                                            Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[2], HoldingRegisters[3]));
            label_Y3.Content = String.Format("{0:f0} %",
                                            Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[4], HoldingRegisters[5]));
            label_Y4.Content = String.Format("{0:f0} %",
                                            Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[6], HoldingRegisters[7]));
            label_Y5.Content = String.Format("{0:f0} %",
                                            Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[8], HoldingRegisters[9]));
            label_Y6.Content = String.Format("{0:f0} %",
                                            Modbus.Utility.ModbusUtility.GetSingle(HoldingRegisters[10], HoldingRegisters[11]));

        }

        private void Refresh_DO_Labels()
        {
            lamp_DO1.Fill = Convert.ToBoolean(HoldingRegisters[0]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO2.Fill = Convert.ToBoolean(HoldingRegisters[2]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO3.Fill = Convert.ToBoolean(HoldingRegisters[4]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO4.Fill = Convert.ToBoolean(HoldingRegisters[6]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO5.Fill = Convert.ToBoolean(HoldingRegisters[8]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO6.Fill = Convert.ToBoolean(HoldingRegisters[10]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO7.Fill = Convert.ToBoolean(HoldingRegisters[12]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO8.Fill = Convert.ToBoolean(HoldingRegisters[14]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO9.Fill = Convert.ToBoolean(HoldingRegisters[16]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO10.Fill = Convert.ToBoolean(HoldingRegisters[18]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO11.Fill = Convert.ToBoolean(HoldingRegisters[20]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO12.Fill = Convert.ToBoolean(HoldingRegisters[22]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO13.Fill = Convert.ToBoolean(HoldingRegisters[24]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO14.Fill = Convert.ToBoolean(HoldingRegisters[26]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO15.Fill = Convert.ToBoolean(HoldingRegisters[28]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO16.Fill = Convert.ToBoolean(HoldingRegisters[30]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO17.Fill = Convert.ToBoolean(HoldingRegisters[32]) ? Brushes.Firebrick : Brushes.Gray;
            lamp_DO18.Fill = Convert.ToBoolean(HoldingRegisters[34]) ? Brushes.Firebrick : Brushes.Gray;
        }

        private void SetDefault_Labels()
        {
            label_U1.Content = "---";
            label_U2.Content = "---";
            label_U3.Content = "---";
            label_U4.Content = "---";
            label_U5.Content = "---";
            label_U6.Content = "---";
            label_U7.Content = "---";
            label_U8.Content = "---";
            label_U9.Content = "---";
            label_U10.Content = "---";

            label_Y1.Content = "---";
            label_Y2.Content = "---";
            label_Y3.Content = "---";
            label_Y4.Content = "---";
            label_Y5.Content = "---";
            label_Y6.Content = "---";

            lamp_DI1.Fill = Brushes.Gray;
            lamp_DI2.Fill = Brushes.Gray;
            lamp_DI3.Fill = Brushes.Gray;
            lamp_DI4.Fill = Brushes.Gray;
            lamp_DI5.Fill = Brushes.Gray;
            lamp_DI6.Fill = Brushes.Gray;
            lamp_DI7.Fill = Brushes.Gray;
            lamp_DI8.Fill = Brushes.Gray;
            lamp_DI9.Fill = Brushes.Gray;
            lamp_DI10.Fill = Brushes.Gray;
            lamp_DI11.Fill = Brushes.Gray;
            lamp_DI12.Fill = Brushes.Gray;
            lamp_DI13.Fill = Brushes.Gray;
            lamp_DI14.Fill = Brushes.Gray;
            lamp_DI15.Fill = Brushes.Gray;
            lamp_DI16.Fill = Brushes.Gray;
            lamp_DI17.Fill = Brushes.Gray;
            lamp_DI18.Fill = Brushes.Gray;

            lamp_DO1.Fill = Brushes.Gray;
            lamp_DO1.Fill = Brushes.Gray;
            lamp_DO2.Fill = Brushes.Gray;
            lamp_DO3.Fill = Brushes.Gray;
            lamp_DO4.Fill = Brushes.Gray;
            lamp_DO5.Fill = Brushes.Gray;
            lamp_DO6.Fill = Brushes.Gray;
            lamp_DO7.Fill = Brushes.Gray;
            lamp_DO8.Fill = Brushes.Gray;
            lamp_DO9.Fill = Brushes.Gray;
            lamp_DO10.Fill = Brushes.Gray;
            lamp_DO11.Fill = Brushes.Gray;
            lamp_DO12.Fill = Brushes.Gray;
            lamp_DO13.Fill = Brushes.Gray;
            lamp_DO14.Fill = Brushes.Gray;
            lamp_DO15.Fill = Brushes.Gray;
            lamp_DO16.Fill = Brushes.Gray;
            lamp_DO17.Fill = Brushes.Gray;
            lamp_DO18.Fill = Brushes.Gray;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            TextBlock selItem = (TextBlock)cmb.SelectedItem;
            string str = selItem.Text;
            switch (str)
            {
                case "Без заголовков":
                    setCotent_NoTitles();
                    break;
                case "POOL":
                    break;
                case "DLE":
                    break;
                case "ICE (W)":
                    setCotent_ICE_W();
                    break;
                case "ICE (E)":
                    setCotent_ICE_E();
                    break;
            }
        }
        private void setCotent_NoTitles()
        {
            descr_U1.Content = "";
            descr_U2.Content = "";
            descr_U3.Content = "";
            descr_U4.Content = "";
            descr_U5.Content = "";
            descr_U6.Content = "";
            descr_U7.Content = "";
            descr_U8.Content = "";
            descr_U9.Content = "";
            descr_U10.Content = "";

            descr_Y1.Content = "";
            descr_Y2.Content = "";
            descr_Y3.Content = "";
            descr_Y4.Content = "";
            descr_Y5.Content = "";
            descr_Y6.Content = "";

            descr_DI1.Content = "";
            descr_DI2.Content = "";
            descr_DI3.Content = "";
            descr_DI4.Content = "";
            descr_DI5.Content = "";
            descr_DI6.Content = "";
            descr_DI7.Content = "";
            descr_DI8.Content = "";
            descr_DI9.Content = "";
            descr_DI10.Content = "";
            descr_DI11.Content = "";
            descr_DI12.Content = "";
            descr_DI13.Content = "";
            descr_DI14.Content = "";
            descr_DI15.Content = "";
            descr_DI16.Content = "";
            descr_DI17.Content = "";
            descr_DI18.Content = "";

            descr_DO1.Content = "";
            descr_DO2.Content = "";
            descr_DO3.Content = "";
            descr_DO4.Content = "";
            descr_DO5.Content = "";
            descr_DO6.Content = "";
            descr_DO7.Content = "";
            descr_DO8.Content = "";
            descr_DO9.Content = "";
            descr_DO10.Content = "";
            descr_DO11.Content = "";
            descr_DO12.Content = "";
            descr_DO13.Content = "";
            descr_DO14.Content = "";
            descr_DO15.Content = "";
            descr_DO16.Content = "";
            descr_DO17.Content = "";
            descr_DO18.Content = "";
        }

        private void setCotent_ICE_W()
        {
            descr_U1.Content = "T обратки";
            descr_U2.Content = "T наружного";
            descr_U3.Content = "T притока";
            descr_U4.Content = "T вытяжного";
            descr_U5.Content = "U вытяжного";
            descr_U6.Content = "Датчик СО2";
            descr_U7.Content = "---";
            descr_U8.Content = "---";
            descr_U9.Content = "---";
            descr_U10.Content = "---";

            descr_Y1.Content = "Наружные заслонки";
            descr_Y2.Content = "Рециркуляция 1";
            descr_Y3.Content = "Вертикальные (ротор)";
            descr_Y4.Content = "3-ходовой клапан";
            descr_Y5.Content = "Горизонтальные (ротор)";
            descr_Y6.Content = "Управление ротором";

            descr_DI1.Content = "Термостат вод.";
            descr_DI2.Content = "Пожар";
            descr_DI3.Content = "Вкл/выкл(RC)";
            descr_DI4.Content = "Компрессор 1";
            descr_DI5.Content = "Компрессор 2";
            descr_DI6.Content = "Приточный вент.";
            descr_DI7.Content = "Вытяжной вент.";
            descr_DI8.Content = "Реле контроля фаз";
            descr_DI9.Content = "ДПД фильтров";
            descr_DI10.Content = "ДПД рекуп.";
            descr_DI11.Content = "---";
            descr_DI12.Content = "Компрессор 3";
            descr_DI13.Content = "Регенератор";
            descr_DI14.Content = "---";
            descr_DI15.Content = "---";
            descr_DI16.Content = "---";
            descr_DI17.Content = "---";
            descr_DI18.Content = "---";

            descr_DO1.Content = "Роторный реген.";
            descr_DO2.Content = "---";
            descr_DO3.Content = "---";
            descr_DO4.Content = "Насос";
            descr_DO5.Content = "---";
            descr_DO6.Content = "---";
            descr_DO7.Content = "Приточный вент.";
            descr_DO8.Content = "Вытяжной вент.";
            descr_DO9.Content = "---";
            descr_DO10.Content = "---";
            descr_DO11.Content = "---";
            descr_DO12.Content = "Работа/Простой";
            descr_DO13.Content = "Работа/Авария";
            descr_DO14.Content = "Бак.секции";
            descr_DO15.Content = "---";
            descr_DO16.Content = "---";
            descr_DO17.Content = "---";
            descr_DO18.Content = "ТЭНы КВУ";
        }

        private void setCotent_ICE_E()
        {
            descr_U1.Content = "---";
            descr_U2.Content = "T наружного";
            descr_U3.Content = "T притока";
            descr_U4.Content = "T вытяжного";
            descr_U5.Content = "U вытяжного";
            descr_U6.Content = "---";
            descr_U7.Content = "---";
            descr_U8.Content = "---";
            descr_U9.Content = "---";
            descr_U10.Content = "---";

            descr_Y1.Content = "Наружные заслонки";
            descr_Y2.Content = "Рециркуляция 1";
            descr_Y3.Content = "Вертикальные (ротор)";
            descr_Y4.Content = "3-ходовой клапан";
            descr_Y5.Content = "Горизонтальные (ротор)";
            descr_Y6.Content = "---";

            descr_DI1.Content = "---";
            descr_DI2.Content = "Пожар";
            descr_DI3.Content = "Вкл/выкл(RC)";
            descr_DI4.Content = "Компрессор 1";
            descr_DI5.Content = "Компрессор 2";
            descr_DI6.Content = "Приточный вент.";
            descr_DI7.Content = "Вытяжной вент.";
            descr_DI8.Content = "Реле контроля фаз";
            descr_DI9.Content = "ДПД фильтров";
            descr_DI10.Content = "ДПД рекуп.";
            descr_DI11.Content = "Э/нагрев";
            descr_DI12.Content = "Компрессор 3";
            descr_DI13.Content = "Регенератор";
            descr_DI14.Content = "---";
            descr_DI15.Content = "---";
            descr_DI16.Content = "---";
            descr_DI17.Content = "---";
            descr_DI18.Content = "---";

            descr_DO1.Content = "Роторный реген.";
            descr_DO2.Content = "---";
            descr_DO3.Content = "---";
            descr_DO4.Content = "Э/нагрев 1 ст.";
            descr_DO5.Content = "Э/нагрев 2 ст.";
            descr_DO6.Content = "Э/нагрев 3 ст.";
            descr_DO7.Content = "Приточный вент.";
            descr_DO8.Content = "Вытяжной вент.";
            descr_DO9.Content = "Э/нагрев 4 ст.";
            descr_DO10.Content = "Э/нагрев 5 ст.";
            descr_DO11.Content = "Э/нагрев 6 ст.";
            descr_DO12.Content = "Работа/Простой";
            descr_DO13.Content = "Работа/Авария";
            descr_DO14.Content = "Бак.секции";
            descr_DO15.Content = "---";
            descr_DO16.Content = "Э/нагрев 7 ст.";
            descr_DO17.Content = "Э/нагрев 8 ст.";
            descr_DO18.Content = "ТЭНы КВУ";
        }
    }
}
