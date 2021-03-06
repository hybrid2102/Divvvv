﻿using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Divvvv
{
    public partial class MainWindow : Window
    {
        private readonly User _user;

        public MainWindow()
        {
            InitializeComponent();
            txtTitle.Focus();
            _user = new User();
            _user.AddedShows += (s, e) => Dispatcher.Invoke(() => UpdateLstHint());
        }

        private async Task LoadShow()
        {
            txtTitle.CaretIndex = txtTitle.Text.Length;
            string matchLink = txtTitle.Text.ReMatch(@"https?://(.*?)/?$");
            string showId;
            if (!string.IsNullOrEmpty(matchLink))
            {
                string[] urlSplit = matchLink.Split('/');
                if (urlSplit.Length < 4 || !matchLink.Contains("vvvvid") || !urlSplit[2].IsInt())
                    return;
                showId = urlSplit[2];
            }
            else
            {
                if (!_user.ShowsDictionary.ContainsKey(txtTitle.Text))
                    return;
                showId = _user.ShowsDictionary[txtTitle.Text];
            }
            DataContext = await _user.GetShowAsync(showId);
            txtTitle.Focus();
        }

        private void button_Click(object sender, RoutedEventArgs e) => LoadShow().DoNotAwait();

        private void txtTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = txtTitle.Text.Trim();
            if (text.StartsWith("http") && text.Contains("vvvvid"))
                lstHint.Visibility = Visibility.Hidden;
            else
                UpdateLstHint();
        }

        private void UpdateLstHint()
        {
            var m = _user.SearchShow(txtTitle.Text.Trim());
            lstHint.ItemsSource = m;
            lstHint.Visibility = m.Any() ? Visibility.Visible : Visibility.Hidden;
        }

        private void LstHintSelection()
        {
            if (lstHint.ItemsSource.Cast<object>().Count() == 1)
                lstHint.SelectedIndex = 0;
            if (lstHint.SelectedIndex == -1)
                return;
            txtTitle.Text = (string)lstHint.SelectedItem;
            lstHint.Visibility = Visibility.Hidden;
        }

        private void lstHint_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LstHintSelection();
            LoadShow().DoNotAwait();
        }

        private void lstHint_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                LstHintSelection();
            else if (e.Key != Key.Up && e.Key != Key.Down)
                txtTitle.Focus();
        }
        
        private void txtTitle_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                    lstHint.Focus();
                    break;
                case Key.Enter:
                    LstHintSelection();
                    break;
                case Key.Escape:
                    lstHint.Visibility = Visibility.Hidden;
                    break;
            }
        }
    }
}
