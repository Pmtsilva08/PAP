using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using OtpNet;
using QRCoder;
using System;
using System.IO;

namespace Projeto_PAP
{
    public sealed partial class Setup2FADialog : UserControl
    {
        private byte[] _secretKey;
        private Totp _totp;

        public string SecretKeyBase32 { get; private set; }

        public Setup2FADialog(string userEmail)
        {
            InitializeComponent();
            GenerateSecretKey(userEmail);
        }

        private void GenerateSecretKey(string userEmail)
        {
            // 1. Gerar chave secreta aleatória (20 bytes = 160 bits)
            _secretKey = KeyGeneration.GenerateRandomKey(20);

            // 2. Converter para Base32 (formato que o Google Authenticator usa)
            SecretKeyBase32 = Base32Encoding.ToString(_secretKey);

            // 3. Criar objeto TOTP para verificar códigos
            _totp = new Totp(_secretKey);

            // 4. Mostrar a chave no ecrã (para entrada manual)
            SecretKeyText.Text = SecretKeyBase32;

            // 5. Gerar QR Code
            GenerateQRCode(userEmail);
        }

        private void GenerateQRCode(string userEmail)
        {
            // Formato URI padrão do Google Authenticator:
            // otpauth://totp/AppName:user@email.com?secret=CHAVE&issuer=AppName
            string uri = $"otpauth://totp/PasswordManager:{userEmail}?secret={SecretKeyBase32}&issuer=PasswordManager";

            // Gerar QR Code
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q))
                {
                    using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeBytes = qrCode.GetGraphic(20);

                        // Converter para imagem WinUI
                        using (MemoryStream stream = new MemoryStream(qrCodeBytes))
                        {
                            stream.Position = 0;
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.SetSource(stream.AsRandomAccessStream());
                            QRCodeImage.Source = bitmap;
                        }
                    }
                }
            }
        }

        // Verificar se o código introduzido está correto
        public bool VerifyCode()
        {
            string code = CodeBox.Text.Trim();

            if (string.IsNullOrEmpty(code) || code.Length != 6)
            {
                ShowError();
                return false;
            }

            // Verificar o código TOTP
            bool isValid = _totp.VerifyTotp(code, out long timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay);

            if (!isValid)
            {
                ShowError();
                return false;
            }

            HideError();
            return true;
        }

        public void ShowError()
        {
            ErrorMessage.Visibility = Visibility.Visible;
        }

        public void HideError()
        {
            ErrorMessage.Visibility = Visibility.Collapsed;
        }
    }
} 