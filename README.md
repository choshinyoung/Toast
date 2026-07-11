# 🍞 Toast

[![NuGet](https://img.shields.io/badge/nuget-1.4.5-blue)](https://www.nuget.org/packages/choshinyoung.Toast/)

> [!WARNING]
> **본 프로젝트는 현재 미완성 상태입니다!**  
> 이전 버전 및 안정화 버전은 [v1.4](https://github.com/choshinyoung/Toast/tree/v1.4) 브랜치를 참고해 주세요.

---

**Toast**는 C#으로 작성된 가벼운 커스텀 DSL (Domain Specific Language) 인터프리터입니다. 직관적인 사용성, 복잡성을 최소화한 깔끔한 구조와 문법, 그리고 유연한 확장성을 목표로 설계되었습니다.

## 🚀 빠른 시작

### 대화형 콘솔 실행
```bash
dotnet run --project Toast.Interactive
```

### 라이브러리로 사용하기 (C#)
```csharp
using Toast;

// Toaster 생성 및 스크립트 실행
var toaster = new Toaster(useBuiltIn: true);
var result = toaster.Execute("1 + 2 * 3");
Console.WriteLine(result); // 7
```
