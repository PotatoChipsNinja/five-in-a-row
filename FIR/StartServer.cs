using System;
using System.Windows.Forms;

namespace FIR
{
    public partial class StartServer : Form
    {
        FIR owner;

        public StartServer()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                int port = int.Parse(textBox1.Text);
                owner.Network.Start(port);
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, ex.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Close();
        }

        private void StartServer_Load(object sender, EventArgs e)
        {
            if (Owner == null || Owner.GetType() != typeof(FIR))
            {
                throw new Exception("无效的父对象!");
            }

            owner = (FIR)Owner;
        }
    }
}
