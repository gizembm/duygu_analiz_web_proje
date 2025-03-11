using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using yazilimMuhProje.Repository;
using yazilimMuhProje.Models.Entities;
using System.Data.Entity;
using yazilimMuhProje.Models.ViewModels;



namespace yazilimMuhProje.Controllers
{
    public class ResimController : Controller
    {
        // GET: Resim
        YorumlarRepository yorumRepo = new YorumlarRepository();
        ResimlerRepository resimRepo = new ResimlerRepository();

        public ActionResult Detay(int id)
        {
            var resim = resimRepo.TGet(id);
            var yorumlar = yorumRepo.GetDbContext().Yorumlar
                .Where(y => y.ResimId == id)
                .Include(y => y.Kullanicilar)
                .Include(y => y.YorumAnaliz)
                .ToList();

            var model = new ResimDetayViewModel
            {
                Resim = resim,
                Yorumlar = yorumlar
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult YorumEkle(int ResimId, string yorumMetni)
        {
            if (Session["KullaniciId"] == null)
            {
                return RedirectToAction("Giris", "Kullanici");
            }

            // Yorum ekleme
            var yeniYorum = new Yorumlar
            {
                ResimId = ResimId,
                KullaniciId = (int)Session["KullaniciId"], // Giriş yapan kullanıcının ID'si
                YorumMetni = yorumMetni,
                OlusturmaTarihi = DateTime.Now
            };

            yorumRepo.TAdd(yeniYorum);
            return RedirectToAction("Detay", new { id = ResimId });
        }
    }
}
