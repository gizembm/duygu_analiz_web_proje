
using System;
using System.Linq;
using System.Web.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using yazilimMuhProje.Repository;
using yazilimMuhProje.Models.Entities;
using System.Data.Entity;
using yazilimMuhProje.Models.ViewModels;
using Newtonsoft.Json;

namespace yazilimMuhProje.Controllers
{
    public class ResimController : Controller
    {
        private readonly YorumlarRepository _yorumRepo;
        private readonly ResimlerRepository _resimRepo;
        private readonly YorumAnalizRepository _analizRepo;
        private static readonly string FlaskApiUrl = "http://127.0.0.1:5001/analyze";

        public ResimController()
        {
            _yorumRepo = new YorumlarRepository();
            _resimRepo = new ResimlerRepository();
            _analizRepo = new YorumAnalizRepository();
        }

        public ActionResult Detay(int id)
        {
            var resim = _resimRepo.TGet(id);
            if (resim == null) return HttpNotFound();

            var yorumlar = _yorumRepo.GetDbContext().Yorumlar
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
        public async Task<ActionResult> YorumEkle(int ResimId, string yorumMetni)
        {
            if (Session["KullaniciId"] == null)
            {
                return RedirectToAction("Giris", "Kullanici");
            }

            if (string.IsNullOrWhiteSpace(yorumMetni))
            {
                TempData["HataMesaji"] = "Boş yorum gönderemezsiniz!";
                return RedirectToAction("Detay", new { id = ResimId });
            }

            var yeniYorum = new Yorumlar
            {
                ResimId = ResimId,
                KullaniciId = (int)Session["KullaniciId"],
                YorumMetni = yorumMetni,
                OlusturmaTarihi = DateTime.Now
            };
            _yorumRepo.TAdd(yeniYorum);

            var analizSonucu = await AnalyzeComment(yorumMetni);

            if (analizSonucu == "Spam")
            {
                _yorumRepo.TRemove(yeniYorum);
                TempData["HataMesaji"] = "Yorumunuz spam olarak algılandı ve kaydedilmedi!";
                return RedirectToAction("Detay", new { id = ResimId });
            }

            var yeniAnaliz = new YorumAnaliz
            {
                YorumId = yeniYorum.YorumId,
                DuyguDurumu = analizSonucu
            };
            _analizRepo.TAdd(yeniAnaliz);

            return RedirectToAction("Detay", new { id = ResimId });
        }

        private async Task<string> AnalyzeComment(string yorumMetni)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // JSON verisini hazırla
                    var content = new StringContent(JsonConvert.SerializeObject(new { comment = yorumMetni }), Encoding.UTF8, "application/json");

                    // Flask API'sine POST isteği gönder
                    var response = await client.PostAsync(FlaskApiUrl, content);

                    // Başarılı yanıt alındıysa
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var jsonResponse = JsonConvert.DeserializeObject<dynamic>(result);

                        // Flask API'sinin döndürdüğü "prediction" anahtarını kullan
                        return jsonResponse.prediction;
                    }
                    else
                    {
                        // Başarısız yanıt durumunda hata mesajı döndür
                        var errorResult = await response.Content.ReadAsStringAsync();
                        var errorResponse = JsonConvert.DeserializeObject<dynamic>(errorResult);
                        return "Hata: " + (errorResponse.error ?? "Duygu analizi yapılamadı.");
                    }
                }
            }
            catch (Exception ex)
            {
                // İstisna durumunda hata mesajı logla ve döndür
                Console.WriteLine("Duygu analizi sırasında hata oluştu: " + ex.Message);
                return "Hata: Duygu analizi yapılamadı.";
            }
        }
    }
}


