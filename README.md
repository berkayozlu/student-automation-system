# Öğrenci Otomasyon Sistemi (Student Automation System)

Bu proje, .NET 9, Blazor Server, Entity Framework Core ve PostgreSQL kullanarak geliştirilmiş bir öğrenci otomasyon sistemidir.

## Özellikler

### Kullanıcı Yönetimi
- **Giriş (Login) ve Kayıt (Register)** sistemi
- **Roller**: Admin, Teacher, Student
- JWT tabanlı kimlik doğrulama
- Şifre hashleme güvenliği

### Öğrenci İşlemleri (CRUD)
- Admin ve öğretmenler öğrenci ekleyebilir, güncelleyebilir, listeleyebilir
- Öğrenciler kendi bilgilerini görüntüleyebilir
- Öğrenci numarası ile benzersiz kimlik

### Öğretmen İşlemleri (CRUD)
- Admin öğretmen ekleyebilir, güncelleyebilir, listeleyebilir
- Çalışan numarası ile benzersiz kimlik

### Ders Yönetimi (CRUD)
- Admin ders oluşturabilir
- Öğretmenler kendi derslerini görebilir ve durumlarını güncelleyebilir
- Öğretmenler derse öğrenci ekleyebilir/silebilir
- Ders durumu takibi (Başlamadı, Devam Ediyor, Tamamlandı, İptal Edildi)

### Not ve Devamsızlık Sistemi
- Öğretmenler öğrencilerine ders bazında not ekleyebilir
- Öğrenciler notlarını görebilir
- Detaylı devamsızlık kaydı (Var, Yok, Geç, Mazur)
- Toplu devamsızlık girişi

##  Teknolojiler

- **Backend**: .NET 9 Web API
- **Frontend**: Blazor Server
- **ORM**: Entity Framework Core
- **Veritabanı**: PostgreSQL
- **Kimlik Doğrulama**: ASP.NET Core Identity + JWT
- **UI Framework**: Bootstrap 5
- **Containerization**: Docker

## Gereksinimler

- .NET 9 SDK
- Docker ve Docker Compose
- PostgreSQL (Docker ile çalışır)

##  Kurulum ve Çalıştırma

### 1. Repository'yi klonlayın
```bash
git clone <repository-url>
```

### 2. Uygulamayı başlatın
```bash
docker-compose up -d
```

## Test Kullanıcıları

### Admin Kullanıcısı
- **Email**: admin@studentautomation.com
- **Şifre**: Admin123!
- **Yetkiler**: Tüm sistem yönetimi

### Test Öğretmeni
Kayıt sayfasından "Teacher" rolü seçerek oluşturabilirsiniz.
Default Şifre: Teacher123!

### Test Öğrencisi
Kayıt sayfasından "Student" rolü seçerek oluşturabilirsiniz.

##  Frontend Sayfaları

- **Login/Register**: Kimlik doğrulama sayfaları
- **Dashboard**: Rol bazlı ana sayfa
- **Öğrenci Listesi**: Öğrenci yönetimi
- **Öğretmen Listesi**: Öğretmen yönetimi
- **Ders Listesi**: Ders yönetimi
- **Not/Devamsızlık**: Not ve devamsızlık takibi

##  API Endpoints

### Authentication
- `POST /api/auth/login` - Giriş
- `POST /api/auth/register` - Kayıt
- `GET /api/auth/profile` - Profil bilgisi

### Students
- `GET /api/students` - Öğrenci listesi
- `GET /api/students/{id}` - Öğrenci detayı
- `POST /api/students` - Öğrenci oluştur
- `PUT /api/students/{id}` - Öğrenci güncelle
- `DELETE /api/students/{id}` - Öğrenci sil

### Teachers
- `GET /api/teachers` - Öğretmen listesi
- `GET /api/teachers/{id}` - Öğretmen detayı
- `POST /api/teachers` - Öğretmen oluştur
- `PUT /api/teachers/{id}` - Öğretmen güncelle

### Courses
- `GET /api/courses` - Ders listesi
- `POST /api/courses` - Ders oluştur
- `PUT /api/courses/{id}` - Ders güncelle
- `POST /api/courses/{id}/enroll` - Öğrenci kaydet

### Grades
- `GET /api/grades` - Not listesi
- `POST /api/grades` - Not ekle
- `PUT /api/grades/{id}` - Not güncelle
- `GET /api/grades/course/{courseId}` - Ders notları

### Attendance
- `GET /api/attendance` - Devamsızlık listesi
- `POST /api/attendance` - Devamsızlık ekle
- `POST /api/attendance/bulk` - Toplu devamsızlık
- `GET /api/attendance/course/{courseId}` - Ders devamsızlığı

## Veritabanı Yapısı

### Ana Tablolar
- **AspNetUsers**: Kullanıcı bilgileri (Identity)
- **Students**: Öğrenci detayları
- **Teachers**: Öğretmen detayları
- **Courses**: Ders bilgileri
- **CourseEnrollments**: Ders kayıtları
- **Grades**: Not kayıtları
- **Attendances**: Devamsızlık kayıtları

## Güvenlik

- Şifreler ASP.NET Core Identity ile hashlenmiştir
- JWT token tabanlı kimlik doğrulama
- Rol tabanlı yetkilendirme
- CORS yapılandırması

## Clean Code Prensipleri

- SOLID prensipleri uygulanmıştır
- Repository pattern kullanılmıştır
- DTO pattern ile veri transferi
- Dependency Injection
- Async/await pattern
- Exception handling

## CI/CD Pipeline

Bu proje GitHub Actions ile otomatik CI/CD pipeline'ına sahiptir:

### Özellikler
- **Otomatik Test**: Her push ve PR'da testler çalışır
- **Docker Build**: Otomatik Docker image oluşturma ve GitHub Container Registry'ye push
- **Code Quality**: Kod formatı ve kalite kontrolleri
- **Security Scan**: Güvenlik taraması
- **Multi-Environment**: Staging ve Production ortamları
- **Dependency Updates**: Haftalık otomatik paket güncellemeleri

### Workflow Dosyaları
- `.github/workflows/ci-cd.yml` - Ana CI/CD pipeline
- `.github/workflows/pr-check.yml` - PR doğrulama
- `.github/workflows/dependency-update.yml` - Paket güncellemeleri

### Kurulum
1. GitHub repository'nizde Actions sekmesini etkinleştirin
2. Container Registry için gerekli izinleri verin
3. Staging ve Production environment'larını oluşturun

## Bonus Özellikler

- Docker containerization
- Swagger API documentation
- Responsive Bootstrap UI
- Real-time Blazor Server
- Comprehensive error handling
- Role-based dashboards
- Bulk operations (attendance)
- Data validation
- Clean architecture
- **GitHub Actions CI/CD Pipeline**

