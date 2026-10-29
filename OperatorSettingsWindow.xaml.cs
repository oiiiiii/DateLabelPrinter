using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DateLabelPrinter
{
    public partial class OperatorSettingsWindow : Window
    {
        // 按拼音（A-Z）比较人员姓名
        private static readonly StringComparer NameComparer = StringComparer.Create(new CultureInfo("zh-CN"), true);

        private readonly ObservableCollection<string> _names;

        public OperatorSettingsWindow(ObservableCollection<string> names)
        {
            InitializeComponent();
            _names = names;
            NamesListBox.ItemsSource = _names;
            NamesInputTextBox.Focus();
        }

        // 添加多行文本框中输入的名单（每行一个姓名），实时同步到开启人下拉列表
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string[] lines = NamesInputTextBox.Text.Split('\n');

            var newNames = new List<string>();
            foreach (string line in lines)
            {
                string name = line.Trim();
                if (!string.IsNullOrEmpty(name) && !_names.Contains(name) && !newNames.Contains(name))
                {
                    newNames.Add(name);
                }
            }

            if (newNames.Count == 0)
            {
                MessageBox.Show("请输入要添加的人员姓名（每行一个）！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (string name in newNames)
            {
                InsertSorted(name);
            }

            NamesInputTextBox.Text = "";
            NamesInputTextBox.Focus();
        }

        // 将姓名按拼音 A-Z 顺序插入到名单中
        private void InsertSorted(string name)
        {
            int index = 0;
            while (index < _names.Count && NameComparer.Compare(_names[index], name) < 0)
            {
                index++;
            }
            _names.Insert(index, name);
        }

        // 输入框中按 Ctrl+Enter 直接添加
        private void NamesInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                AddButton_Click(sender, e);
            }
        }

        // 删除选中人员
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (NamesListBox.SelectedItem is string selected)
            {
                _names.Remove(selected);
            }
            else
            {
                MessageBox.Show("请先在列表中选中要删除的人员！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // 关闭窗口（修改实时生效，主窗口会同步保存到 config.xml）
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
