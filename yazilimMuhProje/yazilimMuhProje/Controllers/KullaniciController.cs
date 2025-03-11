using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using yazilimMuhProje.Models.Entities;
using yazilimMuhProje.Repository;

namespace yazilimMuhProje.Controllers
{
    public class KullaniciController : Controller
    {
        // GET: Kullanici
        private readonly GenericRepositories<Kullanicilar> kullaniciRepo = new GenericRepositories<Kullanicilar>();

        // Kayıt Ol Sayfası
        public ActionResult KayitOl()
        {
            return View();
        }

        [HttpPost]
        public ActionResult KayitOl(Kullanicilar kullanici, string SifreOnay)
        {
            if (ModelState.IsValid)
            {
                // Şifre ve şifre onayı eşleşiyor mu?
                if (kullanici.Sifre != SifreOnay)
                {
                    ModelState.AddModelError("", "Şifreler eşleşmiyor.");
                    return View(kullanici);
                }

                // Veritabanına yeni kullanıcıyı ekle
                kullanici.KayitTarihi = DateTime.Now;
                kullaniciRepo.TAdd(kullanici);

                return RedirectToAction("Giris");
            }
            return View(kullanici);
        }

        // Giriş Yap Sayfası
        public ActionResult Giris()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Giris(string Eposta, string Sifre)
        {
            var kullanici = kullaniciRepo.Find(k => k.Eposta == Eposta && k.Sifre == Sifre);
            if (kullanici != null)
            {
                Session["KullaniciId"] = kullanici.KullaniciId;
                Session["KullaniciAdi"] = kullanici.KullaniciAdi;
                return RedirectToAction("Index", "Home"); // Ana sayfaya yönlendir
            }
            else
            {
                ModelState.AddModelError("", "Eposta veya şifre hatalı.");
                return View();
            }
        }


        // Şifremi Unuttum Sayfası
        public ActionResult SifremiUnuttum()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SifremiUnuttum(string Eposta, string YeniSifre, string SifreOnay)
        {
            var kullanici = kullaniciRepo.Find(k => k.Eposta == Eposta);
            if (kullanici == null)
            {
                ModelState.AddModelError("", "Bu e-posta adresi sistemde bulunamadı.");
                return View();
            }

            if (YeniSifre != SifreOnay)
            {
                ModelState.AddModelError("", "Şifreler eşleşmiyor.");
                return View();
            }

            // Şifreyi güncelle
            kullanici.Sifre = YeniSifre;
            kullaniciRepo.TUpdate(kullanici);

            ViewBag.Mesaj = "Şifreniz başarıyla güncellendi. Şimdi giriş yapabilirsiniz.";
            return RedirectToAction("Giris");
        }

        // Çıkış Yap
        public ActionResult Cikis()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }


    }
}
