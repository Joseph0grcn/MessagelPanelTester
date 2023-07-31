using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsitMessagelPanelTester
{
    internal interface IPort
    {
        bool IsConnected { get; }
        void Connect();
        void Disconnect();
        void Write(byte[] buffer, int offset, int count);
        bool Write(byte[] buffer, byte[] expectedAnswer = null, int timeout = 25);
        byte[] Read();
        int ReadChar();

        bool SendMessage(int rowNo, string text, int marqueeStatus, int alignment);
        bool SetMarquuInterval(int val);
        void SetFont(int font);
        bool SetPanelType(int type);
        string GetVersion();
        bool Setbrightness(int val);

        //yusuf ekleme


        bool SetPanelSize(byte row, byte col);
        bool SetFullLed(byte val);

        bool DeviceReset();

    }
}
