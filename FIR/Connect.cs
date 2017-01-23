using System;
using System.Windows.Forms;
using System.Net;

namespace FIR
{
    public partial class Connect : Form
    {
        FIR owner;

        public Connect()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                IPAddress ip = IPAddress.Parse(textBox1.Text);
                int port = int.Parse(textBox2.Text);

                IPEndPoint end = new IPEndPoint(ip, port);
                owner.Network.Connect(end);
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, ex.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Close();
        }

        private void Connect_Load(object sender, EventArgs e)
        {
            if (Owner == null || Owner.GetType() != typeof(FIR))
            {
                throw new Exception("无效的父对象!");
            }

            owner = (FIR)Owner;
        }
    }
}
