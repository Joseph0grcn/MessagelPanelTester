using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace EsitMessagelPanelTester
{
    public partial class Form1 : Form
    {
        List<string> messageList = new List<string>()
        {
            "türkçe desteği",
            "KANTARA GİRİN",
            "TARTIM YAPILIYOR",
            "34ERD4",
            "TARTIM TAMAM ÇIKINIZ"
        };
        int messageListCount = 0;
        IPort panel1;
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        System.Timers.Timer timer2 = new System.Timers.Timer(3000);
        UXLogger logger = new UXLogger();
        bool IsTestStarted = false;
        int weight = 0;
        
        
        
        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            DipLoad();
            
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (panel1 == null || !panel1.IsConnected)
            {
                return;
            }
            if (!IsTestStarted)
            {

                timer.Elapsed += Timer_Elapsed;
                timer2.Elapsed += Timer2_Elapsed;
                timer.Start();
                timer2.Start();
                tableLayoutPanel2.Enabled = false;
                panel4.Enabled = false;
                panel5.Enabled = false;
                panel9.Enabled = false;
                deviceReset.Enabled = false;
                btnStart.Text = "Testi Durdur";
                IsTestStarted = true;
                logger.PushLog(tssLog, rtxLog, "Test Başlatıldı");

                weight = 0;
            }
            else
            {
                timer.Elapsed -= Timer_Elapsed;
                timer2.Elapsed -= Timer2_Elapsed;
                timer.Stop();
                timer2.Stop();
                tableLayoutPanel2.Enabled = true;
                panel4.Enabled = true;
                panel5.Enabled = true;
                panel9.Enabled = true;
                deviceReset.Enabled = true;
                btnStart.Text = "Testi Başlat";
                IsTestStarted = false;
                logger.PushLog(tssLog, rtxLog, "Test Durduruldu");
            }

        }

        private void Timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (panel1 == null || !panel1.IsConnected)
            {
                return;
            }

            if (panel1.SendMessage(1, messageList[messageListCount], 2, 0))
            {
                lblRow1Data.Text = messageList[messageListCount++];
                if (messageListCount > messageList.Count - 1)
                {
                    messageListCount = 0;
                }
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (panel1 == null || !panel1.IsConnected)
            {
                return;
            }
            //Random random = new Random();
            //string randomWeight = random.Next(10, 60000).ToString();
            //panel1.SendMessage(2, randomWeight, 0, 0);
            string w = weight.ToString();
            if (panel1.SendMessage(2, w, 0, 0))
            {
                lblRow2Data.Text = w;
                weight++;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (panel1 == null || !panel1.IsConnected)
            {
                return;
            }
            panel1.SendMessage(1, txtMessage1.Text, 2, 0);
            logger.PushLog(tssLog, rtxLog, "Birinci Satır Gönderildi");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (panel1 == null || !panel1.IsConnected)
            {
                return;
            }
            panel1.SetMarquuInterval((int)numericUpDown1.Value);
            logger.PushLog(tssLog, rtxLog, "Kayma Hızı Gönderildi");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel1.SetFont(comboBox1.SelectedIndex);
            logger.PushLog(tssLog, rtxLog, "Yazı Stili Gönderildi");
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                
                if (panel1 == null || !panel1.IsConnected)
                {
                    panel1 = new Tcpclient(txtIP.Text, Convert.ToInt32(txtPort.Text));
                    panel1.Connect();
                    btnConnect.Text = "Bağlantı kes";
                    tableLayoutPanel1.Enabled = true;
                    logger.PushLog(tssLog, rtxLog, "Bağlantı Sağlandı");
                    
                    
                }
                else
                {
                    panel1.Disconnect();
                    btnConnect.Text = "Bağlan";
                    tableLayoutPanel1.Enabled= false;
                    panel1 = null;
                    logger.PushLog(tssLog, rtxLog, "Bağlantı kesildi");
                    

                }
                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void btnFolderFirmware_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Binary Files (*.bin)|*.bin";
                openFileDialog.ShowDialog();

                if (openFileDialog.FileName != "")
                {
                    txtFirmwareFolder.Text = openFileDialog.FileName;
                }
            }
        }

        private void btnSendFirmware_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtFirmwareFolder.Text))
            {
                //MessageBox.Show("Dosya Bulunamadı.");
                logger.PushLog(tssLog, rtxLog, "Firmware Dosyası Bulunamadı");
                return;
            }
            if(panel1 == null)
            {
                panel1 = new Tcpclient(txtIP.Text, Convert.ToInt32(txtPort.Text));
            }
            rTxFirmwareLogs.Text = "";
            XModem xModem = new XModem();
            xModem.init_xmodem(panel1);
            
            if (makeReadyForBootloader())
            {
                xModem.CurrentStatusHandler += XModem_CurrentStatusHandler;
                xModem.xmodem_send(txtFirmwareFolder.Text);
                logger.PushLog(tssLog, rtxLog, "Firmware Yüklemesi Başlatıldı");
            }
            else
            {
                rTxFirmwareLogs.Text = "Yükleme Başlatılamadı.";
                logger.PushLog(tssLog, rtxLog, "Firmware Yüklemesi Başlatılamadı");
            }
         
            //xModem.CurrentStatusHandler -= XModem_CurrentStatusHandler;
        }

        bool makeReadyForBootloader()
        {
            bool result = false;
            byte ACK = 0x06;
            byte END = 0x0A;
            int i = 5;
            while (i != 0)
            {
                try
                {
                    byte[] data = new byte[] { 0x02, 0x00, 0x0A };
                    byte[] dataExpected = new byte[] { 0x06, 0x0A };
                    if(panel1.Write(data, dataExpected,250))
                    {
                        result = true;
                        break;
                    }
                    else
                    {
                        result = false;
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    result = false;
                }
                i--;
            }
            Thread.Sleep(1000);
            i = 5;
            while (i != 0)
            {
                try
                {
                    byte[] data = new byte[] { 0x62 };
                    byte b = 0x43;
                    if (panel1.Write(data,new byte[] { b }))
                    {
                        result = true;
                        break;
                    }
                    else
                    {
                        result = false;
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    result = false;
                }
                i--;
            }
            return result;
        }

        private void XModem_CurrentStatusHandler(object sender, string e)
        {
            rTxFirmwareLogs.Text += e + Environment.NewLine;
            // set the current caret position to the end
            rTxFirmwareLogs.SelectionStart = rTxFirmwareLogs.Text.Length;
            // scroll it automatically
            rTxFirmwareLogs.ScrollToCaret();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label4.Text = panel1.GetVersion();
            logger.PushLog(tssLog, rtxLog, "Versiyon Gönderildi");
        }

        private void btnSend2_Click(object sender, EventArgs e)
        {
            if (panel1 == null || !panel1.IsConnected)
            {
                return;
            }
            panel1.SendMessage(2, txtMessage2.Text, 0, 0);
            logger.PushLog(tssLog, rtxLog, "İkinci Satır Gönderildi");


        }

        private void button4_Click(object sender, EventArgs e)
        {
            logger.PushLog(tssLog, rtxLog, "Led Panel Türü Gönderildi");

            if (panel1 == null || !panel1.IsConnected)
            {
                logger.PushLog(tssLog, rtxLog, "Led Panel Türü Değişimi Başarısız");
                return;
            }
            int secim = cmbLedType.SelectedIndex + 2;
            if (panel1.SetPanelType(secim))//logic anlayamadım
            {
                
                if (cmbLedType.SelectedIndex == 0)
                {
                    logger.PushLog(tssLog, rtxLog, "Led Panel Türü Değişimi (SMD) Başarılı");
                }else if (cmbLedType.SelectedIndex == 1)
                {
                    logger.PushLog(tssLog, rtxLog, "Led Panel Türü Değişimi (DİP) Başarılı");
                }
                
            }
            else
            {
                logger.PushLog(tssLog, rtxLog, "Led Panel Türü Değişimi Başarısızzzz");
            }
        }

        private void btnSendBrightness_Click(object sender, EventArgs e)
        {
            if (panel1 == null || !panel1.IsConnected)
            {
                return;
            }
            if (panel1.Setbrightness((int)numericUpDown2.Value))
            {
                logger.PushLog(tssLog, rtxLog, "Parlaklık Ayarı Gönderildi");
            }
        }

        private void setpanel_Click(object sender, EventArgs e)
        {
            if (panel1 == null || !panel1.IsConnected)
            {
                return;
            }
            if (panel1.SetPanelSize((byte)rowNum.Value, (byte)colNum.Value))
            {
                logger.PushLog(tssLog, rtxLog, "Panel Boyutları Düzenlendi");
                if (panel1.DeviceReset())
                {
                    logger.PushLog(tssLog, rtxLog, "Cihaz Resetleniyor");
                    for (int i = 5; i > 0; i--)
                    {
                        logger.PushLog(tssLog, rtxLog, "Cihaz Resetleniyor ->" + i);
                        Thread.Sleep(500);
                    }
                    logger.PushLog(tssLog, rtxLog, "Cihaz Resetlendi");
                }
            }
        }

        private void ledOn_Click(object sender, EventArgs e)
        {
            if (panel1 == null || !panel1.IsConnected)
            {
                return;
            }

            
            if(rdDIP.Checked == false && rdSMD.Checked == false)
            {
                rtxLog.Text += "Tüm Ledleri Yakmak İçin Lütfen Panel Tipi Seçiniz!!!";
                MessageBox.Show("Tüm Ledleri Yakmak İçin Lütfen Panel Tipi Seçiniz!!!", "Hata");
                return ;
            }



            if (panel1.Setbrightness(Convert.ToInt32(lbParlaklık.Text)));
            {
                logger.PushLog(tssLog, rtxLog, "Parlaklık Ayarı Değiştirildi");
            }
            panel1.SendMessage(1, "", 0, 0);
            panel1.SendMessage(2, "", 0, 0);
            if (panel1.SetFullLed(0))
            {
                logger.PushLog(tssLog, rtxLog, "Tüm Ledler Açıldı");
            }
        }

        private void ledOff_Click(object sender, EventArgs e)
        {
            if (panel1 == null || !panel1.IsConnected)
            {
                return;
            }
            
            if (panel1.SetFullLed(1))
            {
                logger.PushLog(tssLog, rtxLog, "Tüm Ledler Kapatıldı");

            }
            
        }

        private void deviceReset_Click(object sender, EventArgs e)
        {
            if (panel1 == null || !panel1.IsConnected)
            {
                return;
            }

            logger.PushLog(tssLog, rtxLog, "Cihaz Resetleniyor");
            
            if (panel1.DeviceReset())
            {
                logger.PushLog(tssLog, rtxLog, "Cihaz Resetlendi");
            }
            else
            {
                logger.PushLog(tssLog, rtxLog, "Cihaz Resetleme İşlemi Başarısız");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (panel1 == null || !panel1.IsConnected)
            {
                return;
            }
            if(rbKayan.Checked == true) 
            {   //direkt olarak kayan yazı yollanırsa panele sığan kelimeleri panele gelmesi uzun sürüyor
                panel1.SendMessage((int)satırSec.Value, txtMessage3.Text, 2, 0);
                logger.PushLog(tssLog, rtxLog,"Seçilen "+ satırSec.Value.ToString() + ". Satıra Kayan Yazı Gönderildi");
            }
            else if(rbSabit.Checked == true)
            {
                panel1.SendMessage((int)satırSec.Value, txtMessage3.Text, 0, 0);
                logger.PushLog(tssLog, rtxLog,"Seçilen "+satırSec.Value.ToString() + ". Satıra Sabit Yazı Gönderildi");
            }
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            rtxLog.Text = "";
            logger.PushLog(tssLog, rtxLog, "Log Defteri Temizlendi.");
        }

        private void rdSMD_CheckedChanged(object sender, EventArgs e)
        {
            if (rdSMD.Checked == true)
            {
                trackBar1.Maximum = 15;
                
                trackBar1.Value = 15;
                lbParlaklık.Text = "15";
            }
        }

        private void rdDIP_CheckedChanged(object sender, EventArgs e)
        {
            if (rdDIP.Checked == true)
            {
                var apps = JsonConvert.DeserializeObject<AppSettings>(Properties.Settings.Default.DipSettings) ?? new AppSettings();

                trackBar1.Maximum = 200;
                trackBar1.Value = 200;
                lbParlaklık.Text = "200";
            }
        }

        
        

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            lbParlaklık.Text = trackBar1.Value.ToString();
        }

        private void DefaultSet_Click(object sender, EventArgs e)
        {
            //fabrika ayarları set edilecek fonksiyon çağrılacak

            AppSettings apps = null;
            if (rdbDIP.Checked == true)
            {
                apps = JsonConvert.DeserializeObject<AppSettings>(Properties.Settings.Default.DipSettings) ?? new AppSettings();
                DipLoad();


            }
            else if (rdbSMD.Checked == true)
            {
                apps = JsonConvert.DeserializeObject<AppSettings>(Properties.Settings.Default.SmdSettings) ?? new AppSettings();

                
                    numericUpDown1.Value = apps.Interval;
                    comboBox1.SelectedIndex = apps.Font;
                    cmbLedType.SelectedIndex = apps.PanelTip;
                    numericUpDown2.Value = apps.Parlaklık;
                    rowNum.Value = apps.Satir;
                    colNum.Value = apps.Sutun;
                
            }
            SetDefault(apps);


        }

        private void SetDefault(AppSettings apps)
        {
            if (panel1.SetMarquuInterval((int)apps.Interval))
            {
                logger.PushLog(tssLog, rtxLog, "Kayma Hızı Gönderildi");
            }
            else
            {
                logger.PushLog(tssLog, rtxLog, "Kayma Hızı Gönderilemedi");
            }



            if (panel1.Setbrightness((int)apps.Parlaklık))
            {
                logger.PushLog(tssLog, rtxLog, "Parlaklık Ayarı Gönderildi");
            }
            else
            {
                logger.PushLog(tssLog, rtxLog, "Parlaklık Ayarı Gönderilemedi");
            }



            panel1.SetFont(apps.Font);
            logger.PushLog(tssLog, rtxLog, "Yazı Stili Gönderildi");



            if (panel1.SetPanelSize((byte)apps.Satir, (byte)apps.Sutun))
            {
                logger.PushLog(tssLog, rtxLog, "Panel Boyutları Düzenlendi");
                if (panel1.DeviceReset())
                {
                    logger.PushLog(tssLog, rtxLog, "Cihaz Resetleniyor");
                    for (int i = 5; i > 0; i--)
                    {
                        logger.PushLog(tssLog, rtxLog, "Cihaz Resetleniyor ->" + i);
                        Thread.Sleep(500);
                    }
                    logger.PushLog(tssLog, rtxLog, "Cihaz Resetlendi");
                }
                else
                {

                }
            }

            if (panel1.SetPanelType(apps.PanelTip + 2))
            {
                if (apps.PanelTip == 0)
                {
                    logger.PushLog(tssLog, rtxLog, "Led Panel Türü Değişimi (SMD) Başarılı");
                }
                else if (apps.PanelTip == 1)
                {
                    logger.PushLog(tssLog, rtxLog, "Led Panel Türü Değişimi (DİP) Başarılı");
                }
            }
            else
            {
                logger.PushLog(tssLog, rtxLog, "Led Panel Türü Değişimi Başarısız");
            }
        }

        private void fabrikaAyarlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(Form2 form2 = new Form2())
            {
                form2.ShowDialog();
                
            }
        }

        private void cmbLedType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLedType.SelectedIndex == 0)
            {
                var apps = JsonConvert.DeserializeObject<AppSettings>(Properties.Settings.Default.SmdSettings) ?? new AppSettings();

                numericUpDown2.Maximum = 15;
                numericUpDown2.Value = apps.Parlaklık;
                
                
            }
            else if (cmbLedType.SelectedIndex == 1)
            {
                var apps = JsonConvert.DeserializeObject<AppSettings>(Properties.Settings.Default.DipSettings) ?? new AppSettings();

                numericUpDown2.Maximum = 200;
                numericUpDown2.Value = apps.Parlaklık;
                
            }
        }

        private void numericUpDown2_KeyUp(object sender, KeyEventArgs e)
        {
            if (cmbLedType.SelectedIndex == 0 && numericUpDown2.Value > 15)
            {
                numericUpDown2.Value = 15;
            }
            else if (cmbLedType.SelectedIndex == 1 && numericUpDown2.Value > 255)
            {
                numericUpDown2.Value = 200;
            }
        }

        private void rdbSMD_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbSMD.Checked) 
            {
                var apps = JsonConvert.DeserializeObject<AppSettings>(Properties.Settings.Default.SmdSettings) ?? new AppSettings();

                numericUpDown1.Value = apps.Interval;
                comboBox1.SelectedIndex = apps.Font;
                cmbLedType.SelectedIndex = apps.PanelTip;
                numericUpDown2.Value = apps.Parlaklık;
                rowNum.Value = apps.Satir;
                colNum.Value = apps.Sutun;
            }
           

        }

        private void rdbDIP_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbDIP.Checked)
            {
                DipLoad();
            }
        }

        private void DipLoad()
        {
            var apps = JsonConvert.DeserializeObject<AppSettings>(Properties.Settings.Default.DipSettings) ?? new AppSettings();

            numericUpDown1.Value = apps.Interval;
            comboBox1.SelectedIndex = apps.Font;
            cmbLedType.SelectedIndex = apps.PanelTip;
            numericUpDown2.Value = apps.Parlaklık;
            rowNum.Value = apps.Satir;
            colNum.Value = apps.Sutun;
        }
    }
}
