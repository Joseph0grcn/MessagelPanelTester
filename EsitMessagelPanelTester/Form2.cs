using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EsitMessagelPanelTester
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();

            
            //AppSettings apps  = JsonConvert.DeserializeObject<AppSettings>(Properties.Settings.Default.DipSettings) ?? new AppSettings();
            


            //nudInterval.Value = apps.Interval;
            //nudParlaklık.Value = apps.Parlaklık;
            //nudFont.Value = apps.Font;
            //nudSatır.Value = apps.Satir;
            //nudSutun.Value = apps.Sutun;
            //cmbPanelType.SelectedIndex = apps.PanelTip;
        }

        private void btnSetSetings_Click(object sender, EventArgs e)
        {
            if (cmbPanelType.SelectedIndex==0)
            {
                var apps = JsonConvert.DeserializeObject<AppSettings>(Properties.Settings.Default.SmdSettings) ?? new AppSettings();

                apps.Interval = Convert.ToInt32(nudInterval.Value);
                apps.Parlaklık = Convert.ToInt32(nudParlaklık.Value);
                apps.Font = Convert.ToInt32(cmbFont.SelectedIndex);
                apps.Satir = Convert.ToInt32(nudSatır.Value);
                apps.Sutun = Convert.ToInt32(nudSutun.Value);
                apps.PanelTip = Convert.ToInt32(cmbPanelType.SelectedIndex);

                string JsonString = JsonConvert.SerializeObject(apps);
                Properties.Settings.Default.SmdSettings = JsonString;
                Properties.Settings.Default.Save();
                
                MessageBox.Show("Kaydetme İşlemi Başarılı");
            }
            else if (cmbPanelType.SelectedIndex == 1)
            {
                var apps = JsonConvert.DeserializeObject<AppSettings>(Properties.Settings.Default.DipSettings) ?? new AppSettings();

                apps.Interval = Convert.ToInt32(nudInterval.Value);
                apps.Parlaklık = Convert.ToInt32(nudParlaklık.Value);
                apps.Font = Convert.ToInt32(cmbFont.SelectedIndex);
                apps.Satir = Convert.ToInt32(nudSatır.Value);
                apps.Sutun = Convert.ToInt32(nudSutun.Value);
                apps.PanelTip = Convert.ToInt32(cmbPanelType.SelectedIndex);

                string JsonString = JsonConvert.SerializeObject(apps);
                Properties.Settings.Default.DipSettings = JsonString;
                Properties.Settings.Default.Save();
               
                MessageBox.Show("Kaydetme İşlemi Başarılı");
            }
            else
            {
                MessageBox.Show("Kaydetme İşlemi Başarısız. Lütfen Panel Tipi seçiniz","Hata",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }

            

            

            
        }

        private void cmbPanelType_SelectedIndexChanged(object sender, EventArgs e)
        {
            AppSettings apps = null;
            if (cmbPanelType.SelectedIndex == 1)
            {
                apps = JsonConvert.DeserializeObject<AppSettings>(Properties.Settings.Default.DipSettings) ?? new AppSettings();
                nudParlaklık.Maximum = 200;
            }
            else if (cmbPanelType.SelectedIndex == 0)
            {
                apps = JsonConvert.DeserializeObject<AppSettings>(Properties.Settings.Default.SmdSettings) ?? new AppSettings();
                nudParlaklık.Maximum = 15;
            }




            nudInterval.Value = apps.Interval;
            nudParlaklık.Value = apps.Parlaklık;
            cmbFont.SelectedIndex = apps.Font;
            nudSatır.Value = apps.Satir;
            nudSutun.Value = apps.Sutun;
            //cmbPanelType.SelectedIndex = apps.PanelTip;
        }

        private void nudParlaklık_KeyUp(object sender, KeyEventArgs e)
        {
            if (cmbPanelType.SelectedIndex == 0 && nudParlaklık.Value > 15)
            {
                nudParlaklık.Value = 15;
            }
            else if (cmbPanelType.SelectedIndex == 1 && nudParlaklık.Value > 200)
            {
                nudParlaklık.Value = 200;
            }
        }

        private void nudInterval_KeyUp(object sender, KeyEventArgs e)
        {
            if (nudInterval.Value > 255)
            {
                nudInterval.Value = 255;
            }
        }
    }
    class AppSettings
    {
        public int Interval;
        public int Parlaklık;
        public int Font;
        public int Satir;
        public int Sutun;
        public int PanelTip;

    }
}
