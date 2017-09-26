using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExplode.Models;
using YoutubeExplode;
using System.IO;
using MediaToolkit;
using MediaToolkit.Model;
namespace YouTube_Downloader_1._0
{
    public partial class Form1 :M_Form
    {
        private List<string> VideoList = new List<string>();
        private string SavePath;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
          if (Properties.Settings.Default.SavePath == string.Empty){
                linkLabel1.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                SavePath = linkLabel1.Text;
                Properties.Settings.Default.SavePath = SavePath;
                Properties.Settings.Default.Save();}
          else{ SavePath = Properties.Settings.Default.SavePath;
                linkLabel1.Text = SavePath;}
            comboBox1.SelectedIndex = 0;

            if (Properties.Settings.Default.SelectedInxed == 0) comboBox1.SelectedIndex = 0;
            else if (Properties.Settings.Default.SelectedInxed == 1) comboBox1.SelectedIndex = 1;
          
            ImageList img = new ImageList();
            img.Images.Add("img", Properties.Resources.Youtube_32);
            listView1.SmallImageList = img;
       }


        private void linkLabel1_Click(object sender, EventArgs e){
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK){ 
            linkLabel1.Text = folderBrowserDialog1.SelectedPath;
            SavePath = folderBrowserDialog1.SelectedPath;
            Properties.Settings.Default.SavePath = folderBrowserDialog1.SelectedPath;
            Properties.Settings.Default.Save();}
        }
        
        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void Button1_click(object sender, EventArgs e)
        {
          if(textBox1.Text!= string.Empty)
            {
                Video v = new Video();
                v.VideoID = YoutubeExplode.YoutubeClient.ParseVideoId(textBox1.Text);
                v.SavePath = linkLabel1.Text;
                textBox1.Text = "";
                if (comboBox1.SelectedIndex == 0) {
                    Properties.Settings.Default.SelectedInxed = 0;
                    Properties.Settings.Default.Save();
                    DownloadVideoToAudio(v);
                }
                if (comboBox1.SelectedIndex==1)
                {
                    Properties.Settings.Default.SelectedInxed = 1;
                    Properties.Settings.Default.Save();
                    DownloadVideoAsync(v);
                }
            }
        }

        private void ConvertVideoToAudio (string filePath,string title, MediaToolkit.Options.ConversionOptions options = null)
        {
            var sourceFile = new MediaFile { Filename = filePath};
            var output = new MediaFile { Filename = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + ".mp3"};

            using (var eng = new Engine())
            {
                eng.GetMetadata(sourceFile);
                eng.Convert(sourceFile, output,options);
            }

            File.Delete(filePath);

            foreach (ListViewItem l in listView1.Items)
            {
                if (l.Name == title) { l.SubItems[3].Text = "Done"; break; }
            }

        }

        private string RemoveInvalidFileNameCharsFromTitle(string title)
        {
            string newTitle="";
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
               newTitle = title.Replace(c.ToString(), "");
            }
            return newTitle;

        }
        private async Task DownloadVideoToAudio(Video v)
        {
            var Client = new YoutubeClient();
            var videoInfo = Client.GetVideoInfoAsync(v.VideoID);
            var streamInfo = videoInfo.Result.AudioStreams.OrderBy(s => s.Bitrate).Last();
            v.FileName = RemoveInvalidFileNameCharsFromTitle($"{videoInfo.Result.Title}.{streamInfo.Container.GetFileExtension()}");
            v.Title = RemoveInvalidFileNameCharsFromTitle(videoInfo.Result.Title);
           

            //Adding listItem & progressBar to the listview
            ListViewItem lvItem = new ListViewItem();
            ProgressBar pb = new ProgressBar();

            lvItem.SubItems[0].Text = v.Title;
            lvItem.Name = v.Title;
            lvItem.ImageIndex = 0;
            double fs = (double)(streamInfo.ContentLength / 1024) / 1024;
            lvItem.SubItems.Add(fs.ToString("n2") );
            lvItem.SubItems.Add("");
            lvItem.SubItems.Add("0");
            listView1.Invoke(new MethodInvoker(delegate () { listView1.Items.Add(lvItem); }));

            Rectangle r = lvItem.SubItems[2].Bounds;
            pb.SetBounds(r.X, r.Y, r.Width, r.Height);
            pb.Minimum = 0;
            pb.Maximum = 100;
            pb.Value = 0;
            pb.Name = v.Title;
            listView1.Invoke(new MethodInvoker(delegate () { listView1.Controls.Add(pb); }));

            Thread thread = new Thread(() => UpdateProgressBar(v.SavePath + @"\" + v.FileName, v.Title));
            thread.IsBackground = true;
            thread.Start();
         
            await Client.DownloadMediaStreamAsync(streamInfo, v.SavePath + @"\" + v.FileName);

            do
            {
                Thread.Sleep(100);
                Application.DoEvents();
            } while (thread.IsAlive);
            ConvertVideoToAudio(v.SavePath + @"\" + v.FileName,v.Title);        
        }
        private async Task DownloadVideoAsync(Video v)
        {
            var Client = new YoutubeClient();
            var videoInfo = Client.GetVideoInfoAsync(v.VideoID);
            var streamInfo = videoInfo.Result.MixedStreams.OrderBy(s => s.VideoQuality).Last();
            v.FileName = $"{videoInfo.Result.Title}.{streamInfo.Container.GetFileExtension()}";
            v.Title = videoInfo.Result.Title;

            // Remove Invalid filename chars
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                v.FileName.Replace(c.ToString(), "");
            }
           
            //Adding listItem & progressBar to the listview
            ListViewItem lvItem = new ListViewItem();
            ProgressBar pb = new ProgressBar();

            lvItem.SubItems[0].Text = v.Title;
            lvItem.Name = v.Title;
            lvItem.ImageIndex = 0;
            double fs = (double)(streamInfo.ContentLength / 1024) / 1024;
            lvItem.SubItems.Add(fs.ToString("n2"));
            lvItem.SubItems.Add("");
            lvItem.SubItems.Add("0");
            listView1.Invoke(new MethodInvoker(delegate () { listView1.Items.Add(lvItem); }));

            Rectangle r = lvItem.SubItems[2].Bounds;
            pb.SetBounds(r.X, r.Y, r.Width, r.Height);
            pb.Minimum = 0;
            pb.Maximum = 100;
            pb.Value = 0;
            pb.Name = v.Title;
            listView1.Invoke(new MethodInvoker(delegate () { listView1.Controls.Add(pb); }));

            Thread thread = new Thread(() => UpdateProgressBar(v.SavePath + @"\" + v.FileName, v.Title));
            thread.IsBackground = true;
            thread.Start();

            await Client.DownloadMediaStreamAsync(streamInfo, v.SavePath + @"\" + v.FileName);

            do
            {
                Thread.Sleep(500);
                Application.DoEvents();
            } while (thread.IsAlive);

            foreach (ListViewItem l in listView1.Items){
                if (l.Name == v.Title) { l.SubItems[3].Text = "Done"; break; }}
        }
        
        private void UpdateProgressBar(string filepath ,  string videoTitle)
        {
            bool Done = false;
            Thread.Sleep(500);

            while (Done == false)
            {
                foreach (ListViewItem l in listView1.Items)
                {
                    if (l.Name== videoTitle)
                    {
                        string Msize = l.SubItems[1].Text;

                        FileInfo f = new FileInfo(filepath);
                        double s = (double)(f.Length / 1024) / 1024;
                        s = Math.Round(s, 2);

                        double downloaded = (double)s / double.Parse(Msize);

                        ProgressBar pb = new ProgressBar();
                        pb = listView1.Controls.OfType<ProgressBar>().FirstOrDefault(q => q.Name == videoTitle);
                        int perc = (int)(downloaded * 100);
                        pb.Value = perc;
                        l.SubItems[3].Text = perc.ToString();

                       
                        Thread.Sleep(500);
                        Application.DoEvents();
                        if (double.Parse(Msize) == s)
                        {
                            l.SubItems[3].Text = "...";
                            Done = true;
                        }
                        break;
                    }
                }
            }

        }

        private void listView1_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel = true;
            e.NewWidth = listView1.Columns[e.ColumnIndex].Width;
           
        }
    }
}

class Video{

    public string VideoID { get; set; }
    public string Title { get; set; }
    public  string SavePath { get; set; }
    public string FileName { get; set; }
}
