using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WordleC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private int _step = 0;
        private string _keyWord = string.Empty;

        private void Form1_Load(object sender, EventArgs e)
        {
            //_keyWord = "manga";
            _keyWord = ChooseWord();
            label1.Visible = false;
            CreateTextBoxes();
            //Buttons();


        }

        private string ChooseWord()
        {
            var word = string.Empty;
            while (string.IsNullOrEmpty(word))
            {
                using (WebClient wc = new WebClient())
                {
                    var random = new Random().Next(1, 90000);
                    var json = wc.DownloadString($"https://sozluk.gov.tr/gts_id?id={random}");
                    Sozluk tempWord = JsonConvert.DeserializeObject<List<Sozluk>>(json).FirstOrDefault();
                    if (tempWord != null)
                    {
                        if (tempWord.Madde.Length == 5)
                        {
                            word = tempWord.Madde;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Servis çalışmıyor");
                        break;
                    }
                }
            }
            return word;
        }

        private bool CheckWordInSozluk(string input)
        {
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString($"https://sozluk.gov.tr/gts?ara={input}");
                Sozluk tempWord = (dynamic)null;
                if (json.StartsWith("{"))
                {
                    tempWord = JsonConvert.DeserializeObject<Sozluk>(json);
                }
                else
                {
                    tempWord = JsonConvert.DeserializeObject<List<Sozluk>>(json).FirstOrDefault();
                }
                
                if (tempWord != null)
                {
                    if (string.IsNullOrEmpty(tempWord.Error))
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    MessageBox.Show("Servis çalışmıyor");
                    return false;
                }
                
            }
        }

        private void CreateTextBoxes()
        {
            for (int i = 0; i < 30; i++)
            {
                TextBox t = new TextBox();
                t.Name= Convert.ToString(i);
                t.Enabled = i<5;
                t.KeyDown += new KeyEventHandler(Form1_KeyDown);
                t.MaxLength = 1;
                t.Size = new Size(50, 50);
                t.TextAlign = HorizontalAlignment.Center;
                t.Multiline = true; //c# autosize
                tableLayoutPanel1.Controls.Add(t);
            }
        }

        private void OpenCloseTextBoxes()
        {
            //eskileri kapatma
            for (int i = (_step - 1) * 5; i < (_step - 1) * 5 + 5; i++)
            {
                TextBox tBox = (TextBox)tableLayoutPanel1.Controls.Find(Convert.ToString(i), false)[0];
                tBox.Enabled = false;
            }
            //yenileri açma
            for (int i = _step * 5; i < _step * 5+5; i++)
            {
                TextBox tBox = (TextBox)tableLayoutPanel1.Controls.Find(Convert.ToString(i), false)[0];
                tBox.Enabled = true;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            
            TextBox t = (TextBox)sender;
            var tIndex = Convert.ToInt32(t.Name);

            var focusIndex = tIndex + 1;

            if (e.KeyCode == Keys.Enter)
            {
                TextSubmit();
            }
            else if (e.KeyCode == Keys.Back)
            {
                focusIndex = tIndex;

                if (tIndex % 5 != 0)
                {
                    focusIndex = tIndex-1;
                }
            }

            try
            {
                TextBox tBox = (TextBox)tableLayoutPanel1.Controls.Find(Convert.ToString(focusIndex), false)[0];
                tBox.Focus();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        private void TextSubmit()
        {
            var inputWord = string.Empty;
            for (int i = _step * 5; i < _step * 5 + 5; i++)
            {
                TextBox tBox = (TextBox)tableLayoutPanel1.Controls.Find(Convert.ToString(i), false)[0];
                inputWord += tBox.Text;
            }
            CheckWord(_keyWord,inputWord);
        }

        private void CheckWord(string keyWord, string inputWord)
        {
            if (inputWord.Length != 5)
            {
                MessageBox.Show("Lütfen doğru uzunlukta kelime girin.");
                return;
            }

            if (!CheckWordInSozluk(inputWord))
            {
                MessageBox.Show("Kelime listesinde bulunamadı.");
                return;
            }

            var count = _step*5;
            var trueLetterCount = 0;

            keyWord = keyWord.ToUpper();
            inputWord = inputWord.ToUpper();
            string newKeyWord = keyWord;

            var letters = new List<LetterIndex>();
            for(var i = 0; i < inputWord.Length; i++)
            {
                letters.Add(new LetterIndex()
                {
                    Index = count+i,
                    Letter = inputWord[i]
                });
            }


            foreach(var c in inputWord)
            {
                var kChar = keyWord[count % 5];
                if (kChar == c)
                {
                    Console.WriteLine("aynı"+c);
                    FillColorTextbox(count, Color.LightGreen);
                    trueLetterCount++;
                    letters.Remove(letters.Find(x=>x.Index== count));
                    newKeyWord = keyWord.Remove(count % 5, 1);
                    if (trueLetterCount == 5)
                    {
                        GameOver(true);
                        return;
                    }
                }
                count++;
            }

            foreach (var letterIndex in letters)
            {
                if (newKeyWord.Contains(letterIndex.Letter))
                {
                    Console.WriteLine("içinde " + letterIndex.Letter);
                    FillColorTextbox(letterIndex.Index, Color.LightYellow);
                    newKeyWord=newKeyWord.Remove(newKeyWord.IndexOf(letterIndex.Letter), 1);
                }
            }

            //foreach (var character in keyWord)
            //{
            //    var iChar = inputWord[count%5];
            //    if (iChar == character)
            //    {
            //        Console.WriteLine(character);
            //        FillColorTextbox(count, Color.LightGreen);
            //        trueLetterCount++;
            //        if (trueLetterCount == 5)
            //        {
            //            GameOver(true);
            //            return;
            //        }
            //    }
            //    else if (keyWord.Contains(iChar) && keyWord[count%5]!=iChar)
            //    {
            //        Console.WriteLine("içinde " + iChar);
            //        FillColorTextbox(count, Color.LightYellow);
            //    }
            //    //inputWord = inputWord.Remove(0, 1);
            //    count++;
            //}


            _step++;

            if (_step > 5)
            {
                GameOver(false);
                return;
            }

            OpenCloseTextBoxes();
        }

        private void FillColorTextbox(int i,Color color)
        {
            var name = Convert.ToString(i);
            TextBox tBox = (TextBox)tableLayoutPanel1.Controls.Find(name, false)[0];
            tBox.BackColor = color;
        }

        private void GameOver(bool win)
        {
            label1.Visible = true;
            if (win)
            {
                label1.Text = "Kazandınız";
            }
            else
            {
                label1.Text = "Kaybettiniz \nKelime: " + _keyWord;
            }
            tableLayoutPanel1.Enabled = false;
        }

        //private void Buttons()
        //{
        //    String Wbutton = "";
        //    string[] keywords = { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "Ğ", "Ü", "A", "S", "D", "F", "G", "H", "J", "K", "L", "Ş", "İ", "Z", "X", "C", "V", "B", "N", "M", "Ö", "Ç" };
        //    string[] keywords2 = { "A", "S", "D", "F", "G", "H", "J", "K", "L", "Ş", "İ" };
        //    string[] keywords3 = { "Z", "X", "C", "V", "B", "N", "M", "Ö", "Ç" };

        //    for (int i = 0; i <= 31; i++)
        //    {

        //        tableLayoutPanel2.ColumnCount = 12;
        //        Button newButton = new Button();
        //        newButton.Width = 50;
        //        newButton.Height = 50;
        //        newButton.Left = (i % 5) * 50;
        //        newButton.Top = (i / 5) * 50;
        //        int sayac = newButton.TabIndex;
        //        //newButton.Text = keywords[i];
        //        //tableLayoutPanel1.Controls.Add(newButton);
        //        if (i <= 11)
        //        {
        //            newButton.Text = keywords[i];
        //            tableLayoutPanel2.Controls.Add(newButton);
        //        }
        //        if (i > 11 && i <= 22)
        //        {
        //            newButton.Text = keywords2[i - 12];
        //            tableLayoutPanel2.Controls.Add(newButton);
        //        }
        //        if (i > 22)
        //        {
        //            newButton.Text = keywords3[i - 23];
        //            tableLayoutPanel2.Controls.Add(newButton);
        //        }


        //    }
        //}

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
