using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

// Harcama takip uygulamasının ana sayfa kod dosyası
namespace Harcama_Takip1
{
    public partial class MainPage : ContentPage
    {
        private readonly HarcamaYoneticisi _harcamaYoneticisi; // Harcama işlemlerini yöneten sınıf
        public ObservableCollection<Sinif1> Giderler { get; set; } // XAML'da bağlanacak harcama listesi

        // Kurucu metod, sayfa başlatıldığında çağrılır
        public MainPage()
        {
            InitializeComponent(); // XAML'daki UI bileşenlerini başlatır
            _harcamaYoneticisi = new HarcamaYoneticisi(new HarcamaVeritabaniServisi()); // Harcama yöneticisi örneği
            Giderler = new ObservableCollection<Sinif1>(); // Gözlemlenebilir koleksiyon oluşturur
            BindingContext = this; // XAML bağlamını bu sınıfa ayarlar
            _ = InitializeDatabaseAsync(); // Veritabanını başlatır
            _ = YukleHarcamalarAsync(); // Harcamaları yükler
            GrafikCiz(); // Grafiği başlatır
        }

        // Veritabanını başlat
        private async Task InitializeDatabaseAsync()
        {
            await _harcamaYoneticisi.Veritabani.InitializeAsync(); // Veritabanını başlatır
        }

        // Harcamaları yükler ve toplamı günceller
        private async Task YukleHarcamalarAsync()
        {
            try
            {
                Giderler.Clear(); // Mevcut listeyi temizler
                var harcamalar = await _harcamaYoneticisi.TumHarcamalariGetirAsync(); // Tüm harcamaları alır
                foreach (var harcama in harcamalar)
                {
                    Giderler.Add(harcama); // Harcamaları koleksiyona ekler
                }
                await ToplamGuncelleAsync(); // Toplam harcama etiketini günceller
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Veriler yüklenirken hata oluştu: {ex.Message}", "Tamam");
            }
        }

        // Ekle butonuna tıklandığında çağrılır
        private async void Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (!decimal.TryParse(TutarGirişi.Text, out decimal miktar) || string.IsNullOrEmpty(KatagoriTürü.SelectedItem?.ToString()))
                {
                    await DisplayAlert("Hata", "Lütfen geçerli bir miktar ve kategori seçin.", "Tamam");
                    return;
                }

                var yeniHarcama = new Sinif1 // Yeni harcama nesnesi oluşturur
                {
                    Miktar = miktar,
                    Katagori = KatagoriTürü.SelectedItem.ToString(),
                    Tarih = TarihSeçici.Date
                };

