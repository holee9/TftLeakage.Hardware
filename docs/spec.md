# .NET Hardware Library Specification

## Summary

이 문서는 aSi TFT 패널 누설 전류 감소 프로젝트를 위한 .NET 하드웨어 통신 라이브러리 사양서입니다.

## 목차

1. [프로젝트 개요](#1-프로젝트-개요)
2. [아키텍처 개요](#2-아키텍처-개요)
3. [API 사양](#3-api-사양)
4. [데이터 모델](#4-데이터-모델)
5. [통신 프로토콜](#5-통신-프로토콜)
6. [UI 요구사항](#6-ui-요구사항)
7. [설정 스키마](#7-설정-스키마)
8. [인수 기준](#8-인수-기준)

---

## 1. 프로젝트 개요

### 목표

- FPGA SPI 통신 추상화 계층 제공
- Ethernet 프로토콜 클라이언트 구현
- 기존 .NET 프로젝트와 독립적인 NuGet 패키지 형태
- 비동기 API 기반의 최신 .NET 8 패턴

### 배포 방식

**NuGet 패키지**:
- `TftLeakage.Hardware` - FPGA SPI 통신
- `TftLeakage.Protocol` - Ethernet 프로토콜

---

## 2. 아키텍처 개요

```
┌─────────────────────────────────────────────────────┐
│                  .NET Application                   │
│              (WPF / WinForms / etc.)                │
└─────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────┐
│              TftLeakage.Hardware Library            │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────┐  ┌─────────────────────┐│
│  │  TftLeakage.Hardware  │  │ TftLeakage.Protocol ││
│  │   (FPGA SPI Client)   │  │  (Ethernet Client)  ││
│  └───────────────────────┘  └─────────────────────┘│
└─────────────────────────────────────────────────────┘
         │                                    │
         ▼                                    ▼
┌─────────────────┐               ┌────────────────────┐
│  FPGA (SPI)     │               │  Yocto Daemon      │
│  /dev/spidev0.0 │               │  Ethernet TCP/UDP  │
└─────────────────┘               └────────────────────┘
```

---

## 3. API 사양

### IFpgaClient

```csharp
public interface IFpgaClient : IDisposable
{
    bool IsConnected { get; }

    Task ConnectAsync(CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);

    Task<bool> SetIdleModeAsync(bool enabled, CancellationToken ct = default);
    Task<BiasLevel> GetBiasAsync(CancellationToken ct = default);
    Task SetBiasAsync(BiasLevel level, CancellationToken ct = default);

    Task<uint> ReadRegisterAsync(byte address, CancellationToken ct = default);
    Task WriteRegisterAsync(byte address, uint value, CancellationToken ct = default);

    Task<string?> GetVersionAsync(CancellationToken ct = default);
}
```

### ITftProtocolClient

```csharp
public interface ITftProtocolClient : IDisposable
{
    bool IsConnected { get; }

    Task ConnectAsync(CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);

    Task<SystemStatus?> GetStatusAsync(CancellationToken ct = default);
    Task SetIdleModeAsync(bool enabled, CancellationToken ct = default);
    Task<BiasLevel> GetBiasAsync(CancellationToken ct = default);
    Task SetBiasAsync(BiasLevel level, CancellationToken ct = default);
}
```

---

## 4. 데이터 모델

### BiasLevel

```csharp
public enum BiasLevel : byte
{
    Normal = 0b000,  // V1 (일반 모드)
    Medium = 0b001,  // V2 (중간 모드)
    Idle = 0b010,    // V3 (Idle 모드)
    Off = 0b111      // OFF
}
```

### SystemStatus

```csharp
public class SystemStatus
{
    public bool IsIdleMode { get; set; }
    public BiasLevel CurrentBias { get; set; }
    public float Temperature { get; set; }
    public ushort AdcValue { get; set; }
    public string? FirmwareVersion { get; set; }
    public uint UptimeSeconds { get; set; }
    public DateTime Timestamp { get; set; }
}
```

---

## 5. 통신 프로토콜

### Ethernet 메시지 형식

```
┌────────────────────────────────────────────────────┐
│ Header (8 bytes)                                   │
├──────────┬──────────┬──────────┬─────────────────┤
│ Magic    │ Type     │ Length   │ Sequence        │
│ (4 bytes)│ (1 byte) │ (2 bytes)│ (1 byte)        │
│ 0x54465400│          │          │                 │
└──────────┴──────────┴──────────┴─────────────────┘
┌────────────────────────────────────────────────────┐
│ Payload (0-65535 bytes)                            │
└────────────────────────────────────────────────────┘
```

### 메시지 타입

| 타입 | 값 | 설명 |
|------|-----|------|
| StatusRequest | 0x01 | 상태 요청 |
| StatusResponse | 0x02 | 상태 응답 |
| SetIdleMode | 0x10 | Idle 모드 설정 |
| SetBiasLevel | 0x11 | Bias 레벨 설정 |
| EventNotification | 0x20 | 이벤트 알림 |
| ErrorResponse | 0xFF | 오류 응답 |

---

## 6. UI 요구사항

### WPF/WinForms 애플리케이션

**필요한 UI 요소:**
- Idle 모드 활성화/비활성화 토글
- Bias 레벨 선택 (Normal/Medium/Idle/Off)
- 현재 상태 표시 (온도, Bias, Uptime)
- 연결 상태 표시

### MVVM 패턴 (WPF)

```csharp
public class MainViewModel : INotifyPropertyChanged
{
    public bool IsIdleMode { get; set; }
    public BiasLevel SelectedBias { get; set; }
    public float Temperature { get; set; }
    public bool IsConnected { get; set; }

    public ICommand ToggleIdleCommand { get; }
    public ICommand SetBiasCommand { get; }
}
```

---

## 7. 설정 스키마

### appsettings.json

```json
{
  "TftLeakage": {
    "Spi": {
      "Device": "/dev/spidev0.0",
      "MaxSpeedHz": 25000000
    },
    "Ethernet": {
      "Host": "192.168.1.100",
      "Port": 8080,
      "ConnectTimeoutMs": 5000,
      "RequestTimeoutMs": 3000
    },
    "Idle": {
      "AutoEnable": true,
      "TimeoutSeconds": 300
    },
    "Logging": {
      "Level": "Information",
      "LogFile": "/var/log/tft-leakage.log"
    }
  }
}
```

---

## 8. 인수 기준

### 단위 테스트

- [TEST-DOTNET-001] 모든 public API 메서드 테스트覆盖
- [TEST-DOTNET-002] 예외 상황 테스트 (연결 실패 등)
- [TEST-DOTNET-003] 비동기 동작 테스트

### 통합 테스트

- [TEST-DOTNET-101] mock SPI 장치로 통신 테스트
- [TEST-DOTNET-102] mock Ethernet 서버로 프로토콜 테스트

### 코드 품질

- [TEST-DOTNET-201] 코드 커버리지 ≥ 80%
- [TEST-DOTNET-202] Stylecop 경고 0
- [TEST-DOTNET-203] XML 문서 주석 완비

---

## 참고 문서

- [docs/delivery/to-dotnet-project/](https://github.com/holee9/TFT-Leak-plan/tree/main/docs/delivery/to-dotnet-project) - 상세 사양서
