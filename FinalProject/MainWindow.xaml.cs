using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;

namespace FinalProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker worker;
        Stopwatch testing = new Stopwatch();
        string wordCurrentlyTyping = "";
        double messageLength = 0;
        double maxWPM = 0;
        double avgWPM = 0;
        int numSessions = 0;
        int selectedPassage = 0;
        List<string> passages = new List<string>();
        LoadSave fileIOHandler;

        public MainWindow()
        {
            InitializeComponent();
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            fileIOHandler = new LoadSave();

            loadPassages();
            maxWPM = Convert.ToDouble(fileIOHandler.loadFile("highestWPM.txt"));
            string tempForAverage = fileIOHandler.loadFile("averageWPM.txt");
            int i;
            for (i = 0; i < tempForAverage.Length; i++)
                if (tempForAverage.ElementAt(i) == ',')
                    break;
            avgWPM = Convert.ToDouble(tempForAverage.Substring(0, i));
            if (i < tempForAverage.Length)
                numSessions = Convert.ToInt32(tempForAverage.Substring(i + 1));
        }

        void loadPassages()
        {
            string fileContents = fileIOHandler.loadFile("passages.txt");
            int i = 0;
            int passageID = 0;

            //parse passages into the passages list
            while (fileContents.Length > 0)
            {
                while (i < fileContents.Length && fileContents.ElementAt(i) != '\n')
                {
                    i++;
                }

                if (i < fileContents.Length - 1)
                {
                    passages.Add(fileContents.Substring(0, i));
                    fileContents = fileContents.Substring(i + 1);
                }
                else
                {
                    passages.Add(fileContents);
                    passages[passages.Count - 1] = passages[passages.Count - 1].Substring(0, passages[passages.Count - 1].Length - 2);
                    fileContents = "";
                }
                i = 0;
                passageID++;
            }
        }

        //saves the passages to the text file
        void savePassages()
        {
            string toSave = "";

            for (int i = 0; i < passages.Count; i++)
            {
                toSave += passages[i];

                if (i < passages.Count - 1)
                    toSave += "\n";
            }

            fileIOHandler.saveFile("passages.txt", toSave);
        }

        //save wpm to a text file.
        void saveWPM()
        {
            fileIOHandler.saveFile("highestWPM.txt", maxWPM.ToString());
            fileIOHandler.saveFile("averageWPM.txt", avgWPM.ToString() + "," + numSessions.ToString());
        }

        void calcNewAvgWPM(double newWPM)
        {
            avgWPM = ((avgWPM * numSessions) + newWPM) / (numSessions + 1);
            numSessions++;
        }

        void setupRandomPassage()
        {
            Random rand = new Random();
            int passageID = rand.Next(0, passages.Count);
            int i = 0;

            while (passages[passageID].ElementAt(i) != ' ')
            {
                i++;
            }

            previousWords.Text = "";
            currentWord.Text = passages[passageID].Substring(0, i);
            futureWords.Text = passages[passageID].Substring(i);
        }

        //practice button on main menu was clicked
        private void practiceButton_Click(object sender, RoutedEventArgs e)
        {
            //practice button functionality changes depending on the current value of the button
            if (practiceButton.Content.ToString() == "Practice")
            {
                practiceButton.Content = "Back";
                startButton.Visibility = Visibility.Visible;
                statisticsButton.Visibility = Visibility.Collapsed;
                Title.Visibility = Visibility.Collapsed;
                passageButton.Visibility = Visibility.Collapsed;
            }
            else if (practiceButton.Content.ToString() == "Back")
            {
                practiceButton.Content = "Practice";
                textToType.Visibility = Visibility.Collapsed;
                textBox.Visibility = Visibility.Collapsed;
                startButton.Visibility = Visibility.Collapsed;
                Countdown.Visibility = Visibility.Collapsed;
                statisticsButton.Visibility = Visibility.Visible;
                Title.Visibility = Visibility.Visible;
                passageButton.Visibility = Visibility.Visible;
                rightButton.Visibility = Visibility.Collapsed;
                leftButton.Visibility = Visibility.Collapsed;
                passageValue.Visibility = Visibility.Collapsed;
                editPassage.Visibility = Visibility.Collapsed;
                saveButton.Visibility = Visibility.Collapsed;
                newButton.Visibility = Visibility.Collapsed;

                if (worker != null)
                    worker.CancelAsync();
            }
        }

        //start button was clicked
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (worker == null)
            {
                worker = new BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            }
            if (!worker.IsBusy)//start typing race if worker is not busy
            {
                textToType.Visibility = Visibility.Visible;
                textBox.Visibility = Visibility.Visible;
                textBox.Text = "";
                testing.Reset();
                testing.Stop();
                Countdown.Visibility = Visibility.Visible;
                textBox.IsEnabled = false;

                setupRandomPassage();

                wordCurrentlyTyping = currentWord.Text;
                messageLength = currentWord.Text.Length + futureWords.Text.Length;

                worker.RunWorkerAsync();

                testing.Start();
                //typingThreadRef.Start();
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (wordCurrentlyTyping.Length != 0)
            {
                if (worker != null && worker.CancellationPending)
                {
                    worker.Dispose();
                    worker = null;
                    testing.Reset();
                    testing.Stop();
                    return;
                }

                textToType.Dispatcher.Invoke(new Action(() =>
                {
                    if (testing.Elapsed.Seconds >= 3)
                    {
                        if (textBox.Text == currentWord.Text)
                        {
                            textBox.Text = "";
                            previousWords.Text += currentWord.Text;

                            if (futureWords.Text.Length != 0)
                            {
                                int i = 0;

                                do
                                {
                                    //prevent out of bounds errors
                                    if (i < futureWords.Text.Length)
                                    {
                                        //search for spaces
                                        if (futureWords.Text.ElementAt(i) != ' ')
                                        {
                                            i++;
                                        }
                                    }
                                } while (i < futureWords.Text.Length && futureWords.Text.ElementAt(i) != ' ');

                                if (i == futureWords.Text.Length)
                                {
                                    currentWord.Text = wordCurrentlyTyping = futureWords.Text.Substring(0, i);
                                    futureWords.Text = "";
                                }
                                else
                                {
                                    i++;
                                    currentWord.Text = wordCurrentlyTyping = futureWords.Text.Substring(0, i);
                                    futureWords.Text = futureWords.Text.Substring(i);
                                }
                            }
                            else
                            {
                                currentWord.Text = wordCurrentlyTyping = "";
                            }
                        }
                        else if (textBox.Text.Length > 0 && (textBox.Text.Length > currentWord.Text.Length || (textBox.Text.Length <= currentWord.Text.Length && textBox.Text != currentWord.Text.Substring(0, textBox.Text.Length))))
                        {//if the user made a mistake
                            textBox.Background = Brushes.LightPink;
                        }
                        else
                        {
                            textBox.Background = Brushes.White;
                        }
                    }
                    else if (testing.Elapsed.Seconds == 0)
                    {
                        //3!
                        Countdown.Content = "3";
                    }
                    else if (testing.Elapsed.Seconds == 1)
                    {
                        //2!
                        Countdown.Content = "2";
                    }
                    else if (testing.Elapsed.Seconds == 2)
                    {
                        //1!
                        Countdown.Content = "1";
                    }

                    if (testing.Elapsed.Seconds == 4)
                    {
                        //hide countdown
                        Countdown.Visibility = Visibility.Collapsed;
                    }
                    else if (testing.Elapsed.Seconds == 3)
                    {
                        //Go!
                        Countdown.Content = "Go!";
                        textBox.IsEnabled = true;
                        textBox.Focus();
                    }

                }));
            }

            if (wordCurrentlyTyping.Length == 0)
            {
                double t = (60 * testing.Elapsed.Minutes + testing.Elapsed.Seconds + (((double)testing.Elapsed.Milliseconds) / 1000)) - 3;
                double wpm = (12 * messageLength) / t;

                //A word is considered to have an average length of 5. t is the time in seconds, so dividing by 60 gives us the time in minutes.
                //Dividing a word by 5 gives the number of words (on average), so we divide messageLength by t / 12.
                MessageBox.Show("Time: " + t.ToString() + "\nWPM: " + (messageLength / (t / 12)).ToString());

                //repace maxWPM if new WPM is higher
                if (maxWPM < wpm)
                {
                    maxWPM = wpm;
                }

                calcNewAvgWPM(wpm);
                saveWPM();
            }

            testing.Reset();
            testing.Stop();

            textToType.Dispatcher.Invoke(new Action(() =>
            {
                previousWords.Text = "";
                currentWord.Text = "";
                futureWords.Text = "";
                textBox.Visibility = Visibility.Collapsed;
            }));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //worker.WorkerSupportsCancellation = true;
            if (worker != null)
            {
                worker.CancelAsync();
                worker.Dispose();
            }
        }

        private void statisticsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Highest WPM: " + maxWPM + "\nAverage WPM: " + avgWPM + "\nNumber of sessions: " + numSessions);
        }

        private void passageButton_Click(object sender, RoutedEventArgs e)
        {
            Title.Visibility = Visibility.Collapsed;
            passageButton.Visibility = Visibility.Collapsed;
            statisticsButton.Visibility = Visibility.Collapsed;
            rightButton.Visibility = Visibility.Visible;
            leftButton.Visibility = Visibility.Visible;
            editPassage.Visibility = Visibility.Visible;
            saveButton.Visibility = Visibility.Visible;
            newButton.Visibility = Visibility.Visible;
            passageValue.Visibility = Visibility.Visible;
            selectedPassage = 0;
            passageValue.Content = "Passage: 1";
            editPassage.Text = passages[selectedPassage];
            practiceButton.Content = "Back";
        }

        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            selectedPassage++;

            if (selectedPassage >= passages.Count)
                selectedPassage = 0;

            passageValue.Content = "Passage: " + (selectedPassage + 1).ToString();
            editPassage.Text = passages[selectedPassage];
        }

        private void leftButton_Click(object sender, RoutedEventArgs e)
        {
            selectedPassage--;

            if (selectedPassage < 0)
                selectedPassage = passages.Count - 1;

            passageValue.Content = "Passage: " + (selectedPassage + 1).ToString();
            editPassage.Text = passages[selectedPassage];
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (editPassage.Text.Length == 0)
            {
                MessageBox.Show("The passage cannot be empty.");
            }
            else
            {
                passages[selectedPassage] = editPassage.Text;
                savePassages();
            }
        }

        private void newButton_Click(object sender, RoutedEventArgs e)
        {
            passages.Add("Edit");
            selectedPassage = passages.Count - 1;
            passageValue.Content = "Passage: " + (selectedPassage + 1).ToString();
            editPassage.Text = passages[selectedPassage];
        }
    }
}
