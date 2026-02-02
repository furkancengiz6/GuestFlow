# VISION: Turizm Operasyon Intelligence Layer

**"GuestFlow = Otelin İnsan İlişkileri Hafızası"**

## 1. Vizyon Özeti

Geleneksel otel yazılımları (PMS) sadece "işlem" (transaction) kaydeder: "Oda satıldı", "Fatura kesildi".
**GuestFlow** ise bu işlemlerin arkasındaki "insan hikayesini" ve "ilişkiyi" modeller.

Amacımız: Bir otelin en değerli varlığı olan **misafir-personel etkileşimini** dijitalleştirmek, ölçmek ve yönetilebilir bir veriye dönüştürmektir.

## 2. Temel Katmanlar

### A. Transactional Layer (SQL Server)

- Mevcut operasyonel veriler (Guest, Reservation, Invoice, Transfer).
- Stabilite ve tutarlılık (ACID) odaklı.
- Kaynak: PMS entegrasyonları + GuestFlow operasyon modülleri.

### B. Graph Intelligence Layer (Neo4j)

- **Varlıklar (Nodes)**: Misafir, Personel, Hizmet, Zaman, Duygu.
- **İlişkiler (Edges)**:
  - `INTERACTS`: Misafir X Personel Y ile etkileşime girdi (Sentiment: Positive).
  - `PREFERS`: Misafir Z, İtalyan Mutfağını tercih eder (Weight: 0.9).
  - `SATISFIES`: Tur Hizmet A, Aile gruplarını memnun eder.

### C. Predictive Layer (AI/ML)

- **Davranış Tahmini**: "Bu misafir %80 ihtimalle odaya şarap isteyecek."
- **Risk Analizi**: "Bu misafir %60 ihtimalle check-out sırasında şikayet edecek (çünkü transferi gecikti)."
- **Operasyonel Optimizasyon**: "Yarın sabah 08:00-10:00 arası transfer yoğunluğu %120 artacak."

## 3. Yol Haritası (Intelligence)

- [x] **Faz 1: Veri Toplama** (Logs, Feedbacks, History)
- [x] **Faz 2: Graph Modelleme** (Neo4j altyapısı ve node tasarımı)
- [ ] **Faz 3: Örüntü Tanıma** (Basit kural tabanlı öneriler)
- [ ] **Faz 4: AI Entegrasyonu** (ML.NET ile gelişmiş tahminleme)

*Bu doküman projenin stratejik vizyonunu tanımlar ve sürekli güncellenmelidir.*
