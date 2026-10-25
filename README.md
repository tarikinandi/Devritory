# Devritory

Merhaba   
Bu proje, Kocaeli Üniversitesi Bilgisayar Mühendisliği öğrencisi olarak geliştirdiğim, modern özelliklere sahip bir blog uygulamasıdır. Hem kişisel öğrenim sürecimi ilerletmek hem de portföyümde güçlü bir yer edinmesi amacıyla geliştirdim.

## Amaç
Devritory, kullanıcıların blog yazılarını paylaşabildiği, yorum alabildiği ve içeriklerin yönetilebildiği profesyonel görünümlü bir blog platformudur.

## Kullanılan Teknolojiler
- **ASP.NET Core 9.0 (MVC)**
- **Entity Framework Core** (Code-first & Migration)
- **SQL Server** (Veritabanı)
- **JWT + Cookie Authentication**
- **Bootstrap 5** & **Animate.css**
- **jQuery, AJAX**
- **SweetAlert2** & **Toastify.js**
- **ViewComponent**, **PartialView**
- **Role-based Admin Panel**
- **Unit Test (Moq)**
- **Google reCAPTCHA**
- **PDF export**, **SEO-Friendly URL**
- **Dark mode**, **Bildirim sistemi**, **Şifre güncelleme ekranı**, **KVKK, Gizlilik, İletişim sayfaları** ve daha fazlası...

## Güvenlik & Doğrulama
- Kullanıcı kayıt/giriş işlemleri JWT + Cookie ile sağlanıyor.
- Formlar hem sunucu hem istemci tarafında doğrulanıyor.
- reCAPTCHA ile botlara karşı koruma sağlanıyor.
- Şifre güncelleme işlemi için mevcut şifre doğrulama zorunluluğu var.

### Kimlik doğrulama (auth)
- **Şifreler hash'li saklanır.** ASP.NET Core `PasswordHasher<User>` (PBKDF2, rastgele salt) kullanılır; veritabanında düz metin şifre tutulmaz. Kayıt, şifre değiştirme ve şifre sıfırlama bu yoldan geçer; `UserManager.CreateAsync/UpdateAsync` düz metin görürse kaydetmeden önce hash'ler.
- **Eski (düz metin) kayıtlar için geçiş:**
  1. Uygulama açılırken `MigrateLegacyPasswordsAsync` hâlâ düz metin olan bütün satırları hash'ler (tekrar çalıştırmak güvenlidir, hash'li satırlara dokunmaz).
  2. Bu adım atlansa bile kullanıcı başarıyla giriş yaptığında düz metin eşleşmesi görülürse şifre hemen hash'lenip kaydedilir.
  3. Kayıtlı hash'in kendisi şifre olarak kabul edilmez.
  Not: Sütun adı `Password` olarak kaldı (şema değişmedi, yeni migration yok). Geçişten önce alınmış veritabanı yedekleri hâlâ düz metin içerir; eski yedekleri silin ve eski şifreleri geçersiz sayın.
- **Çerezler:** `blogToken` ve antiforgery çerezi `HttpOnly`, `Secure`; JWT çerezi `SameSite=Lax`, antiforgery çerezi `SameSite=Strict`. Süre 2 saat (UTC).
- **CSRF:** Tüm POST istekleri için antiforgery doğrulaması global olarak açık. Formlar token'ı otomatik alır, jQuery/fetch çağrıları `RequestVerificationToken` başlığıyla gönderir.
- **Giriş hatası** hesabın var olup olmadığını belli etmez; şifre sıfırlama isteği de her e-posta için aynı cevabı verir. Sıfırlama token'ı rastgele üretilir, veritabanında yalnızca SHA-256 özeti tutulur, 1 saat geçerlidir.
- **Brute-force koruması:** Aynı e-posta + IP için 5 başarısız denemeden sonra 15 dakika kilit (bellekte tutulur).
- **`returnUrl`** yalnızca yerel yollara yönlendirir (`Url.IsLocalUrl`).
- Profil resmi yüklemede sunucu dosya adını kendisi seçer ve yalnızca resim uzantılarını kabul eder (kayıt ekranı).

### Yapılandırma (değer commit edilmez)
Gerçek değerler `dotnet user-secrets` veya ortam değişkenleriyle verilir. `appsettings.json` içinde yalnızca yer tutucular vardır.

