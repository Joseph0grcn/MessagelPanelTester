using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsitMessagelPanelTester
{
    internal class SerPort:IPort
    {
        SerialPort SPort = new SerialPort();

        public bool IsConnected { get { return SPort.IsOpen; } }

        public SerPort(string PortName,int BaudRate,int DataBits,Parity Parity,StopBits StopBits,int readTimeout,int writeTimeout)
        {
            SPort.PortName = PortName;
            SPort.BaudRate = BaudRate;
            SPort.DataBits = DataBits;
            SPort.Parity = Parity;
            SPort.StopBits = StopBits;
            SPort.ReadTimeout = readTimeout;
            SPort.WriteTimeout = writeTimeout;
        }

        public void Connect()
        {
            SPort.Open();
        }

        public int ReadChar()
        {
            return SPort.ReadChar();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            SPort.Write(buffer, offset, count);
        }

        public byte[] Read()
        {
            throw new NotImplementedException();
        }

        public bool Write(byte[] buffer, byte[] expectedAnswer, int timeout = 25)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void SendMessage(int rowNo, string text, int marqueeStatus, int alignment)
        {
            throw new NotImplementedException();
        }

        public void SetMarquuInterval(int val)
        {
            throw new NotImplementedException();
        }

        public void SetFont(int font)
        {
            throw new NotImplementedException();
        }

        public string GetVersion()
        {
            throw new NotImplementedException();
        }

        public void SetPanelType(int type)
        {
            throw new NotImplementedException();
        }

        bool IPort.SetMarquuInterval(int val)
        {
            throw new NotImplementedException();
        }

        bool IPort.SetPanelType(int type)
        {
            throw new NotImplementedException();
        }

        bool IPort.SendMessage(int rowNo, string text, int marqueeStatus, int alignment)
        {
            throw new NotImplementedException();
        }

        public bool Setbrightness(int val)
        {
            throw new NotImplementedException();
        }

        public bool SetPanelSize(byte row, byte col)
        {
            throw new NotImplementedException();
        }

        public bool SetFullLed(byte val)
        {
            throw new NotImplementedException();
        }

        public bool DeviceReset()
        {
            throw new NotImplementedException();
        }
    }
}
