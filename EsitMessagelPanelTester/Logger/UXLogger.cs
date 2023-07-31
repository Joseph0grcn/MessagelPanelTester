using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EsitMessagelPanelTester
{
    internal class UXLogger : ILogger
    {
        public string CallTime()
        {
            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            return currentTime.ToString();
        }

        public void PushLog(ToolStripStatusLabel statuslabel,RichTextBox rbx,String message)
        {
            statuslabel.Text = message +" "+ CallTime();
            rbx.Text += statuslabel.Text + "\n";
            rbx.SelectionStart = rbx.Text.Length;
            rbx.ScrollToCaret();
        }

        public void PushLog()
        {
            throw new NotImplementedException();
        }
    }
}
