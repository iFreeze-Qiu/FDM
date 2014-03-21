using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace FiniteDifferenceMethod
{
    public partial class Form1 : Form
    {
        private readonly IView _view;
        private readonly Controller _controller;

        public Form1()
        {
            InitializeComponent();
            Form1_SizeChanged(null, null);
            _view = new View(pictureBox1, pictureBox2, pictureBox4, textBox1);
            _view.SwitchToIdle += UnlockControls;
            _controller = new Controller(_view);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            LockControls(false);
            _controller.DoInjection();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (_controller.ModelState == ModelState.Idleing)
            {
                _controller.DoSolve(double.Parse(textBox2.Text, NumberStyles.Number, CultureInfo.InvariantCulture), 10);
                LockControls(true);
                return;
            }
            if (_controller.ModelState == ModelState.Solving)
            {
                _controller.DoStop();
                UnlockControls();
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            LockControls(false);
            _controller.DoInterpolation();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            pictureBox1.Width = (ClientRectangle.Width - 9) / 2;
            pictureBox1.Height = (ClientRectangle.Height - 9) / 2;
            pictureBox1.Left = 3;
            pictureBox1.Top = 3;

            pictureBox2.Width = (ClientRectangle.Width - 9) / 2;
            pictureBox2.Height = (ClientRectangle.Height - 9) / 2;
            pictureBox2.Left = 6 + pictureBox1.Width;
            pictureBox2.Top = 3;

            pictureBox4.Width = (ClientRectangle.Width - 9) / 2;
            pictureBox4.Height = (ClientRectangle.Height - 9) / 2;
            pictureBox4.Left = 6 + pictureBox1.Width;
            pictureBox4.Top = 6 + pictureBox1.Height;

            textBox1.Width = (ClientRectangle.Width - 9) / 2;
            textBox1.Height = (ClientRectangle.Height - 9) / 2 - 46;
            textBox1.Left = 3;
            textBox1.Top = 6 + pictureBox1.Height;
        }

        private void LockControls(bool solving)
        {
            if (solving) button6.Text = "Pause";
            else button6.Enabled = false;
            button5.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
            button10.Enabled = false;
            button11.Enabled = false;
            button12.Enabled = false;
        }
        public void UnlockControls()
        {
            button6.Text = "Solve";
            button6.Enabled = true;
            button5.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = true;
            button10.Enabled = true;
            button11.Enabled = true;
            button12.Enabled = true;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Point p = pictureBox2.PointToClient(Cursor.Position);
            _view.PositionY = (int)Math.Round(((double)p.X / pictureBox2.Width) * _view.GetSizeY());
            _view.PositionZ = _view.GetSizeZ() - (int)Math.Round(((double)p.Y / pictureBox2.Height) * _view.GetSizeZ()) - 1;
            _view.Display();
        }
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            Point p = pictureBox4.PointToClient(Cursor.Position);
            _view.PositionY = (int)Math.Round(((double)p.X / pictureBox2.Width) * _view.GetSizeY());
            _view.PositionX = (int)Math.Round(((double)p.Y / pictureBox2.Height) * _view.GetSizeX());
            _view.Display();
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Point p = pictureBox1.PointToClient(Cursor.Position);
            _view.PositionX = _view.GetSizeX() - (int)Math.Round(((double)p.X / pictureBox2.Width) * _view.GetSizeX()) - 1;
            _view.PositionZ = _view.GetSizeZ() - (int)Math.Round(((double)p.Y / pictureBox2.Height) * _view.GetSizeZ()) - 1;
            _view.Display();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _view.DisplayMode = VariableType.APotential;
            _view.Display();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            _view.DisplayMode = VariableType.BMagneticField;
            _view.Display();
        }
        private void button13_Click(object sender, EventArgs e)
        {
            _view.DisplayMode = VariableType.BAbsoluteValue;
            _view.Display();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            _view.DisplayMode = VariableType.JCurrent;
            _view.Display();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            _view.DisplayMode = VariableType.MPermiability;
            _view.Display();
        }

        private void pictureBox4_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) pictureBox4_Click(null, null);
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) pictureBox2_Click(null, null);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) pictureBox1_Click(null, null);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            _controller.DoSave("test");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            LockControls(false);
            _controller.DoLoad("coils and rail");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //_model = new Model(InputGenerator.GenerateSphereTask(0.3f, 0, 0, 10f));
            //_view.SetImage(_model.ShowState());
        }

        private void button11_Click(object sender, EventArgs e)
        {
            //_model = new Model(InputGenerator.GenerateWareTask(14f));
            //_view.SetImage(_model.ShowState());
        }

        private void button12_Click(object sender, EventArgs e)
        {
            LockControls(false);
            _controller.DoAutoscale();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LockControls(false);
        }
    }
}

