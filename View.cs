using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FiniteDifferenceMethod
{
    class View : IView
    {
        public delegate void SwitchToIdleEventHandler();
        public event SwitchToIdleEventHandler SwitchToIdle;

        public VariableType DisplayMode { get; set; }

        public int PositionX
        {
            get { return _posX; }
            set
            {
                if (_image == null) return;
                _posX = value >= _image.Width ? _image.Width - 1 : value < 0 ? 0 : value;
            }
        }
        public int PositionY
        {
            get { return _posY; }
            set
            {
                if (_image == null) return;
                _posY = value >= _image.Height ? _image.Height - 1 : value < 0 ? 0 : value;
            }
        }
        public int PositionZ
        {
            get { return _posZ; }
            set
            {
                if (_image == null) return;
                _posZ = value >= _image.Depth ? _image.Depth - 1 : value < 0 ? 0 : value;
            }
        }

        private Controller _controller;
        private int _posX, _posY, _posZ;
        private readonly PictureBox _display1, _display2, _display3;
        private readonly TextBox _textBox;
        private Bitmap _bmp1, _bmp2, _bmp3;
        private IGridImage _image;

        public View(PictureBox leftTopXZ, PictureBox rightTopZY, PictureBox rightBottomXY,
                    TextBox textBox)
        {
            _display1 = leftTopXZ;
            _display2 = rightTopZY;
            _display3 = rightBottomXY;
            _textBox = textBox;
            DisplayMode = VariableType.APotential;
            //_display1.Image = new Bitmap(_display1.Width, _display1.Height, PixelFormat.Format32bppArgb);
            //_display2.Image = new Bitmap(_display2.Width, _display1.Height, PixelFormat.Format32bppArgb);
            //_display3.Image = new Bitmap(_display3.Width, _display1.Height, PixelFormat.Format32bppArgb);
        }

        public void SetImage(IGridImage image)
        {
            double relativePositionX;
            double relativePositionY;
            double relativePositionZ;
            if (_image == null)
            {
                relativePositionX = 0.5;
                relativePositionY = 0.5;
                relativePositionZ = 0.5;
            }
            else
            {
                relativePositionX = (double)_posX / _image.Width;
                relativePositionY = (double)_posY / _image.Height;
                relativePositionZ = (double)_posZ / _image.Depth;
            }
            _image = image;
            if (_image == null) return;
            PositionX = (int)Math.Round(relativePositionX * _image.Width);
            PositionY = (int)Math.Round(relativePositionY * _image.Height);
            PositionZ = (int)Math.Round(relativePositionZ * _image.Depth);
            _bmp1 = new Bitmap(image.Width, image.Depth, PixelFormat.Format32bppRgb);
            _bmp2 = new Bitmap(image.Height, image.Depth, PixelFormat.Format32bppRgb);
            _bmp3 = new Bitmap(image.Height, image.Width, PixelFormat.Format32bppRgb);
            _display1.Image = _bmp1;
            _display2.Image = _bmp2;
            _display3.Image = _bmp3;
            Display();
            if (SwitchToIdle != null) SwitchToIdle();
        }

        public void SetController(Controller controller)
        {
            _controller = controller;
        }

        public IGridImage GetImage()
        {
            return _image;
        }

        public void Display()
        {
            if (_image == null) return;

            Bitmap drawHere = _bmp1;
            int[] drawThat = _image.GetLayerY(DisplayMode, PositionY);
            drawThat[(_image.Width - PositionX - 1) + (_image.Depth - PositionZ - 1) * _image.Width] = 0xffffff;
            BitmapData bData = drawHere.LockBits(new Rectangle(new Point(), drawHere.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            Marshal.Copy(drawThat, 0, bData.Scan0, drawThat.Length);
            drawHere.UnlockBits(bData);
            _display1.Refresh();

            drawHere = _bmp2;
            drawThat = _image.GetLayerX(DisplayMode, PositionX);
            drawThat[PositionY + (_image.Depth - PositionZ - 1) * _image.Height] = 0xffffff;
            bData = drawHere.LockBits(new Rectangle(new Point(), drawHere.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            Marshal.Copy(drawThat, 0, bData.Scan0, drawThat.Length);
            drawHere.UnlockBits(bData);
            _display2.Refresh();

            drawHere = _bmp3;
            drawThat = _image.GetLayerZ(DisplayMode, PositionZ);
            drawThat[PositionY + PositionX * _image.Height] = 0xffffff;
            bData = drawHere.LockBits(new Rectangle(new Point(), drawHere.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            Marshal.Copy(drawThat, 0, bData.Scan0, drawThat.Length);
            drawHere.UnlockBits(bData);
            _display3.Refresh();

            string st = "";
            st += "position = (" + PositionX + ", " + PositionY + ", " + PositionZ + ")" + Environment.NewLine +
                "grid size = (" + _image.Width + ", " + _image.Height + ", " + _image.Depth + ")" + Environment.NewLine +
                "grid step = " + _image.Step + Environment.NewLine;
            Cell cell;
            st += "variables:" + Environment.NewLine;
            if (_controller.GetCell(_posX, _posY, _posZ, out cell))
            {
                st += 
                "   B = (" + cell.Bx.ToString("F4") + ", " + cell.By.ToString("F4") + ", " + cell.Bz.ToString("F4") + ")" + Environment.NewLine +
                "   |B| = " + Math.Sqrt(cell.Bx * cell.Bx + cell.By * cell.By + cell.Bz * cell.Bz).ToString("F4") + Environment.NewLine +
                "   A = (" + cell.Ax.ToString("F4") + ", " + cell.Ay.ToString("F4") + ", " + cell.Az.ToString("F4") + ")" + Environment.NewLine +
                "   |A| = " + Math.Sqrt(cell.Ax * cell.Ax + cell.Ay * cell.Ay + cell.Az * cell.Az).ToString("F4") + Environment.NewLine +
                "   J = (" + cell.Jx.ToString("F4") + ", " + cell.Jy.ToString("F4") + ", " + cell.Jz.ToString("F4") + ")" + Environment.NewLine +
                "   |J| = " + Math.Sqrt(cell.Jx * cell.Jx + cell.Jy * cell.Jy + cell.Jz * cell.Jz).ToString("F4") + Environment.NewLine +
                "   M = " + cell.M.ToString("F4") + Environment.NewLine;
            }
            else
            {
                st += "   unavailable (calculations are in progress)" + Environment.NewLine;
            }
            int cb = _image.GetPoint(VariableType.BMagneticField, _posX, _posY, _posZ);
            int ca = _image.GetPoint(VariableType.APotential, _posX, _posY, _posZ);
            int cj = _image.GetPoint(VariableType.JCurrent, _posX, _posY, _posZ);
            int cm = _image.GetPoint(VariableType.MPermiability, _posX, _posY, _posZ);
            st += "colors:" + Environment.NewLine +
                "   B = " + cb.ToString("x6") + " (scale = " + _image.Converter.BScale.ToString("F4") + ")" + Environment.NewLine +
                "   A = " + ca.ToString("x6") + " (scale = " + _image.Converter.AScale.ToString("F4") + ")" + Environment.NewLine +
                "   J = " + cj.ToString("x6") + " (scale = " + _image.Converter.JScale.ToString("F4") + ")" + Environment.NewLine +
                "   M = " + cm.ToString("x6") + " (scale = " + _image.Converter.MScale.ToString("F4") + ")" + Environment.NewLine;
            float xx, yy, zz;
            st += "reconstructed values:" + Environment.NewLine;
            _image.Converter.Color2Vector(cb, out xx, out yy, out zz, _image.Converter.BScale);
            st += "   B = (" + xx.ToString("F4") + ", " + yy.ToString("F4") + ", " + zz.ToString("F4") + ")" + Environment.NewLine;
            _image.Converter.Color2Vector(ca, out xx, out yy, out zz, _image.Converter.AScale);
            st += "   A = (" + xx.ToString("F4") + ", " + yy.ToString("F4") + ", " + zz.ToString("F4") + ")" + Environment.NewLine;
            _image.Converter.Color2Vector(cj, out xx, out yy, out zz, _image.Converter.JScale);
            st += "   J = (" + xx.ToString("F4") + ", " + yy.ToString("F4") + ", " + zz.ToString("F4") + ")" + Environment.NewLine;
            st += "   M = " + _image.Converter.Color2PositiveScalar(cm, _image.Converter.MScale).ToString("F4");
            _textBox.Text = st;
        }
        public int GetSizeX()
        {
            return _image == null ? 1 : _image.Width;
        }
        public int GetSizeY()
        {
            return _image == null ? 1 : _image.Height;
        }
        public int GetSizeZ()
        {
            return _image == null ? 1 : _image.Depth;
        }
    }
}

