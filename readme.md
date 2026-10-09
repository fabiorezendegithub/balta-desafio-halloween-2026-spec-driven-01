<img width="100%" alt="Halloween 2026" src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/halloween-2026/banner.jpg" />

## 🎃 Halloween - Desafio 1

Oi, eu sou Fabio e este é o espaço onde compartilho minha jornada de aprendizado durante o desafio **Halloween 2026**, realizado pelo [balta.io](https://balta.io). 👻

Aqui você vai encontrar projetos, exercícios e códigos que estou desenvolvendo durante o desafio.

### Sobre este desafio
Neste desafio o objetivo é consolidar os fundamentos do Spec Driven Development, criando uma constituição, especificação, planejamento e tarefas manualmente para serem implementadas pela IA posteriormente.

#### O que foi implementado
- API REST para geração e consulta de senhas fortes
- `POST /api/password` para gerar uma senha e retornar seu GUID
- `GET /api/password` para consultar a senha usando o header `X-Guid`
- Senhas com exatamente 16 caracteres, contendo maiúscula, minúscula, número e caractere especial
- Persistência em SQLite com Entity Framework Core
- Validação de GUID e respostas de erro usando `ProblemDetails`
- Persistência dos GUIDs e senhas após o reinício da aplicação
- Testes unitários e de integração com xUnit
- Documentação para execução e testes no arquivo [docs/como-iniciar-e-testar.md](docs/como-iniciar-e-testar.md)

Neste processo eu aprendi:
* ✅ Como aplicar Spec Driven Development em um projeto completo
* ✅ Como transformar uma constituição, uma especificação e um plano em tarefas implementáveis
* ✅ Como estruturar uma Minimal API em .NET
* ✅ Como separar endpoints, serviço de aplicação, domínio e persistência
* ✅ Como criar testes unitários e testes de integração para endpoints
* ✅ Como tratar erros HTTP com `ProblemDetails`

## Badge
<img src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/halloween-2026/01.png" width="200" />

## Problema
Criar e armazenar senhas fortes de forma simples, permitindo que uma pessoa gere uma senha e consulte o valor posteriormente usando um GUID.

Uma senha gerada deve:

- Ter exatamente 16 caracteres
- Conter pelo menos uma letra maiúscula
- Conter pelo menos uma letra minúscula
- Conter pelo menos um número
- Conter pelo menos um caractere especial permitido
- Não conter espaços em branco

O GUID funciona como um segredo de acesso: quem possuir o GUID poderá consultar a senha associada.

## Stack

- .NET 10
- C#
- ASP.NET Core Minimal API
- Entity Framework Core
- SQLite
- xUnit

## Como executar

Na raiz do projeto, execute:

```bash
dotnet run --project src/PasswordGenerator
```

A API ficará disponível em `http://localhost:5035`. A aplicação também possui uma URL HTTPS em `https://localhost:7250`.

Para executar todos os testes:

```bash
dotnet test src/PasswordGenerator.Tests/PasswordGenerator.Tests.csproj
```

## Endpoints

### Gerar uma senha

```http
POST http://localhost:5035/api/password
```

O endpoint não recebe corpo. A resposta é `201 Created` e contém somente o GUID:

```json
{
  "guid": "3f2504e0-4f89-11d3-9a0c-0305e82c3301"
}
```

### Consultar uma senha

```http
GET http://localhost:5035/api/password
X-Guid: 3f2504e0-4f89-11d3-9a0c-0305e82c3301
```

A resposta é `200 OK`:

```json
{
  "password": "A1b#xY9@mK2_pQ7!"
}
```

Respostas de erro:

- `400 Bad Request`: header `X-Guid` ausente, vazio ou inválido
- `404 Not Found`: GUID válido, mas inexistente
- `500 Internal Server Error`: falha interna da aplicação

## Documentação do projeto

- [Constituição](specs/constitution.md)
- [Especificação](specs/spec.md)
- [Plano de implementação](specs/plan.md)
- [Tarefas](specs/tasks.md)
- [Como iniciar e testar](docs/como-iniciar-e-testar.md)

## Sobre o Halloween 2026
O desafio **Halloween 2026** consiste em implementar o modelo Spec Driven Development de ponta a ponta, criando aplicações completas com IA.

### Veja meu progresso no desafio
[Repositório no GitHub](https://github.com/fabiorezendegithub/balta-desafio-halloween-2026-spec-driven-01)
