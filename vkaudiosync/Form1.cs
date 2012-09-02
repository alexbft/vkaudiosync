using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vkaudiosync
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            webBrowser1.ObjectForScripting = new Helpers(webBrowser1);
            webBrowser1.Url = new Uri(Path.Combine(Application.StartupPath, "client.html"));
        }
    }
}
