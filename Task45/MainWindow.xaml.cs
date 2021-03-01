
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Task45
{

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        private int countFiles;
        private int countDublicates;
        private int countMoving;

        public int CountFiles { get { return countFiles; } set { if (value != countFiles) { countFiles = value; OnPropertyChanged(); } } }
        public int CountDublicates { get { return countDublicates; } set { if (value != countDublicates) { countDublicates = value; OnPropertyChanged(); } } }
        public int CountMoving { get { return countMoving; } set { if (value != countMoving) { countMoving = value; OnPropertyChanged(); } } }
        private string fromPath;
       private string toPath;
       public string FromPath { get { return fromPath; }set{ if (value != fromPath) { fromPath = value; OnPropertyChanged(); } } }
       public string ToPath { get { return toPath; } set { if (value != toPath) { toPath = value; OnPropertyChanged(); } } }
        List<FileInfo> originals = new List<FileInfo>();
        private void fromButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                FromPath = fbd.SelectedPath;
      
        }

        private void toButton_Click(object sender, RoutedEventArgs e)
        {

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                ToPath = fbd.SelectedPath;
            

        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Task.Run(Search).ContinueWith(FileTransfer);

            }catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        public void FileTransfer(Task t)
        {
            foreach (var item in originals)
            {                
                File.Move(item.FullName, Path.Combine(ToPath, item.Name));
                CountMoving++;
            }
            
            CountDublicates = CountFiles - CountMoving;
        }
        public void Search()
        {
        
            DirectoryInfo di = new DirectoryInfo(FromPath);
            List<FileInfo> files = new List<FileInfo>(di.GetFiles());
            int count;

            CountFiles = files.Count;
           
            var dublicates = files.GroupBy(f => f.Length).Where(f =>f.Count() > 1).ToList();
            foreach (var group in files.GroupBy(f => f.Length).Where(f => f.Count()== 1))
            {
                foreach (var item in group)
                {

                originals.Add(item);
                
                }

            }
            foreach (var g in dublicates)
            {
                
                foreach (var file in g)
                {
                    count = 0;

                    foreach (var item in g)
                    {
                        if (file.FullName == item.FullName)
                            continue;
                        using (StreamReader sr1 = new StreamReader(File.OpenRead(file.FullName)))
                        {
                            using (StreamReader sr2 = new StreamReader(File.OpenRead(item.FullName)))
                            {
                                string line1 = sr1.ReadToEnd();
                                string line2 = sr2.ReadToEnd();
                                if (line1 == line2)
                                    count++;
                                sr2.Close();
                            }
                            sr1.Close();

                        }

                    }
                        if (count == 0)
                        {
                        originals.Add(file);
     

                    }
                }

            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