                await _harcamaYoneticisi.HarcamaEkleAsync(yeniHarcama); // Harcamayı ekler
                Giderler.Add(yeniHarcama); // Koleksiyona ekler
                await ToplamGuncelleAsync(); // Toplamı günceller
                GrafikCiz(); // Grafiği yeniden çizer
                TemizleForm(); // Formu temizler
                await DisplayAlert("Başarılı", "Harcama başarıyla eklendi.", "Tamam");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Harcama eklenirken hata oluştu: {ex.Message}", "Tamam");
            }
        }

        // Sil butonuna tıklandığında çağrılır
        private async void DeleteButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Microsoft.Maui.Controls.Button button && button.BindingContext is Sinif1 harcama)
            {
                bool cevap = await DisplayAlert("Sil", "Bu harcamayı silmek istediğinizden emin misiniz?", "Evet", "Hayır");
                if (cevap)
                {
                    await _harcamaYoneticisi.HarcamaSilAsync(harcama); // Harcamayı siler
                    Giderler.Remove(harcama); // Koleksiyondan kaldırır
                    await ToplamGuncelleAsync(); // Toplamı günceller
                    GrafikCiz(); // Grafiği yeniden çizer
                    await DisplayAlert("Başarılı", "Harcama silindi.", "Tamam");
                }
            }
        }

        // Toplam harcama etiketini günceller
        private async Task ToplamGuncelleAsync()
        {
            var toplam = await _harcamaYoneticisi.ToplamHarcamalariGetirAsync(); // Toplam miktarı alır
            ToplameEtiket.Text = $"Toplam: {toplam:C}"; // Etiketi günceller
        }

        // Grafiği çizer (SkiaSharp ile)
        private void GrafikCiz()
        {
            var skiaView = new SKCanvasView(); // SkiaSharp tuval oluşturur
            skiaView.PaintSurface += OnPaintSurface; // Tuval çizim olayını bağlar
            GrafikKontrol.Children.Clear(); // Mevcut grafik kontrolünü temizler
            GrafikKontrol.Children.Add(skiaView); // Yeni tuvali ekler
        }

        // SkiaSharp ile grafik çizimi
        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var surface = e.Surface; // Çizim yüzeyini alır
            var canvas = surface.Canvas; // Tuvali alır
            canvas.Clear(SKColors.White); // Tuvali temizler

            var harcamalar = Giderler
                .GroupBy(h => h.Katagori)
                .Select(g => new { Katagori = g.Key, Toplam = g.Sum(h => h.Miktar) })
                .ToList(); // Kategorilere göre toplamları hesaplar

            float barWidth = e.Info.Width / (float)(harcamalar.Count + 1); // Çubuk genişliğini hesaplar
            float maxToplam = harcamalar.Any() ? harcamalar.Max(h => (float)h.Toplam) : 1; // Maksimum toplamı alır
            float heightScale = e.Info.Height / maxToplam; // Yükseklik ölçeğini hesaplar

            for (int i = 0; i < harcamalar.Count; i++)
            {
                var harcama = harcamalar[i];
                var color = new KatagoriColorConverter().Convert(harcama.Katagori, typeof(Color), null, null) as Color; // Kategoriye göre renk alır
                var paint = new SKPaint { Color = SKColor.Parse(color.ToHex()) }; // Boya nesnesi oluşturur
                float x = i * barWidth; // Çubuğun x konumunu belirler
                float height = (float)harcama.Toplam * heightScale; // Çubuğun yüksekliğini hesaplar
                canvas.DrawRect(x, e.Info.Height - height, barWidth - 5, height, paint); // Çubuğu çizer
            }

            if (harcamalar.Count == 0)
            {
                var paint = new SKPaint
                {
                    Color = SKColors.Gray,
                    TextSize = 20,
                    IsAntialias = true
                };
                canvas.DrawText("Henüz harcama kaydı bulunmuyor", e.Info.Width / 2 - 100, e.Info.Height / 2, paint);
            }
        }

        // Formu temizler
        private void TemizleForm()
        {
            TutarGirişi.Text = string.Empty; // Miktar girişini temizler
            KatagoriTürü.SelectedIndex = -1; // Kategori seçimini sıfırlar
            TarihSeçici.Date = DateTime.Now; // Tarihi bugüne ayarlar
        }
    }

    // Kategorilere göre renk döndüren dönüştürücü
    public class KatagoriColorConverter : IValueConverter
    {
        // Kategoriye göre renk döndürür
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string katagori = value as string; // Gelen değeri kategori olarak alır
            return katagori switch
            {
                "Yiyecek" => Color.FromArgb("#F87171"), // Kırmızı
                "Ulaşım" => Color.FromArgb("#60A5FA"), // Mavi
                "Eğlence" => Color.FromArgb("#FBBF24"), // Sarı
                "Eğitim" => Color.FromArgb("#34D399"), // Yeşil
                "Alışveriş" => Color.FromArgb("#A78BFA"), // Mor
                "Sağlık" => Color.FromArgb("#F472B6"), // Pembe
                "Kira" => Color.FromArgb("#FBBF24"), // Sarı
                "Tatil" => Color.FromArgb("#3B82F6"), // Lacivert
                "Fatura" => Color.FromArgb("#9CA3AF"), // Gri
                "İhtiyaçlar" => Color.FromArgb("#6B7280"), // Koyu gri
                _ => Color.FromArgb("#6B7280") // Varsayılan koyu gri
            };
        }

        // Geri dönüşüm desteklenmiyor
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException(); // Geri dönüşüm desteklenmiyor
        }
    }
}