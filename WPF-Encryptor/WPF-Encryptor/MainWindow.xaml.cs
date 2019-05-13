using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
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
        string fileExtension;
        byte[] fileBytes;
        byte[] tripleKey;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button3DESEncrypt_Click(object sender, RoutedEventArgs e)
        {
            fileBytes = System.IO.File.ReadAllBytes(fileName);
            Dictionary<string, byte[]> encrypted = TripleDES.Encrypt(fileBytes);
            byte[] encryptedBytes = encrypted["text"];
            textbox3DESKey.Text = Encoding.Unicode.GetString( encrypted["key"]);
            tripleKey = encrypted["key"];

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "EncryptedFile";
            dlg.Filter = "File (All) |*" + fileExtension;

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                File.WriteAllBytes(filename, encryptedBytes);
            }
        }

        private void Button3DESDecrypt_Click(object sender, RoutedEventArgs e)
        {
            fileBytes = System.IO.File.ReadAllBytes(fileName);
            byte[] key = Encoding.Unicode.GetBytes(textbox3DESKey.Text);
            byte[] decrypted = TripleDES.Decrypt(fileBytes, tripleKey);

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "DecryptedFile";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName + fileExtension;
                File.WriteAllBytes(filename, decrypted);
            }
        }

        private void Button3DESOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                // Open document
                fileName = dlg.FileName;
                fileExtension = Path.GetExtension(fileName);
            }
            
        }

        private void Button3DESSaveKey_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "EncryptedImage";
            dlg.Filter = "Text (.txt)|*.txt";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                File.WriteAllText(filename, textbox3DESKey.Text);
            }
        }

        private void ButtonStegaEncrypt_Click(object sender, RoutedEventArgs e)
        {
            string toEncrypt = textBoxStega.Text;
            Bitmap file = new Bitmap(fileName);
            Bitmap btm = Stega.embedText(toEncrypt, file);
            BitmapImage btmToSave = ToBitmapImage(btm);

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "3DESKey";
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
