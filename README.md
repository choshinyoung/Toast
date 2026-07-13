# 🍞 Toast

[![Library NuGet](https://img.shields.io/nuget/vpre/choshinyoung.Toast?label=choshinyoung.Toast&logo=nuget)](https://www.nuget.org/packages/choshinyoung.Toast/)
[![Tool NuGet](https://img.shields.io/nuget/vpre/choshinyoung.Toast.Interactive?label=choshinyoung.Toast.Interactive&logo=nuget)](https://www.nuget.org/packages/choshinyoung.Toast.Interactive/)

> [!WARNING]
> **본 프로젝트는 현재 미완성 상태입니다!**  
> 이전 버전 및 안정화 버전은 [v1.4](https://github.com/choshinyoung/Toast/tree/v1.4) 브랜치를 참고해 주세요.

---

**Toast**는 C#으로 작성된 가벼운 커스텀 DSL (Domain Specific Language) 인터프리터입니다. 직관적인 사용성, 복잡성을 최소화한 깔끔한 구조와 문법, 그리고 유연한 확장성을 목표로 설계되었습니다.

---

## 🛠️ 대화형 도구 (CLI Tool)

전역 도구 설치:
```bash
dotnet tool install -g choshinyoung.Toast.Interactive --version 2.0.0-beta
```

대화형 도구 실행:
```bash
toast
```

---

## 📦 라이브러리

라이브러리 설치:
```bash
dotnet add package choshinyoung.Toast --version 2.0.0-beta
```

C# 코드 사용 예시:
```csharp
using Toast;

var toaster = new Toaster(useBuiltIn: true);
var result = toaster.Execute("1 + 2 * 3");
Console.WriteLine(result); // 출력 결과: 7
```
