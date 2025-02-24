using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Collections;

namespace Socketdeneme
{
    public partial class Form1: Form
    {
        public TcpClient Istemci;
        private NetworkStream AgAkimi;
        private StreamReader AkimOkuyucu;
        private StreamWriter AkimYazici;
        private System.Windows.Forms.Button buton;
        private System.Windows.Forms.TextBox textbox;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
