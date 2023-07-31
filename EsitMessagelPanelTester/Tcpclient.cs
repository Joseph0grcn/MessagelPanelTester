using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace EsitMessagelPanelTester
{
    public class Tcpclient : IPort
    {
        enum ConnectionState
        {
            NULL,
            CONNECTING,
            CONNECTED,
            INITIALIZED,
            DISCONNECTED
        }

        public string IpAddress;
        public int Port;
        TcpClient client;
        Task mainProcess;
        string weightValue = "0";
        string prevWeightValue = "0";
        string message = "";
        string prevMessage = "";
        // Create CancellationTokenSource.
        CancellationTokenSource source;
        ConnectionState connectionState = ConnectionState.NULL;

        public bool IsConnected { get {
                bool result = false;
                try
                {
                    result = client.Connected;
                }
                catch
                {

                } 
                return result;
            }}

        public Tcpclient(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            Port = port;
        }
        public void SetWeight(string val)
        {
            weightValue = val;
        }
        public void SetMessage(string val)
        {
            message = val;
        }

        public bool SetMarquuInterval(int val)
        {
            try
            {
                //string newText = string.Format("02{0}0{1}", val.ToString(), Environment.NewLine);
                //Byte[] data = System.Text.Encoding.UTF8.GetBytes(newText);
                //NetworkStream stream = client.GetStream();

                byte[] data = new byte[]
                {
                    (byte)0,
                    (byte)2,
                    (byte)val,
                    (byte)0x0A
                };
                byte[] dataExpected = new byte[] { 0x06, 0x0A };
                int try_count = 5;
                while (try_count != 0)
                {
                    try
                    {
                        
                        if (Write(data, dataExpected))
                        {
                           return true;
                        }
                    }
                    catch (Exception ex)
                    {
                       
                    }
                    try_count--;
                }
                return false;

                //stream.WriteTimeout = 10000;

                //// Send the message to the connected TcpServer.
                //stream.Write(data, 0, data.Length);

                ////stream.Close();

                //Console.WriteLine("Sent: {0}", string.Join("-", data));
            }
            catch (Exception ex)
            {
                connectionState = ConnectionState.DISCONNECTED;
                throw ex;
            }
        }
        public void SetFont(int font)
        {
            try
            {
                //string newText = string.Format("02{0}0{1}", val.ToString(), Environment.NewLine);
                //Byte[] data = System.Text.Encoding.UTF8.GetBytes(newText);
                NetworkStream stream = client.GetStream();

                byte[] data = new byte[]
                {
                    (byte)0,
                    (byte)3,
                    (byte)font,
                    (byte)'\n'
                };

                stream.WriteTimeout = 10000;

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);

                //stream.Close();

                Console.WriteLine("Sent: {0}", string.Join("-", data));
            }
            catch (Exception ex)
            {
                connectionState = ConnectionState.DISCONNECTED;
                throw ex;
            }
        }
        private bool connect()
        {
            bool result = false;
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.
                connectionState = ConnectionState.CONNECTING;
                client = new TcpClient();
                if (!client.ConnectAsync(IpAddress, Port).Wait(5000))
                {
                    throw new Exception("BAĞLANTI HATASI!");
                }
                connectionState = ConnectionState.CONNECTED;
                result = true;
            }
            catch (SocketException se)
            {
                connectionState = ConnectionState.DISCONNECTED;
                Console.WriteLine("SocketException: " + se.Message);
                result = connect();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public void Start()
        {
            try
            {
                source = new CancellationTokenSource();
                // ... Get Token from source.
                var token = source.Token;
                mainProcess = Task.Run(() => {
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            connect();
                            while (!token.IsCancellationRequested)
                            {


                                switch (connectionState)
                                {
                                    case ConnectionState.CONNECTED:

                                        if(prevMessage != message)
                                        {
                                          if(send(1, message, 2, 0))
                                            {
                                                prevMessage = message;
                                                Thread.Sleep(100);
                                            }
                                        /*
                                       if(send("1", message, "2","0"))
                                      {
                                          prevMessage = message;
                                          Thread.Sleep(100);
                                      }
                                        */
                                  }
                                  if (prevWeightValue != weightValue)
                                  {
                                      if (send(2, weightValue, 0, 0))
                                      {
                                          prevWeightValue = weightValue;
                                      }
                                  }
                                  break;
                              case ConnectionState.INITIALIZED:
                                  break;
                          }
                          Thread.Sleep(100);
                      }
                      Thread.Sleep(200);
                  }
                  catch (Exception ex)
                  {
                      try
                      {
                          if (client != null)
                          {
                              client.Close();
                              client = null;
                          }

                      }
                      catch (Exception ex1)
                      {
                          Console.WriteLine(ex1.Message);
                      }

                      Console.WriteLine(ex.Message);
                  }



              }

          });
            }
      catch (ArgumentNullException e)
      {
          Console.WriteLine("ArgumentNullException: {0}", e);
      }
      catch (SocketException e)
      {
          Console.WriteLine("SocketException: {0}", e);
      }

      Console.WriteLine("\n Press Enter to continue...");
  }

  public void Stop()
  {
      try
      {
          if (client != null)
          {
              source.Cancel();
              client.Close();
              client = null;
          }
      }
      catch (Exception ex)
      {
          Console.WriteLine(ex.Message);
          connectionState = ConnectionState.DISCONNECTED;
      }
  }

        private bool send(int rowNo, string text, int marqueeStatus, int alignment)
        {
            bool result = false;
            try
            {
                List<byte> data = new List<byte>
                {
                    (byte)1,
                (byte)rowNo,
                (byte)marqueeStatus,
                (byte)alignment
                };

                data.AddRange(System.Text.Encoding.UTF8.GetBytes(text));

                data.Add((byte)'\n');

                byte[] dataExpected = new byte[] { 0x06, 0x0A };
                int try_count = 5;
                while (try_count != 0)
                {
                    if (Write(data.ToArray(), dataExpected))
                    {
                        return true;
                    }
                    try_count--;
                }
                return false;
            }
            catch (Exception ex)
            {
                connectionState = ConnectionState.DISCONNECTED;
                throw ex;
            }
        }

        private string read()
        {
            try
            {
                //---get the incoming data through a network stream---
                NetworkStream nwStream = client.GetStream();
                nwStream.ReadTimeout = 10000;
                byte[] buffer = new byte[client.ReceiveBufferSize];

                //---read incoming stream---
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                //---convert the data received into a string---
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received: {0}", dataReceived);

                // Close everything.
                //nwStream.Close();
                return dataReceived;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Connect()
        {
            connect();
        }

        public byte[] Read()
        {
            try
            {
                //---get the incoming data through a network stream---
                NetworkStream nwStream = client.GetStream();
                nwStream.ReadTimeout = 10000;
                byte[] buffer = new byte[client.ReceiveBufferSize];

                //---read incoming stream---
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                return buffer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int ReadChar()
        {
            byte[] byteArray = Read();
            return byteArray[0];
        }

        public bool Write(byte[] buffer, byte[] expectedAnswer = null, int timeout = 25)
        {
            bool result = false;
            try
            {
                NetworkStream stream = client.GetStream();
                stream.WriteTimeout = 10000;

                // Send the message to the connected TcpServer.
                stream.Write(buffer, 0, buffer.Length);

                if(expectedAnswer != null)
                {
                    Thread.Sleep(timeout);
                    if(!stream.DataAvailable)
                    {
                        return result;
                    }
                    byte[] bufferReceived = new byte[client.ReceiveBufferSize];
              
                    //---read incoming stream---
                    int bytesRead = stream.Read(bufferReceived, 0, bufferReceived.Length);
                    if (bytesRead >= expectedAnswer.Length)
                    {
                        int indexOfExpectedAnswer = -1;
                        int indexOfPreviousExpextedAnswer = -1;
                        for(int i = 0; i < expectedAnswer.Length; i++)
                        {
                            indexOfExpectedAnswer = Array.IndexOf(bufferReceived, expectedAnswer[i]);
                            if (indexOfExpectedAnswer != -1)
                            {
                                if (i != 0)
                                {
                                    if (indexOfPreviousExpextedAnswer == indexOfExpectedAnswer - 1)
                                    {
                                        result = true;
                                    }
                                    else
                                    {
                                        result = false;
                                    }
                                }
                                else
                                {
                                    result = true;
                                }
                                indexOfPreviousExpextedAnswer = indexOfExpectedAnswer;
                            }
                        }
                    }
                }
                else
                {
                    result = true;
                }
            }
            catch(Exception ex)
            {
                result = false;
            }
            return result;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            NetworkStream stream = client.GetStream();
            // Send the message to the connected TcpServer.
            stream.Write(buffer, offset, count);
        }

        public void Disconnect()
        {
           client.Close();
        }

        public bool SendMessage(int rowNo, string text, int marqueeStatus, int alignment)
        {
            return send(rowNo, text, marqueeStatus, alignment);
        }

        public string GetVersion()
        {
            int i = 5;
            while (i != 0)
            {
                try
                {
                    byte End = 0x0A;
                    byte[] data = new byte[] { 0x02, 0x01, 0x0A };
                    Write(data, 0, data.Length);
                    Thread.Sleep(50);
                    byte[] bytes = Read();
                    if (bytes != null && bytes.Length > 2)
                    {
                        int locend = Array.IndexOf(bytes, End);
                        if (locend != -1)
                        {
                            byte major = bytes[locend - 2];
                            byte minor = bytes[locend - 1];
                            return major + "." + minor;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if(i == 1) { throw ex; }
                    
                }
                i--;
            }
            return "";
        }

        public bool SetPanelType(int type)
        {
            try
            {
                //string newText = string.Format("02{0}0{1}", val.ToString(), Environment.NewLine);
                //Byte[] data = System.Text.Encoding.UTF8.GetBytes(newText);
                //NetworkStream stream = client.GetStream();

                byte[] data = new byte[]
                {
                    (byte)2,
                    (byte)type,
                    (byte)0x0A
                };
                byte[] dataExpected = new byte[] { 0x06, 0x0A };
                int try_count = 5;
                while (try_count != 0)
                {
                    try
                    {

                        if (Write(data, dataExpected,500))
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    try_count--;
                }
                return false;

                //stream.WriteTimeout = 10000;

                //// Send the message to the connected TcpServer.
                //stream.Write(data, 0, data.Length);

                ////stream.Close();

                //Console.WriteLine("Sent: {0}", string.Join("-", data));
            }
            catch (Exception ex)
            {
                connectionState = ConnectionState.DISCONNECTED;
                throw ex;
                
            }
        }

        public bool Setbrightness(int val)
        {
            try
            {
                //string newText = string.Format("02{0}0{1}", val.ToString(), Environment.NewLine);
                //Byte[] data = System.Text.Encoding.UTF8.GetBytes(newText);
                //NetworkStream stream = client.GetStream();

                byte[] data = new byte[]
                {
                    (byte)0,
                    (byte)0,
                    (byte)val,
                    (byte)0x0A
                };
                byte[] dataExpected = new byte[] { 0x06, 0x0A };
                int try_count = 5;
                while (try_count != 0)
                {
                    try
                    {

                        if (Write(data, dataExpected))
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    try_count--;
                }
                return false;

                //stream.WriteTimeout = 10000;

                //// Send the message to the connected TcpServer.
                //stream.Write(data, 0, data.Length);

                ////stream.Close();

                //Console.WriteLine("Sent: {0}", string.Join("-", data));
            }
            catch (Exception ex)
            {
                connectionState = ConnectionState.DISCONNECTED;
                throw ex;
            }
        }

        public bool SetPanelSize(byte row, byte col)
        {
            try
            {
                byte[] data = new byte[]
                {
                    (byte)0,
                    (byte)1,
                    (byte)row,
                    (byte)col,
                    (byte)0x0A
                };
                byte[] dataExpected = new byte[] { 0x06, 0x0A };
                int try_count = 5;
                while (try_count != 0)
                {
                    try
                    {

                        if (Write(data, dataExpected))
                        {
                            return true;
                        }
                        Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {

                    }
                    try_count--;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return false;
            
        }

        public bool SetFullLed(byte val)
        {
            try
            {
                byte[] data = new byte[]
                {
                    (byte)0,
                    (byte)4,
                    (byte)val,
                    (byte)0x0A

                };
                byte[] dataExpected = new byte[] { 0x06, 0x0A };
                int try_count = 5;
                while (try_count != 0)
                {
                    try
                    {

                        if (Write(data, dataExpected))
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    try_count--;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return false;

        }

        public bool DeviceReset()
        {
            try
            {
                byte[] data = new byte[]
                {
                    (byte)2,
                    (byte)0,
                    (byte)0x0A
                };
                byte[] dataExpected = new byte[] { 0x06, 0x0A };
                int try_count = 5;
                while (try_count != 0)
                {
                    try
                    {
                        if (Write(data, dataExpected))
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    try_count--;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return false;
        }
    }
}
