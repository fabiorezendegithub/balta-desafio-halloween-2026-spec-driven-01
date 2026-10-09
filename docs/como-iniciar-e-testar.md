# Como iniciar e testar a API

## Iniciar a aplicação

Na raiz do projeto, execute:

```bash
dotnet run --project src/PasswordGenerator
```

A API estará disponível em:

```text
http://localhost:5035
```

Também existe uma URL HTTPS:

```text
https://localhost:7250
```

## Endpoints

A aplicação possui dois endpoints para gerar e consultar senhas.

### Gerar uma senha

```http
POST http://localhost:5035/api/password
```

Não é necessário enviar corpo ou headers.

Resposta esperada: `201 Created`

```json
{
  "guid": "3f2504e0-4f89-11d3-9a0c-0305e82c3301"
}
```

A senha não é retornada nesse endpoint. Guarde o `guid` recebido para consultar a senha depois.

### Consultar uma senha

```http
GET http://localhost:5035/api/password
X-Guid: 3f2504e0-4f89-11d3-9a0c-0305e82c3301
```

O header `X-Guid` é obrigatório e deve conter um GUID no formato padrão, com hífens.

Resposta esperada: `200 OK`

```json
{
  "password": "A1b#xY9@mK2_pQ7!"
}
```

## Testando pelo VS Code

Se estiver usando a extensão **REST Client**, crie um arquivo `api.http` com o conteúdo abaixo:

```http
@baseUrl = http://localhost:5035

### Gerar senha
POST {{baseUrl}}/api/password

### Consultar senha
GET {{baseUrl}}/api/password
X-Guid: COLE_O_GUID_AQUI
```

Execute primeiro a requisição `POST`, copie o `guid` retornado e substitua `COLE_O_GUID_AQUI` na requisição `GET`. Depois, clique em **Send Request** acima da requisição.

## Possíveis respostas de erro

- `400 Bad Request`: o header `X-Guid` está ausente ou contém um GUID inválido.
- `404 Not Found`: o GUID é válido, mas não existe na aplicação.
- `500 Internal Server Error`: ocorreu uma falha interna na aplicação.

