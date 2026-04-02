using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CleanCrow.ViewModels;

namespace CleanCrow.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            CarregarImagens();
        }
        
        private void CarregarImagens()
        {
            try
            {
                string pastaImagens = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Images");
                
                if (!Directory.Exists(pastaImagens))
                {
                    pastaImagens = @"E:\projetos\refatoração de projetos em C#\cleancrow\Resources\Images";
                }
                
                string logoPath = Path.Combine(pastaImagens, "crowico.png");
                if (File.Exists(logoPath))
                {
                    LogoImage.Source = CarregarBitmap(logoPath);
                }
                
                string broomPath = Path.Combine(pastaImagens, "broom.png");
                if (File.Exists(broomPath))
                {
                    BroomImage.Source = CarregarBitmap(broomPath);
                }
                
                string refreshPath = Path.Combine(pastaImagens, "refresh.png");
                if (File.Exists(refreshPath))
                {
                    RefreshImage.Source = CarregarBitmap(refreshPath);
                }
                
                string trashPath = Path.Combine(pastaImagens, "trash.png");
                if (File.Exists(trashPath))
                {
                    TrashImage.Source = CarregarBitmap(trashPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar imagens: {ex.Message}");
            }
        }
        
        private BitmapImage CarregarBitmap(string caminho)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(caminho);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }
    }
}