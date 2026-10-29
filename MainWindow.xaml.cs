using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Drawing.Printing;

namespace DateLabelPrinter
{
    public partial class MainWindow : Window
    {
        private PrintConfig _printConfig = new PrintConfig();

        public MainWindow()
        {
            InitializeComponent();
            // 数据绑定
            DataContext = _printConfig;
            // 初始化打印机下拉框
            InitPrinters();
            // 初始化有效期单选框
            InitValidityRadioButtons();
            // 加载保存的设置
            LoadSettings();
            // 配置变化时自动刷新预览（防抖）
            InitPreviewAutoRefresh();
            // 窗口加载完成后再弹出添加人员窗口（不要在构造函数里 ShowDialog）
            Loaded += MainWindow_Loaded;
            // 初始生成预览
            PreviewButton_Click(null, null);
        }

        // 窗口加载完成
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 名单为空时弹出添加人员窗口
            if (_printConfig.OperatorList.Count == 0)
            {
                OperatorSettings_Click(null, null);
            }
        }

        // 加载保存的设置
        private void LoadSettings()
        {
            try
            {
                string configPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.xml");
                if (System.IO.File.Exists(configPath))
                {
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    doc.Load(configPath);
                    
                    // 加载开启人
                    System.Xml.XmlNode operatorNode = doc.SelectSingleNode("/Config/Operator");
                    if (operatorNode != null && !string.IsNullOrEmpty(operatorNode.InnerText))
                    {
                        _printConfig.Operator = operatorNode.InnerText;
                    }

                    // 加载开启人名单（旧版配置无此节点时保留默认名单）
                    System.Xml.XmlNodeList operatorListNodes = doc.SelectNodes("/Config/Operators/Operator");
                    if (operatorListNodes != null && operatorListNodes.Count > 0)
                    {
                        var names = operatorListNodes.Cast<System.Xml.XmlNode>()
                            .Select(n => n.InnerText)
                            .Where(n => !string.IsNullOrEmpty(n))
                            .Distinct()
                            .OrderBy(n => n, StringComparer.Create(new System.Globalization.CultureInfo("zh-CN"), true))
                            .ToList();
                        _printConfig.OperatorList.Clear();
                        foreach (string name in names)
                        {
                            _printConfig.OperatorList.Add(name);
                        }
                    }

                    // 加载打印机
                    System.Xml.XmlNode printerNode = doc.SelectSingleNode("/Config/PrinterName");
                    if (printerNode != null && !string.IsNullOrEmpty(printerNode.InnerText))
                    {
                        _printConfig.PrinterName = printerNode.InnerText;
                        // 选择对应的打印机
                        for (int i = 0; i < PrinterComboBox.Items.Count; i++)
                        {
                            if (PrinterComboBox.Items[i].ToString() == printerNode.InnerText)
                            {
                                PrinterComboBox.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                    
                    // 加载字体
                    System.Xml.XmlNode fontFamilyNode = doc.SelectSingleNode("/Config/FontFamily");
                    if (fontFamilyNode != null && !string.IsNullOrEmpty(fontFamilyNode.InnerText))
                    {
                        _printConfig.FontFamily = fontFamilyNode.InnerText;
                    }
                    
                    // 加载字号
                    System.Xml.XmlNode fontSizeNode = doc.SelectSingleNode("/Config/FontSize");
                    if (fontSizeNode != null && int.TryParse(fontSizeNode.InnerText, out int fontSize))
                    {
                        _printConfig.FontSize = fontSize;
                    }
                    
                    // 加载行间距
                    System.Xml.XmlNode lineSpacingNode = doc.SelectSingleNode("/Config/LineSpacing");
                    if (lineSpacingNode != null && double.TryParse(lineSpacingNode.InnerText, out double lineSpacing))
                    {
                        _printConfig.LineSpacing = lineSpacing;
                    }
                    
                    // 加载页边距
                    System.Xml.XmlNode marginLeftNode = doc.SelectSingleNode("/Config/MarginLeft");
                    if (marginLeftNode != null && double.TryParse(marginLeftNode.InnerText, out double marginLeft))
                    {
                        _printConfig.MarginLeft = marginLeft;
                    }
                    
                    System.Xml.XmlNode marginTopNode = doc.SelectSingleNode("/Config/MarginTop");
                    if (marginTopNode != null && double.TryParse(marginTopNode.InnerText, out double marginTop))
                    {
                        _printConfig.MarginTop = marginTop;
                    }
                    
                    System.Xml.XmlNode marginRightNode = doc.SelectSingleNode("/Config/MarginRight");
                    if (marginRightNode != null && double.TryParse(marginRightNode.InnerText, out double marginRight))
                    {
                        _printConfig.MarginRight = marginRight;
                    }
                    
                    System.Xml.XmlNode marginBottomNode = doc.SelectSingleNode("/Config/MarginBottom");
                    if (marginBottomNode != null && double.TryParse(marginBottomNode.InnerText, out double marginBottom))
                    {
                        _printConfig.MarginBottom = marginBottom;
                    }
                }
            }
            catch (Exception ex)
            {
                // 配置损坏时提示（不弹窗，避免影响启动），状态栏保留最后一条异常
                ShowStatus($"加载配置文件失败：{ex.Message}", Brushes.Red);
            }
        }

        // 保存设置
        private void SaveSettings()
        {
            string configPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.xml");
            try
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                
                // 创建根节点
                System.Xml.XmlElement root = doc.CreateElement("Config");
                doc.AppendChild(root);
                
                // 添加开启人
                System.Xml.XmlElement operatorElement = doc.CreateElement("Operator");
                operatorElement.InnerText = _printConfig.Operator;
                root.AppendChild(operatorElement);

                // 添加开启人名单
                System.Xml.XmlElement operatorsElement = doc.CreateElement("Operators");
                foreach (string name in _printConfig.OperatorList)
                {
                    System.Xml.XmlElement nameElement = doc.CreateElement("Operator");
                    nameElement.InnerText = name;
                    operatorsElement.AppendChild(nameElement);
                }
                root.AppendChild(operatorsElement);

                // 添加打印机
                System.Xml.XmlElement printerElement = doc.CreateElement("PrinterName");
                printerElement.InnerText = _printConfig.PrinterName;
                root.AppendChild(printerElement);
                
                // 添加字体
                System.Xml.XmlElement fontFamilyElement = doc.CreateElement("FontFamily");
                fontFamilyElement.InnerText = _printConfig.FontFamily;
                root.AppendChild(fontFamilyElement);
                
                // 添加字号
                System.Xml.XmlElement fontSizeElement = doc.CreateElement("FontSize");
                fontSizeElement.InnerText = _printConfig.FontSize.ToString();
                root.AppendChild(fontSizeElement);
                
                // 添加行间距
                System.Xml.XmlElement lineSpacingElement = doc.CreateElement("LineSpacing");
                lineSpacingElement.InnerText = _printConfig.LineSpacing.ToString();
                root.AppendChild(lineSpacingElement);
                
                // 添加页边距
                System.Xml.XmlElement marginLeftElement = doc.CreateElement("MarginLeft");
                marginLeftElement.InnerText = _printConfig.MarginLeft.ToString();
                root.AppendChild(marginLeftElement);
                
                System.Xml.XmlElement marginTopElement = doc.CreateElement("MarginTop");
                marginTopElement.InnerText = _printConfig.MarginTop.ToString();
                root.AppendChild(marginTopElement);
                
                System.Xml.XmlElement marginRightElement = doc.CreateElement("MarginRight");
                marginRightElement.InnerText = _printConfig.MarginRight.ToString();
                root.AppendChild(marginRightElement);
                
                System.Xml.XmlElement marginBottomElement = doc.CreateElement("MarginBottom");
                marginBottomElement.InnerText = _printConfig.MarginBottom.ToString();
                root.AppendChild(marginBottomElement);
                
                // 保存文件
                doc.Save(configPath);
            }
            catch (Exception ex)
            {
                // 无写入权限等原因导致保存失败时，明确告知用户原因
                ShowError("保存设置失败", $"无法写入配置文件：{configPath}\n\n原因：{ex.Message}\n\n请检查程序所在目录的写入权限" +
                    "（程序不要放在 Program Files 等受保护目录，或将整个程序目录移到有写权限的位置）。");
            }
        }

        // 窗口关闭时保存设置
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            base.OnClosing(e);
        }

        // 点击"人员设置"标题打开开启人设置对话框
        private void OperatorSettings_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e != null && e.ChangedButton != System.Windows.Input.MouseButton.Left) return;
            var dialog = new OperatorSettingsWindow(_printConfig.OperatorList) { Owner = this };
            dialog.ShowDialog();
            // 名单可能被修改，立即持久化
            SaveSettings();
        }

        #region 初始化方法
        // 枚举系统打印机并填充下拉框
        private void InitPrinters()
        {
            PrinterComboBox.Items.Clear();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                PrinterComboBox.Items.Add(printer);
            }
            // 默认选中第一个打印机
            if (PrinterComboBox.Items.Count > 0)
            {
                PrinterComboBox.SelectedIndex = 0;
                _printConfig.PrinterName = PrinterComboBox.SelectedItem.ToString();
            }
        }

        // 初始化有效期单选框
        private void InitValidityRadioButtons()
        {
            // 绑定单选框与有效期枚举（自定义单选框嵌套在子面板中，单独用名称引用）
            IEnumerable<RadioButton> radios = ValidityRadioGroup.Children.OfType<RadioButton>().Concat(new[] { CustomRadioButton });
            foreach (RadioButton rb in radios)
            {
                rb.Checked += (s, e) =>
                {
                    if (Enum.TryParse<ValidityType>(rb.Tag.ToString(), out var type))
                    {
                        _printConfig.ValidityType = type;
                    }
                };
            }
            // 默认选中自定义
            CustomRadioButton.IsChecked = true;
        }
        #endregion

        #region 输入处理方法
        private void DateTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            string input = textBox.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            // 移除所有非数字字符
            string numbersOnly = new string(input.Where(char.IsDigit).ToArray());

            if (numbersOnly.Length == 8) // 格式：20260201
            {
                try
                {
                    int year = int.Parse(numbersOnly.Substring(0, 4));
                    int month = int.Parse(numbersOnly.Substring(4, 2));
                    int day = int.Parse(numbersOnly.Substring(6, 2));

                    DateTime newDate = new DateTime(year, month, day);

                    // 确定是修改开启时间还是过期时间
                    // 通过查看文本框的位置来判断
                    // 开启时间的日期输入框在第0行第1列
                    // 过期时间的日期输入框在第1行第1列
                    var startDateTextBox = FindName("StartDateTextBox") as TextBox;
                    var expireDateTextBox = FindName("ExpireDateTextBox") as TextBox;

                    if (textBox == startDateTextBox)
                    {
                        DateTime current = _printConfig.StartTime;
                        _printConfig.StartTime = new DateTime(newDate.Year, newDate.Month, newDate.Day, current.Hour, current.Minute, current.Second);
                    }
                    else if (textBox == expireDateTextBox)
                    {
                        DateTime current = _printConfig.ExpireTime;
                        _printConfig.ExpireTime = new DateTime(newDate.Year, newDate.Month, newDate.Day, current.Hour, current.Minute, current.Second);
                    }
                }
                catch { }
            }
        }

        private void TimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            string input = textBox.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            // 移除所有非数字字符
            string numbersOnly = new string(input.Where(char.IsDigit).ToArray());

            if (numbersOnly.Length == 4) // 格式：0505
            {
                try
                {
                    int hour = int.Parse(numbersOnly.Substring(0, 2));
                    int minute = int.Parse(numbersOnly.Substring(2, 2));

                    if (hour >= 0 && hour < 24 && minute >= 0 && minute < 60)
                    {
                        // 确定是修改开启时间还是过期时间
                        var startTimeTextBox = FindName("StartTimeTextBox") as TextBox;
                        var expireTimeTextBox = FindName("ExpireTimeTextBox") as TextBox;

                        if (textBox == startTimeTextBox)
                        {
                            DateTime current = _printConfig.StartTime;
                            _printConfig.StartTime = new DateTime(current.Year, current.Month, current.Day, hour, minute, current.Second);
                        }
                        else if (textBox == expireTimeTextBox)
                        {
                            DateTime current = _printConfig.ExpireTime;
                            _printConfig.ExpireTime = new DateTime(current.Year, current.Month, current.Day, hour, minute, current.Second);
                        }
                    }
                }
                catch { }
            }
        }

        // 自定义有效期天数：只允许输入数字
        private void CustomDaysTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            string digitsOnly = new string(textBox.Text.Where(char.IsDigit).ToArray());
            if (textBox.Text != digitsOnly)
            {
                int caretIndex = textBox.CaretIndex;
                textBox.Text = digitsOnly;
                textBox.CaretIndex = Math.Min(caretIndex, digitsOnly.Length);
            }
        }

        // 自定义有效期天数：按回车提交
        private void CustomDaysTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                CommitCustomDays();
            }
        }

        // 自定义有效期天数：失去焦点时提交
        private void CustomDaysTextBox_Commit(object sender, RoutedEventArgs e)
        {
            CommitCustomDays();
        }

        // 根据自定义天数计算过期时间并刷新预览
        private void CommitCustomDays()
        {
            // 窗口关闭/未加载时跳过（失焦事件在关闭过程中也会触发）
            if (CustomDaysTextBox == null || !IsLoaded) return;

            string input = CustomDaysTextBox.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            if (!int.TryParse(input, out int days) || days <= 0)
            {
                ShowStatus("自定义天数必须为大于0的整数！", Brushes.Red);
                return;
            }

            // 选中自定义单选框（Checked 处理器对 Custom 不改过期时间，不影响后面的计算）
            if (CustomRadioButton.IsChecked != true)
            {
                CustomRadioButton.IsChecked = true;
            }

            // 根据天数计算过期时间（过期时间变化会触发 PropertyChanged 自动刷新预览）
            _printConfig.ExpireTime = _printConfig.StartTime.AddDays(days);

            // 立即刷新预览并停掉防抖计时器，避免重复生成
            _previewRefreshTimer.Stop();
            GeneratePreview(true);
        }

        // 打印份数输入校验：非法输入时提示
        private void CopyCountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            string input = textBox.Text.Trim();
            if (input.Length == 0) return; // 尚未输入完成，不提示
            if (!int.TryParse(input, out int copyCount) || copyCount < 1)
            {
                ShowStatus("打印份数必须为大于0的整数！", Brushes.Red);
            }
        }
        #endregion

        #region 打印核心方法
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 验证打印份数
                if (_printConfig.CopyCount < 1)
                {
                    ShowStatus("打印份数必须大于等于1！", Brushes.Red);
                    return;
                }

                // 占位符校验：开启人为空或存在未识别的占位符时不打印
                string printContent = _printConfig.RenderedPrintContent;
                if (_printConfig.PrintContentTemplate.Contains("{{开启人}}") && string.IsNullOrWhiteSpace(_printConfig.Operator))
                {
                    ShowStatus("开启人为空，请点击\"人员设置\"选择或添加开启人！", Brushes.Red);
                    return;
                }
                if (printContent.Contains("{{"))
                {
                    ShowStatus("打印内容中存在未替换的占位符，请检查打印内容模板！", Brushes.Red);
                    return;
                }

                // 在 UI 线程采集打印参数快照，供后台线程使用
                var job = new PrintJob
                {
                    PrinterName = _printConfig.PrinterName,
                    Content = printContent,
                    FontFamily = _printConfig.FontFamily,
                    FontSize = _printConfig.FontSize,
                    LineSpacing = _printConfig.LineSpacing,
                    MarginLeft = _printConfig.MarginLeft,
                    MarginTop = _printConfig.MarginTop,
                    MarginRight = _printConfig.MarginRight,
                    MarginBottom = _printConfig.MarginBottom,
                    CopyCount = _printConfig.CopyCount,
                };

                // 在独立 STA 后台线程执行打印，避免打印机响应慢时阻塞 UI
                System.Threading.Thread printThread = new System.Threading.Thread(PrintWorker);
                printThread.SetApartmentState(System.Threading.ApartmentState.STA);
                printThread.IsBackground = true;
                printThread.Start(job);

                // 打印期间禁用打印按钮
                PrintButton.IsEnabled = false;
                PrintButton.Content = "打印中...";
            }
            catch (Exception ex)
            {
                ShowError("打印失败", $"打印失败：{ex.Message}");
            }
        }

        // 打印参数快照（在 UI 线程采集）
        private class PrintJob
        {
            public string PrinterName;
            public string Content;
            public string FontFamily;
            public int FontSize;
            public double LineSpacing;
            public double MarginLeft;
            public double MarginTop;
            public double MarginRight;
            public double MarginBottom;
            public int CopyCount;
        }

        // 后台打印线程入口（STA）
        private void PrintWorker(object state)
        {
            PrintJob job = (PrintJob)state;
            try
            {
                // 创建打印对话框
                PrintDialog printDialog = new PrintDialog();
                // 设置打印机
                if (!string.IsNullOrEmpty(job.PrinterName))
                {
                    try
                    {
                        string printerName = job.PrinterName;
                        
                        // 尝试1: 不设置PrintQueue，让PrintDialog使用默认打印机
                        // 然后检查是否是我们想要的打印机
                        if (printDialog.PrintQueue != null && 
                            (printDialog.PrintQueue.Name.Equals(printerName, StringComparison.OrdinalIgnoreCase) ||
                             printDialog.PrintQueue.FullName.Equals(printerName, StringComparison.OrdinalIgnoreCase)))
                        {
                            // 默认打印机就是我们想要的，直接使用
                        }
                        else
                        {
                            // 尝试2: 枚举所有可用的打印机队列并查找匹配项
                            try
                            {
                                System.Printing.PrintServer printServer = new System.Printing.PrintServer();
                                var printQueues = printServer.GetPrintQueues(
                                    new[] { System.Printing.EnumeratedPrintQueueTypes.Local, System.Printing.EnumeratedPrintQueueTypes.Connections }
                                );
                                
                                // 查找匹配的打印机（更宽松的匹配规则）
                                var targetQueue = printQueues.FirstOrDefault(pq => 
                                    pq.FullName.Equals(printerName, StringComparison.OrdinalIgnoreCase) ||
                                    pq.Name.Equals(printerName, StringComparison.OrdinalIgnoreCase) ||
                                    pq.FullName.Contains(printerName, StringComparison.OrdinalIgnoreCase) ||
                                    pq.Name.Contains(printerName, StringComparison.OrdinalIgnoreCase)
                                );
                                
                                if (targetQueue != null)
                                {
                                    printDialog.PrintQueue = targetQueue;
                                }
                                else
                                {
                                    // 尝试3: 使用System.Drawing.Printing.PrinterSettings获取所有打印机
                                    // 并设置默认打印机为我们想要的打印机
                                    try
                                    {
                                        System.Drawing.Printing.PrinterSettings printerSettings = new System.Drawing.Printing.PrinterSettings();
                                        printerSettings.PrinterName = printerName;
                                        
                                        if (printerSettings.IsValid)
                                        {
                                            // 打印机有效，使用它
                                            // 这里我们不直接设置PrintQueue，而是让PrintDialog使用默认打印机
                                            // 因为System.Drawing和System.Printing的API不同
                                            // 但至少我们确认了打印机存在
                                        }
                                        else
                                        {
                                            throw new Exception($"找不到打印机: {printerName}");
                                        }
                                    }
                                    catch (Exception innerEx)
                                    {
                                        throw new Exception($"设置打印机失败: {innerEx.Message}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                // 如果所有尝试都失败，使用默认打印机
                                // 至少可以保证打印功能正常工作
                                Dispatcher.Invoke(() => ShowStatus($"无法连接到指定打印机，将使用默认打印机: {ex.Message}", Brushes.Yellow));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 如果设置打印机失败，仍然继续打印，使用默认打印机
                        Dispatcher.Invoke(() => ShowStatus($"设置打印机失败，将使用默认打印机: {ex.Message}", Brushes.Yellow));
                    }
                }

                // 设置纸张宽度（固定56mm），高度根据内容动态计算
                var pageSizeInInchWidth = 56 * 0.03937;

                // 创建打印文档（FlowDocument）
                FlowDocument doc = new FlowDocument
                {
                    // 页面宽度固定，高度暂时设为较大值以容纳内容
                    PageWidth = pageSizeInInchWidth * 96,
                    PageHeight = 1000, // 足够大的高度
                    PagePadding = new Thickness(job.MarginLeft, job.MarginTop, job.MarginRight, job.MarginBottom),
                };

                // 添加打印内容（和预览区样式完全一致）
                Paragraph para = new Paragraph
                {
                    TextAlignment = TextAlignment.Center,
                    FontFamily = new FontFamily(job.FontFamily),
                    FontSize = job.FontSize, // 使用配置的字号
                    LineHeight = job.FontSize * job.LineSpacing // 使用配置的行间距（与预览保持一致）
                };
                para.Inlines.Add(job.Content);
                doc.Blocks.Add(para);

                // 计算内容高度（根据行数和字体大小），内容只计算一次
                string[] lines = job.Content.Split(new[] { '\n' }, StringSplitOptions.None);
                int lineCount = lines.Length;

                // 估算每行高度（字体大小 * 行高因子）
                double estimatedLineHeight = job.FontSize * job.LineSpacing;

                // 计算总内容高度（包括边距）
                double totalHeight = lineCount * estimatedLineHeight + job.MarginTop + job.MarginBottom + 10; // 额外10像素安全边距

                // 转换为英寸
                double pageSizeInInchHeight = Math.Max(totalHeight / 96, 0.5); // 最小0.5英寸（约12.7mm）

                // 创建页面尺寸
                var pageMediaSize = new System.Printing.PageMediaSize(
                    pageSizeInInchWidth,
                    pageSizeInInchHeight);
                printDialog.PrintTicket.PageMediaSize = pageMediaSize;

                // 设置页面方向为纵向（热敏纸通常纵向）
                printDialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Portrait;

                // 设置打印份数
                printDialog.PrintTicket.CopyCount = job.CopyCount;

                // 更新FlowDocument的页面高度以匹配计算值
                doc.PageHeight = pageSizeInInchHeight * 96;

                // 直接执行打印，不弹出系统打印机选择弹窗
                printDialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "日期贴打印");
                Dispatcher.Invoke(() =>
                {
                    RestorePrintButton();
                    ShowStatus("打印成功！", Brushes.Green);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    RestorePrintButton();
                    ShowError("打印失败", $"打印失败：{ex.Message}");
                });
            }
        }

        // 打印结束后恢复打印按钮（必须在 UI 线程调用）
        private void RestorePrintButton()
        {
            PrintButton.IsEnabled = true;
            PrintButton.Content = "打印";
        }

        private DispatcherTimer _statusClearTimer;

        private void ShowStatus(string message, Brush foreground)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = foreground;

            if (_statusClearTimer == null)
            {
                _statusClearTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                _statusClearTimer.Tick += (s, e) =>
                {
                    _statusClearTimer.Stop();
                    StatusTextBlock.Text = string.Empty;
                };
            }

            _statusClearTimer.Stop();
            // 错误信息（红色）保留在状态栏，直到下一条消息出现；其余信息3秒后自动清除
            if (foreground != Brushes.Red)
            {
                _statusClearTimer.Start();
            }
        }

        // 异常处理：弹窗提示，并在状态栏保留最后一条异常
        private void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            ShowStatus(message.Replace("\n", " "), Brushes.Red);
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            GeneratePreview(true);
        }

        // 生成预览；showStatus 为 false 时不显示成功提示（用于自动刷新）
        private void GeneratePreview(bool showStatus)
        {
            try
            {
                // 内容只计算一次
                string previewContent = _printConfig.RenderedPrintContent;

                // 创建打印文档（FlowDocument），使用与打印完全相同的逻辑
                var pageSizeInInchWidth = 56 * 0.03937;

                // 创建FlowDocument
                FlowDocument doc = new FlowDocument
                {
                    // 页面宽度固定，高度暂时设为较大值以容纳内容
                    PageWidth = pageSizeInInchWidth * 96,
                    PageHeight = 1000, // 足够大的高度
                    PagePadding = new Thickness(_printConfig.MarginLeft, _printConfig.MarginTop, _printConfig.MarginRight, _printConfig.MarginBottom),
                };

                // 添加打印内容（与打印完全相同的样式）
                Paragraph para = new Paragraph
                {
                    TextAlignment = TextAlignment.Center,
                    FontFamily = new FontFamily(_printConfig.FontFamily),
                    FontSize = _printConfig.FontSize,
                    LineHeight = _printConfig.LineSpacing
                };
                para.Inlines.Add(previewContent);
                doc.Blocks.Add(para);

                // 计算内容高度（根据行数和字体大小）
                string[] lines = previewContent.Split(new[] { '\n' }, StringSplitOptions.None);
                int lineCount = lines.Length;

                // 估算每行高度（字体大小 * 行高因子）
                double estimatedLineHeight = _printConfig.FontSize * _printConfig.LineSpacing;

                // 计算总内容高度（包括边距）
                double totalHeight = lineCount * estimatedLineHeight + _printConfig.MarginTop + _printConfig.MarginBottom + 10; // 额外10像素安全边距

                // 转换为英寸
                double pageSizeInInchHeight = Math.Max(totalHeight / 96, 0.5); // 最小0.5英寸（约12.7mm）

                // 更新FlowDocument的页面高度以匹配计算值
                doc.PageHeight = pageSizeInInchHeight * 96;

                // 将FlowDocument转换为FixedDocument
                FixedDocument fixedDoc = ConvertFlowDocumentToFixedDocument(doc, previewContent);

                // 在右侧的DocumentViewer中显示预览
                PreviewDocumentViewer.Document = fixedDoc;
                // 调整缩放级别，使页面显示更大（适应宽度）
                PreviewDocumentViewer.FitToWidth();
                if (showStatus)
                {
                    ShowStatus("预览生成成功！", Brushes.Green);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"预览失败：{ex.Message}", Brushes.Red);
            }
        }

        #region 预览自动刷新
        private DispatcherTimer _previewRefreshTimer;

        // 监听配置变化，防抖后自动刷新预览
        private void InitPreviewAutoRefresh()
        {
            _previewRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _previewRefreshTimer.Tick += (s, e) =>
            {
                _previewRefreshTimer.Stop();
                GeneratePreview(false);
            };
            _printConfig.PropertyChanged += (s, e) =>
            {
                // 窗口未加载完成（如启动时加载配置）或尚未生成初始预览时不刷新
                if (!IsLoaded || PreviewDocumentViewer == null) return;
                _previewRefreshTimer.Stop();
                _previewRefreshTimer.Start();
            };
        }
        #endregion

        private FixedDocument ConvertFlowDocumentToFixedDocument(FlowDocument flowDocument, string content)
        {
            FixedDocument fixedDocument = new FixedDocument();

            // 创建一个页面内容
            FixedPage fixedPage = new FixedPage
            {
                Width = flowDocument.PageWidth,
                Height = flowDocument.PageHeight
            };

            // 创建TextBlock并设置与FlowDocument相同的内容和样式
            TextBlock textBlock = new TextBlock
            {
                Text = content,
                FontFamily = new FontFamily(_printConfig.FontFamily),
                FontSize = _printConfig.FontSize,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = _printConfig.FontSize * _printConfig.LineSpacing, // 使用配置的行间距
                Margin = new Thickness(_printConfig.MarginLeft, _printConfig.MarginTop, _printConfig.MarginRight, _printConfig.MarginBottom),
                Width = flowDocument.PageWidth - (_printConfig.MarginLeft + _printConfig.MarginRight) // 设置宽度为页面宽度减去左右边距
            };

            // 将TextBlock添加到FixedPage
            fixedPage.Children.Add(textBlock);

            // 创建页面内容并添加到FixedDocument
            PageContent pageContent = new PageContent();
            ((IAddChild)pageContent).AddChild(fixedPage);
            fixedDocument.Pages.Add(pageContent);

            return fixedDocument;
        }
        #endregion
    }
}