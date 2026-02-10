# TftLeakage.Hardware

aSi TFT 패널 누설 전류 감소를 위한 .NET 하드웨어 통신 라이브러리입니다.

## 목표

- FPGA SPI 통신 추상화
- Ethernet 프로토콜 구현
- 기존 .NET 프로젝트와 독립적인 NuGet 패키지

## 구조

```
TftLeakage.Hardware/
├── src/
│   ├── TftLeakage.Hardware/      # FPGA SPI 통신
│   └── TftLeakage.Protocol/      # Ethernet 프로토콜
└── tests/
    ├── TftLeakage.Hardware.Tests/
    └── TftLeakage.Protocol.Tests/
```

## 사용 방법

### NuGet 패키지

```bash
dotnet add package TftLeakage.Hardware
```

### FPGA SPI 통신

```csharp
using TftLeakage.Hardware;

// SPI 클라이언트 생성
var client = new FpgaSpiClient(bus: 0, device: 0);

// Idle 모드 활성화
await client.SetIdleModeAsync(true);

// Bias 전압 설정
await client.SetBiasAsync(BiasLevel.Idle);
```

### Ethernet 프로토콜

```csharp
using TftLeakage.Protocol;

// 프로토콜 클라이언트 생성
var client = new TftProtocolClient("192.168.1.100", 8080);

// 상태 조회
var status = await client.GetStatusAsync();
Console.WriteLine($"Temperature: {status.Temperature}°C");
```

## API

### IFpgaClient

```csharp
public interface IFpgaClient : IDisposable
{
    Task ConnectAsync(CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);

    Task<bool> SetIdleModeAsync(bool enabled, CancellationToken ct = default);
    Task<BiasLevel> GetBiasAsync(CancellationToken ct = default);
    Task SetBiasAsync(BiasLevel level, CancellationToken ct = default);

    Task<uint> ReadRegisterAsync(byte address, CancellationToken ct = default);
    Task WriteRegisterAsync(byte address, uint value, CancellationToken ct = default);
}
```

### ITftProtocolClient

```csharp
public interface ITftProtocolClient : IDisposable
{
    Task ConnectAsync(CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);

    Task<SystemStatus> GetStatusAsync(CancellationToken ct = default);
    Task SetIdleModeAsync(bool enabled, CancellationToken ct = default);
    Task<BiasLevel> GetBiasAsync(CancellationToken ct = default);
    Task SetBiasAsync(BiasLevel level, CancellationToken ct = default);
}
```

## 빌드

```bash
# 솔루션 빌드
dotnet build TftLeakage.Hardware.sln

# 테스트
dotnet test

# 패키지 생성
dotnet pack -c Release
```

## 의존성

- .NET 8.0
- System.IO.Ports (SPI용, Linux 전용)
- System.Net.Sockets (Ethernet용)

## 라이선스

MIT License - [LICENSE](LICENSE)

## 문서

### 배포 패키지

| 문서 | 설명 |
|------|------|
| [README.md](docs/delivery/README.md) | 배포 패키지 개요 |
| [00_project_overview.md](docs/delivery/00_project_overview.md) | 프로젝트 개요 |
| [01_architecture_overview.md](docs/delivery/01_architecture_overview.md) | 아키텍처 개요 |
| [02_api_specification.md](docs/delivery/02_api_specification.md) | API 사양 |
| [03_data_models.md](docs/delivery/03_data_models.md) | 데이터 모델 |
| [04_communication_protocol.md](docs/delivery/04_communication_protocol.md) | 통신 프로토콜 |
| [05_ui_requirements.md](docs/delivery/05_ui_requirements.md) | UI 요구사항 |
| [06_configuration_schema.md](docs/delivery/06_configuration_schema.md) | 설정 스키마 |
| [07_acceptance_criteria.md](docs/delivery/07_acceptance_criteria.md) | 인수 기준 |
| [reference/](docs/delivery/reference/) | 참고 문서 (2D Dark LUT 등) |

### 요약 사양

- [docs/spec.md](docs/spec.md) - 간단 사양 요약

## 관련 프로젝트

- [tft-panel-fpga](https://github.com/holee9/tft-panel-fpga) - FPGA RTL
- [meta-tft-leakage](https://github.com/holee9/meta-tft-leakage) - Yocto Layer
