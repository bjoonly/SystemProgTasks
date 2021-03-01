using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SystemProgTasks
{
    class TextInfo : INotifyPropertyChanged
    {
        private int numberSentances;
        private int numberInterrogativeSentances;
        private int numberExclamationSentances;
        private int numberCharacters;
        private int numberWords;

        public int NumberSentances { get { return numberSentances; } set { if (numberSentances != value) { numberSentances = value; OnPropertyChanged(); } } }
        public int NumberInterrogativeSentances { get { return numberInterrogativeSentances; } set { if (numberInterrogativeSentances != value) { numberInterrogativeSentances = value; OnPropertyChanged(); } } }
        public int NumberExclamationSentances { get { return numberExclamationSentances; } set { if (numberExclamationSentances != value) { numberExclamationSentances = value; OnPropertyChanged(); } } }
        public int NumberCharacters { get { return numberCharacters; } set { if (numberCharacters != value) { numberCharacters = value; OnPropertyChanged(); } } }
        public int NumberWords { get { return numberWords; } set { if (numberWords != value) { numberWords = value; OnPropertyChanged(); } } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    class ViewModel : INotifyPropertyChanged
    {
        private string text;
        public string Text { get { return text; } set { if (text != value) { text = value; OnPropertyChanged(); } } }

        private bool isSaving;
        public bool IsSaving { get { return isSaving; } set { if (value != isSaving) { isSaving = value; OnPropertyChanged(); } } }


        private bool isAddingNumberSentances;
        private bool isAddingNumberInterrogativeSentances;
        private bool isAddingNumberExclamationSentances;
        private bool isAddingNumberCharacters;
        private bool isAddingNumberWords;
        public bool IsAddingNumberSentances { get { return isAddingNumberSentances; } set { if (isAddingNumberSentances != value) { isAddingNumberSentances = value; OnPropertyChanged(); } } }
        public bool IsAddingNumberInterrogativeSentances { get { return isAddingNumberInterrogativeSentances; } set { if (isAddingNumberInterrogativeSentances != value) { isAddingNumberInterrogativeSentances = value; OnPropertyChanged(); } } }
        public bool IsAddingNumberExclamationSentances { get { return isAddingNumberExclamationSentances; } set { if (isAddingNumberExclamationSentances != value) { isAddingNumberExclamationSentances = value; OnPropertyChanged(); } } }
        public bool IsAddingNumberCharacters { get { return isAddingNumberCharacters; } set { if (isAddingNumberCharacters != value) { isAddingNumberCharacters = value; OnPropertyChanged(); } } }
        public bool IsAddingNumberWords { get { return isAddingNumberWords; } set { if (isAddingNumberWords != value) { isAddingNumberWords = value; OnPropertyChanged(); } } }

        private Command startCommand;
        public ICommand StartCommand => startCommand;


        private Command stopCommand;
        public ICommand StopCommand => stopCommand;

        CancellationTokenSource cancelTokenSource;
        CancellationToken token;
        private TextInfo info;
        public TextInfo Info { get { return info; } set { if (value != info) { info = value; OnPropertyChanged(); } } }

        public void Start()
        {
            try
            {

                if (token.IsCancellationRequested)
                    return;
                Task.Run(Calculate,token).ContinueWith(Save,token);
                   
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void Stop()
        {
            cancelTokenSource.Cancel();

        }

        public void Save(Task t)
        {
            if (isSaving) { 
            string path = "Report.txt";
                using (StreamWriter sw = new StreamWriter(File.Create(path)))
                {
                    sw.WriteLine("Report");
                    if (isAddingNumberSentances)
                        sw.WriteLine("Number of sentances: " + Info.NumberSentances);
                    if (isAddingNumberInterrogativeSentances)
                        sw.WriteLine("Number of interrogative sentances: " + Info.NumberInterrogativeSentances);
                    if (isAddingNumberExclamationSentances)
                        sw.WriteLine("Number of exclamation sentances: " + Info.NumberExclamationSentances);
                    if (isAddingNumberCharacters)
                        sw.WriteLine("Number of characters: " + Info.NumberCharacters);
                    if (isAddingNumberWords)
                        sw.WriteLine("Number of words: " + Info.NumberWords);

                }


            }
        }

        public void Calculate()
        {
            try
           {
                Info.NumberSentances = 0;
                Info.NumberExclamationSentances = 0;
                Info.NumberInterrogativeSentances = 0;
                Info.NumberCharacters = 0;
                Info.NumberWords = 0;
                string[] sentances = text.Split(new string[] { "...", ".", "?", "!", }, StringSplitOptions.RemoveEmptyEntries).AsParallel().WithCancellation(token).ToArray();
                string[] words = text.Split(new char[] { ' ', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);


                Info.NumberSentances = sentances.Length;
                Thread.Sleep(500);

                Info.NumberWords = words.Length;
                Thread.Sleep(500);

                Info.NumberCharacters = text.Where(t => t != ' ').AsParallel().WithCancellation(token).Count();
                Thread.Sleep(500);

                for (int i = 0; i < text.Length; i++)
                {
                    if (token.IsCancellationRequested)
                        return;

                    if (text[i] == '!' && i > 0 && text[i - 1] != ' ')
                        Info.NumberExclamationSentances++;
                    else if (text[i] == '?' && i > 0 && text[i - 1] != ' ')
                        Info.NumberInterrogativeSentances++;

                }



            }
            catch ( OperationCanceledException ex){
                MessageBox.Show(ex.Message);
            }
        }
        public ViewModel()
        {
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            startCommand = new DelegateCommand(Start, () => !String.IsNullOrEmpty(Text));
            stopCommand = new DelegateCommand(Stop);
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(Text))
                {
                    startCommand.RaiseCanExecuteChanged();


                }
            };
            Info = new TextInfo();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
