# MAD MLAgent Drift

**Proje Bağlantısı:** [GitHub - batuhancetinkaya1/MAD-MLAgentDrift](https://github.com/batuhancetinkaya1/MAD-MLAgentDrift)

Bu proje, Unity ve ML-Agents kullanarak 2D ortamda drift yapabilen bir araba sürüş ajanı geliştirmeyi amaçlar. Yapay zekâ, birden fazla pistte aynı anda eğitilerek genel sürüş yeteneklerini ve adaptasyon becerisini geliştirir.

---

## İçindekiler
- [Özellikler](#özellikler)
- [Hikâye ve Pistler](#hikâye-ve-pistler)
  - [Catalunya](#catalunya)
  - [Istanbul Park](#istanbul-park)
  - [Monaco](#monaco)
- [Model ve Eğitim](#model-ve-eğitim)
- [Car Prefab'leri](#car-prefableri)
- [Kurulum](#kurulum)
- [Kullanım](#kullanım)
- [Görseller](#görseller)
- [Katkıda Bulunma](#katkıda-bulunma)
- [Lisans](#lisans)

---

## Özellikler
- **Çoklu Pist Eğitimi:** Ajan, üç farklı pistte (Catalunya, Istanbul Park, Monaco) eşzamanlı olarak eğitilir.  
- **Gelişmiş Gözlem Uzayı:** Ajan, bir sonraki checkpoint’e olan uzaklığı ve açı farkını gözlemleyerek daha tutarlı ve etkili sürüş kararları alır.  
- **Drift Fiziği:** 2D ortamda gerçekçi drift deneyimi sağlayacak özel fizik ayarları.  
- **En İyi Model:** Eğitim sonucunda elde edilen en iyi model dosyası: `5499989.onnx`.  
- **Kolay Entegrasyon:** Car prefab’leri farklı pistlere otomatik olarak ölçeklenerek adapte olur.

---

## Hikâye ve Pistler

### Catalunya
Barselona’nın yakınlarında bulunan **Catalunya**, uzun düzlükleri ve akıcı virajlarıyla ünlüdür. Burada hızlı tepkiler ve istikrarlı driftler bir araya gelerek sürücünün sabrını ve aracın dengesini sınar.

### Istanbul Park
Kıvrımlı yapısı ve ünlü 8. virajıyla bilinen **Istanbul Park**, yüksek hız ve keskin dönüşlerin birleştiği zorlu bir parkurdur. Pistteki dalgalı eğimler, drift kabiliyetinizi gerçekten sınar.

### Monaco
Dar sokakları ve keskin virajlarıyla **Monaco**, hataya yer bırakmayan bir pisttir. Duvarlar çok yakındır ve ufak bir hata çarpışmaya neden olabilir. Bu pistte hassas direksiyon kontrolü ve stratejik yavaşlama becerileri ön plana çıkar.

---

## Model ve Eğitim
Proje, Unity ML-Agents’ı kullanarak derin takviye öğrenmesi (Deep Reinforcement Learning) yaklaşımıyla geliştirilmiştir.  
- **Çoklu Pist Eğitimi:** Tüm pistler tek seferde eğitilerek ajan her piste uyum sağlamayı öğrenir.  
- **Eğitim Süreci:**  
  - Ortalama çarpma sayısı 15’ten 6’ya düşürülmüştür.  
  - TensorBoard üzerinden anlık eğitim grafikleri takip edilmiştir.  
- **En İyi Model Dosyası:** `5499989.onnx` (Ajanın en yüksek performansı elde ettiği model).

---

## Car Prefab'leri
Her pist için ortak kullanılan `CarAgent` prefab’i bulunur. Bu prefab, pist boyutlarına göre otomatik olarak ölçeklenir ve şu unsurları içerir:
- **Drift Fiziği Bileşenleri:** Drift esnasında aracın yol tutuşunu ayarlayan özel scriptler.  
- **Sensör Bileşenleri:** Ajanın, duvarları ve checkpoint’leri algılayarak çevreyi yorumlamasını sağlayan kolajlı sensörler.  
- **ML-Agents Bağlantıları:** Davranış parametreleri, gözlem alanı ve eylem tanımları.

---

## Kurulum
1. **Unity ve ML-Agents Kurulumu**  
   - Unity 2023.x veya üstü sürümü indirin.  
   - Proje içindeki *Package Manager* üzerinden `ML-Agents` paketini ekleyin (varsa güncelleyin).

2. **Proje Dosyalarını İndirin**  
   ```bash
   git clone https://github.com/batuhancetinkaya1/MAD-MLAgentDrift.git
