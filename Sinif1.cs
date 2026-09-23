using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harcama_Takip1
{
    public class Sinif1
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } // Her harcama için benzersiz kimlik
        public decimal Miktar { get; set; } // Harcama miktarı
        public string Katagori { get; set; } // Harcama kategorisi
        public DateTime Tarih { get; set; } // Harcama tarihi
    }
}
