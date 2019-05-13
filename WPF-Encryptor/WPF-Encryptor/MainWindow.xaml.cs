using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WPF_Encryptor.Encryption;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Xml.XPath;
using WPF_Encryptor.Encryption.RSAx;

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

            this.publicKey = null;

            this.freeEvent = new EventWaitHandle(true, EventResetMode.ManualReset);
        }

        private void Button3DESEncrypt_Click(object sender, RoutedEventArgs e)
        {
            fileBytes = System.IO.File.ReadAllBytes(fileName);
            Dictionary<string, byte[]> encrypted = TripleDESx.Encrypt(fileBytes);
            byte[] encryptedBytes = encrypted["text"];
            textbox3DESKey.Text = Encoding.Unicode.GetString(encrypted["key"]);
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
            byte[] decrypted = TripleDESx.Decrypt(fileBytes, tripleKey);

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
                fileExtension = System.IO.Path.GetExtension(fileName);
            }

        }

        private void Button3DESSaveKey_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "3DESKey";
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

        // ----- RSA Part starts here -----
        private void bt_selPlain_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".*";
            ofd.Filter = Properties.Resources.All_File_Type;
            ofd.Title = Properties.Resources.DialogTitle_SelectPlain;
            Nullable<bool> result = ofd.ShowDialog();
            if (result == true)
            {
                this.tb_plainFilePath.Text = ofd.FileName;
            }
        }

        private void tb_output_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.tb_output.ScrollToEnd();
        }


        private void bt_encrypt_Click(object sender, RoutedEventArgs e)
        {
            if (!this.freeEvent.WaitOne(0))
            {
                MessageBox.Show(Properties.Resources.Backend_Busy);
                return;
            }

            if (string.IsNullOrEmpty(publicKey))
            {
                MessageBox.Show(this, Properties.Resources.Error_Need_PublicKey);
                return;
            }

            string plainFilePath = this.tb_plainFilePath.Text;
            string encryptedFilePath = MakePath(plainFilePath, ".encrypted");
            string manifestFilePath = MakePath(plainFilePath, ".manifest.xml");

            this.tb_output.Text = Properties.Resources.Out_msg_start_encryption;

            var t = Task.Factory.StartNew(() =>
            {
                freeEvent.Reset();
                string s = Encipher.Encrypt(plainFilePath,
                    encryptedFilePath,
                    manifestFilePath,
                    this.publicKey);

                freeEvent.Set();
                this.UpdateOutput(this.tb_output, Properties.Resources.Out_msg_encrypt_success + "\r\n" + s, true);
            });
        }

        private void UpdateOutput(TextBox tb, string message, bool append)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                string newMessage = append ? tb.Text + "\r\n" + message : message;
                tb.Text = newMessage;
            }), null);
        }

        private string publicKey;

        private string privateKey;
        private string manifestFilePath;

        private void bt_setting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow sw = new SettingWindow();
            sw.PublicKey = this.publicKey;
            Nullable<bool> result = sw.ShowDialog();

            if (result == true)
            {
                this.publicKey = sw.PublicKey;
            }
        }

        private static string MakePath(string plainFilePath, string newSuffix)
        {
            string encryptedFileName = System.IO.Path.GetFileNameWithoutExtension(plainFilePath) + newSuffix;
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(plainFilePath), encryptedFileName);
        }

        private void mi_genKeyPair_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fbd = new SaveFileDialog();
            fbd.FileName = "publicKey.xml";
            Nullable<bool> result = fbd.ShowDialog();
            if (result == true)
            {

                Encipher.GenerateRSAKeyPair(out publicKey, out privateKey);
                using (StreamWriter sw = File.CreateText(fbd.FileName))
                {
                    sw.Write(publicKey);
                }


            }

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "privateKey.xml";
            Nullable<bool> resultTwo = dlg.ShowDialog();
            if (result == true)
            {
                using (StreamWriter sw = File.CreateText(dlg.FileName))
                {
                    sw.Write(privateKey);
                }
            }
        }

        private EventWaitHandle freeEvent;

        private void mi_switch_Click(object sender, RoutedEventArgs e)
        {
            if (!this.freeEvent.WaitOne(0))
            {
                MessageBox.Show(Properties.Resources.Backend_Busy);
                return;
            }

            if (this.grid_encrypt.Visibility == System.Windows.Visibility.Visible)
            {
                this.grid_encrypt.Visibility = System.Windows.Visibility.Collapsed;
                this.grid_decrypt.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.grid_encrypt.Visibility = System.Windows.Visibility.Visible;
                this.grid_decrypt.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void bt_selEncrypted_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".*";
            ofd.Filter = Properties.Resources.All_File_Type;
            ofd.Title = Properties.Resources.DialogTitle_SelectPlain;
            Nullable<bool> result = ofd.ShowDialog();
            if (result == true)
            {
                this.tb_encryptedFilePath.Text = ofd.FileName;
            }
        }

        private void bt_decrypt_Click(object sender, RoutedEventArgs e)
        {
            if (!this.freeEvent.WaitOne(0))
            {
                MessageBox.Show(Properties.Resources.Backend_Busy);
                return;
            }

            string rsaKey = this.privateKey;
            string encryptedFile = this.tb_encryptedFilePath.Text;
            string plainFile = MakePath(encryptedFile, ".decrypted");
            string manifestFile = this.manifestFilePath;
            XDocument doc = XDocument.Load(manifestFile);
            XElement aesKeyElement = doc.Root.XPathSelectElement("./DataEncryption/AESEncryptedKeyValue/Key");
            byte[] aesKey = Encipher.RSADescryptBytes(Convert.FromBase64String(aesKeyElement.Value), rsaKey);
            XElement aesIvElement = doc.Root.XPathSelectElement("./DataEncryption/AESEncryptedKeyValue/IV");
            byte[] aesIv = Encipher.RSADescryptBytes(Convert.FromBase64String(aesIvElement.Value), rsaKey);

            this.tb_outputDecrypt.Text = Properties.Resources.Out_msg_start_decryption;
            var t = Task.Factory.StartNew(() =>
            {
                freeEvent.Reset();
                Encipher.DecryptFile(plainFile, encryptedFile, aesKey, aesIv);
                freeEvent.Set();
                //this.UpdateOutput(this.tb_outputDecrypt, string.Format(Properties.Resources.Out_msg_decryption_success, plainFile), true);
            });
        }

        private void bt_settingDecrypt_Click(object sender, RoutedEventArgs e)
        {
            DecryptionSettingWindow dsw = new DecryptionSettingWindow();
            dsw.Key = this.privateKey;
            dsw.ManifestFilePath = this.manifestFilePath;
            Nullable<bool> result = dsw.ShowDialog();
            if (result == true)
            {
                this.privateKey = dsw.Key;
                this.manifestFilePath = dsw.ManifestFilePath;
            }
        }

        private void mi_convertKey_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".pem";
            ofd.Filter = WPF_Encryptor.Properties.Resources.PEM_File_Type;
            ofd.Title = WPF_Encryptor.Properties.Resources.DialogTitle_SelectPem;
            Nullable<bool> result = ofd.ShowDialog();
            if (result == true)
            {
                string pemStr = null;
                using (StreamReader sr = File.OpenText(ofd.FileName))
                {
                    pemStr = sr.ReadToEnd().Trim();
                }

                string publicKey;
                string privateKey;
                opensslkey.DecodePEMKey(pemStr, out publicKey, out privateKey);

                string publicKeyFile = MakePath(ofd.FileName, ".xml");
                string privateKeyFile = MakePath(ofd.FileName, ".xml");

                if (publicKey != null)
                {
                    using (StreamWriter sw = File.CreateText(publicKeyFile))
                    {
                        sw.Write(publicKey);
                    }
                }
                else
                {
                    MessageBox.Show(Properties.Resources.Error_convertToPublicKey);

                }

                if (privateKey != null)
                {
                    using (StreamWriter sw = File.CreateText(privateKeyFile))
                    {
                        sw.Write(privateKey);
                    }
                }
                else
                {
                    MessageBox.Show(Properties.Resources.Error_convertToPrivateKey);
                }
            }
        }

        private void tb_outputDecrypt_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.tb_outputDecrypt.ScrollToEnd();
        }
    }
}
