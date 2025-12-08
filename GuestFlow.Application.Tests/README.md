# GuestFlow.Application.Tests

Unit test projesi için test framework kurulumu ve temel yapı.

## 📦 Kurulu Paketler

- **xUnit** (2.5.3) - Test framework
- **Moq** (4.20.72) - Mock framework
- **FluentAssertions** (8.8.0) - Assertion library
- **coverlet.collector** (6.0.0) - Code coverage collector
- **coverlet.msbuild** (6.0.4) - Code coverage (MSBuild)
- **Microsoft.NET.Test.Sdk** (17.8.0) - Test SDK

## 📁 Proje Yapısı

```
GuestFlow.Application.Tests/
├── Helpers/
│   ├── TestBase.cs              # Base test class
│   └── TestDataBuilder.cs       # Test data builder pattern
├── Operations/
│   └── Guest/
│       └── GuestManagerTests.cs # Örnek test class
└── README.md
```

## 🚀 Test Çalıştırma

### Tüm testleri çalıştır
```bash
dotnet test
```

### Code coverage ile test çalıştır
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Belirli bir test class'ını çalıştır
```bash
dotnet test --filter FullyQualifiedName~GuestManagerTests
```

### Verbose output ile test çalıştır
```bash
dotnet test --verbosity normal
```

## 📝 Test Yazma Örnekleri

### Test Class Yapısı

```csharp
public class MyServiceTests : TestBase
{
    private readonly Mock<IRepository<Entity>> _repositoryMock;
    private readonly MyService _service;

    public MyServiceTests()
    {
        _repositoryMock = CreateMock<IRepository<Entity>>();
        _service = new MyService(_repositoryMock.Object);
    }

    [Fact]
    public async Task MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        var entity = TestDataBuilder.CreateEntity();

        // Act
        var result = await _service.Method(entity);

        // Assert
        result.Should().NotBeNull();
    }
}
```

### Test Data Builder Kullanımı

```csharp
// Varsayılan değerlerle
var guest = TestDataBuilder.CreateGuest();

// Özel değerlerle
var guest = TestDataBuilder.CreateGuest(
    id: 1,
    fullName: "John Doe",
    email: "john@example.com"
);
```

## ✅ Test Best Practices

1. **AAA Pattern**: Arrange-Act-Assert
2. **Test Isolation**: Her test bağımsız olmalı
3. **Naming Convention**: `MethodName_Scenario_ExpectedBehavior`
4. **Mock Setup**: Dependency'leri doğru şekilde mock'la
5. **FluentAssertions**: Okunabilir assertion'lar kullan

## 📊 Code Coverage

Coverlet ile code coverage raporları oluşturulabilir. Coverage threshold %70 olarak hedeflenmektedir.

## 🔗 Referanslar

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)