| Ad (ortam değişkeni) | Açıklama |
|---|---|
| `ConnectionStrings__DefaultConnection` | SQL Server bağlantı dizesi |
| `Jwt__Key` | JWT imza anahtarı, en az 32 bayt. Boş/yer tutucu ise uygulama başlamaz |
| `Jwt__Issuer`, `Jwt__Audience` | JWT yayıncı ve hedef |
| `EmailSettings__Host`, `__Port`, `__EnableSSL`, `__Username`, `__Password`, `__ToEmail` | SMTP |
| `GoogleReCaptcha__SiteKey`, `GoogleReCaptcha__SecretKey` | reCAPTCHA |
| `SeedAdmin__Email`, `SeedAdmin__UserName`, `SeedAdmin__Password` | İlk yönetici hesabı (isteğe bağlı) |

İlk yönetici yalnızca üç `SeedAdmin__*` değeri de verilirse ve şifre en az 10 karakterse oluşturulur; verilmezse hiçbir kullanıcı eklenmez. Aynı e-posta/kullanıcı adı varsa tekrar çalıştırmak hiçbir şeyi değiştirmez. Koda gömülü hesap yoktur. Örnek blog yazıları da yalnızca bir yönetici varsa eklenir.

```
cd BlogProject.Web
dotnet user-secrets set "Jwt:Key" "<en az 32 baytlık rastgele değer>"
dotnet user-secrets set "SeedAdmin:Email" "<e-posta>"
dotnet user-secrets set "SeedAdmin:UserName" "<kullanıcı adı>"
dotnet user-secrets set "SeedAdmin:Password" "<en az 10 karakter>"
```

### Testler
`dotnet test` (xUnit + Moq; seed testleri EF Core InMemory kullanır, veritabanı/Docker gerekmez). Hash'leme, eski düz metin geçişi, düz metin saklanmaması, config'ten seed ve giriş kilidi testleri `BlogProject.Tests/Security` altındadır.

### Bilinen sınırlamalar
- Kayıt ekranı "e-posta/kullanıcı adı zaten alınmış" diyerek hesap varlığını belli eder.
- Giriş kilidi bellekte tutulur: uygulama yeniden başlayınca ve birden çok sunucuda sıfırlanır/paylaşılmaz. Ters proxy arkasında IP için `ForwardedHeaders` ayarlanmalıdır.
- Rol JWT içinde 2 saat boyunca geçerlidir; bir kullanıcının yetkisi alınsa bile mevcut oturumu süre dolana kadar eski rolle devam eder. Sunucu tarafında iptal/oturum listesi yok.
- Şifre politikası yalnızca en az 6 karakter (kayıt/değiştirme); yönetici seed şifresi için 10.
- E-posta doğrulaması ve iki adımlı doğrulama yok.
- `SettingsController.UpdateProfile` yüklenen dosya uzantısını denetlemez (kayıt ekranındaki gibi izin listesi eklenmeli).
- Güvenlik başlıkları (CSP vb.) eklenmedi; CDN'den betik yükleniyor.
- Uygulama gerçek SQL Server üzerinde uçtan uca çalıştırılıp test edilmedi; birim testleri ve derleme (Razor dahil) doğrulandı.
## Ekstra Özellikler
- Yorum sistemi: AJAX tabanlı (ekleme, silme, güncelleme)
- Bildirim çanı: Son yorumlara göre canlı veri çekiyor
- Arama: AJAX destekli, kategori bazlı filtreleme ile gelişmiş arama sistemi
- Admin paneli: Dashboard, kullanıcı/yorum/blog yönetimi
- PDF formatında blog indirme
- Blog detaylarında içerik içi "Table of Contents" (uzun içerikler için)

## Geliştirici
**Muhammet Tarık İnandı**  
> Bilgisayar Mühendisliği Öğrencisi  
> Kocaeli Üniversitesi

NOT: Gizli değerleri `appsettings.json` içine yazmayın; "Yapılandırma" bölümündeki adlarla user-secrets veya ortam değişkenleri kullanın.

İletişim:  
- [LinkedIn](https://www.linkedin.com/in/tarikinandi/)
- [GitHub](https://github.com/tarikinandi)
- [E-posta](mailto:inandi.tarik@gmail.com)

## Lisans
MIT Lisansı altında açık kaynak olarak paylaşılmıştır. Ayrıntılar için `LICENSE` dosyasına göz atabilirsiniz.

---

