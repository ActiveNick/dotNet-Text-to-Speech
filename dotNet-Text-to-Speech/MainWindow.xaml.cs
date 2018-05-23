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

            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (en-US, Jessa24KRUS)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (en-US, Guy24KRUS)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (en-US, BenjaminRUS)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (en-AU, Catherine)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (en-AU, HayleyRUS)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (en-CA, Linda)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (en-CA, HeatherRUS)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (en-GB, Susan, Apollo)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (en-GB, HazelRUS)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (en-GB, George, Apollo)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (en-IE, Sean)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (en-IN, Heera, Apollo)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (en-IN, PriyaRUS)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (en-IN, Ravi, Apollo)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (fr-FR, Julie, Apollo)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (fr-FR, HortenseRUS)");
            cboVoices.Items.Add("Microsoft Server Speech Text to Speech Voice (fr-FR, Paul, Apollo)");
            cboVoices.SelectedIndex = 0;

            // FOR MORE INFO ON AUTHENTICATION AND HOW TO GET YOUR API KEY, PLEASE VISIT
            // https://docs.microsoft.com/en-us/azure/cognitive-services/speech/how-to/how-to-authentication
            Authentication auth = new Authentication("https://api.cognitive.microsoft.com/sts/v1.0/issueToken", 
                                                     "INSERT-YOUR-BING-SPEECH-API-KEY-HERE");

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
        /// This method is called once the audio returned from the service.
        /// It will then attempt to play that audio file.
        /// Note that the playback will fail if the output audio format is not pcm encoded.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="GenericEventArgs{Stream}"/> instance containing the event data.</param>
        private static void PlayAudio(object sender, GenericEventArgs<Stream> args)
        {
            Console.WriteLine(args.EventData);

            // For SoundPlayer to be able to play the wav file, it has to be encoded in PCM.
            // Use output audio format AudioOutputFormat.Riff16Khz16BitMonoPcm to do that.
            SoundPlayer player = new SoundPlayer(args.EventData);
            player.PlaySync();
            args.EventData.Dispose();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Starting TTSSample request code execution.");
            // Synthesis endpoint for old Bing Speech API: https://speech.platform.bing.com/synthesize
            // For new unified SpeechService API: https://westus.tts.speech.microsoft.com/cognitiveservices/v1
            // Note: new unified SpeechService API synthesis endpoint is per region
            string requestUri = "https://westus.tts.speech.microsoft.com/cognitiveservices/v1";
            var cortana = new Synthesize();

            cortana.OnAudioAvailable += PlayAudio;
            cortana.OnError += ErrorHandler;

            // Reuse Synthesize object to minimize latency
            cortana.Speak(CancellationToken.None, new Synthesize.InputOptions()
            {
                RequestUri = new Uri(requestUri),
                // Text to be spoken.
                Text = txtInput.Text,
                VoiceType = Gender.Female,
                // Refer to the documentation for complete list of supported locales.
                Locale = "en-US",
                // You can also customize the output voice. Refer to the documentation to view the different
                // voices that the TTS service can output.
                VoiceName = cboVoices.Text,

                // Service can return audio in different output format.
                OutputFormat = AudioOutputFormat.Riff24Khz16BitMonoPcm,
                AuthorizationToken = "Bearer " + accessToken,
            }).Wait();

        }
    }
}
