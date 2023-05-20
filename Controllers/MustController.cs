using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using WebApplication1.Models;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MustController : Controllerbase
    {
        Random rn = new Random();
        private string randomchar(int uzunluk)
        {
            string randomchar = "";
            for (int sayac = 0; sayac < uzunluk; sayac++)
            {
                randomchar += ((char)rn.Next('A', 'Z')).ToString();
            }
            return randomchar;
        }

        string[] sehirler = { "Ankara", "İstanbul", "İzmir", "Bursa", "Edirne", "Konya", "Antalya", "Diyarbakır", "Van", "Rize" };
        private AppDbContext _context;
        public MustController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost(Name = "MusteriveSepetOlustur")]
        public void MusteriveSepetOlustur(int musteriAdet, int sepetAdet)
        {
            for (int i = 0; i < musteriAdet; i++)
            {
                int sehir = rn.Next(1, 10);
                Musteri nMusteri = new Musteri { Ad = randomchar(5), Soyad = randomchar(5), Sehir = sehirler[sehir] };
                _context.TblMusteri.Add(nMusteri);
                _context.SaveChanges();
            }

            List<Musteri> Musteriler = _context.TblMusteri.OrderByDescending(x => x.Id).Take(musteriAdet).ToList();
            for (int i = 0; i < sepetAdet; i++)
            {
                int b = rn.Next(0, musteriAdet);
                Sepet nSepet = new Sepet { MusteriId = Musteriler[b].Id };
                _context.TblSepet.Add(nSepet);
                _context.SaveChanges();
            }

            List<Sepet> Sepetlist = _context.TblSepet.OrderByDescending(x => x.Id).Take(sepetAdet).ToList();
            foreach (var item in Sepetlist)
            {
                int a = rn.Next(1, 5);
                for (int i = 0; i < a; i++)
                {
                    SepetUrun nSepetUrun = new SepetUrun { Aciklama = randomchar(10), SepetId = item.Id, Tutar = rn.Next(100, 1000) };
                    _context.TblSepetUrun.Add(nSepetUrun);
                    _context.SaveChanges();
                }
            }
        }

        // 1. YÖNTEM

        [HttpGet(Name = "SehirAnaliz")]
        public async Task<IActionResult> GetSehirler()
        {
            var sorgu = await (from musteriSorguTutar in _context.TblMusteri
                               join sepet in _context.TblSepet on musteriSorguTutar.Id equals sepet.MusteriId
                               join sepetUrun in _context.TblSepetUrun on sepet.Id equals sepetUrun.SepetId
                               group new { sepet, sepetUrun } by musteriSorguTutar.Sehir into sehirGrup
                               select new
                               {
                                   Sehir = sehirGrup.Key,
                                   SepetSayisi = sehirGrup.Select(x => x.sepet.Id).Distinct().Count(),
                                   ToplamTutar = sehirGrup.Sum(x => x.sepetUrun.Tutar)
                               }).ToListAsync();
            return new OkObjectResult(sorgu);
        }


        // 2. YÖNTEM

        //string baglantiad = "Data Source=DESKTOP-3AFCJCF\\SQLEXPRESS;Initial Catalog=SepetEntity;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
        //[HttpGet(Name = "DtoSehirAnaliz")]
        //public List<DtoSehirAnaliz> DtoAnaliz()
        //{
        //    SqlConnection baglanti = new SqlConnection();
        //    baglanti.ConnectionString = baglantiad;
        //    baglanti.Open();
        //    DataTable dtSehirler = new DataTable();
        //    using (var daSehirler = new SqlDataAdapter("SELECT Sehir FROM TblMusteri GROUP BY Sehir", baglantiad))
        //    {
        //        daSehirler.Fill(dtSehirler);
        //    }

        //    List<string> sehirListesi = new List<string>();
        //    List<DtoSehirAnaliz> dtoAnaliz = new List<DtoSehirAnaliz>();
        //    foreach (DataRow drSehirler in dtSehirler.Rows)
        //    {
        //        int adet = 0;
        //        int tutar = 0;
        //        DataTable dtMusteriAdet = new DataTable();
        //        DataTable dtTutar = new DataTable();
        //        using (var daMusteriAdet = new SqlDataAdapter("SELECT * FROM TblMusteri WHERE Sehir = '" + drSehirler["Sehir"].ToString() + "'", baglantiad))
        //        {
        //            daMusteriAdet.Fill(dtMusteriAdet);
        //        }

        //        string musteriIdleri = "";
        //        foreach (DataRow drMusteriAdet in dtMusteriAdet.Rows)
        //        {
        //            musteriIdleri += drMusteriAdet["Id"].ToString() + ",";
        //        }
        //        if (musteriIdleri != "")
        //        {
        //            DataTable dtSepetAdet = new DataTable();
        //            using (var daSepetAdet = new SqlDataAdapter("SELECT * FROM TblSepet WHERE MusteriId IN (" + musteriIdleri.TrimEnd(',') + ")", baglantiad))
        //            {
        //                daSepetAdet.Fill(dtSepetAdet);
        //                adet = dtSepetAdet.Rows.Count;
        //            }

        //            string sepetIdleri = "";
        //            foreach (DataRow drSepetAdet in dtSepetAdet.Rows)
        //            {
        //                sepetIdleri += drSepetAdet["Id"].ToString() + ",";
        //            }
        //            if (sepetIdleri != "")
        //            {
        //                using (var daTutar = new SqlDataAdapter("SELECT SUM(TUTAR) AS TUTAR FROM TblSepetUrun WHERE SepetId IN (" + sepetIdleri.TrimEnd(',') + ")", baglantiad))
        //                {
        //                    daTutar.Fill(dtTutar);
        //                }
        //                tutar = Convert.ToInt32(dtTutar.Rows[0]["Tutar"]);
        //                dtoAnaliz.Add(new DtoSehirAnaliz()
        //                {
        //                    SehirAdi = drSehirler["Sehir"].ToString(),
        //                    SepetAdet = adet,
        //                    ToplamTutar = tutar
        //                });
        //            }
        //        }
        //    }
        //    baglanti.Close();
        //    return dtoAnaliz;
        //}
    }
}

public class Controllerbase
{

}
