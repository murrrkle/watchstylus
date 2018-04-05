using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech;
using System.Speech.Recognition;


namespace TestingConcepts
{
    public class AstralSpeechEventArgs : EventArgs
    {
        public string RecognizedWord { get; set; }

        public AstralSpeechEventArgs(string recognizedWord)
        {
            this.RecognizedWord = recognizedWord;
        }
    }

    public class AstralSpeech
    {
        private List<string> acceptedWords;

        private Choices choices;
        private Grammar grammar;
        private GrammarBuilder grammarBuilder;
        private SpeechRecognitionEngine recognizer;

        public event EventHandler<AstralSpeechEventArgs> WordRecognized;

        private void RaiseSpeechRecognized(AstralSpeechEventArgs e)
        {
            WordRecognized?.Invoke(this, e);
        }

        public List<string> AcceptedWords
        {
            get
            {
                return this.acceptedWords;
            }
            set
            {
                this.acceptedWords = value;
            }
        }

        public void AddWord(string word)
        {
            this.recognizer.RecognizeAsyncStop();
            this.grammarBuilder.Append(word);
            this.grammar = new Grammar(this.grammarBuilder);
            this.recognizer.LoadGrammarAsync(grammar);
        }

        private void InitializeSpeech()
        {
            this.acceptedWords.Add("hello");
            this.choices = new Choices();
            this.choices.Add(this.acceptedWords.ToArray<string>());
            this.grammarBuilder = new GrammarBuilder();
            this.grammarBuilder.Append(choices);

            this.grammar = new Grammar(grammarBuilder);
            this.recognizer.LoadGrammarAsync(this.grammar);
            this.recognizer.SpeechRecognized += OnSpeechRecognized;
            this.recognizer.RecognizeAsync();

        }

        private void OnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine("RECOGNIZED SPEECH " + e.Result.Words);
            string word = this.acceptedWords.Where(s => e.Result.Text.Contains(s)).ToList().First();

            RaiseSpeechRecognized(new AstralSpeechEventArgs(word));
        }

        public AstralSpeech()
        {
            this.recognizer = new SpeechRecognitionEngine();
            this.recognizer.SetInputToDefaultAudioDevice();
            InitializeSpeech();
        }

    }
}
