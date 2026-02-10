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

## 관련 프로젝트

- [tft-panel-fpga](https://github.com/holee9/tft-panel-fpga) - FPGA RTL
- [meta-tft-leakage](https://github.com/holee9/meta-tft-leakage) - Yocto Layer
