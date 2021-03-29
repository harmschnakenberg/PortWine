using System;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PortWine
{
    public class ReliableSerialPort : SerialPort
    {
        
        #region Connection
        public ReliableSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();

            if (ports.Length == 0)
            {
                throw new Exception("Es sind keine COM-Ports vorhanden.");
            }


            if (!Array.Exists(ports, x => x == portName))
            {
                int pos = ports.Length - 1;
                portName = ports[pos];
            }

            PortName = portName;
            BaudRate = baudRate;
            DataBits = dataBits;
            Parity = parity;
            StopBits = stopBits;
            Handshake = Handshake.None;
            DtrEnable = true;
            NewLine = Environment.NewLine;
            ReceivedBytesThreshold = 1024;

        }

        new public void Open()
        {
            int Try = 10;

            do
            {
                try
                {
                    base.Open();
                    ContinuousRead();
                }
                catch
                {
                    Console.WriteLine(base.PortName + " verbleibende Verbindungsversuche: " + Try);
                    System.Threading.Thread.Sleep(2000);
                }

            } while (!base.IsOpen && --Try > 0);
        }

        #endregion

        #region Read
        private void ContinuousRead()
        {
            byte[] buffer = new byte[4096];
            Action kickoffRead = null;
            kickoffRead = (Action)(() => BaseStream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult ar)
            {
                try
                {
                    int count = BaseStream.EndRead(ar);
                    byte[] dst = new byte[count];
                    Buffer.BlockCopy(buffer, 0, dst, 0, count);
                    OnDataReceived(dst);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("OptimizedSerialPort exception !" + exception.Message);
                }
                kickoffRead();
            }, null)); kickoffRead();
        }

        public delegate void DataReceivedEventHandler(object sender, DataReceivedArgs e);
        new public event EventHandler<DataReceivedArgs> DataReceived;

        static string recLine = string.Empty;

        public virtual void OnDataReceived(byte[] data)
        {
            string rec = System.Text.Encoding.UTF8.GetString(data);
            recLine += rec;

            if (recLine.Contains("\r\nOK\r\n") || recLine.Contains("ERROR"))
            {
                var handler = DataReceived;
                if (handler != null)
                {
                    handler(this, new DataReceivedArgs { Data = recLine });
                    recLine = string.Empty;
                }
            }
        }

        #endregion

        #region Write

        new async public void WriteLine(string message)
        {
            base.WriteLine(message);
            await Task.Delay(base.WriteTimeout);
        }

        #endregion
    }

    public class DataReceivedArgs : EventArgs
    {
        public string Data { get; set; }
    }
}