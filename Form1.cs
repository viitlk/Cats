using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cats
{
    public partial class Form1 : Form
    {
        private bool loading = false;
        string fileName = "";
        private CancellationTokenSource cts;
        private static readonly HttpClient httpClient = new HttpClient();
        bool update = false;

        public Form1()
        {
            InitializeComponent();
            httpClient.DefaultRequestHeaders.ConnectionClose = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            change_picture();
        }

        private async void change_picture()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            if (loading) return;

            try
            {
                loading = true;
                var token = cts.Token;

                pictureBox1.Image?.Dispose();
                pictureBox1.Image = null;

                string imageUrl = $"http://cataas.com/cat?t={DateTime.Now.Ticks}";

                using (var response = await httpClient.GetAsync(imageUrl, token))
                {
                    response.EnsureSuccessStatusCode();

                    byte[] imageData = await response.Content.ReadAsByteArrayAsync();

                    fileName = $"{DateTime.Now.Ticks}.jpg";
                    string tempPath = Path.Combine(Path.GetTempPath(), fileName);

                    await Task.Run(() => File.WriteAllBytes(tempPath, imageData));

                    using (var ms = new MemoryStream(imageData))
                    {
                        pictureBox1.Image = new Bitmap(ms);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show("Error downloading!\n" + ex.Message);
            }
            finally
            {
                loading = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            change_picture();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), fileName);
            File.Copy(tempPath, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName));
            MessageBox.Show("File saved as: " + fileName);
        }

        async void autoUpdate()
        {
            while (update)
            {
                await Task.Delay(System.TimeSpan.FromSeconds((double)numericUpDown1.Value));
                change_picture();
            }
        }

        async Task startAutoUpdate()
        {
            await Task.Run(() => autoUpdate());
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked) {
                update = true;
                startAutoUpdate();
            } else
            {
                update = false;
            }
        }
    }
}