using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace TextEditor
{
    public partial class MainWindow : Window
    {
        private string currentFileName = null;
        private bool isModified = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeFontControls();
            UpdateTitle();

            // Підключення обробників подій для ComboBox
            FontFamilyComboBox.SelectionChanged += FontFamily_Changed;
            FontSizeComboBox.SelectionChanged += FontSize_Changed;
            MainTextBox.SelectionChanged += TextBox_SelectionChanged;
            MainTextBox.TextChanged += TextBox_TextChanged;
        }

        private void InitializeFontControls()
        {
            // Заповнення ComboBox шрифтів
            foreach (FontFamily font in Fonts.SystemFontFamilies)
            {
                FontFamilyComboBox.Items.Add(font.Source);
            }
            FontFamilyComboBox.SelectedItem = "Segoe UI";

            // Заповнення ComboBox розмірів
            double[] fontSizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            foreach (double size in fontSizes)
            {
                FontSizeComboBox.Items.Add(size);
            }
            FontSizeComboBox.SelectedItem = 12.0;
        }

        private void UpdateTitle()
        {
            string fileName = currentFileName ?? "Без назви";
            string modified = isModified ? "*" : "";
            Title = $"{fileName}{modified} - Текстовий редактор";
        }

        // === Команди файлів ===
        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CheckSaveChanges())
            {
                MainTextBox.Document.Blocks.Clear();
                currentFileName = null;
                isModified = false;
                UpdateTitle();
                StatusTextBlock.Text = "Створено новий документ";
            }
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CheckSaveChanges())
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "Rich Text Format (*.rtf)|*.rtf|Текстові файли (*.txt)|*.txt|Всі файли (*.*)|*.*";

                if (openDialog.ShowDialog() == true)
                {
                    try
                    {
                        TextRange range = new TextRange(MainTextBox.Document.ContentStart, MainTextBox.Document.ContentEnd);
                        using (FileStream fs = new FileStream(openDialog.FileName, FileMode.Open))
                        {
                            if (openDialog.FileName.EndsWith(".rtf"))
                                range.Load(fs, DataFormats.Rtf);
                            else
                                range.Load(fs, DataFormats.Text);
                        }
                        currentFileName = openDialog.FileName;
                        isModified = false;
                        UpdateTitle();
                        StatusTextBlock.Text = $"Відкрито: {openDialog.FileName}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка відкриття файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (currentFileName == null)
            {
                SaveAs_Executed(sender, e);
            }
            else
            {
                SaveFile(currentFileName);
            }
        }

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Rich Text Format (*.rtf)|*.rtf|Текстові файли (*.txt)|*.txt|Всі файли (*.*)|*.*";

            if (saveDialog.ShowDialog() == true)
            {
                SaveFile(saveDialog.FileName);
                currentFileName = saveDialog.FileName;
            }
        }

        private void SaveFile(string fileName)
        {
            try
            {
                TextRange range = new TextRange(MainTextBox.Document.ContentStart, MainTextBox.Document.ContentEnd);
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    if (fileName.EndsWith(".rtf"))
                        range.Save(fs, DataFormats.Rtf);
                    else
                        range.Save(fs, DataFormats.Text);
                }
                isModified = false;
                UpdateTitle();
                StatusTextBlock.Text = $"Збережено: {fileName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CheckSaveChanges()
        {
            if (isModified)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Документ було змінено. Зберегти зміни?",
                    "Текстовий редактор",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (currentFileName == null)
                    {
                        SaveFileDialog saveDialog = new SaveFileDialog();
                        saveDialog.Filter = "Rich Text Format (*.rtf)|*.rtf|Текстові файли (*.txt)|*.txt|Всі файли (*.*)|*.*";

                        if (saveDialog.ShowDialog() == true)
                        {
                            SaveFile(saveDialog.FileName);
                            currentFileName = saveDialog.FileName;
                        }
                    }
                    else
                    {
                        SaveFile(currentFileName);
                    }
                    return !isModified;
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }
            return true;
        }

        // === Команди редагування ===
        private void Cut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainTextBox.Cut();
            StatusTextBlock.Text = "Текст вирізано";
        }

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainTextBox.Copy();
            StatusTextBlock.Text = "Текст скопійовано";
        }

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainTextBox.Paste();
            StatusTextBlock.Text = "Текст вставлено";
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainTextBox.Undo();
            StatusTextBlock.Text = "Скасовано останню дію";
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainTextBox.Redo();
            StatusTextBlock.Text = "Повторено останню дію";
        }

        private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainTextBox.SelectAll();
            StatusTextBlock.Text = "Весь текст виділено";
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            MainTextBox.Selection.Text = "";
            StatusTextBlock.Text = "Текст видалено";
        }

        // === Форматування ===
        private void FontFamily_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (MainTextBox == null || MainTextBox.Selection.IsEmpty)
                return;

            string fontName = FontFamilyComboBox.SelectedItem as string;
            if (fontName != null)
            {
                MainTextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily(fontName));
            }
        }

        private void FontSize_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (MainTextBox == null || MainTextBox.Selection.IsEmpty)
                return;

            if (FontSizeComboBox.SelectedItem != null)
            {
                MainTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, FontSizeComboBox.SelectedItem);
            }
        }

        private void Bold_Click(object sender, RoutedEventArgs e)
        {
            if (MainTextBox.Selection.GetPropertyValue(TextElement.FontWeightProperty).Equals(FontWeights.Bold))
                MainTextBox.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            else
                MainTextBox.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
        }

        private void Italic_Click(object sender, RoutedEventArgs e)
        {
            if (MainTextBox.Selection.GetPropertyValue(TextElement.FontStyleProperty).Equals(FontStyles.Italic))
                MainTextBox.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
            else
                MainTextBox.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
        }

        private void Underline_Click(object sender, RoutedEventArgs e)
        {
            TextDecorationCollection decorations = (TextDecorationCollection)MainTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);

            if (decorations == null || !decorations.Equals(TextDecorations.Underline))
                MainTextBox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            else
                MainTextBox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
        }

        private void AlignLeft_Click(object sender, RoutedEventArgs e)
        {
            MainTextBox.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Left);
            StatusTextBlock.Text = "Вирівнювання по лівому краю";
        }

        private void AlignCenter_Click(object sender, RoutedEventArgs e)
        {
            MainTextBox.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Center);
            StatusTextBlock.Text = "Вирівнювання по центру";
        }

        private void AlignRight_Click(object sender, RoutedEventArgs e)
        {
            MainTextBox.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Right);
            StatusTextBlock.Text = "Вирівнювання по правому краю";
        }

        private void AlignJustify_Click(object sender, RoutedEventArgs e)
        {
            MainTextBox.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Justify);
            StatusTextBlock.Text = "Вирівнювання по ширині";
        }

        private void Bullets_Click(object sender, RoutedEventArgs e)
        {
            // Створення списку з маркерами
            var list = new List();
            list.MarkerStyle = TextMarkerStyle.Disc;
            var listItem = new ListItem(new Paragraph(new Run(MainTextBox.Selection.Text)));
            list.ListItems.Add(listItem);

            StatusTextBlock.Text = "Маркери застосовано";
        }

        private void Numbering_Click(object sender, RoutedEventArgs e)
        {
            // Створення нумерованого списку
            var list = new List();
            list.MarkerStyle = TextMarkerStyle.Decimal;
            var listItem = new ListItem(new Paragraph(new Run(MainTextBox.Selection.Text)));
            list.ListItems.Add(listItem);

            StatusTextBlock.Text = "Нумерацію застосовано";
        }

        private void Font_Click(object sender, RoutedEventArgs e)
        {
            // Створюємо діалогове вікно для вибору шрифту
            var fontDialog = new FontSelectionDialog();

            // Встановлюємо поточні значення
            if (MainTextBox.Selection.GetPropertyValue(TextElement.FontFamilyProperty) is FontFamily currentFont)
            {
                fontDialog.SelectedFontFamily = currentFont;
            }

            if (MainTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty) is double currentSize)
            {
                fontDialog.SelectedFontSize = currentSize;
            }

            if (fontDialog.ShowDialog() == true)
            {
                MainTextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, fontDialog.SelectedFontFamily);
                MainTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontDialog.SelectedFontSize);
                StatusTextBlock.Text = "Шрифт змінено";
            }
        }

        private void TextColor_Click(object sender, RoutedEventArgs e)
        {
            // Створюємо діалогове вікно для вибору кольору
            var colorDialog = new ColorPickerDialog();

            // Встановлюємо поточний колір
            if (MainTextBox.Selection.GetPropertyValue(TextElement.ForegroundProperty) is SolidColorBrush currentBrush)
            {
                colorDialog.SelectedColor = currentBrush.Color;
            }

            if (colorDialog.ShowDialog() == true)
            {
                MainTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(colorDialog.SelectedColor));
                StatusTextBlock.Text = "Колір тексту змінено";
            }
        }

        private void BackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            // Створюємо діалогове вікно для вибору кольору фону
            var colorDialog = new ColorPickerDialog();

            // Встановлюємо поточний колір фону
            if (MainTextBox.Selection.GetPropertyValue(TextElement.BackgroundProperty) is SolidColorBrush currentBrush)
            {
                colorDialog.SelectedColor = currentBrush.Color;
            }

            if (colorDialog.ShowDialog() == true)
            {
                MainTextBox.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(colorDialog.SelectedColor));
                StatusTextBlock.Text = "Колір фону змінено";
            }
        }

        private void Indentation_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функція відступів буде реалізована у діалоговому вікні", "Відступи");
        }

        // === Вставка зображення ===
        private void InsertImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Зображення (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Всі файли (*.*)|*.*";
            openDialog.Title = "Виберіть зображення";

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(openDialog.FileName));
                    Image image = new Image();
                    image.Source = bitmap;
                    image.Width = bitmap.Width > 400 ? 400 : bitmap.Width;

                    // Вставляємо зображення в документ
                    InlineUIContainer container = new InlineUIContainer(image, MainTextBox.CaretPosition);

                    StatusTextBlock.Text = "Зображення вставлено";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка вставки зображення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // === Пошук та заміна ===
        private void Find_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функція пошуку буде реалізована у окремому вікні", "Пошук");
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функція заміни буде реалізована у окремому вікні", "Заміна");
        }

        // === Друк ===
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintDocument(((IDocumentPaginatorSource)MainTextBox.Document).DocumentPaginator, "Друк документу");
                    StatusTextBlock.Text = "Документ надіслано на друк";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка друку: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // === Інше ===
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Текстовий редактор\nВерсія 1.0\n\nЛабораторна робота №9\n© 2025",
                "Про програму",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateCursorPosition();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            isModified = true;
            UpdateTitle();
        }

        private void UpdateCursorPosition()
        {
            try
            {
                TextPointer caretPos = MainTextBox.CaretPosition;
                TextPointer start = MainTextBox.Document.ContentStart;

                int line = 1;
                int column = 1;

                while (start != null && start.CompareTo(caretPos) < 0)
                {
                    if (start.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                    {
                        string text = start.GetTextInRun(LogicalDirection.Forward);
                        int index = text.IndexOf('\n');
                        if (index >= 0)
                        {
                            line++;
                            column = 1;
                        }
                        else
                        {
                            column += text.Length;
                        }
                    }
                    start = start.GetNextContextPosition(LogicalDirection.Forward);
                }

                CursorPositionTextBlock.Text = $"Рядок: {line}, Стовпець: {column}";
            }
            catch
            {
                // Ігноруємо помилки
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!CheckSaveChanges())
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }
    }

    // === Діалог вибору шрифту ===
    public class FontSelectionDialog : Window
    {
        private ComboBox fontFamilyComboBox;
        private ComboBox fontSizeComboBox;
        private TextBlock previewTextBlock;

        public FontFamily SelectedFontFamily { get; set; } = new FontFamily("Segoe UI");
        public double SelectedFontSize { get; set; } = 12.0;

        public FontSelectionDialog()
        {
            Title = "Вибір шрифту";
            Width = 450;
            Height = 350;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid();
            grid.Margin = new Thickness(10);

            // Визначаємо рядки
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Панель вибору шрифту
            var fontPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };

            var fontLabel = new Label { Content = "Шрифт:", Width = 80 };
            fontFamilyComboBox = new ComboBox { Width = 200, Margin = new Thickness(5, 0, 0, 0) };
            foreach (FontFamily font in Fonts.SystemFontFamilies)
            {
                fontFamilyComboBox.Items.Add(font);
            }
            fontFamilyComboBox.SelectedItem = SelectedFontFamily;
            fontFamilyComboBox.SelectionChanged += (s, e) => UpdatePreview();

            fontPanel.Children.Add(fontLabel);
            fontPanel.Children.Add(fontFamilyComboBox);

            Grid.SetRow(fontPanel, 0);
            grid.Children.Add(fontPanel);

            // Панель вибору розміру
            var sizePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };

            var sizeLabel = new Label { Content = "Розмір:", Width = 80 };
            fontSizeComboBox = new ComboBox { Width = 100, Margin = new Thickness(5, 0, 0, 0) };
            double[] sizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            foreach (double size in sizes)
            {
                fontSizeComboBox.Items.Add(size);
            }
            fontSizeComboBox.SelectedItem = SelectedFontSize;
            fontSizeComboBox.SelectionChanged += (s, e) => UpdatePreview();

            sizePanel.Children.Add(sizeLabel);
            sizePanel.Children.Add(fontSizeComboBox);

            Grid.SetRow(sizePanel, 1);
            grid.Children.Add(sizePanel);

            // Попередній перегляд
            var previewBorder = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10)
            };
            previewTextBlock = new TextBlock
            {
                Text = "AaBbCcДдЄєЇї 0123456789",
                TextWrapping = TextWrapping.Wrap
            };
            previewBorder.Child = previewTextBlock;

            Grid.SetRow(previewBorder, 2);
            grid.Children.Add(previewBorder);

            // Кнопки
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = System.Windows.HorizontalAlignment.Right };

            var okButton = new Button { Content = "OK", Width = 80, Margin = new Thickness(0, 0, 10, 0), Padding = new Thickness(5) };
            okButton.Click += (s, e) => { DialogResult = true; Close(); };

            var cancelButton = new Button { Content = "Скасувати", Width = 80, Padding = new Thickness(5) };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            Grid.SetRow(buttonPanel, 3);
            grid.Children.Add(buttonPanel);

            Content = grid;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (fontFamilyComboBox.SelectedItem is FontFamily family)
            {
                previewTextBlock.FontFamily = family;
                SelectedFontFamily = family;
            }

            if (fontSizeComboBox.SelectedItem is double size)
            {
                previewTextBlock.FontSize = size;
                SelectedFontSize = size;
            }
        }
    }

    // === Діалог вибору кольору ===
    public class ColorPickerDialog : Window
    {
        private System.Windows.Shapes.Rectangle colorPreview;
        private Slider redSlider, greenSlider, blueSlider;
        private TextBox redTextBox, greenTextBox, blueTextBox;

        public Color SelectedColor { get; set; }

        public ColorPickerDialog()
        {
            Title = "Вибір кольору";
            Width = 400;
            Height = 350;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid();
            grid.Margin = new Thickness(10);

            // Визначаємо рядки
            for (int i = 0; i < 5; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Попередній перегляд кольору
            var previewLabel = new Label { Content = "Попередній перегляд:", Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(previewLabel, 0);
            grid.Children.Add(previewLabel);

            colorPreview = new System.Windows.Shapes.Rectangle
            {
                Width = 380,
                Height = 60,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Margin = new Thickness(0, 0, 0, 15)
            };
            Grid.SetRow(colorPreview, 1);
            grid.Children.Add(colorPreview);

            // Слайдери RGB
            CreateColorSlider(grid, 2, "Червоний (R):", out redSlider, out redTextBox);
            CreateColorSlider(grid, 3, "Зелений (G):", out greenSlider, out greenTextBox);
            CreateColorSlider(grid, 4, "Синій (B):", out blueSlider, out blueTextBox);

            // Встановлюємо початковий колір
            redSlider.Value = SelectedColor.R;
            greenSlider.Value = SelectedColor.G;
            blueSlider.Value = SelectedColor.B;

            // Швидкий вибір кольорів
            var quickColorsPanel = new WrapPanel { Margin = new Thickness(0, 15, 0, 15) };
            Color[] quickColors =
            {
                Colors.Black, Colors.White, Colors.Red, Colors.Green, Colors.Blue,
                Colors.Yellow, Colors.Orange, Colors.Purple, Colors.Pink, Colors.Brown,
                Colors.Gray, Colors.LightGray, Colors.DarkGray, Colors.Cyan, Colors.Magenta
            };

            foreach (var color in quickColors)
            {
                var colorButton = new Button
                {
                    Width = 30,
                    Height = 30,
                    Margin = new Thickness(2),
                    Background = new SolidColorBrush(color),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1)
                };
                colorButton.Click += (s, e) =>
                {
                    redSlider.Value = color.R;
                    greenSlider.Value = color.G;
                    blueSlider.Value = color.B;
                };
                quickColorsPanel.Children.Add(colorButton);
            }

            Grid.SetRow(quickColorsPanel, 5);
            grid.Children.Add(quickColorsPanel);

            // Кнопки
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = System.Windows.HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };

            var okButton = new Button { Content = "OK", Width = 80, Margin = new Thickness(0, 0, 10, 0), Padding = new Thickness(5) };
            okButton.Click += (s, e) => { DialogResult = true; Close(); };

            var cancelButton = new Button { Content = "Скасувати", Width = 80, Padding = new Thickness(5) };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            Grid.SetRow(buttonPanel, 6);
            grid.Children.Add(buttonPanel);

            Content = grid;
            UpdateColor();
        }

        private void CreateColorSlider(Grid grid, int row, string label, out Slider slider, out TextBox textBox)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };

            var labelControl = new Label { Content = label, Width = 120 };
            panel.Children.Add(labelControl);

            slider = new Slider { Width = 200, Minimum = 0, Maximum = 255, Margin = new Thickness(5, 0, 5, 0) };
            textBox = new TextBox { Width = 50, Text = "0" };

            // Локальні змінні для використання в lambda
            var localSlider = slider;
            var localTextBox = textBox;

            slider.ValueChanged += (s, e) =>
            {
                localTextBox.Text = ((int)localSlider.Value).ToString();
                UpdateColor();
            };
            panel.Children.Add(slider);

            textBox.TextChanged += (s, e) =>
            {
                if (int.TryParse(localTextBox.Text, out int value))
                {
                    if (value >= 0 && value <= 255)
                        localSlider.Value = value;
                }
            };
            panel.Children.Add(textBox);

            Grid.SetRow(panel, row);
            grid.Children.Add(panel);
        }

        private void UpdateColor()
        {
            byte r = (byte)redSlider.Value;
            byte g = (byte)greenSlider.Value;
            byte b = (byte)blueSlider.Value;

            SelectedColor = Color.FromRgb(r, g, b);
            colorPreview.Fill = new SolidColorBrush(SelectedColor);
        }
    }
}