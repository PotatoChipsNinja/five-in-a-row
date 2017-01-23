using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FIR
{
    public partial class FIR : Form
    {
        public const int BoardSize = 15;

        public delegate void Act();

        public enum Piece
        {
            None,
            Black,
            White,
            Threat,
            Danger,
        }

        public Piece Self;
        public Piece Now;

        public Piece GameOver { get; private set; }

        public NetworkInterface Network;

        Piece[,] board;

        int size;
        int block;
        int blacks;
        int whites;

        Point loction;
        Point position;
        Point last;
        Size psize;

        Timer timer;
        List<Point> GameOverPoints;

        public FIR()
        {
            InitializeComponent();

            Network = new NetworkInterface(this);
            Network.ClientConnected += Network_ClientConnected;
        }

        private void Network_ClientConnected(object sender, EventArgs e)
        {
            Network.Send(MessageType.Reset);
        }

        private void FIR_Load(object sender, EventArgs e)
        {
            Size size = new Size(pictureBox2.Width - 1, pictureBox2.Height - 1);
            Rectangle r = new Rectangle(new Point(), size);
            Rectangle re = new Rectangle(1, 1, size.Width - 2, size.Height - 2);

            Bitmap black = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            Graphics g = Graphics.FromImage(black);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;

            g.Clear(Color.DarkKhaki);
            g.DrawRectangle(Pens.Black, r);
            Bitmap white = new Bitmap(black);
            g.FillEllipse(Brushes.Black, re);
            g.Dispose();

            g = Graphics.FromImage(white);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;

            g.FillEllipse(Brushes.White, re);
            g.Dispose();

            pictureBox2.Image = black;
            pictureBox3.Image = white;
            // 以上代码用于生成界面中两方的图标
            GameOverPoints = new List<Point>();

            Reset();
        }

        public static Piece Reserve(Piece input)
        {
            if (input == Piece.White)
            {
                return Piece.Black;
            }
            if (input == Piece.Black)
            {
                return Piece.White;
            }
            return Piece.None;
        }

        public static Color Convert(Piece input)
        {
            switch (input)
            {
                case Piece.Black:
                    return Color.Black;
                case Piece.White:
                    return Color.White;
                default:
                    return Color.Transparent;
            }
        }

        public void RefreshText(Piece self, Piece now)
        {
            if (self != Piece.None)
            {
                Self = self;
                Invoke(new Act(() =>
                {
                    label3.Text = (Self == Piece.Black) ? "我方" : string.Empty;
                    label5.Text = (Self == Piece.White) ? "我方" : string.Empty;
                }));
            }
            if (now != Piece.None)
            {
                Now = now;
            }
            RefreshText();
        }

        public void RefreshText()
        {
            Invoke(new Act(() =>
            {
                if (GameOver != Piece.None)
                {
                    label4.Text = (GameOver == Piece.Black) ? "胜利" : string.Empty;
                    label6.Text = (GameOver == Piece.White) ? "胜利" : string.Empty;
                }
                else
                {
                    label4.Text = (Now == Piece.Black) ? "当前" : string.Empty;
                    label6.Text = (Now == Piece.White) ? "当前" : string.Empty;
                }
            }));
        }

        public void Reset()
        {
            GameOver = Piece.None;
            GameOverPoints.Clear();

            board = new Piece[BoardSize, BoardSize];

            position.X = -1;
            position.X = -1;

            loction.X = int.MinValue;
            loction.Y = int.MinValue;

            last.X = -1;
            last.Y = -1;

            psize.Width = -1;
            psize.Height = -1;

            Invoke(new Act(() =>
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
                timer = new Timer();
                timer.Interval = 34;
                timer.Tick += Timer_Tick;
                timer.Start();
            }));
            textBox2.AppendText("----------棋局已重置----------\n");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (block < 1 || panel1.Size != psize)
            {
                psize = panel1.Size;
                int len = Math.Min(psize.Width, psize.Height) - 1;
                int t = len / BoardSize;
                len = t * BoardSize + 1;
                pictureBox1.Size = new Size(len, len);
                pictureBox1.Location = new Point((psize.Width - len) / 2, (psize.Height - len) / 2);

                size = Math.Min(pictureBox1.Width, pictureBox1.Height) - 1;
                block = size / BoardSize;
                size = block * BoardSize + 1;

                draw();
                return;
            }
            int x = loction.X / block;
            int y = loction.Y / block;

            if (x != position.X || y != position.Y)
            {
                position.X = x;
                position.Y = y;

                draw();
            }
        }

        void draw()
        {
            const int A = 192;
            const int Alpha = 64;

            Bitmap image = new Bitmap(size, size);
            Graphics g = Graphics.FromImage(image);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;

            g.Clear(Color.DarkKhaki);
            for (int i = 0; i < BoardSize; i++)
            {
                g.DrawLine(Pens.Black, block / 2, i * block + block / 2, block * BoardSize - block / 2, i * block + block / 2);
            }
            for (int i = 0; i < BoardSize; i++)
            {
                g.DrawLine(Pens.Black, i * block + block / 2, block / 2, i * block + block / 2, block * BoardSize - block / 2);
            }

            int x = position.X;
            int y = position.Y;

            if (x >= 0 && y >= 0 && x < BoardSize && y < BoardSize)
            {
                Rectangle select = new Rectangle(x * block + 1, y * block + 1, block - 2, block - 2);
                g.FillRectangle(new SolidBrush(Color.FromArgb(Alpha, Color.LightBlue)), select);
                g.DrawRectangle(new Pen(Color.FromArgb(A, Color.LightBlue)), select);
            }

            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    Piece p = board[i, j];

                    if (GameOver != Piece.None)
                    {
                        if (GameOverPoints.Contains(new Point(i, j)))
                        {
                            Rectangle select = new Rectangle(i * block + 1, j * block + 1, block - 2, block - 2);
                            g.FillRectangle(new SolidBrush(Color.FromArgb(Alpha, Color.Green)), select);
                            g.DrawRectangle(new Pen(Color.FromArgb(A, Color.Green)), select);
                        }
                    }
                    else if (Self == Now)
                    {
                        if (last.X == i && last.Y == j)
                        {
                            Rectangle select = new Rectangle(i * block + 1, j * block + 1, block - 2, block - 2);
                            g.FillRectangle(new SolidBrush(Color.FromArgb(Alpha, Color.Gray)), select);
                            g.DrawRectangle(new Pen(Color.FromArgb(A, Color.Gray)), select);
                        }
                        else if (p == Piece.Threat || p == Piece.Danger)
                        {
                            Color c = (p == Piece.Threat) ? Color.Orange : Color.Red;
                            Rectangle select = new Rectangle(i * block + 1, j * block + 1, block - 2, block - 2);
                            g.FillRectangle(new SolidBrush(Color.FromArgb(Alpha, c)), select);
                            g.DrawRectangle(new Pen(Color.FromArgb(A, c)), select);
                        }
                    }

                    if (p == Piece.Black || p == Piece.White)
                    {
                        RectangleF rect = new RectangleF((i + 0.05F) * block, (j + 0.05F) * block, block * 0.9F, block * 0.9F);
                        g.FillEllipse(new SolidBrush(Convert(p)), rect);
                    }
                    else if ((i == (BoardSize / 2) && j == (BoardSize / 2)) || ((i == 3 || i == BoardSize - 1 - 3) && (j == 3 || j == BoardSize - 1 - 3)))
                    {
                        RectangleF rect = new RectangleF(i * block + block * 0.375F, j * block + block * 0.375F, block / 4, block / 4);
                        g.FillEllipse(Brushes.Black, rect);
                    }


                }
            }

            Image gc = pictureBox1.Image;
            pictureBox1.Image = image;

            g.Dispose();
            if (gc != null)
            {
                gc.Dispose();
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            loction = e.Location;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            loction.X = int.MinValue;
            loction.Y = int.MinValue;
        }

        void danger(Piece type)
        {
            const int Sum = 5;

            Action<int, int, int, int> check = (x, y, h, v) =>
                {
                    Point[] points = new Point[Sum];

                    for (int i = 0; i < Sum; i++)
                    {
                        if (x < 0 || y < 0 || x >= BoardSize || y >= BoardSize)
                        {
                            return;
                        }

                        points[i].X = x;
                        points[i].Y = y;

                        if (h > 0) x++;
                        if (v > 0) y++;
                        if (h < 0) x--;
                        if (v < 0) y--;
                    }

                    int count = 0;
                    Piece[] p = new Piece[Sum];

                    for (int i = 0; i < Sum; i++)
                    {
                        p[i] = board[points[i].X, points[i].Y];

                        if (p[i] == Reserve(type))
                        {
                            return;
                        }
                        else if (p[i] == type)
                        {
                            count++;
                        }
                        // 出现两个连续的空位 无威胁
                        else if (i >= 1 && p[i - 1] != type)
                        {
                            return;
                        }
                    }
                    if (count < 3 || (count == 3 && (p[1] != type || p[3] != type)))
                    {
                        return;
                    }

                    for (int i = 0; i < Sum; i++)
                    {
                        if (p[i] == Piece.None || p[i] == Piece.Threat)
                        {
                            board[points[i].X, points[i].Y] = count == 3 ? Piece.Threat : Piece.Danger;
                        }
                    }

                    if (count == 5)
                    {
                        GameOver = type;
                        GameOverPoints.AddRange(points);
                    }
                };

            blacks = 0;
            whites = 0;

            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    if (board[i, j] == Piece.Black)
                    {
                        blacks++;
                    }
                    else if (board[i, j] == Piece.White)
                    {
                        whites++;
                    }
                    else
                    {
                        board[i, j] = Piece.None;
                    }
                }
            }

            Invoke(new Act(() =>
            {
                label1.Text = blacks.ToString();
                label2.Text = whites.ToString();
            }));

            if (type != Piece.Black && type != Piece.White)
            {
                return;
            }

            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    check(i, j, 1, 0);
                    check(i, j, 0, 1);
                    check(i, j, 1, 1);
                    check(i, j, 1, -1);
                }
            }
        }

        public void ChatRecv(string ChatStr)
        {
            textBox2.AppendText("接收：" + ChatStr + "\n");
        }

        public void Set(Point input, bool send = true)
        {
            if (GameOver != Piece.None)
            {
                return;
            }

            int x = input.X;
            int y = input.Y;

            if (x >= 0 && y >= 0 && x < BoardSize && y < BoardSize)
            {
                if (board[x, y] != Piece.Black && board[x, y] != Piece.White)
                {
                    board[x, y] = Now;
                    danger(Now);
                    Now = Reserve(Now);

                    last.X = x;
                    last.Y = y;

                    if (send)
                    {
                        Network.Send(MessageType.Set, x, y);
                    }
                    Invoke(new Act(() =>
                    {
                        draw();
                        RefreshText();
                    }));
                }
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

            if (Self != Now)
            {
                return;
            }

            int x = e.Location.X / block;
            int y = e.Location.Y / block;

            Set(new Point(x, y));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Network.Send(MessageType.Reset);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Network.Running)
            {
                if (DialogResult.Yes == MessageBox.Show(this, "服务器已启动, 需要断开么?", string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
                {
                    Network.Abort();
                    MessageBox.Show(this, "服务器已停止.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                new StartServer().ShowDialog(this);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Network.Running)
            {
                if (DialogResult.Yes == MessageBox.Show(this, "已连接到服务器, 需要断开么?", string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
                {
                    Network.Abort();
                    MessageBox.Show(this, "连接已中断.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                new Connect().ShowDialog(this);
            }
        }

        private void FIR_FormClosing(object sender, FormClosingEventArgs e)
        {
            Network.Abort();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new About().ShowDialog(this);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (Network.Running)
            {
                if (textBox1.Text == "")
                {
                    MessageBox.Show(this, "发送文本不能为空.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Network.Send(MessageType.Chat, 0, 0, textBox1.Text);
                    textBox2.AppendText("发送：" + textBox1.Text + "\n");
                    textBox1.Text = "";
                }
            }
            else
            {
                MessageBox.Show(this, "请先建立连接.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
