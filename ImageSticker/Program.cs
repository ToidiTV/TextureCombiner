using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TextureCombiner
{
    public partial class MainForm : Form
    {
        private PictureBox[] textureBoxes;
        private TextBox exportWidthBox, exportHeightBox;
        private Button exportButton;
        private PictureBox sourceImageBox = null;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = string.Empty;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ClientSize = new Size(500, 600);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.StartPosition = FormStartPosition.CenterScreen;

            Panel titleBar = new Panel
            {
                BackColor = Color.FromArgb(45, 45, 45),
                Dock = DockStyle.Top,
                Height = 40
            };
            titleBar.MouseDown += TitleBar_MouseDown;
            this.Controls.Add(titleBar);

            Label titleLabel = new Label
            {
                Text = "Texture Combiner",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 10)
            };
            titleBar.Controls.Add(titleLabel);

            Button closeButton = new Button
            {
                Text = "X",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 45, 45),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(40, 30),
                Location = new Point(this.ClientSize.Width - 50, 5)
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => this.Close();
            titleBar.Controls.Add(closeButton);

            textureBoxes = new PictureBox[4];
            for (int i = 0; i < 4; i++)
            {
                PictureBox box = new PictureBox
                {
                    Size = new Size(200, 200),
                    BackColor = Color.FromArgb(50, 50, 50),
                    BorderStyle = BorderStyle.FixedSingle,
                    Location = new Point(50 + (i % 2) * 220, 60 + (i / 2) * 220),
                    AllowDrop = true,
                    SizeMode = PictureBoxSizeMode.Zoom
                };
                box.DragEnter += TextureBoxDragEnter;
                box.DragDrop += TextureBoxDragDrop;
                box.MouseClick += Box_MouseClick;
                this.Controls.Add(box);
                textureBoxes[i] = box;
            }

            Label sizeLabel = new Label
            {
                Text = "Export size (Width x Height):",
                ForeColor = Color.White,
                Location = new Point(50, 480),
                AutoSize = true
            };
            this.Controls.Add(sizeLabel);

            exportWidthBox = new TextBox
            {
                Location = new Point(220, 480),
                Size = new Size(80, 25),
                Text = "2048"
            };
            this.Controls.Add(exportWidthBox);

            exportHeightBox = new TextBox
            {
                Location = new Point(310, 480),
                Size = new Size(80, 25),
                Text = "2048"
            };
            this.Controls.Add(exportHeightBox);

            exportButton = new Button
            {
                Text = "Export",
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(150, 520),
                Size = new Size(200, 30)
            };
            exportButton.FlatAppearance.BorderSize = 0;
            exportButton.Click += (s, e) => ExportImage();
            this.Controls.Add(exportButton);
        }

        private void Box_MouseClick(object sender, MouseEventArgs e)
        {
            var clickedBox = sender as PictureBox;

            if (e.Button == MouseButtons.Right && ModifierKeys == Keys.Shift)
            {
                if (clickedBox.Image != null)
                {
                    sourceImageBox = clickedBox;
                }
            }

            if (e.Button == MouseButtons.Left && ModifierKeys == Keys.Shift)
            {
                if (sourceImageBox != null && sourceImageBox.Image != null)
                {
                    clickedBox.Image = (Image)sourceImageBox.Image.Clone();
                }
            }
        }

        private void TextureBoxDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && files[0].EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void TextureBoxDragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0 && files[0].EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                var box = sender as PictureBox;
                box.Image = Image.FromFile(files[0]);
            }
        }

        private void ExportImage()
        {
            int width = int.Parse(exportWidthBox.Text);
            int height = int.Parse(exportHeightBox.Text);

            using (Bitmap combinedImage = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(combinedImage))
                {
                    g.Clear(Color.Transparent);

                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                    int gridWidth = width / 2;
                    int gridHeight = height / 2;

                    for (int i = 0; i < textureBoxes.Length; i++)
                    {
                        if (textureBoxes[i].Image != null)
                        {
                            int x = (i % 2) * gridWidth;
                            int y = (i / 2) * gridHeight;

                            g.DrawImage(textureBoxes[i].Image, new Rectangle(x, y, gridWidth, gridHeight),
                                new Rectangle(0, 0, textureBoxes[i].Image.Width, textureBoxes[i].Image.Height),
                                GraphicsUnit.Pixel);
                        }
                    }
                }

                using (SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "PNG Files (*.png)|*.png",
                    DefaultExt = "png",
                })
                {
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        combinedImage.Save(saveDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                        MessageBox.Show("Image successfully exported with transparency!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0xA1, new IntPtr(0x2), IntPtr.Zero);
            }
        }
    }

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}