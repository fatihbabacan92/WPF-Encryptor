using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Encryptor.Encryption
{
    class TripleDESx
    {
        public TripleDESCryptoServiceProvider tdes { get; set; }

        public TripleDESx()
        {
            tdes = new TripleDESCryptoServiceProvider();
        }

        public static Dictionary<string, byte[]> Encrypt(byte[] toEncryptArray)
        {
            Dictionary<string, byte[]> output = new Dictionary<string, byte[]>();
            byte[] keyArray;

            // Generate key (never weak key)
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.GenerateKey();

            keyArray = tdes.Key;

            //mode of operation. there are other 4 modes.
            //We choose ECB(Electronic code Book)
            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)

            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            //transform the specified region of bytes array to resultArray
            try
            {
                byte[] resultArray =
                    cTransform.TransformFinalBlock(toEncryptArray, 0,
                        toEncryptArray.Length);
                //Release resources held by TripleDes Encryptor
                tdes.Clear();

                //Add key and encrypted data to output Dictionary
                output.Add("text", resultArray);
                output.Add("key", keyArray);

                //Return the encrypted data into unreadable string format
                return output;
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException(ex.Message);
            }
        }

        public static byte[] Decrypt(byte[] toDecryptArray, byte[] keyArray)
        {
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes. 
            //We choose ECB(Electronic code Book)

            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(
                toDecryptArray, 0, toDecryptArray.Length);
            //Release resources held by TripleDes Encryptor                
            tdes.Clear();
            //return the Clear decrypted TEXT
            return resultArray;
        }

        public byte[] GetKey()
        {
            return tdes.Key;
        }

    }
}
