using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Harcama_Takip1
{
    public class HarcamaYoneticisi
    {
        public readonly HarcamaVeritabaniServisi Veritabani; // Veritabanı servisi

        public HarcamaYoneticisi(HarcamaVeritabaniServisi veritabani)
        {
            Veritabani = veritabani; // Veritabanı servisini başlatır
        }

        // Tüm harcamaları getir
        public async Task<List<Sinif1>> TumHarcamalariGetirAsync()
        {
            return await Veritabani.TumHarcamalariGetirAsync();
        }

        // Toplam harcamayı hesapla
        public async Task<decimal> ToplamHarcamalariGetirAsync()
        {
            return await Veritabani.ToplamHarcamalariGetirAsync();
        }

        // Yeni harcama ekle
        public async Task HarcamaEkleAsync(Sinif1 harcama)
        {
            if (harcama.Miktar <= 0)
                throw new ArgumentException("Miktar sıfır veya negatif olamaz!");
            if (string.IsNullOrEmpty(harcama.Katagori))
                throw new ArgumentException("Kategori boş olamaz!");

            await Veritabani.HarcamaEkleAsync(harcama); // DOĞRU: parametre nesneyi kullan
        }

        // Harcama sil
        public async Task HarcamaSilAsync(Sinif1 harcama)
        {
            await Veritabani.HarcamaSilAsync(harcama); // DOĞRU: parametre nesneyi kullan
        }

        // Belirli kategoriye göre harcamaları getir
        public async Task<List<Sinif1>> KatagoriyeGoreGetirAsync(string katagori)
        {
            var harcamalar = await Veritabani.TumHarcamalariGetirAsync();
            return harcamalar.Where(h => h.Katagori == katagori).ToList();
        }
    }
}
