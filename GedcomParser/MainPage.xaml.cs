using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GedcomParser
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            //Обзор
            
        }

        private string DeleteSign(string str, char sign)
        {
            string str_res = "";
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] != sign)
                {
                    str_res += str[i];
                }
            }
            return str_res;
        }

        private void ParseFile(StreamReader reader)
        {
            try
            {
                const int ID_POS = 3;
                const int ID_LEN = 7;
                const int NAME_POS = 7;
                const int GENDER_POS = 6;
                const int FID_POS = 8;
                int mode = 0;
                string current_id = "";

                string out_s = "";
                out_s += "gender(Person, Gend):-\n";
                out_s += "\tname(Person, Pid),\n";
                out_s += "\tgenderId(Gend, Pid).\n\n";
                out_s += "parent(Parent, Child):-\n";
                out_s += "\tname(Parent, ParID),\n";
                out_s += "\tname(Child, ChildID),\n";
                out_s += "\tparentId(ParID, ChildID).\n\n";

                string father_id = "";
                string mother_id = "";

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line[0] == '0')
                    {
                        father_id = "";
                        mother_id = "";
                        if (line.Contains(" @I"))
                        {
                            mode = 1;
                            current_id = line.Substring(ID_POS, ID_LEN);
                    }
                        else if (line.Contains(" @F"))
                        {
                            mode = 2;
                            current_id = line.Substring(ID_POS, ID_LEN);
                        }
                        else
                        {
                            mode = 0;
                        }
                    }
                    if (mode == 0)
                    {
                        continue;
                    }
                 
                    switch (mode)
                    {
                        case 1:
                            if (line.Contains("NAME"))
                            {
                                line = DeleteSign(line, '/');
                                string name = line.Substring(NAME_POS);
                                out_s += "name('" + name + "', '" + current_id + "').\n";
                                break;
                            }
                            if (line.Contains("SEX"))
                            {
                                string gender = line.Substring(GENDER_POS);
                                out_s += "genderId('" + gender + "', '" + current_id + "').\n";
                                break;
                            }
                            break;
                        case 2:
                            if (line.Contains("HUSB"))
                            {
                                father_id = line.Substring(FID_POS, ID_LEN);
                                break;
                            }
                            if (line.Contains("WIFE"))
                            {
                                mother_id = line.Substring(FID_POS, ID_LEN);
                                break;
                            }
                            if (line.Contains("CHIL"))
                            {
                                string child_id = line.Substring(FID_POS, ID_LEN);
                                if (father_id != "")
                                {
                                    out_s += "parentId('" + father_id + "', '" + child_id + "').\n";
                                }
                                if (mother_id != "")
                                {
                                    out_s += "parentId('" + mother_id + "', '" + child_id + "').\n";
                                }
                                break;
                            }
                            break;
                        default:
                            break;
                    }
                }
                output.Document.Selection.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, out_s);
            } catch
            {
                string err_message = "FATAL ERROR\n";
                output.Document.Selection.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, err_message);
            }
        }

        private async void parse_Click(object sender, RoutedEventArgs e)
        {
            //Parser
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".ged");
            //openPicker.FileTypeFilter.Add(".txt");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file == null)
            {
                return;
            }

            helloText.Visibility = Visibility.Collapsed;
            parse.Visibility = Visibility.Collapsed;
            progressRing.IsActive = true;

            await Task.Delay(2000);
            //output.Document.Selection.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, file.Path);
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            
            using (StreamReader reader = new StreamReader(stream.AsStream()))
            {
                ParseFile(reader);
            }


            progressRing.IsActive = false;
            resultText.Visibility = Visibility.Visible;
            output.Visibility = Visibility.Visible;
            toStart.Visibility = Visibility.Visible;
        }

        private void toStart_Click(object sender, RoutedEventArgs e)
        {
            resultText.Visibility = Visibility.Collapsed;
            output.Visibility = Visibility.Collapsed;
            toStart.Visibility = Visibility.Collapsed;

            helloText.Visibility = Visibility.Visible;
            parse.Visibility = Visibility.Visible;
        }

        private void helloText_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void output_TextChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
