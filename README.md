# 🛡️ Phishing Detector - AI Phishing Detection System

**Ollama tabanlı yapay zeka destekli e-posta phishing tespit sistemi**

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)
![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791)
![Ollama](https://img.shields.io/badge/Ollama-AI-FF6B35)
Aspire

---

## 📋 Özellikler

- ✅ **3 Boyutlu Risk Analizi**: URL, Header ve İçerik ayrı ayrı analiz edilir
- 🤖 **Ollama AI**: Lokal LLM ile gizlilik odaklı analiz
- 🏷️ **3 Risk Seviyesi**: Normal, Spam, Tehlikeli
- 📊 **Kapsamlı Dashboard**: İstatistikler, grafikler, son analizler
- 📅 **Analiz Geçmişi**: Filtrelenebilir tüm geçmiş kayıtlar
- 💾 **PostgreSQL**: Tüm analizler kalıcı olarak kaydedilir
- 🎨 **Profesyonel Dark UI**: Blazor Server ile modern arayüz
- ⚡ **Paralel Analiz**: 3 analiz aynı anda çalışır (hız optimizasyonu)

---

## 🏗️ Mimari

```
PhishingDetector/
├── src/
│   ├── PhishingDetector.Core/          # Paylaşılan modeller & interface'ler
│   │   ├── Models     # Veritabanı modeli
│   │   ├── DTOs          # API DTO'ları
│   │   └── Interfaces    # Servis sözleşmeleri
│   │
│   ├── PhishingDetector.API/           # ASP.NET Core Web API
│   │   ├── Controllers/               # REST endpoint'leri
│   │   ├── Services/
│   │   │   ├── OllamaService.cs       # Ollama LLM entegrasyonu
│   │   │   └── PhishingAnalysisService.cs # Ana analiz motoru
│   │   ├── Data/PhishingDbContext.cs  # EF Core DbContext
│   │   └── Migrations/                # EF Core migration'ları
│   │
│   └── PhishingDetector.Web/           # Blazor Server Frontend
│       ├── Components/
│       │   ├── Pages/
│       │   │   ├── Dashboard.razor    # Ana dashboard
│       │   │   ├── Analyze.razor      # E-posta analiz sayfası
│       │   │   └── History.razor      # Geçmiş kayıtlar
│       │   ├── Layout/MainLayout.razor
│       │   └── Shared/ScoreChip.razor
│       └── wwwroot/css/app.css        # Dark theme CSS
│
|
└── PhishingDetector.sln
```

---

## 🚀 Kurulum



### Yöntem 1: Manuel Kurulum

#### Gereksinimler
- .NET 10 SDK
- PostgreSQL 15+
- Ollama download



---

## ⚙️ Yapılandırma

### API - appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "postgreconnectionstring"
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "Model": "glm-5:cloud"    // Farklı model de kullanabilirsiniz: mistral, phi3, gemma2
  }
}
```

### Web - appsettings.json

```json
{
  "ApiBaseUrl": "http://localhost:55022"
}
```

---

## 📡 API Endpoints

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | `/api/phishing/analyze` | E-posta analiz et |
| GET | `/api/phishing/dashboard` | Dashboard istatistikleri |
| GET | `/api/phishing/history?count=20` | Analiz geçmişi |
| GET | `/api/phishing/{id}` | Tek analiz detayı |
| GET | `/api/phishing/health` | Servis sağlık durumu |

### Örnek İstek

```bash
curl -X POST http://localhost:55022/api/phishing/analyze \
  -H "Content-Type: application/json" \
  -d '{
    "sender": "security@payp4l-secure.com",
    "subject": "ACİL: Hesabınız askıya alındı!",
    "urls": "https://payp4l-secure.com/login\nhttps://bit.ly/abc123",
    "headers": "DKIM-Signature: FAILED\nSPF: FAIL",
    "content": "Hesabınız güvenlik nedeniyle kilitlendi. Hemen doğrulayın..."
  }'
```

### Örnek Yanıt

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "overallRisk": "Dangerous",
  "overallRiskScore": 87,
  "urlRiskScore": 95,
  "headerRiskScore": 80,
  "contentRiskScore": 82,
  "urlAnalysis": "Şüpheli domain tespit edildi...",
  "headerAnalysis": "DKIM ve SPF doğrulama başarısız...",
  "contentAnalysis": "Aciliyet dili ve kimlik bilgisi isteği...",
  "summary": "Bu e-posta yüksek ihtimalle phishing girişimidir...",
  "indicators": ["🔴 Tehlikeli URL'ler", "🔴 Header sahtecilik", "⚠️ Aciliyet dili"],
  "processingTimeMs": 8423
}
```

---

## 🧠 Risk Skoru Hesaplama

| Bileşen | Ağırlık | Açıklama |
|---------|---------|----------|
| URL Analizi | %40 | Şüpheli domain, IP tabanlı URL, kısaltıcılar |
| Header Analizi | %30 | SPF/DKIM/DMARC, sahte gönderen |
| İçerik Analizi | %30 | Aciliyet dili, marka taklidi, kimlik isteği |

| Skor | Seviye | Açıklama |
|------|--------|----------|
| 0-34 | ✅ Normal | Güvenli görünüyor |
| 35-69 | ⚠️ Spam | Şüpheli, dikkatli olun |
| 70-100 | 🔴 Tehlikeli | Phishing! Tıklamayın |

---

## 🎨 Ekran Görüntüleri

- **Dashboard**: Genel istatistikler, pie chart, bar chart, son analizler tablosu
- **Analiz Sayfası**: Form + gerçek zamanlı sonuç paneli
- **Geçmiş**: Filtrelenebilir ve sıralanabilir tüm analizler

---

## 🔒 Gizlilik

Tüm analizler **lokal olarak** Ollama üzerinden yapılır. Hiçbir e-posta verisi dışarıya gönderilmez.

---

## 📦 Bağımlılıklar

### Backend
- ASP.NET Core 10
- Entity Framework Core 10 + Npgsql
- Serilog

### Frontend  
- Blazor Server (.NET 10)
- Font Awesome 6
- Google Fonts (Inter)
