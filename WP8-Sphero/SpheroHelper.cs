﻿//using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Windows.Media;
using System.ComponentModel;

namespace WP8_Sphero
{
    public class SpheroHelper : INotifyPropertyChanged
    {
        private StreamSocket spheroSocket;
        
        private bool isConnected;
        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                isConnected = value;
                OnPropertyChanged("IsConnected");
            }
        }
        private Color spheroColor;
        public Color SpheroColor
        {
            get { return spheroColor; }
            set
            {
                spheroColor = value;
                OnPropertyChanged("SpheroColor");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }       


        public SpheroHelper()
        {
            this.isConnected = false;
            this.spheroColor = new Color();
            spheroColor.A = 255; spheroColor.R = 255; spheroColor.G = 255; spheroColor.B = 255;

        }

        public async void Bluetooth_ConnectToSphero()
        {
            PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";

            try
            {
                var peers = await PeerFinder.FindAllPeersAsync();

                if (!peers.Any(p => p.DisplayName.Contains("Sphero")))
                {
                    MessageBox.Show("Sphero not found. Is bluetooth on? Is Sphero paired?");
                }
                else
                {
                    try
                    {
                        PeerInformation spheroPeer = peers.First(p => p.DisplayName.Contains("Sphero"));

                        spheroSocket = new StreamSocket();

                        await spheroSocket.ConnectAsync(spheroPeer.HostName, "1");

                        MessageBox.Show("Connected to " + peers[0].DisplayName + " bluetooth service.");
                        this.IsConnected = true;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Something went wrong with bluetooth pairing: " + e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No bluetooth devices found. Is Sphero paired and on (blinking in 3 colors)?");
                return;
            }
        }

        public void Bluetooth_Disconnect()
        {
            spheroSocket.Dispose();
            this.IsConnected = false;
        }
        
        public async void Bluetooth_SpheroToFixedColor(Color color)
        {
            if (spheroSocket == null)
            {
                MessageBox.Show("Connect to Sphero first.");
                return;
            }

            byte[] package = new ColorLedCommand(color).ToPacket();
            await spheroSocket.OutputStream.WriteAsync(GetBufferFromByteArray(package));
        }

        public async void Bluetooth_ShowBackLed(int bright)
        {
            if (spheroSocket == null)
            {
                MessageBox.Show("Connect to Sphero first.");
                return;
            }

            byte[] package = new BackLedCommand(bright).ToPacket();
            await spheroSocket.OutputStream.WriteAsync(GetBufferFromByteArray(package));
        }

        public async void Bluetooth_SetHeading(int heading)
        {
            if (spheroSocket == null)
            {
                MessageBox.Show("Connect to Sphero first.");
                return;
            }

            byte[] package = new SetHeading(heading).ToPacket();
            await spheroSocket.OutputStream.WriteAsync(GetBufferFromByteArray(package));
        }

        public async void Bluetooth_Roll(int direction, int speed)
        {
            if (spheroSocket == null)
            {
                MessageBox.Show("Connect to Sphero first.");
                return;
            }

            byte[] package = new Roll(direction, speed).ToPacket();
            await spheroSocket.OutputStream.WriteAsync(GetBufferFromByteArray(package));
        }




        private IBuffer GetBufferFromByteArray(byte[] package)
        {
            using (DataWriter dw = new DataWriter())
            {
                dw.WriteBytes(package);
                return dw.DetachBuffer();
            }
        }

    }
}