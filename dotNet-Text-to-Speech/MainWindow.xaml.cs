using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CognitiveServicesTTS;
using System.Diagnostics;

namespace dotNet_Text_to_Speech
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string accessToken;

        public MainWindow()
        {
            InitializeComponent();

            // Add all the voices in the enum to the droplist in the UI
            foreach (VoiceName voice in Enum.GetValues(typeof(VoiceName)))
            {
                cboVoices.Items.Add(voice);
            }
            cboVoices.SelectedIndex = (int)VoiceName.enUSJessaRUS;

            // FOR MORE INFO ON AUTHENTICATION AND HOW TO GET YOUR API KEY, PLEASE VISIT
            // https://docs.microsoft.com/en-us/azure/cognitive-services/speech/how-to/how-to-authentication
            Authentication auth = new Authentication("https://api.cognitive.microsoft.com/sts/v1.0/issueToken",
                                                     "4d5a1beefe364f8986d63a877ebd51d5");
            // INSERT-YOUR-BING-SPEECH-API-KEY-HERE

            try
            {
                accessToken = auth.GetAccessToken();
                Console.WriteLine("Token: {0}\n", accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed authentication.");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Handler an error when a TTS request failed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="GenericEventArgs{Exception}"/> instance containing the event data.</param>
        private static void ErrorHandler(object sender, GenericEventArgs<Exception> e)
        {
            Console.WriteLine("Unable to complete the TTS request: [{0}]", e.ToString());
        }

        private async void btnSpeak_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Starting TTSSample request code execution.");
            // Synthesis endpoint for old Bing Speech API: https://speech.platform.bing.com/synthesize
            // For new unified SpeechService API: https://westus.tts.speech.microsoft.com/cognitiveservices/v1
            // Note: new unified SpeechService API synthesis endpoint is per region
            string requestUri = "https://westus.tts.speech.microsoft.com/cognitiveservices/v1";
            var cortana = new Synthesize();

            //cortana.OnAudioAvailable += PlayAudio;
            cortana.OnError += ErrorHandler;

            // Reuse Synthesize object to minimize latency
            Stream audiostream = await cortana.Speak(CancellationToken.None, new Synthesize.InputOptions()
            {
                RequestUri = new Uri(requestUri),
                // Text to be spoken.
                Text = txtInput.Text,
                VoiceType = Gender.Female,
                PitchDelta = int.Parse(txtPitch.Text),
                // Refer to the documentation for complete list of supported locales.
                Locale = "en-US",
                // You can also customize the output voice. Refer to the documentation to view the different
                // voices that the TTS service can output.
                VoiceName = (VoiceName)cboVoices.SelectedItem,

                // Service can return audio in different output format.
                OutputFormat = AudioOutputFormat.Riff24Khz16BitMonoPcm,
                AuthorizationToken = "Bearer " + accessToken,
            });

            if ((bool)chkIsSavingEnabled.IsChecked)
            {
                bool fileExists = false;
                string filename = txtSavefile.Text;
                string path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, filename);
                fileExists = File.Exists(path);
                Debug.WriteLine("Saving audio clip to file:" + path);

                FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
                MemoryStream ms = (MemoryStream)audiostream;
                ms.WriteTo(fs);
                fs.Dispose();
            }

            // For SoundPlayer to be able to play the wav file, it has to be encoded in PCM.
            // Use output audio format AudioOutputFormat.Riff16Khz16BitMonoPcm to do that.
            SoundPlayer player = new SoundPlayer(audiostream);
            player.PlaySync();
            audiostream.Dispose();
        }

        private void btnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", System.AppDomain.CurrentDomain.BaseDirectory);
        }

        private void btnClearText_Click(object sender, RoutedEventArgs e)
        {
            txtInput.Text = "";
        }

        private void btnCopyName_Click(object sender, RoutedEventArgs e)
        {
            int maxlenght = Math.Min(128, txtInput.Text.Trim().Length);
            txtSavefile.Text = txtInput.Text.Trim().Substring(0, maxlenght) + ".wav";
        }
    }
}
