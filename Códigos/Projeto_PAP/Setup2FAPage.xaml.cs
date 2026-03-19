using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using OtpNet;
using QRCoder;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Projeto_PAP
{
    public sealed partial class Setup2FAPage : Page
    {
        private byte[] _secretKey;
        private Totp _totp;
        private string _secretKeyBase32;
        private Setup2FAParameters _parameters;

        public Setup2FAPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Setup2FAParameters parameters)
            {
                _parameters = parameters;
                GenerateSecretKey(parameters.UserEmail);
            }
        }

        private void GenerateSecretKey(string userEmail)
        {
            // 1. Gerar chave secreta aleatória
            _secretKey = KeyGeneration.GenerateRandomKey(20);

            // 2. Converter para Base32
            _secretKeyBase32 = Base32Encoding.ToString(_secretKey);

            // 3. Criar objeto TOTP
            _totp = new Totp(_secretKey);

            // 4. Mostrar a chave no ecrã
            SecretKeyText.Text = _secretKeyBase32;

            // 5. Gerar QR Code
            GenerateQRCode(userEmail);
        }

        private void GenerateQRCode(string userEmail)
        {
            string uri = $"otpauth://totp/PasswordManager:{userEmail}?secret={_secretKeyBase32}&issuer=PasswordManager";

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q))
                {
                    using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeBytes = qrCode.GetGraphic(20);

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

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string code = CodeBox.Text.Trim();

            if (string.IsNullOrEmpty(code) || code.Length != 6)
            {
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            // Verificar o código TOTP
            bool isValid = _totp.VerifyTotp(code, out long timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay);

            if (!isValid)
            {
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            // Código válido - Guardar no MongoDB
            bool saved = await LoginQuery.SaveTotpSecret(_parameters.UserEmail, _secretKeyBase32);

            if (!saved)
            {
                ContentDialog errorDialog = new ContentDialog()
                {
                    Title = "Error",
                    Content = "Failed to save 2FA settings. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
                return;
            }

            // Navegar para MainPage
            var navigationParams = new NavigationsParameters
            {
                Encryptor = _parameters.Encryptor,
                UserEmail = _parameters.UserEmail,
                TotpSecret = _secretKeyBase32
            };
            Frame.Navigate(typeof(MainPage), navigationParams);
        }
    }
}