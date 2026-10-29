# 日期贴打印工具（DateLabelPrinter）

一个 Windows 桌面小工具，用于打印食品/物品的开启日期贴。选择开启时间与有效期后自动计算过期时间，生成 56mm 宽热敏纸格式的打印内容，支持打印预览、自定义模板与人员名单管理。

## 功能特性

- **有效期快捷选项**：1 天 / 3 天 / 7 天 / 30 天 / 90 天单选，或输入自定义天数（回车或移开焦点自动计算过期时间并刷新预览）
- **过期时间自动计算**：过期时间 = 开启时间 + 有效天数，也可手动编辑过期日期/时间
- **开启人名单管理**：点击"人员设置"标题打开管理窗口，支持多行文本框批量粘贴添加（每行一个姓名），名单按拼音 A-Z 自动排序；首次使用名单为空时会自动弹出添加窗口
- **打印内容模板**：支持 `{{开启时间}}`、`{{过期时间}}`、`{{开启人}}` 占位符，可自由编辑排版
- **打印预览**：右侧实时预览，配置变化后自动刷新（防抖）
- **打印设置**：打印机选择、字体/字号/行间距、四边页边距、打印份数；页面宽度固定 56mm，高度按内容自适应
- **配置持久化**：所有设置保存在程序目录的 `config.xml`，下次启动自动恢复；写入失败时会弹窗提示原因

## 环境要求

- Windows 10/11
- [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)（运行已发布的便携版时需要）
- 开发/编译需要 .NET 9 SDK

## 从源码运行

```bash
dotnet run
```

## 编译发布

不包含 .NET 运行时的便携版（目标机器需安装 .NET 9 Desktop Runtime）：

```bash
dotnet publish DateLabelPrinter.csproj -c Release -r win-x64 --self-contained false -o publish
```

包含 .NET 运行时的自包含版（目标机器无需安装任何运行时）：

```bash
dotnet publish DateLabelPrinter.csproj -c Release -r win-x64 --self-contained true -o publish
```

发布前请注意：`dotnet publish` 会清空输出目录，不要把重要文件放在 `publish/` 里。

## 使用说明

1. 选择开启时间（默认当天 08:00）和有效期
2. 点击"人员设置"标题维护开启人名单（首次使用必须先添加），然后选择开启人
3. 按需编辑打印内容模板、字体、边距等
4. 点击"预览"确认效果，点击"打印"输出（打印在后台线程执行，不卡界面）

## 项目结构

```
DateLabelPrinter/
├── App.xaml / App.xaml.cs              # 应用入口，全局异常处理
├── MainWindow.xaml / MainWindow.xaml.cs # 主窗口：配置、预览、打印
├── OperatorSettingsWindow.xaml/.cs      # 开启人名单管理窗口
├── PrintConfig.cs                       # 配置模型与打印内容渲染
└── Properties/logo.ico                  # 程序图标
```

## 许可证

仅供内部使用。
