using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
namespace Video_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<TimeSpan> srtstarttime = new List<TimeSpan>();
        List<TimeSpan> srtendtime = new List<TimeSpan>();
        List<String> srtcontent = new List<String>();
        StringBuilder temp = new StringBuilder();
        SHDocVw.InternetExplorer ie;
        Boolean sliderpressed = false;
        Boolean playing = false;
        public MainWindow()
        {
            InitializeComponent();
            String path = FileAssociationPath.getPath();
            if (path != null)
            {
                setSource(path);
            }
            //calls the subtitle method every 0.5 seconds
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += Timer;
            timer.Start();
           
        }

        private void Timer(object sender, EventArgs e)
        {
            if(srtcontent.Count!=0)
            getSubtitle(0, srtstarttime.Count);
            if (sliderpressed == false)
            {
                UIMediaSlider.Value = UIMediaElement.Position.TotalMinutes;
                UICurrentTime.Text = UIMediaElement.Position.ToString().Substring(0, 8);
            }
        }
        //algorithm to reduce the search space
        void getSubtitle(int start,int end)
        {
            List<WordClass> words = new List<WordClass>();
            int diff = end - start;
            int middle = start + diff/ 2;

            if (srtstarttime[middle] > UIMediaElement.Position && diff > 15)
            {
                getSubtitle(start, middle);
            }
            else if (srtstarttime[middle] < UIMediaElement.Position && diff > 15)
            {
                getSubtitle(middle, end);
            }
            else
            {
                for(int i=start;i<end;i++)
                {

                    if (srtstarttime[i] <UIMediaElement.Position && srtendtime[i] > UIMediaElement.Position)
                    {
                        words.Clear();
                        UISubtitleItemsControl.ItemsSource = null;
                        String[] contents=srtcontent[i].Split(new String[]{" ","\n"},StringSplitOptions.RemoveEmptyEntries);
                        for (int k = 0; k < contents.Length;k++)
                        {
                            words.Add(new WordClass(contents[k]));
                        }
                        UISubtitleItemsControl.ItemsSource = words;
                        break;
                    }
                    else
                    {
                        UISubtitleItemsControl.ItemsSource = null;
                    }
                }
            }
        }
        public void setSource(String path)
        {
            UIMediaElement.Source = new Uri(path);
            UIMediaElement.Play();
        }
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            //To open a dialog windows to choose a specific file
            OpenFileDialog filedialog = new OpenFileDialog();
            filedialog.Filter = "Media Files|*.mpg;*.avi;*.wma;*.mov;*.mp4|All Files|*.*";           
            filedialog.ShowDialog();
            if (filedialog.FileName != "" && filedialog.FileName != null)
            {
                //clears the last played videos sub content
                srtcontent.Clear();
                srtstarttime.Clear();
                srtendtime.Clear();
                setSource(filedialog.FileName);
                setSrtContents(filedialog.FileName);
                UISubtitleItemsControl.ItemsSource = null;
            }

        }
        void setSrtContents(String path)
        {
            String[] subtitlelines;
            int index = path.LastIndexOf(".");
            String srtfile = path.Substring(0, index) + ".srt";
            
            if(File.Exists(srtfile))
            {
                int j=1;
                subtitlelines=File.ReadAllLines(srtfile);
                for(int i=0;i+3<subtitlelines.Length;i++)
                {
                    //checks the subtitle s.no with the j which is incremented at the end of the loop
                    if(subtitlelines[i]==j.ToString())
                    {
                        //moves to the time line
                        i++;
                        subtitlelines[i]= subtitlelines[i].Replace(',', '.');
                        String [] split=subtitlelines[i].Split(new String[]{" --> "},StringSplitOptions.None);
                        if (split.Length == 2)
                        {
                            srtstarttime.Add(TimeSpan.Parse(split[0]));
                            srtendtime.Add(TimeSpan.Parse(split[1]));
                        }
                        else
                            break;
                        //moves to the subtitle line
                        i++;
                        
                        temp.Clear();
                        //loops until the current subtitle ends
                        while(i<subtitlelines.Length&&subtitlelines[i]!=(j+1).ToString())
                        {
                            //removes the html tags
                            while(subtitlelines[i].Contains("<")&&subtitlelines[i].Contains(">"))
                            {
                                int indexstart = subtitlelines[i].IndexOf("<");
                                int range=subtitlelines[i].IndexOf(">")-indexstart+1;
                                if(range>0)
                                subtitlelines[i] = subtitlelines[i].Remove(indexstart, range);
                            }
                            temp.Append(subtitlelines[i]);
                            temp.Append("\n");
                            i++;
                        }
                        i--;
                        //adds the subtitle in the srtcontent list
                        srtcontent.Add(temp.ToString());
                    }
                    j++;
                }
                subtitlelines = null;
            }
        }

        void UIWindow_MouseMove(object sender, MouseEventArgs e)
        {
            //for dragging the window
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        
        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //gets the selected word and opens up the internet explorer for showing the word meaning
            //UIMediaElement.Pause();
            TextBlock a=(TextBlock)e.Source;
            String word = a.Text.Replace(" ", "").Replace(".", "").Replace("!", "").Replace(",", "").Replace(";", "").Replace(":", "").Replace("?", "").Replace("\n","").Replace("'","").Replace("\"","");
            try
            {
                if (ie == null)
                {
                    ie = new SHDocVw.InternetExplorer();
                    ie.ToolBar = 0;
                    //ie.StatusBar = false;
                    ie.MenuBar = false;
                    ie.Width = 450;
                    ie.Height = 500;
                    ie.Visible = true;
                    ie.Navigate("www.storyboardlabs.net/dict/getmeaning.php?word=" + word);
                }
                else
                {
                    ie.Navigate("www.storyboardlabs.net/dict/getmeaning.php?word=" + word);
                }
                UIMediaElement.Pause();
                playing = false;
                UIPlayPauseButton.Content = "Play";
                
            }
            catch(Exception ex)
            {
                ie = new SHDocVw.InternetExplorer();
                ie.ToolBar = 0;
                //ie.StatusBar = false;
                ie.MenuBar = false;
                ie.Width = 450;
                ie.Height = 500;
                ie.Visible = true;
                ie.Navigate("www.storyboardlabs.net/dict/getmeaning.php?word=" + word);
            }
        }

        private void UICloseButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void UIMinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void UIWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UIMediaElement.Height = Application.Current.MainWindow.ActualHeight - 96;
            UIMediaElement.Width = Application.Current.MainWindow.ActualWidth;
            //sets the subtitle at the bottom with the margin 20
            double margin = UIMediaElement.Height - UIMediaElement.ActualHeight;
            margin = (double)(margin / 2) + 15;
            if (UIMediaElement.ActualHeight != 0)
                UISubtitleItemsControl.Margin = new Thickness(0, 0, 0, margin);
            else
                UISubtitleItemsControl.Margin = new Thickness(0, 0, 0, 15);
        }

        private void UIMediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            UIMediaSlider.Value = 0.0;
            UIMediaSlider.Maximum = UIMediaElement.NaturalDuration.TimeSpan.TotalMinutes;
            UIMovieTitleLabel.Content = UIMediaElement.Source.Segments[UIMediaElement.Source.Segments.Length-1].Replace("%20"," ").Replace("%5B","-").Replace("%5D","");
            //sets the windows according to naturalvideosize
            UIWindow.Width = UIMediaElement.NaturalVideoWidth;
            UIWindow.Height = UIMediaElement.NaturalVideoHeight + 96;
            //sets the subtitle at the bottom with the margin 20
            double margin = UIMediaElement.Height - UIMediaElement.ActualHeight;
            margin = (double)(margin / 2) + 15;
            if (UIMediaElement.ActualHeight != 0)
                UISubtitleItemsControl.Margin = new Thickness(0, 0, 0, margin);
            else
                UISubtitleItemsControl.Margin = new Thickness(0, 0, 0, 15);
            UIEndTime.Text = UIMediaElement.NaturalDuration.TimeSpan.ToString().Substring(0, 8);
            UIMediaElement.Play();
            playing = true;
            UIPlayPauseButton.Content = "Pause";
        }
        private void UIMediaSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            sliderpressed = true;
        }

        private void UIMediaSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UIMediaElement.Position = TimeSpan.FromMinutes(UIMediaSlider.Value);
            UICurrentTime.Text = UIMediaElement.Position.ToString().Substring(0, 8);
            sliderpressed = false;
        }

        private void UIVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UIMediaElement.Volume = UIVolume.Value;
        }

        private void UIPlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if(playing)
            {
                UIMediaElement.Pause();
                playing = false;
                UIPlayPauseButton.Content = "Play";
            }
            else
            {
                UIMediaElement.Play();
                playing = true;
                UIPlayPauseButton.Content = "Pause";
            }
        }

        private void AddSubtitle_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog filedialog = new OpenFileDialog();
            filedialog.Filter = "Subtitle|*.srt";           
            filedialog.ShowDialog();
            if (filedialog.FileName != "" && filedialog.FileName != null)
            {
                srtcontent.Clear();
                srtstarttime.Clear();
                srtendtime.Clear();
                setSrtContents(filedialog.FileName);
            }
        }

        private void UIWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Space)
            {
                UIPlayPauseButton_Click(sender, e);
            }
        }
    }
}
