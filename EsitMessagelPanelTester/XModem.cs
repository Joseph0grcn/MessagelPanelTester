using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace EsitMessagelPanelTester
{
    static class Constants
    {
        public const byte SOH = 0x01;
        public const byte SOX = 0x02;
        public const byte EOT = 0x04;
        public const byte ETB = 0x17;

        public const byte ACK = 0x06;
        public const byte NAK = 0x15;

        public const byte CAN = 0x18;
    }
    class XModem
    {
        const int BUFFER_SIZE = 1024;//128
        crc16 Crc_o = new crc16();

        byte[] Sender_Packet = new byte[3];
        byte[] Sender_Data = new byte[BUFFER_SIZE];
        byte[] Sender_Crc = new byte[2];
        byte[] Sender_EOT = new byte[1];
        ushort Sender_Crc_us;

        IPort SPort;
        public event EventHandler<string> CurrentStatusHandler;
        byte Sender_Packet_Number = 0;

        /*
		 * 수정 시작 : init 할 때 시리얼 포트 정보를 받는다.
		 */
        public int init_xmodem(IPort Port)
        {
            Sender_EOT[0] = Constants.EOT;

            SPort = Port;
            SPort.Connect();
            if (SPort.IsConnected)
            {
                return 1;
            }
            else
            {
                return 0;
            }


        }

        /************************************************
		 * 파일을 받아서 송신하는 것을 전부 관장함.
		 */
        public void xmodem_send(string fileName)
        {
            Task.Factory.StartNew(() =>
            {
                /*File 연결*/
                using (FileStream fs = File.Open(fileName, FileMode.Open))
                {
                    using (BinaryReader B_reader = new BinaryReader(fs))
                    {
                        int err = 0;
                        string Read_line;
                        Sender_Packet_Number = 1;
                        this.wait_c();
                        Sender_Data = B_reader.ReadBytes(BUFFER_SIZE);
                        CurrentStatusHandler?.Invoke(this, "Firmware upload started");
                        err = Send_Packet(Sender_Data, Sender_Packet_Number, BUFFER_SIZE);
                        while (true)
                        {
                            if (err == 1)
                            {
                                Sender_Data = B_reader.ReadBytes(BUFFER_SIZE);

                                if (Sender_Data.Length == 0)
                                {
                                    SPort.Write(Sender_EOT, 0, 1);
                                    while (true)
                                    {
                                        //Read_line = SPort.ReadLine();
                                        if (Wait_ACK_NAK(1) == 1)
                                        {
                                            CurrentStatusHandler?.Invoke(this, "Firmware uploaded successfully");
                                            Console.WriteLine("Firmware uploaded successfully");
                                            return;
                                        }

                                    }

                                    //break;
                                }
                                else if (Sender_Data.Length != BUFFER_SIZE)
                                {
                                    byte[] full_stream = new byte[BUFFER_SIZE];
                                    byte[] zero_ary = new byte[BUFFER_SIZE - Sender_Data.Length];
                                    Array.Clear(zero_ary, 0, zero_ary.Length);

                                    Console.WriteLine(Sender_Data.Length);

                                    Array.Copy(Sender_Data, 0, full_stream, 0, Sender_Data.Length);
                                    Array.Copy(zero_ary, 0, full_stream, Sender_Data.Length, zero_ary.Length);

                                    Sender_Data = full_stream;
                                }

                                Sender_Packet_Number++;
                                err = Send_Packet(Sender_Data, Sender_Packet_Number, BUFFER_SIZE);
                            }
                            else
                            {
                                err = Send_Packet(Sender_Data, Sender_Packet_Number, err);
                                //throw new Exception("EXCEPTION");
                            }
                        }
                    }
                }
            });
        }

        /*
         * XModem 시작 문자 대기 'C'
         */
        private void wait_c()
        {
            char readchar;
            while (true)
            {
                readchar = (char)SPort.ReadChar();
                Console.WriteLine(readchar);
                if (readchar == 'C')
                {
                    break;
                }
            }
        }

        /********************************************************
		 * send 형식 바꿀 때는 이 곳을 변경
         * make packet and send
         */
        private int Send_Packet(byte[] data, byte SPN, int Length)
        {
            if (Length == 128)
            {
                Sender_Packet[0] = Constants.SOH;
            }
            else if (Length == 1024)
            {
                Sender_Packet[0] = Constants.SOX;
            }
            Sender_Packet[1] = SPN;

            /*byte -> int 형변환 때문에 더하기 빼기도 어렵네...*/
            //Sender_Packet[2] = BitConverter.GetBytes(255 - SPN)[0];
            Sender_Packet[2] = (byte)(~SPN);
            CurrentStatusHandler?.Invoke(this, "Sent Packet Number:" + Sender_Packet_Number);
            Console.WriteLine("Packet Number:" + Sender_Packet_Number);
            Console.WriteLine(BitConverter.ToString(Sender_Packet));
            SPort.Write(Sender_Packet, 0, 3);

            Console.WriteLine(BitConverter.ToString(data));
            SPort.Write(data, 0, Length);

            //Sender_Crc = crc16.ComputeCrc(data);
            //Sender_Crc = Crc_o.calcrc(data);
            Sender_Crc_us = crc16.CRC16_ccitt(data, 0, Length);
            Sender_Crc[0] = (byte)((Sender_Crc_us >> 8) & 0xFF);
            Sender_Crc[1] = (byte)(Sender_Crc_us & 0xFF);

            Console.WriteLine(BitConverter.ToString(Sender_Crc));
            SPort.Write(Sender_Crc, 0, 2);

            return Wait_ACK_NAK(Length);
        }

        private int Wait_ACK_NAK(int Length)
        {
            int SPort_read;

            while (true)
            {
                Console.WriteLine("Wait_ACK_NAK");
                SPort_read = SPort.ReadChar();//Console.Read();

                if (SPort_read == Constants.NAK) 		//Constants.NAK
                {
                    Console.WriteLine("NAK");
                    return Length;
                }
                else if (SPort_read == Constants.ACK) 	//Constants.ACK
                {
                    Console.WriteLine("ACK");
                    return 1;
                }
                else
                {
                    Console.WriteLine(SPort_read);
                }
            }
        }

        //private byte 
    }
}
