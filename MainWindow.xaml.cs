using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Aspose.Words; // Не забудьте добавить ссылку на Aspose.Words
using Aspose.Pdf;   // Не забудьте добавить ссылку на Aspose.Pdf
using MediaToolkit;  // Не забудьте добавить ссылку на MediaToolkit
using MediaToolkit.Model;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        private string _selectedImagePath;
        private string _selectedTextFilePath;
        private string _selectedMediaFilePath;
        private const string HistoryFilePath = "conversion_history.txt"; // Путь к файлу для хранения истории
        private string _lastConvertedFilePath; // Хранит путь к последнему конвертированному файлу

        public MainWindow()
        {
            InitializeComponent(); // Инициализация компонентов из XAML
            LoadConversionHistory(); // Загрузка истории конвертаций при запуске
        }

        // Image Conversion Methods
        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Выберите изображение"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedImagePath = openFileDialog.FileName;
                SelectedImagePath.Text = _selectedImagePath;

                // Запуск анимации
                AnimateTextBlock(SelectedImagePath);
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedImagePath))
            {
                StatusTextBlock.Text = "Пожалуйста, выберите изображение.";
                return;
            }

            string selectedFormat = ((ComboBoxItem)FormatComboBox.SelectedItem)?.Tag.ToString();
            if (string.IsNullOrEmpty(selectedFormat))
            {
                StatusTextBlock.Text = "Пожалуйста, выберите формат.";
                return;
            }

            try
            {
                string newFilePath = Path.ChangeExtension(_selectedImagePath, selectedFormat);
                BitmapImage bitmap = new BitmapImage(new Uri(_selectedImagePath));

                using (FileStream stream = new FileStream(newFilePath, FileMode.Create))
                {
                    BitmapEncoder encoder = GetEncoder(selectedFormat);
                    if (encoder != null)
                    {
                        encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        encoder.Save(stream);
                    }
                }

                StatusTextBlock.Text = $"Изображение успешно конвертировано в {newFilePath}.";
                AddToConversionHistory(newFilePath, selectedFormat);
                _lastConvertedFilePath = newFilePath;
                ConversionHistoryListBox.Items.Add($"{DateTime.Now}: {newFilePath} -> {selectedFormat}"); // Обновление истории

                OpenFolderButton.IsEnabled = true; // Активируем кнопку после успешной конвертации
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Ошибка: {ex.Message}";
            }
        }

        // Text File Conversion Methods
        private void SelectTextFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files|*.txt;*.pdf;*.docx;*.doc", // Добавлен .doc
                Title = "Выберите текстовый файл"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedTextFilePath = openFileDialog.FileName;
                SelectedTextFilePath.Text = _selectedTextFilePath;

                // Запуск анимации
                AnimateTextBlock(SelectedTextFilePath);
            }
        }

        private void ConvertTextButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedTextFilePath))
            {
                TextStatusTextBlock.Text = "Пожалуйста, выберите текстовый файл.";
                return;
            }

            string selectedFormat = ((ComboBoxItem)TextFormatComboBox.SelectedItem)?.Tag.ToString();
            if (string.IsNullOrEmpty(selectedFormat))
            {
                TextStatusTextBlock.Text = "Пожалуйста, выберите формат.";
                return;
            }

            try
            {
                ConvertTextFile(selectedFormat); // Вызов нового метода для конвертации
                OpenFolderButton.IsEnabled = true; // Активируем кнопку после успешной конвертации
            }
            catch (Exception ex)
            {
                TextStatusTextBlock.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void ConvertTextFile(string selectedFormat)
        {
            string newFilePath = Path.ChangeExtension(_selectedTextFilePath, selectedFormat);

            // Конвертация в зависимости от выбранного формата
            switch (selectedFormat)
            {
                case "pdf":
                    // Конвертация в PDF
                    var docPdf = new Aspose.Words.Document(_selectedTextFilePath);
                    docPdf.Save(newFilePath, Aspose.Words.SaveFormat.Pdf);
                    break;

                case "docx":
                    // Конвертация в DOCX
                    var docx = new Aspose.Words.Document(_selectedTextFilePath);
                    docx.Save(newFilePath, Aspose.Words.SaveFormat.Docx);
                    break;

                case "txt":
                    // Конвертация в TXT
                    var txtDoc = new Aspose.Words.Document(_selectedTextFilePath);
                    txtDoc.Save(newFilePath, Aspose.Words.SaveFormat.Text);
                    break;

                case "doc":
                    // Конвертация в DOC
                    var docDoc = new Aspose.Words.Document(_selectedTextFilePath);
                    docDoc.Save(newFilePath, Aspose.Words.SaveFormat.Doc);
                    break;

                default:
                    throw new InvalidOperationException("Неподдерживаемый формат.");
            }

            TextStatusTextBlock.Text = $"Текстовый файл успешно конвертирован в {newFilePath}.";
            AddToConversionHistory(newFilePath, selectedFormat);
            _lastConvertedFilePath = newFilePath; // Обновляем путь к последнему конвертированному файлу

            // Обновление списка истории конвертаций
            ConversionHistoryListBox.Items.Add($"{DateTime.Now}: {newFilePath} -> {selectedFormat}");
        }

        // Multimedia File Conversion Methods
        private void SelectMediaFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Media Files|*.mp3;*.wav;*.mp4;*.avi",
                Title = "Выберите мультимедийный файл"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedMediaFilePath = openFileDialog.FileName;
                SelectedMediaFilePath.Text = _selectedMediaFilePath;

                // Запуск анимации
                AnimateTextBlock(SelectedMediaFilePath);
            }
        }

        private void AnimateTextBlock(TextBlock textBlock)
        {
            // Устанавливаем начальное значение прозрачности
            textBlock.Opacity = 0;

            // Создаем анимацию
            var fadeInAnimation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            textBlock.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
        }


        private async void ConvertMediaButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedMediaFilePath))
            {
                MediaStatusTextBlock.Text = "Пожалуйста, выберите мультимедийный файл.";
                return;
            }

            string selectedFormat = ((ComboBoxItem)MediaFormatComboBox.SelectedItem)?.Tag.ToString();
            if (string.IsNullOrEmpty(selectedFormat))
            {
                MediaStatusTextBlock.Text = "Пожалуйста, выберите формат.";
                return;
            }

            try
            {
                string newFilePath = Path.ChangeExtension(_selectedMediaFilePath, selectedFormat);
                await ConvertMediaFile(_selectedMediaFilePath, newFilePath);

                MediaStatusTextBlock.Text = $"Мультимедийный файл успешно конвертирован в {newFilePath}.";
                AddToConversionHistory(newFilePath, selectedFormat);
                _lastConvertedFilePath = newFilePath; // Обновляем путь к последнему конвертированному файлу

                // Обновление списка истории конвертаций
                ConversionHistoryListBox.Items.Add($"{DateTime.Now}: {newFilePath} -> {selectedFormat}");
                OpenFolderButton.IsEnabled = true; // Активируем кнопку после успешной конвертации
            }
            catch (Exception ex)
            {
                MediaStatusTextBlock.Text = $"Ошибка: {ex.Message}";
            }
        }

        private async Task ConvertMediaFile(string inputFilePath, string outputFilePath)
        {
            var inputFile = new MediaFile { Filename = inputFilePath };
            var outputFile = new MediaFile { Filename = outputFilePath };

            using (var engine = new Engine())
            {
                await Task.Run(() => engine.Convert(inputFile, outputFile));
            }
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_lastConvertedFilePath) && File.Exists(_lastConvertedFilePath))
            {
                string folderPath = Path.GetDirectoryName(_lastConvertedFilePath);
                Process.Start(new ProcessStartInfo("explorer.exe", folderPath));
            }
            else
            {
                MessageBox.Show("Нет последнего конвертированного файла.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void FormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StatusTextBlock.Text = "Выбран формат: " + ((ComboBoxItem)FormatComboBox.SelectedItem)?.Content;
        }

        private void TextFormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextStatusTextBlock.Text = "Выбран формат: " + ((ComboBoxItem)TextFormatComboBox.SelectedItem)?.Content;
        }

        private void MediaFormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MediaStatusTextBlock.Text = "Выбран формат: " + ((ComboBoxItem)MediaFormatComboBox.SelectedItem)?.Content;
        }

        private BitmapEncoder GetEncoder(string format)
        {
            return format switch
            {
                "jpg" => new JpegBitmapEncoder(),
                "png" => new PngBitmapEncoder(),
                "bmp" => new BmpBitmapEncoder(),
                "gif" => new GifBitmapEncoder(),
                _ => null,
            };
        }

        private void AddToConversionHistory(string filePath, string format)
        {
            using (StreamWriter writer = new StreamWriter(HistoryFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {filePath} -> {format}");
            }
        }

        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(HistoryFilePath))
            {
                File.Delete(HistoryFilePath);
            }

            ConversionHistoryListBox.Items.Clear();
            HistoryStatusTextBlock.Text = "История конвертаций очищена.";
        }

        private void LoadConversionHistory()
        {
            if (File.Exists(HistoryFilePath))
            {
                string[] historyLines = File.ReadAllLines(HistoryFilePath);
                foreach (string line in historyLines)
                {
                    ConversionHistoryListBox.Items.Add(line);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Здесь можно добавить код для сохранения состояния приложения, если это необходимо
        }

        // Обработчики событий для перетаскивания файлов
        private void ImageTab_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                ShowPlusSign(PlusSignImage);
                ShowPlusSign(DragFilesText);
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void ImageTab_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    _selectedImagePath = files[0];
                    SelectedImagePath.Text = _selectedImagePath;
                }
            }
            HidePlusSign(PlusSignImage);
            HidePlusSign(DragFilesText);
        }
        private void ImageTab_DragLeave(object sender, DragEventArgs e)
        {
            HidePlusSign(PlusSignImage);
            HidePlusSign(DragFilesText);
        }

        private void TextTab_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                ShowPlusSign(PlusSignText);
                ShowPlusSign(DragFilesTextText);
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void TextTab_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    _selectedTextFilePath = files[0];
                    SelectedTextFilePath.Text = _selectedTextFilePath;
                }
            }
            HidePlusSign(PlusSignText);
            HidePlusSign(DragFilesTextText);
        }

        private void TextTab_DragLeave(object sender, DragEventArgs e)
        {
            HidePlusSign(PlusSignText);
            HidePlusSign(DragFilesTextText);
        }

        private void MediaTab_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                ShowPlusSign(PlusSignMedia);
                ShowPlusSign(DragFilesTextMedia);
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void MediaTab_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    _selectedMediaFilePath = files[0];
                    SelectedMediaFilePath.Text = _selectedMediaFilePath;
                }
            }
            HidePlusSign(PlusSignMedia);
            HidePlusSign(DragFilesTextMedia);
        }

        private void MediaTab_DragLeave(object sender, DragEventArgs e)
        {
            HidePlusSign(PlusSignMedia);
            HidePlusSign(DragFilesTextMedia);
        }

        private void ShowPlusSign(TextBlock plusSign)
        {
            plusSign.Opacity = 1; // Устанавливаем видимость знака "+"
        }

        private void HidePlusSign(TextBlock plusSign)
        {
            plusSign.Opacity = 0; // Скрываем знак "+"
        }
    }
}