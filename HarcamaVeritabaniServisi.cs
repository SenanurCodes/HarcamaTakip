using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Harcama_Takip1
{
    public class HarcamaVeritabaniServisi
    {
        private readonly SQLiteAsyncConnection _database; // SQLite bağlantısı

        public HarcamaVeritabaniServisi()
        {
            _database = new SQLiteAsyncConnection(GetDatabasePath()); // Veritabanı bağlantısını kurar
        }

        // Veritabanını başlat
        public async Task InitializeAsync()
        {
            await _database.CreateTableAsync<Sinif1>(); // Harcama tablosunu oluşturur
        }

        private string GetDatabasePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "harcamalar.db3"); // Veritabanı yolu
        }

        // Tüm harcamaları getir
        public async Task<List<Sinif1>> TumHarcamalariGetirAsync()
        {
            return await _database.Table<Sinif1>().ToListAsync(); // Tüm harcamaları async getirir
        }

        // Yeni harcama ekle
        public async Task<int> HarcamaEkleAsync(Sinif1 harcama)
        {
            return await _database.InsertAsync(harcama); // Harcamayı veritabanına ekler
        }

        // Harcama sil
        public async Task<int> HarcamaSilAsync( Sinif1 harcama)
        {
            return await _database.DeleteAsync(harcama); // Harcamayı veritabanından siler
        }

        // Toplam harcamayı getir
        public async Task<decimal> ToplamHarcamalariGetirAsync()
        {
            var harcamalar = await _database.Table<Sinif1>().ToListAsync(); // Tüm harcamaları getirir
            return harcamalar.Sum(h => h.Miktar); // Toplamı hesaplar
        }
    }
}