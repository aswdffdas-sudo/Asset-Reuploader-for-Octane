using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.Windows.Threading;

[assembly: AssemblyTitle("Octane Asset Reuploader Launcher")]
[assembly: AssemblyDescription("Setup and launcher utility for Octane 2021 Studio Asset Reuploader")]
[assembly: AssemblyCompany("Octane Revival Tools")]
[assembly: AssemblyProduct("Octane Asset Reuploader")]
[assembly: AssemblyVersion("1.4.2.0")]
[assembly: AssemblyFileVersion("1.4.2.0")]

namespace OctaneReuploaderSetup
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application app = new Application();
            MainWindow win = new MainWindow();
            app.Run(win);
        }
    }

    public class MainWindow : Window
    {
        // Neutral Zinc / Carbon Palette
        private static readonly Color CWindowBg = Color.FromRgb(18, 18, 20);       // #121214
        private static readonly Color CSurface = Color.FromRgb(22, 22, 24);        // #161618
        private static readonly Color CCard = Color.FromRgb(27, 27, 30);           // #1B1B1E
        private static readonly Color CCardSubtle = Color.FromRgb(23, 23, 26);     // #17171A
        private static readonly Color CCodeBg = Color.FromRgb(14, 14, 16);         // #0E0E10
        private static readonly Color CBorder = Color.FromRgb(39, 39, 43);         // #27272B
        private static readonly Color CBorderSubtle = Color.FromRgb(32, 32, 36);   // #202024
        private static readonly Color CTextPrimary = Color.FromRgb(244, 244, 246); // #F4F4F6
        private static readonly Color CTextBody = Color.FromRgb(200, 200, 206);    // #C8C8CE
        private static readonly Color CTextMuted = Color.FromRgb(138, 138, 146);   // #8A8A92
        private static readonly Color CTextDim = Color.FromRgb(107, 107, 115);     // #6B6B73
        private static readonly Color CGreen = Color.FromRgb(52, 211, 153);        // #34D399
        private static readonly Color CAmber = Color.FromRgb(251, 191, 36);        // #FBBF24
        private static readonly Color CNeutralDot = Color.FromRgb(82, 82, 91);     // #52525B

        private static readonly SolidColorBrush BWindowBg = new SolidColorBrush(CWindowBg);
        private static readonly SolidColorBrush BSurface = new SolidColorBrush(CSurface);
        private static readonly SolidColorBrush BCard = new SolidColorBrush(CCard);
        private static readonly SolidColorBrush BCardSubtle = new SolidColorBrush(CCardSubtle);
        private static readonly SolidColorBrush BCodeBg = new SolidColorBrush(CCodeBg);
        private static readonly SolidColorBrush BBorder = new SolidColorBrush(CBorder);
        private static readonly SolidColorBrush BBorderSubtle = new SolidColorBrush(CBorderSubtle);
        private static readonly SolidColorBrush BTextPrimary = new SolidColorBrush(CTextPrimary);
        private static readonly SolidColorBrush BTextBody = new SolidColorBrush(CTextBody);
        private static readonly SolidColorBrush BTextMuted = new SolidColorBrush(CTextMuted);
        private static readonly SolidColorBrush BTextDim = new SolidColorBrush(CTextDim);
        private static readonly SolidColorBrush BGreen = new SolidColorBrush(CGreen);
        private static readonly SolidColorBrush BAmber = new SolidColorBrush(CAmber);
        private static readonly SolidColorBrush BNeutralDot = new SolidColorBrush(CNeutralDot);

        private string repoDir;
        private string pluginRbxmPath;
        private string pluginRbxmxPath;
        private string cookieFilePath;
        private string robloxCookieFilePath;
        private string configIniPath;
        private string backendExePath;
        private string pluginsFolder;

        private bool isPluginInstalled = false;
        private bool hasOctaneCookie = false;
        private bool hasRobloxCookie = false;
        private bool isBackendRunning = false;
        private int configuredPort = 38073;

        private int currentStep = 1;
        private List<Border> navRows = new List<Border>();
        private List<TextBlock> navCheckTexts = new List<TextBlock>();
        private List<Border> navCheckBoxes = new List<Border>();
        private List<UIElement> stepPages = new List<UIElement>();
        private ScrollViewer mainScroll;

        // Titlebar & Sidebar health indicators
        private Ellipse titleStatusDot;
        private TextBlock titleStatusText;
        private Ellipse sideDotPlugin;
        private TextBlock sideTxtPlugin;
        private Ellipse sideDotOctaneCookie;
        private TextBlock sideTxtOctaneCookie;
        private Ellipse sideDotRobloxCookie;
        private TextBlock sideTxtRobloxCookie;
        private Ellipse sideDotServer;
        private TextBlock sideTxtServer;

        // Page 1 Controls
        private TextBlock lblPluginStatusDetail;
        private Button btnInstallPlugin;

        // Page 2 Controls (Cookies)
        private TextBox txtOctaneCookie;
        private TextBox txtRobloxCookie;

        // Page 3 Controls (config.ini)
        private TextBox txtUserId;
        private TextBox txtGroupId;
        private TextBox txtDomain;
        private TextBox txtPort;

        // Footer & Toast
        private Button btnPrev;
        private Button btnNext;
        private TextBlock lblFooterStep;
        private Border toastBorder;
        private TextBlock toastLabel;
        private DispatcherTimer toastTimer;
        private DispatcherTimer pollTimer;

        public MainWindow()
        {
            ResolvePaths();

            this.Title = "Octane Asset Reuploader";
            this.Width = 900;
            this.Height = 600;
            this.MinWidth = 760;
            this.MinHeight = 500;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.CanResize;
            this.Background = BWindowBg;
            this.Foreground = BTextPrimary;
            this.FontFamily = new FontFamily("Segoe UI");

            WindowChrome chrome = new WindowChrome
            {
                CaptionHeight = 38,
                ResizeBorderThickness = new Thickness(5),
                GlassFrameThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                UseAeroCaptionButtons = false
            };
            WindowChrome.SetWindowChrome(this, chrome);

            LoadStyles();
            TryLoadIcon();
            BuildLayout();
            LoadConfigAndCookiesIntoUI();

            RefreshStatusAsync();
            pollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.0) };
            pollTimer.Tick += delegate { RefreshStatusAsync(); };
            pollTimer.Start();
        }

        private void LoadStyles()
        {
            string styleXaml = @"
<ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>

  <Style x:Key='ScrollBarThumb' TargetType='{x:Type Thumb}'>
    <Setter Property='OverridesDefaultStyle' Value='true'/>
    <Setter Property='IsTabStop' Value='false'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='{x:Type Thumb}'>
          <Border x:Name='thumbBd' Background='#2E2E34' CornerRadius='3' Margin='2,0,2,0'/>
          <ControlTemplate.Triggers>
            <Trigger Property='IsMouseOver' Value='true'>
              <Setter TargetName='thumbBd' Property='Background' Value='#45454E'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style TargetType='{x:Type ScrollBar}'>
    <Setter Property='OverridesDefaultStyle' Value='true'/>
    <Setter Property='Width' Value='8'/>
    <Setter Property='Background' Value='Transparent'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='{x:Type ScrollBar}'>
          <Grid Background='Transparent'>
            <Track x:Name='PART_Track' IsDirectionReversed='true'>
              <Track.Thumb>
                <Thumb Style='{StaticResource ScrollBarThumb}'/>
              </Track.Thumb>
            </Track>
          </Grid>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
    <Style.Triggers>
      <Trigger Property='Orientation' Value='Horizontal'>
        <Setter Property='Width' Value='Auto'/>
        <Setter Property='Height' Value='8'/>
        <Setter Property='Template'>
          <Setter.Value>
            <ControlTemplate TargetType='{x:Type ScrollBar}'>
              <Grid Background='Transparent'>
                <Track x:Name='PART_Track' IsDirectionReversed='false'>
                  <Track.Thumb>
                    <Thumb Style='{StaticResource ScrollBarThumb}'/>
                  </Track.Thumb>
                </Track>
              </Grid>
            </ControlTemplate>
          </Setter.Value>
        </Setter>
      </Trigger>
    </Style.Triggers>
  </Style>

  <Style x:Key='BtnPrimary' TargetType='{x:Type Button}'>
    <Setter Property='Background' Value='#F4F4F6'/>
    <Setter Property='Foreground' Value='#141416'/>
    <Setter Property='BorderBrush' Value='#F4F4F6'/>
    <Setter Property='BorderThickness' Value='1'/>
    <Setter Property='Padding' Value='13,6,13,6'/>
    <Setter Property='FontSize' Value='12'/>
    <Setter Property='FontWeight' Value='SemiBold'/>
    <Setter Property='Cursor' Value='Hand'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='{x:Type Button}'>
          <Border x:Name='bd' Background='{TemplateBinding Background}'
                  BorderBrush='{TemplateBinding BorderBrush}'
                  BorderThickness='{TemplateBinding BorderThickness}'
                  CornerRadius='6' Padding='{TemplateBinding Padding}'>
            <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
          </Border>
          <ControlTemplate.Triggers>
            <Trigger Property='IsMouseOver' Value='true'>
              <Setter TargetName='bd' Property='Background' Value='#FFFFFF'/>
            </Trigger>
            <Trigger Property='IsPressed' Value='true'>
              <Setter TargetName='bd' Property='Background' Value='#DCDCE0'/>
            </Trigger>
            <Trigger Property='IsEnabled' Value='false'>
              <Setter Property='Opacity' Value='0.45'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style x:Key='BtnSecondary' TargetType='{x:Type Button}'>
    <Setter Property='Background' Value='#222226'/>
    <Setter Property='Foreground' Value='#E4E4E8'/>
    <Setter Property='BorderBrush' Value='#303036'/>
    <Setter Property='BorderThickness' Value='1'/>
    <Setter Property='Padding' Value='12,6,12,6'/>
    <Setter Property='FontSize' Value='12'/>
    <Setter Property='FontWeight' Value='Medium'/>
    <Setter Property='Cursor' Value='Hand'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='{x:Type Button}'>
          <Border x:Name='bd' Background='{TemplateBinding Background}'
                  BorderBrush='{TemplateBinding BorderBrush}'
                  BorderThickness='{TemplateBinding BorderThickness}'
                  CornerRadius='6' Padding='{TemplateBinding Padding}'>
            <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
          </Border>
          <ControlTemplate.Triggers>
            <Trigger Property='IsMouseOver' Value='true'>
              <Setter TargetName='bd' Property='Background' Value='#2B2B30'/>
              <Setter TargetName='bd' Property='BorderBrush' Value='#3E3E46'/>
            </Trigger>
            <Trigger Property='IsPressed' Value='true'>
              <Setter TargetName='bd' Property='Background' Value='#1C1C20'/>
            </Trigger>
            <Trigger Property='IsEnabled' Value='false'>
              <Setter Property='Opacity' Value='0.4'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style x:Key='BtnSubtle' TargetType='{x:Type Button}'>
    <Setter Property='Background' Value='#1D1D21'/>
    <Setter Property='Foreground' Value='#A0A0A8'/>
    <Setter Property='BorderBrush' Value='#2B2B31'/>
    <Setter Property='BorderThickness' Value='1'/>
    <Setter Property='Padding' Value='9,4,9,4'/>
    <Setter Property='FontSize' Value='11'/>
    <Setter Property='FontWeight' Value='Medium'/>
    <Setter Property='Cursor' Value='Hand'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='{x:Type Button}'>
          <Border x:Name='bd' Background='{TemplateBinding Background}'
                  BorderBrush='{TemplateBinding BorderBrush}'
                  BorderThickness='{TemplateBinding BorderThickness}'
                  CornerRadius='5' Padding='{TemplateBinding Padding}'>
            <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
          </Border>
          <ControlTemplate.Triggers>
            <Trigger Property='IsMouseOver' Value='true'>
              <Setter TargetName='bd' Property='Background' Value='#27272C'/>
              <Setter Property='Foreground' Value='#F4F4F6'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style x:Key='BtnWinCtrl' TargetType='{x:Type Button}'>
    <Setter Property='Background' Value='Transparent'/>
    <Setter Property='Foreground' Value='#8A8A92'/>
    <Setter Property='BorderThickness' Value='0'/>
    <Setter Property='Width' Value='42'/>
    <Setter Property='Height' Value='38'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='{x:Type Button}'>
          <Border x:Name='bd' Background='{TemplateBinding Background}'>
            <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
          </Border>
          <ControlTemplate.Triggers>
            <Trigger Property='IsMouseOver' Value='true'>
              <Setter TargetName='bd' Property='Background' Value='#26262B'/>
              <Setter Property='Foreground' Value='#F4F4F6'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style x:Key='BtnWinClose' TargetType='{x:Type Button}' BasedOn='{StaticResource BtnWinCtrl}'>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='{x:Type Button}'>
          <Border x:Name='bd' Background='Transparent'>
            <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
          </Border>
          <ControlTemplate.Triggers>
            <Trigger Property='IsMouseOver' Value='true'>
              <Setter TargetName='bd' Property='Background' Value='#C42B1C'/>
              <Setter Property='Foreground' Value='#FFFFFF'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>
</ResourceDictionary>";
            ResourceDictionary dict = (ResourceDictionary)XamlReader.Parse(styleXaml);
            this.Resources.MergedDictionaries.Add(dict);
        }

        private void ResolvePaths()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            pluginsFolder = System.IO.Path.Combine(localAppData, "Roblox", "Plugins");

            string baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
            if (File.Exists(System.IO.Path.Combine(baseDir, "assetreuploader.exe")) &&
                File.Exists(System.IO.Path.Combine(baseDir, "AssetReuploader2021.rbxm")))
            {
                repoDir = baseDir;
            }
            else
            {
                DirectoryInfo parent = Directory.GetParent(baseDir);
                if (parent != null &&
                    File.Exists(System.IO.Path.Combine(parent.FullName, "assetreuploader.exe")) &&
                    File.Exists(System.IO.Path.Combine(parent.FullName, "AssetReuploader2021.rbxm")))
                {
                    repoDir = parent.FullName;
                }
                else
                {
                    string standaloneDir = System.IO.Path.Combine(localAppData, "OctaneAssetReuploader");
                    ExtractEmbeddedPayload(standaloneDir);
                    repoDir = standaloneDir;
                }
            }

            pluginRbxmPath = System.IO.Path.Combine(repoDir, "AssetReuploader2021.rbxm");
            pluginRbxmxPath = System.IO.Path.Combine(repoDir, "AssetReuploader2021.rbxmx");
            cookieFilePath = System.IO.Path.Combine(repoDir, "cookie.txt");
            robloxCookieFilePath = System.IO.Path.Combine(repoDir, "roblox_cookie.txt");
            configIniPath = System.IO.Path.Combine(repoDir, "config.ini");
            backendExePath = System.IO.Path.Combine(repoDir, "assetreuploader.exe");
        }

        private void ExtractEmbeddedPayload(string targetDir)
        {
            try
            {
                Directory.CreateDirectory(targetDir);
                Assembly asm = Assembly.GetExecutingAssembly();
                using (Stream resStream = asm.GetManifestResourceStream("OctaneReup.Payload.zip"))
                {
                    if (resStream == null) return;
                    using (ZipArchive archive = new ZipArchive(resStream, ZipArchiveMode.Read))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string fullPath = System.IO.Path.Combine(targetDir, entry.FullName.Replace('/', '\\'));
                            if (String.IsNullOrEmpty(entry.Name))
                            {
                                Directory.CreateDirectory(fullPath);
                                continue;
                            }
                            string dir = System.IO.Path.GetDirectoryName(fullPath);
                            if (!String.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                            // Preserve user's saved cookies & config if they already exist
                            string lowerName = entry.Name.ToLowerInvariant();
                            if ((lowerName == "cookie.txt" || lowerName == "roblox_cookie.txt" || lowerName == "config.ini") &&
                                File.Exists(fullPath))
                            {
                                continue;
                            }

                            using (Stream inStream = entry.Open())
                            using (FileStream outStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                            {
                                inStream.CopyTo(outStream);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void TryLoadIcon()
        {
            try
            {
                string iconPng = System.IO.Path.Combine(repoDir, "assets", "PluginIcon.png");
                if (File.Exists(iconPng))
                {
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(iconPng, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    this.Icon = bmp;
                }
            }
            catch { }
        }

        private void BuildLayout()
        {
            Border outerFrame = new Border
            {
                BorderBrush = BBorder,
                BorderThickness = new Thickness(1),
                Background = BWindowBg
            };

            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(38) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(48) });

            Border titleBar = BuildTitleBar();
            Grid.SetRow(titleBar, 0);
            root.Children.Add(titleBar);

            Grid body = new Grid();
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(224) });
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(body, 1);

            Border sidebar = BuildSidebar();
            Grid.SetColumn(sidebar, 0);
            body.Children.Add(sidebar);

            Grid pageContainer = new Grid();
            Grid.SetColumn(pageContainer, 1);

            mainScroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Padding = new Thickness(28, 22, 28, 24)
            };

            Grid pagesGrid = new Grid();
            stepPages.Add(BuildPage1Plugin());
            stepPages.Add(BuildPage2Cookies());
            stepPages.Add(BuildPage3Config());
            stepPages.Add(BuildPage4LaunchAndGuide());

            for (int i = 0; i < stepPages.Count; i++)
            {
                pagesGrid.Children.Add(stepPages[i]);
            }

            mainScroll.Content = pagesGrid;
            pageContainer.Children.Add(mainScroll);

            toastBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(28, 28, 32)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(58, 58, 66)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(7),
                Padding = new Thickness(13, 8, 13, 8),
                Margin = new Thickness(0, 0, 20, 14),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Visibility = Visibility.Collapsed
            };
            toastLabel = new TextBlock
            {
                Foreground = BTextPrimary,
                FontSize = 12,
                FontWeight = FontWeights.Medium
            };
            toastBorder.Child = toastLabel;
            pageContainer.Children.Add(toastBorder);

            body.Children.Add(pageContainer);
            root.Children.Add(body);

            Border footer = BuildFooter();
            Grid.SetRow(footer, 2);
            root.Children.Add(footer);

            outerFrame.Child = root;
            this.Content = outerFrame;

            SelectStep(1);
        }

        private Border BuildTitleBar()
        {
            Border bar = new Border
            {
                Background = BSurface,
                BorderBrush = BBorder,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel left = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(14, 0, 0, 0)
            };

            if (this.Icon != null)
            {
                Image ic = new Image
                {
                    Source = this.Icon,
                    Width = 15,
                    Height = 15,
                    Margin = new Thickness(0, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                RenderOptions.SetBitmapScalingMode(ic, BitmapScalingMode.HighQuality);
                left.Children.Add(ic);
            }

            left.Children.Add(new TextBlock
            {
                Text = "Octane Asset Reuploader",
                FontSize = 12.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = BTextPrimary,
                VerticalAlignment = VerticalAlignment.Center
            });

            left.Children.Add(new TextBlock
            {
                Text = "2021 Studio",
                FontSize = 11,
                Foreground = BTextDim,
                Margin = new Thickness(7, 1, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            });

            Grid.SetColumn(left, 0);
            g.Children.Add(left);

            StackPanel statusLine = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 14, 0)
            };
            titleStatusDot = new Ellipse
            {
                Width = 7,
                Height = 7,
                Fill = BNeutralDot,
                Margin = new Thickness(0, 0, 7, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            titleStatusText = new TextBlock
            {
                Text = "Checking configuration...",
                FontSize = 11.5,
                Foreground = BTextMuted,
                VerticalAlignment = VerticalAlignment.Center
            };
            statusLine.Children.Add(titleStatusDot);
            statusLine.Children.Add(titleStatusText);
            Grid.SetColumn(statusLine, 2);
            g.Children.Add(statusLine);

            StackPanel winCtrls = new StackPanel { Orientation = Orientation.Horizontal };

            Button btnMin = new Button { Style = (Style)FindResource("BtnWinCtrl"), Content = CreateVectorIcon("M 0,5 L 10,5", 10, 10) };
            WindowChrome.SetIsHitTestVisibleInChrome(btnMin, true);
            btnMin.Click += delegate { this.WindowState = WindowState.Minimized; };

            Button btnMax = new Button { Style = (Style)FindResource("BtnWinCtrl"), Content = CreateVectorIcon("M 0.5,0.5 L 9.5,0.5 L 9.5,9.5 L 0.5,9.5 Z", 10, 10) };
            WindowChrome.SetIsHitTestVisibleInChrome(btnMax, true);
            btnMax.Click += delegate
            {
                this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
            };

            Button btnClose = new Button { Style = (Style)FindResource("BtnWinClose"), Content = CreateVectorIcon("M 0.5,0.5 L 9.5,9.5 M 9.5,0.5 L 0.5,9.5", 10, 10) };
            WindowChrome.SetIsHitTestVisibleInChrome(btnClose, true);
            btnClose.Click += delegate { this.Close(); };

            winCtrls.Children.Add(btnMin);
            winCtrls.Children.Add(btnMax);
            winCtrls.Children.Add(btnClose);
            Grid.SetColumn(winCtrls, 3);
            g.Children.Add(winCtrls);

            bar.Child = g;
            return bar;
        }

        private UIElement CreateVectorIcon(string pathData, double w, double h)
        {
            System.Windows.Shapes.Path p = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse(pathData),
                Stroke = BTextMuted,
                StrokeThickness = 1.2,
                Width = w,
                Height = h,
                Stretch = Stretch.None,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            p.SetBinding(System.Windows.Shapes.Path.StrokeProperty, new System.Windows.Data.Binding("Foreground")
            {
                RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.FindAncestor, typeof(Button), 1)
            });
            return p;
        }

        private Border BuildSidebar()
        {
            Border side = new Border
            {
                Background = BSurface,
                BorderBrush = BBorder,
                BorderThickness = new Thickness(0, 0, 1, 0),
                Padding = new Thickness(12, 16, 12, 14)
            };

            DockPanel dock = new DockPanel { LastChildFill = false };

            TextBlock lblSetup = CreateSectionLabel("Setup steps");
            lblSetup.Margin = new Thickness(8, 0, 0, 8);
            DockPanel.SetDock(lblSetup, Dock.Top);
            dock.Children.Add(lblSetup);

            string[] stepTitles = new string[]
            {
                "Studio Plugin",
                "Cookies Setup",
                "Account & Config",
                "Launch & Studio Guide"
            };

            for (int i = 0; i < stepTitles.Length; i++)
            {
                int stepNum = i + 1;
                TextBlock checkTxt;
                Border checkBox;
                Border row = CreateSidebarStepRow(stepNum, stepTitles[i], out checkTxt, out checkBox);
                navRows.Add(row);
                navCheckTexts.Add(checkTxt);
                navCheckBoxes.Add(checkBox);
                DockPanel.SetDock(row, Dock.Top);
                dock.Children.Add(row);
            }

            Border sep = new Border
            {
                Height = 1,
                Background = BBorderSubtle,
                Margin = new Thickness(6, 14, 6, 14)
            };
            DockPanel.SetDock(sep, Dock.Top);
            dock.Children.Add(sep);

            TextBlock lblEnv = CreateSectionLabel("Status");
            lblEnv.Margin = new Thickness(8, 0, 0, 8);
            DockPanel.SetDock(lblEnv, Dock.Top);
            dock.Children.Add(lblEnv);

            Border h1 = CreateHealthItem("Studio plugin", "Checking...", out sideDotPlugin, out sideTxtPlugin);
            Border h2 = CreateHealthItem("Octane cookie", "Checking...", out sideDotOctaneCookie, out sideTxtOctaneCookie);
            Border h3 = CreateHealthItem("Roblox cookie", "Checking...", out sideDotRobloxCookie, out sideTxtRobloxCookie);
            Border h4 = CreateHealthItem("Server :38073", "Checking...", out sideDotServer, out sideTxtServer);

            DockPanel.SetDock(h1, Dock.Top);
            DockPanel.SetDock(h2, Dock.Top);
            DockPanel.SetDock(h3, Dock.Top);
            DockPanel.SetDock(h4, Dock.Top);
            dock.Children.Add(h1);
            dock.Children.Add(h2);
            dock.Children.Add(h3);
            dock.Children.Add(h4);

            StackPanel bottomStack = new StackPanel();
            Button btnLaunch = new Button
            {
                Style = (Style)FindResource("BtnPrimary"),
                Content = "Start Reuploader Server",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 6)
            };
            btnLaunch.Click += delegate { LaunchReuploaderExe(); };
            bottomStack.Children.Add(btnLaunch);

            Button btnOpenFolder = new Button
            {
                Style = (Style)FindResource("BtnSecondary"),
                Content = "Open Project Folder",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            btnOpenFolder.Click += delegate { OpenFolder(repoDir); };
            bottomStack.Children.Add(btnOpenFolder);

            DockPanel.SetDock(bottomStack, Dock.Bottom);
            dock.Children.Add(bottomStack);

            side.Child = dock;
            return side;
        }

        private Border CreateSidebarStepRow(int idx, string title, out TextBlock badgeText, out Border badgeBox)
        {
            Border row = new Border
            {
                Background = Brushes.Transparent,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 7, 8, 7),
                Margin = new Thickness(0, 0, 0, 2),
                Cursor = Cursors.Hand
            };

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            badgeBox = new Border
            {
                Width = 18,
                Height = 18,
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(Color.FromRgb(32, 32, 36)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(46, 46, 52)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 9, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            badgeText = new TextBlock
            {
                Text = idx.ToString(),
                FontSize = 10.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = BTextMuted,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            badgeBox.Child = badgeText;
            Grid.SetColumn(badgeBox, 0);
            g.Children.Add(badgeBox);

            TextBlock lbl = new TextBlock
            {
                Text = title,
                FontSize = 12.5,
                FontWeight = FontWeights.Medium,
                Foreground = BTextBody,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(lbl, 1);
            g.Children.Add(lbl);

            row.Child = g;
            row.MouseLeftButtonUp += delegate { SelectStep(idx); };
            return row;
        }

        private Border CreateHealthItem(string name, string detail, out Ellipse dot, out TextBlock detailLbl)
        {
            Border item = new Border
            {
                Background = BCardSubtle,
                BorderBrush = BBorderSubtle,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(9, 6, 9, 6),
                Margin = new Thickness(0, 0, 0, 4)
            };

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            dot = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = BNeutralDot,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(dot, 0);
            g.Children.Add(dot);

            TextBlock nameLbl = new TextBlock
            {
                Text = name,
                FontSize = 11.5,
                FontWeight = FontWeights.Medium,
                Foreground = BTextBody,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameLbl, 1);
            g.Children.Add(nameLbl);

            detailLbl = new TextBlock
            {
                Text = detail,
                FontSize = 10.5,
                Foreground = BTextDim,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(detailLbl, 2);
            g.Children.Add(detailLbl);

            item.Child = g;
            return item;
        }

        private void SelectStep(int stepIdx)
        {
            if (stepIdx < 1) stepIdx = 1;
            if (stepIdx > stepPages.Count) stepIdx = stepPages.Count;
            currentStep = stepIdx;

            for (int i = 0; i < stepPages.Count; i++)
            {
                stepPages[i].Visibility = (i + 1 == currentStep) ? Visibility.Visible : Visibility.Collapsed;
            }

            for (int i = 0; i < navRows.Count; i++)
            {
                bool active = (i + 1 == currentStep);
                navRows[i].Background = active
                    ? new SolidColorBrush(Color.FromRgb(34, 34, 39))
                    : Brushes.Transparent;
            }

            if (btnPrev != null) btnPrev.IsEnabled = (currentStep > 1);
            if (btnNext != null) btnNext.Content = (currentStep < stepPages.Count) ? "Next" : "Done";
            if (lblFooterStep != null) lblFooterStep.Text = String.Format("Step {0} of {1}", currentStep, stepPages.Count);

            if (mainScroll != null) mainScroll.ScrollToTop();
        }

        // ====================================================================
        // PAGE 1: STUDIO PLUGIN
        // ====================================================================
        private UIElement BuildPage1Plugin()
        {
            StackPanel page = new StackPanel();
            page.Children.Add(CreatePageHeader(
                "Install Studio Plugin",
                "Install AssetReuploader2021.rbxm into your local Roblox Studio plugins folder so the Asset Reuploader toolbar button appears inside 2021 Studio."
            ));

            Border card = CreateSurfaceCard();
            Grid cg = new Grid();
            cg.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cg.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel info = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 14, 0) };
            info.Children.Add(new TextBlock
            {
                Text = "AssetReuploader2021.rbxm",
                FontSize = 13.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = BTextPrimary
            });
            lblPluginStatusDetail = new TextBlock
            {
                Text = pluginsFolder,
                FontSize = 11.5,
                Foreground = BTextMuted,
                Margin = new Thickness(0, 2, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            info.Children.Add(lblPluginStatusDetail);
            Grid.SetColumn(info, 0);
            cg.Children.Add(info);

            StackPanel actions = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            Button btnOpenPlugins = new Button
            {
                Style = (Style)FindResource("BtnSecondary"),
                Content = "Open Plugins Folder",
                Margin = new Thickness(0, 0, 8, 0)
            };
            btnOpenPlugins.Click += delegate
            {
                try { Directory.CreateDirectory(pluginsFolder); } catch { }
                OpenFolder(pluginsFolder);
            };
            actions.Children.Add(btnOpenPlugins);

            btnInstallPlugin = new Button
            {
                Style = (Style)FindResource("BtnPrimary"),
                Content = "Install Plugin"
            };
            btnInstallPlugin.Click += delegate { InstallPlugin(); };
            actions.Children.Add(btnInstallPlugin);

            Grid.SetColumn(actions, 1);
            cg.Children.Add(actions);
            card.Child = cg;
            page.Children.Add(card);

            page.Children.Add(CreateSectionLabel("Manual installation steps"));

            Border stepsContainer = CreateSurfaceCard();
            stepsContainer.Padding = new Thickness(16, 6, 16, 6);
            StackPanel stepsList = new StackPanel();

            stepsList.Children.Add(CreateStepRow(
                1,
                "Open your Roblox Plugins folder",
                "Click Install Plugin above, or press Win + R and open your local plugins folder:",
                "%localappdata%\\Roblox\\Plugins",
                true
            ));
            stepsList.Children.Add(CreateRowDivider());

            stepsList.Children.Add(CreateStepRow(
                2,
                "Copy AssetReuploader2021.rbxm into Plugins",
                "Copy AssetReuploader2021.rbxm from this folder into %localappdata%\\Roblox\\Plugins (remove any duplicate .rbxmx copy so the button doesn't appear twice).",
                null,
                false
            ));
            stepsList.Children.Add(CreateRowDivider());

            stepsList.Children.Add(CreateStepRow(
                3,
                "Enable HTTP Requests in your Place",
                "In 2021 Roblox Studio, go to Home > Game Settings > Security > Allow HTTP Requests, or run this in the Command Bar:",
                "game:GetService(\"HttpService\").HttpEnabled = true",
                true
            ));

            stepsContainer.Child = stepsList;
            page.Children.Add(stepsContainer);

            return page;
        }

        // ====================================================================
        // PAGE 2: COOKIES SETUP
        // ====================================================================
        private UIElement BuildPage2Cookies()
        {
            StackPanel page = new StackPanel();
            page.Children.Add(CreatePageHeader(
                "Octane & Roblox Cookies",
                "Asset Reuploader uses two cookies: your Octane cookie (to publish assets to octane.wtf) and your official Roblox cookie (to download private animations and audio from roblox.com)."
            ));

            // Cookie 1: Octane cookie.txt
            Border octaneCard = CreateSurfaceCard();
            StackPanel ocStack = new StackPanel();

            DockPanel ocHead = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
            Button btnOpenOctane = new Button
            {
                Style = (Style)FindResource("BtnSubtle"),
                Content = "Open octane.wtf ->"
            };
            btnOpenOctane.Click += delegate { OpenUrl("https://octane.wtf"); };
            DockPanel.SetDock(btnOpenOctane, Dock.Right);
            ocHead.Children.Add(btnOpenOctane);
            ocHead.Children.Add(new TextBlock
            {
                Text = "1. Octane Session Cookie (cookie.txt) — Required",
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = BTextPrimary,
                VerticalAlignment = VerticalAlignment.Center
            });
            ocStack.Children.Add(ocHead);

            ocStack.Children.Add(new TextBlock
            {
                Text = "Log in to https://octane.wtf -> press F12 (DevTools) -> Application tab -> Cookies -> copy your session cookie value and paste it below:",
                FontSize = 11.5,
                Foreground = BTextMuted,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            });

            Border ocInputBd = new Border
            {
                Background = BCodeBg,
                BorderBrush = BBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6)
            };
            txtOctaneCookie = new TextBox
            {
                Background = Brushes.Transparent,
                Foreground = BTextBody,
                BorderThickness = new Thickness(0),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11.5,
                Padding = new Thickness(10, 8, 10, 8),
                TextWrapping = TextWrapping.Wrap,
                Height = 56,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            ocInputBd.Child = txtOctaneCookie;
            ocStack.Children.Add(ocInputBd);
            octaneCard.Child = ocStack;
            page.Children.Add(octaneCard);

            // Cookie 2: Official Roblox roblox_cookie.txt
            Border rbxCard = CreateSurfaceCard();
            StackPanel rbxStack = new StackPanel();

            DockPanel rbxHead = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
            Button btnOpenRbx = new Button
            {
                Style = (Style)FindResource("BtnSubtle"),
                Content = "Open roblox.com ->"
            };
            btnOpenRbx.Click += delegate { OpenUrl("https://www.roblox.com"); };
            DockPanel.SetDock(btnOpenRbx, Dock.Right);
            rbxHead.Children.Add(btnOpenRbx);
            rbxHead.Children.Add(new TextBlock
            {
                Text = "2. Official Roblox Cookie (roblox_cookie.txt) — Recommended",
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = BTextPrimary,
                VerticalAlignment = VerticalAlignment.Center
            });
            rbxStack.Children.Add(rbxHead);

            rbxStack.Children.Add(new TextBlock
            {
                Text = "Required for downloading private/protected animations and audio from official Roblox. Open roblox.com -> F12 -> Application -> Cookies -> copy your authentication cookie (including the _|WARNING:... prefix):",
                FontSize = 11.5,
                Foreground = BTextMuted,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            });

            Border rbxInputBd = new Border
            {
                Background = BCodeBg,
                BorderBrush = BBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6)
            };
            txtRobloxCookie = new TextBox
            {
                Background = Brushes.Transparent,
                Foreground = BTextBody,
                BorderThickness = new Thickness(0),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11.5,
                Padding = new Thickness(10, 8, 10, 8),
                TextWrapping = TextWrapping.Wrap,
                Height = 56,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            rbxInputBd.Child = txtRobloxCookie;
            rbxStack.Children.Add(rbxInputBd);
            rbxCard.Child = rbxStack;
            page.Children.Add(rbxCard);

            // Save Cookies Button Row
            StackPanel saveRow = new StackPanel { Orientation = Orientation.Horizontal };
            Button btnSaveCookies = new Button
            {
                Style = (Style)FindResource("BtnPrimary"),
                Content = "Save Cookies to Disk",
                Margin = new Thickness(0, 0, 8, 0)
            };
            btnSaveCookies.Click += delegate { SaveCookiesFromUI(); };
            saveRow.Children.Add(btnSaveCookies);

            Button btnReloadCookies = new Button
            {
                Style = (Style)FindResource("BtnSecondary"),
                Content = "Reload from Files"
            };
            btnReloadCookies.Click += delegate
            {
                LoadConfigAndCookiesIntoUI();
                ShowToast("Reloaded cookie.txt and roblox_cookie.txt");
            };
            saveRow.Children.Add(btnReloadCookies);

            page.Children.Add(saveRow);
            return page;
        }

        // ====================================================================
        // PAGE 3: ACCOUNT & CONFIG (config.ini)
        // ====================================================================
        private UIElement BuildPage3Config()
        {
            StackPanel page = new StackPanel();
            page.Children.Add(CreatePageHeader(
                "Account & Configuration (config.ini)",
                "Configure your Octane numeric User ID (and optional Group ID if uploading assets to an Octane group)."
            ));

            Border cfgCard = CreateSurfaceCard();
            StackPanel form = new StackPanel();

            form.Children.Add(CreateFormField(
                "Octane User ID (user_id)",
                "Your numeric ID from your Octane profile URL (e.g., https://octane.wtf/users/24811/profile -> 24811):",
                out txtUserId
            ));

            form.Children.Add(CreateFormField(
                "Octane Group ID (group_id) — Optional",
                "Leave blank to upload to your personal profile, or enter an Octane group ID to upload animations/sounds/T-Shirts to a group:",
                out txtGroupId
            ));

            form.Children.Add(CreateFormField(
                "Revival Domain (domain)",
                "Default is octane.wtf:",
                out txtDomain
            ));

            form.Children.Add(CreateFormField(
                "Local Server Port (port)",
                "Default is 38073 (must match the Port setting in the Studio plugin):",
                out txtPort
            ));

            StackPanel btnRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0) };
            Button btnSaveCfg = new Button
            {
                Style = (Style)FindResource("BtnPrimary"),
                Content = "Save config.ini"
            };
            btnSaveCfg.Click += delegate { SaveConfigIniFromUI(); };
            btnRow.Children.Add(btnSaveCfg);

            form.Children.Add(btnRow);
            cfgCard.Child = form;
            page.Children.Add(cfgCard);

            return page;
        }

        private StackPanel CreateFormField(string label, string hint, out TextBox tb)
        {
            StackPanel sp = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            sp.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 12.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = BTextPrimary
            });
            sp.Children.Add(new TextBlock
            {
                Text = hint,
                FontSize = 11.5,
                Foreground = BTextMuted,
                Margin = new Thickness(0, 1, 0, 6),
                TextWrapping = TextWrapping.Wrap
            });
            Border bd = new Border
            {
                Background = BCodeBg,
                BorderBrush = BBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6)
            };
            tb = new TextBox
            {
                Background = Brushes.Transparent,
                Foreground = BTextBody,
                BorderThickness = new Thickness(0),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Padding = new Thickness(10, 6, 10, 6)
            };
            bd.Child = tb;
            sp.Children.Add(bd);
            return sp;
        }

        // ====================================================================
        // PAGE 4: LAUNCH & STUDIO USAGE GUIDE
        // ====================================================================
        private UIElement BuildPage4LaunchAndGuide()
        {
            StackPanel page = new StackPanel();
            page.Children.Add(CreatePageHeader(
                "Launch & Studio Usage Guide",
                "Start the local reuploader backend (assetreuploader.exe), then use the Asset Reuploader plugin inside 2021 Roblox Studio."
            ));

            Border launchCard = CreateSurfaceCard();
            Grid lg = new Grid();
            lg.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            lg.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel info = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 14, 0) };
            info.Children.Add(new TextBlock
            {
                Text = "Reuploader Backend (assetreuploader.exe)",
                FontSize = 13.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = BTextPrimary
            });
            info.Children.Add(new TextBlock
            {
                Text = "Authenticates your cookies and listens on localhost:38073 for requests from the Studio plugin.",
                FontSize = 11.5,
                Foreground = BTextMuted,
                Margin = new Thickness(0, 2, 0, 0),
                TextWrapping = TextWrapping.Wrap
            });
            Grid.SetColumn(info, 0);
            lg.Children.Add(info);

            Button btnRunExe = new Button
            {
                Style = (Style)FindResource("BtnPrimary"),
                Content = "Start assetreuploader.exe",
                VerticalAlignment = VerticalAlignment.Center
            };
            btnRunExe.Click += delegate { LaunchReuploaderExe(); };
            Grid.SetColumn(btnRunExe, 1);
            lg.Children.Add(btnRunExe);
            launchCard.Child = lg;
            page.Children.Add(launchCard);

            page.Children.Add(CreateSectionLabel("How to reupload in 2021 Roblox Studio"));

            Border guideCard = CreateSurfaceCard();
            guideCard.Padding = new Thickness(16, 6, 16, 6);
            StackPanel list = new StackPanel();

            list.Children.Add(CreateStepRow(
                1,
                "Open the Asset Reuploader plugin in Studio",
                "Open your place in 2021 Roblox Studio, go to the Plugins tab at the top, and click 'Asset Reuploader'.",
                null,
                false
            ));
            list.Children.Add(CreateRowDivider());

            list.Children.Add(CreateStepRow(
                2,
                "Animations Tab (develop?View=24)",
                "Select the Animation tab and click Reupload. If an animation comes from a private Roblox place, add that original place ID under the Filter tab first so Roblox allows downloading it.",
                null,
                false
            ));
            list.Children.Add(CreateRowDivider());

            list.Children.Add(CreateStepRow(
                3,
                "Sound Tab (develop?View=3)",
                "Select the Sound tab and click Reupload. Downloads original MP3/OGG audio from Roblox, uploads to Octane (note: Octane charges 15 Robux per audio upload), and replaces SoundIds across your place.",
                null,
                false
            ));
            list.Children.Add(CreateRowDivider());

            list.Children.Add(CreateStepRow(
                4,
                "Gamepass Tab (develop?View=2)",
                "Scans all scripts for Gamepass IDs, uploads their icons to Octane as T-Shirts, replaces the IDs, and provides a 1-click 'Convert APIs' button to convert UserOwnsGamePassAsync into PlayerOwnsAsset.",
                null,
                false
            ));

            guideCard.Child = list;
            page.Children.Add(guideCard);

            return page;
        }

        private Border BuildFooter()
        {
            Border footer = new Border
            {
                Background = BSurface,
                BorderBrush = BBorder,
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(20, 0, 20, 0)
            };

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            lblFooterStep = new TextBlock
            {
                Text = "Step 1 of 4",
                FontSize = 11.5,
                Foreground = BTextDim,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(lblFooterStep, 0);
            g.Children.Add(lblFooterStep);

            StackPanel nav = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            btnPrev = new Button
            {
                Style = (Style)FindResource("BtnSecondary"),
                Content = "Back",
                Width = 72,
                Margin = new Thickness(0, 0, 8, 0)
            };
            btnPrev.Click += delegate { SelectStep(currentStep - 1); };
            nav.Children.Add(btnPrev);

            btnNext = new Button
            {
                Style = (Style)FindResource("BtnPrimary"),
                Content = "Next",
                Width = 80
            };
            btnNext.Click += delegate
            {
                if (currentStep < stepPages.Count) SelectStep(currentStep + 1);
                else ShowToast("Setup complete.");
            };
            nav.Children.Add(btnNext);

            Grid.SetColumn(nav, 2);
            g.Children.Add(nav);

            footer.Child = g;
            return footer;
        }

        // ====================================================================
        // HELPERS & ACTIONS
        // ====================================================================
        private StackPanel CreatePageHeader(string title, string subtitle)
        {
            StackPanel sp = new StackPanel { Margin = new Thickness(0, 0, 0, 18) };
            sp.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = BTextPrimary,
                Margin = new Thickness(0, 0, 0, 4)
            });
            sp.Children.Add(new TextBlock
            {
                Text = subtitle,
                FontSize = 12.5,
                Foreground = BTextMuted,
                TextWrapping = TextWrapping.Wrap
            });
            return sp;
        }

        private TextBlock CreateSectionLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = BTextDim,
                Margin = new Thickness(0, 4, 0, 8)
            };
        }

        private Border CreateSurfaceCard()
        {
            return new Border
            {
                Background = BCard,
                BorderBrush = BBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 13, 16, 13),
                Margin = new Thickness(0, 0, 0, 14)
            };
        }

        private Border CreateRowDivider()
        {
            return new Border
            {
                Height = 1,
                Background = BBorderSubtle,
                Margin = new Thickness(0, 2, 0, 2)
            };
        }

        private Grid CreateStepRow(int num, string title, string desc, string codeSnippet, bool showCopy)
        {
            Grid g = new Grid { Margin = new Thickness(0, 10, 0, 10) };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Border numBadge = new Border
            {
                Width = 20,
                Height = 20,
                CornerRadius = new CornerRadius(5),
                Background = new SolidColorBrush(Color.FromRgb(34, 34, 39)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 1, 0, 0)
            };
            numBadge.Child = new TextBlock
            {
                Text = num.ToString(),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = BTextMuted,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(numBadge, 0);
            g.Children.Add(numBadge);

            StackPanel body = new StackPanel();
            body.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 12.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = BTextPrimary,
                Margin = new Thickness(0, 0, 0, 2)
            });
            body.Children.Add(new TextBlock
            {
                Text = desc,
                FontSize = 12,
                Foreground = BTextMuted,
                TextWrapping = TextWrapping.Wrap
            });

            if (!String.IsNullOrEmpty(codeSnippet))
            {
                Border codeBox = new Border
                {
                    Background = BCodeBg,
                    BorderBrush = BBorderSubtle,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(10, 6, 8, 6),
                    Margin = new Thickness(0, 7, 0, 0)
                };
                DockPanel dp = new DockPanel();
                if (showCopy)
                {
                    Button btnCopy = new Button
                    {
                        Style = (Style)FindResource("BtnSubtle"),
                        Content = "Copy",
                        Margin = new Thickness(8, 0, 0, 0)
                    };
                    string captured = codeSnippet;
                    btnCopy.Click += delegate { CopyText(captured, "Copied to clipboard"); };
                    DockPanel.SetDock(btnCopy, Dock.Right);
                    dp.Children.Add(btnCopy);
                }
                dp.Children.Add(new TextBlock
                {
                    Text = codeSnippet,
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 11.5,
                    Foreground = BTextBody,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                });
                codeBox.Child = dp;
                body.Children.Add(codeBox);
            }

            Grid.SetColumn(body, 1);
            g.Children.Add(body);
            return g;
        }

        private void ShowToast(string msg)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                toastLabel.Text = msg;
                toastBorder.Visibility = Visibility.Visible;
                if (toastTimer == null)
                {
                    toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.6) };
                    toastTimer.Tick += delegate
                    {
                        toastBorder.Visibility = Visibility.Collapsed;
                        toastTimer.Stop();
                    };
                }
                toastTimer.Stop();
                toastTimer.Start();
            }));
        }

        private void CopyText(string text, string notice)
        {
            try
            {
                Clipboard.SetText(text);
                ShowToast(notice);
            }
            catch (Exception ex)
            {
                ShowToast("Clipboard error: " + ex.Message);
            }
        }

        private void OpenFolder(string path)
        {
            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                Process.Start("explorer.exe", "\"" + path + "\"");
            }
            catch (Exception ex)
            {
                ShowToast("Error: " + ex.Message);
            }
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ShowToast("Error: " + ex.Message);
            }
        }

        private void InstallPlugin()
        {
            try
            {
                string src = File.Exists(pluginRbxmPath) ? pluginRbxmPath : pluginRbxmxPath;
                if (!File.Exists(src))
                {
                    ShowToast("Plugin file not found in " + repoDir);
                    return;
                }
                Directory.CreateDirectory(pluginsFolder);
                string fileName = System.IO.Path.GetFileName(src);
                File.Copy(src, System.IO.Path.Combine(pluginsFolder, fileName), true);

                // Remove duplicate .rbxmx if we installed .rbxm so button doesn't show twice
                if (fileName.EndsWith(".rbxm", StringComparison.OrdinalIgnoreCase))
                {
                    string dup = System.IO.Path.Combine(pluginsFolder, "AssetReuploader2021.rbxmx");
                    if (File.Exists(dup))
                    {
                        try { File.Delete(dup); } catch { }
                    }
                }

                ShowToast("Installed " + fileName + " to Roblox Plugins");
                RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                ShowToast("Install failed: " + ex.Message);
            }
        }

        private void LoadConfigAndCookiesIntoUI()
        {
            try
            {
                if (File.Exists(cookieFilePath))
                {
                    txtOctaneCookie.Text = File.ReadAllText(cookieFilePath).Trim();
                }
                if (File.Exists(robloxCookieFilePath))
                {
                    txtRobloxCookie.Text = File.ReadAllText(robloxCookieFilePath).Trim();
                }

                string port = "38073";
                string domain = "octane.wtf";
                string userId = "";
                string groupId = "";

                if (File.Exists(configIniPath))
                {
                    foreach (string rawLine in File.ReadAllLines(configIniPath))
                    {
                        string line = rawLine.Trim();
                        int eq = line.IndexOf('=');
                        if (eq <= 0) continue;
                        string k = line.Substring(0, eq).Trim();
                        string v = line.Substring(eq + 1).Trim();
                        if (k == "port") port = v;
                        else if (k == "domain") domain = v;
                        else if (k == "user_id") userId = v;
                        else if (k == "group_id") groupId = v;
                    }
                }

                txtUserId.Text = userId;
                txtGroupId.Text = groupId;
                txtDomain.Text = domain;
                txtPort.Text = port;

                int parsedPort;
                if (Int32.TryParse(port, out parsedPort) && parsedPort > 0) configuredPort = parsedPort;
            }
            catch { }
        }

        private void SaveCookiesFromUI()
        {
            try
            {
                string oc = (txtOctaneCookie.Text ?? "").Trim();
                if (oc.StartsWith(".") && oc.IndexOf('=') > 0 && oc.IndexOf('=') < 20)
                {
                    oc = oc.Substring(oc.IndexOf('=') + 1).Trim();
                }
                File.WriteAllText(cookieFilePath, oc + "\n", new UTF8Encoding(false));

                string rbx = (txtRobloxCookie.Text ?? "").Trim();
                if (rbx.StartsWith(".") && rbx.IndexOf('=') > 0 && rbx.IndexOf('=') < 20)
                {
                    rbx = rbx.Substring(rbx.IndexOf('=') + 1).Trim();
                }
                File.WriteAllText(robloxCookieFilePath, rbx + "\n", new UTF8Encoding(false));

                ShowToast("Saved cookie.txt and roblox_cookie.txt");
                RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                ShowToast("Failed to save cookies: " + ex.Message);
            }
        }

        private void SaveConfigIniFromUI()
        {
            try
            {
                string port = (txtPort.Text ?? "").Trim();
                if (String.IsNullOrEmpty(port)) port = "38073";
                string domain = (txtDomain.Text ?? "").Trim();
                if (String.IsNullOrEmpty(domain)) domain = "octane.wtf";
                string userId = (txtUserId.Text ?? "").Trim();
                string groupId = (txtGroupId.Text ?? "").Trim();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("port=" + port);
                sb.AppendLine("domain=" + domain);
                sb.AppendLine("user_id=" + userId);
                sb.AppendLine("group_id=" + groupId);
                sb.AppendLine("cookie_file=cookie.txt");
                sb.AppendLine("roblox_cookie_file=roblox_cookie.txt");

                File.WriteAllText(configIniPath, sb.ToString(), new UTF8Encoding(false));

                int p;
                if (Int32.TryParse(port, out p) && p > 0) configuredPort = p;

                ShowToast("Saved config.ini");
                RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                ShowToast("Failed to save config.ini: " + ex.Message);
            }
        }

        private void LaunchReuploaderExe()
        {
            try
            {
                if (!File.Exists(backendExePath))
                {
                    ShowToast("assetreuploader.exe not found in " + repoDir);
                    return;
                }
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = backendExePath,
                    WorkingDirectory = repoDir,
                    UseShellExecute = true
                };
                Process.Start(psi);
                ShowToast("Launched assetreuploader.exe");
                RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                ShowToast("Launch error: " + ex.Message);
            }
        }

        private void RefreshStatusAsync()
        {
            Task.Run(delegate
            {
                bool plugOk = File.Exists(System.IO.Path.Combine(pluginsFolder, "AssetReuploader2021.rbxm")) ||
                              File.Exists(System.IO.Path.Combine(pluginsFolder, "AssetReuploader2021.rbxmx"));

                bool ocOk = false;
                try
                {
                    if (File.Exists(cookieFilePath))
                    {
                        ocOk = !String.IsNullOrWhiteSpace(File.ReadAllText(cookieFilePath));
                    }
                }
                catch { }

                bool rbxOk = false;
                try
                {
                    if (File.Exists(robloxCookieFilePath))
                    {
                        rbxOk = !String.IsNullOrWhiteSpace(File.ReadAllText(robloxCookieFilePath));
                    }
                }
                catch { }

                bool portOpen = IsPortOpen(configuredPort);

                isPluginInstalled = plugOk;
                hasOctaneCookie = ocOk;
                hasRobloxCookie = rbxOk;
                isBackendRunning = portOpen;

                Dispatcher.Invoke(new Action(delegate
                {
                    if (isBackendRunning)
                    {
                        titleStatusDot.Fill = BGreen;
                        titleStatusText.Text = "Server running on :" + configuredPort;
                    }
                    else if (isPluginInstalled && hasOctaneCookie)
                    {
                        titleStatusDot.Fill = BAmber;
                        titleStatusText.Text = "Ready · start assetreuploader.exe";
                    }
                    else
                    {
                        titleStatusDot.Fill = BNeutralDot;
                        titleStatusText.Text = "Setup required";
                    }

                    sideDotPlugin.Fill = isPluginInstalled ? BGreen : BAmber;
                    sideTxtPlugin.Text = isPluginInstalled ? "installed" : "missing";

                    sideDotOctaneCookie.Fill = hasOctaneCookie ? BGreen : BAmber;
                    sideTxtOctaneCookie.Text = hasOctaneCookie ? "saved" : "empty";

                    sideDotRobloxCookie.Fill = hasRobloxCookie ? BGreen : BNeutralDot;
                    sideTxtRobloxCookie.Text = hasRobloxCookie ? "saved" : "empty";

                    sideDotServer.Fill = isBackendRunning ? BGreen : BNeutralDot;
                    sideTxtServer.Text = isBackendRunning ? "online" : "stopped";

                    if (lblPluginStatusDetail != null)
                    {
                        lblPluginStatusDetail.Text = isPluginInstalled
                            ? "Installed in " + pluginsFolder
                            : "Target: " + pluginsFolder;
                    }
                    if (btnInstallPlugin != null)
                    {
                        btnInstallPlugin.Content = isPluginInstalled ? "Reinstall Plugin" : "Install Plugin";
                    }

                    UpdateStepCheck(0, isPluginInstalled);
                    UpdateStepCheck(1, hasOctaneCookie);
                    UpdateStepCheck(2, File.Exists(configIniPath));
                    UpdateStepCheck(3, isBackendRunning);
                }));
            });
        }

        private void UpdateStepCheck(int idx, bool done)
        {
            if (idx < 0 || idx >= navCheckTexts.Count) return;
            navCheckTexts[idx].Text = done ? "✓" : (idx + 1).ToString();
            navCheckTexts[idx].Foreground = done ? BGreen : BTextMuted;
            navCheckBoxes[idx].BorderBrush = done
                ? new SolidColorBrush(Color.FromArgb(90, 52, 211, 153))
                : new SolidColorBrush(Color.FromRgb(46, 46, 52));
        }

        private bool IsPortOpen(int port)
        {
            try
            {
                IPGlobalProperties props = IPGlobalProperties.GetIPGlobalProperties();
                foreach ( System.Net.IPEndPoint ep in props.GetActiveTcpListeners())
                {
                    if (ep.Port == port) return true;
                }
            }
            catch { }
            return false;
        }
    }
}
