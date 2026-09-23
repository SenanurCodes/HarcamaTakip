using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Harcama_Takip1
{
    class HarcamaTest
    {
        // Main metod: Programın başlangıç noktası
        public static async Task Main(string[] args)
        {
            // 1️⃣ Veritabanı servisini oluştur
            var veritabani = new HarcamaVeritabaniServisi();

            // 2️⃣ Veritabanını başlat (tablo oluşturulur)
            await veritabani.InitializeAsync();

            // 3️⃣ Harcama yöneticisini oluştur
            var yonetici = new HarcamaYoneticisi(veritabani);

            // 4️⃣ Yeni harcamalar oluştur
            var harcama1 = new Sinif1 { Miktar = 100, Katagori = "Yemek", Tarih = DateTime.Now };
            var harcama2 = new Sinif1 { Miktar = 50, Katagori = "Ulaşım", Tarih = DateTime.Now };

            // 5️⃣ Harcamaları veritabanına ekle
            await yonetici.HarcamaEkleAsync(harcama1);
            await yonetici.HarcamaEkleAsync(harcama2);

            Console.WriteLine("Harcamalar eklendi.");

            // 6️⃣ Tüm harcamaları al ve yazdır
            var tumHarcamalar = await yonetici.TumHarcamalariGetirAsync();
            Console.WriteLine("Tüm Harcamalar:");
            foreach (var h in tumHarcamalar)
            {
                Console.WriteLine($"Id: {h.Id}, Miktar: {h.Miktar}, Katagori: {h.Katagori}, Tarih: {h.Tarih:dd/MM/yyyy}");
            }

            // 7️⃣ Belirli kategoriye göre filtrele
            var yemekHarcamalari = await yonetici.KatagoriyeGoreGetirAsync("Yemek");
            Console.WriteLine("\nYemek kategorisi harcamaları:");
            foreach (var h in yemekHarcamalari)
            {
                Console.WriteLine($"Id: {h.Id}, Miktar: {h.Miktar}, Katagori: {h.Katagori}");
            }

            // 8️⃣ Bir harcamayı sil (ilk kaydı)
            if (tumHarcamalar.Count > 0)
            {
                await yonetici.HarcamaSilAsync(tumHarcamalar[0]);
                Console.WriteLine($"\nId {tumHarcamalar[0].Id} silindi.");
            }

            // 9️⃣ Silindikten sonra kalan harcamalar
            var kalanHarcamalar = await yonetici.TumHarcamalariGetirAsync();
            Console.WriteLine("\nKalan Harcamalar:");
            foreach (var h in kalanHarcamalar)
            {
                Console.WriteLine($"Id: {h.Id}, Miktar: {h.Miktar}, Katagori: {h.Katagori}");
            }

            // 🔟 Toplam harcamayı göster
            var toplam = await yonetici.ToplamHarcamalariGetirAsync();
            Console.WriteLine($"\nToplam Harcama: {toplam}");
        }
    }
}
