using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using WPF_Encryptor.Encryption;

namespace WPF_Encryptor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string fileName;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button3DESEncrypt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button3DESDecrypt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonStegaEncrypt_Click(object sender, RoutedEventArgs e)
        {
            string toEncrypt = textBoxStega.Text;
            Bitmap file = new Bitmap(fileName);
            Bitmap btm = Stega.embedText(toEncrypt, file);
            BitmapImage btmToSave = ToBitmapImage(btm);

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "EncryptedImage";
            dlg.Filter = "Image (.png)|*.png";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(btmToSave));

                using (FileStream fileToSave = File.Create(filename))
                {
                    encoder.Save(fileToSave);
                }
            }
        }

        private void ButtonStegaDecrypt_Click(object sender, RoutedEventArgs e)
        {
            Bitmap file = new Bitmap(fileName);
            String decrypted = Stega.extractText(file);
            textBoxStega.Clear();
            textBoxStega.Text = decrypted;
        }

        private void ButtonStegaFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image (.png)|*.png";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                // Open document
                fileName = dlg.FileName;
            }
        }

        private BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
