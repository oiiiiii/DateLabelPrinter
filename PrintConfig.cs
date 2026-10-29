using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DateLabelPrinter
{
    // 有效期枚举
    public enum ValidityType
    {
        OneDay = 1,
        ThreeDays = 3,
        SevenDays = 7,
        ThirtyDays = 30,
        NinetyDays = 90,
        Custom
    }

    public class PrintConfig : INotifyPropertyChanged
    {
        // 开启时间（默认当前日期，08:00）
        private DateTime _startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 0, 0);
        public DateTime StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(); UpdateExpireTime(); }
        }

        // 有效期类型
        private ValidityType _validityType = ValidityType.Custom;
        public ValidityType ValidityType
        {
            get => _validityType;
            set { _validityType = value; OnPropertyChanged(); UpdateExpireTime(); }
        }

        // 过期时间（默认开启时间+1天，08:00）
        private DateTime _expireTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 0, 0).AddDays(1);
        public DateTime ExpireTime
        {
            get => _expireTime;
            set { _expireTime = value; OnPropertyChanged(); }
        }

        // 开启人（默认为空，通过启动弹窗或设置对话框添加）
        private string _operator = "";
        public string Operator
        {
            get => _operator;
            set { _operator = value; OnPropertyChanged(); }
        }

        // 开启人名单（默认为空，添加后保存到 config.xml）
        private ObservableCollection<string> _operatorList = new ObservableCollection<string>();
        public ObservableCollection<string> OperatorList
        {
            get => _operatorList;
            set { _operatorList = value; OnPropertyChanged(); }
        }

        // 打印内容模板（默认值）
        private string _printContentTemplate = "开启时间：{{开启时间}}\n过期时间：{{过期时间}}\n开启人：{{开启人}}";
        public string PrintContentTemplate
        {
            get => _printContentTemplate;
            set { _printContentTemplate = value; OnPropertyChanged(); }
        }

        // 最终渲染后的打印内容
        public string RenderedPrintContent
        {
            get
            {
                string content = PrintContentTemplate
                    .Replace("{{开启时间}}", StartTime.ToString("yyyy.MM.dd HH:mm"))
                    .Replace("{{过期时间}}", ExpireTime.ToString("yyyy.MM.dd HH:mm"));

                content = content.Replace("{{开启人}}", Operator);

                return content;
            }
        }

        // 打印机名称
        private string _printerName;
        public string PrinterName
        {
            get => _printerName;
            set { _printerName = value; OnPropertyChanged(); }
        }

        // 打印份数（默认1）
        private int _copyCount = 1;
        public int CopyCount
        {
            get => _copyCount;
            set { _copyCount = value; OnPropertyChanged(); }
        }

        // 字体（默认黑体）
        private string _fontFamily = "黑体";
        public string FontFamily
        {
            get => _fontFamily;
            set { _fontFamily = value; OnPropertyChanged(); }
        }

        // 字号（默认14）
        private int _fontSize = 14;
        public int FontSize
        {
            get => _fontSize;
            set { _fontSize = value; OnPropertyChanged(); }
        }

        // 行间距（默认1.5）
        private double _lineSpacing = 1.5;
        public double LineSpacing
        {
            get => _lineSpacing;
            set { _lineSpacing = value; OnPropertyChanged(); }
        }

        // 页边距（默认5）
        private double _marginLeft = 5;
        public double MarginLeft
        {
            get => _marginLeft;
            set { _marginLeft = value; OnPropertyChanged(); }
        }

        private double _marginTop = 15;
        public double MarginTop
        {
            get => _marginTop;
            set { _marginTop = value; OnPropertyChanged(); }
        }

        private double _marginRight = 5;
        public double MarginRight
        {
            get => _marginRight;
            set { _marginRight = value; OnPropertyChanged(); }
        }

        private double _marginBottom = 5;
        public double MarginBottom
        {
            get => _marginBottom;
            set { _marginBottom = value; OnPropertyChanged(); }
        }

        // 自动更新过期时间（根据有效期类型）
        private void UpdateExpireTime()
        {
            if (ValidityType != ValidityType.Custom)
            {
                ExpireTime = StartTime.AddDays((int)ValidityType);
            }
        }

        // 属性变更通知
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // 触发渲染内容更新
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RenderedPrintContent)));
        }
    }
}