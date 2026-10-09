# Gerador de Senhas Fortes

## Problema
Geração de senhas fortes exigem várias regras difíceis de serem atingidas.

## Objetivo
Permitir que uma pessoa gere senhas fortes seguindo um padrão e outro que dado um GUID, permite consultar uma senha associada.

## Usuários
- Criador da senha: gera senha forte e recebe um guid vinculado a ela
- Visitante: com o guid, informado no header da requisição, permite consultar uma senha associada

## Histórias
- Como criador, quero gerar uma senha forte e receber um guid de retorno
- Como visitante, quero informar o guid no header da requisição e receber uma senha forte

## Requisitos funcionais
- RF-01: a criação não recebe nenhum dado de entrada. O sistema gera uma senha forte, vincula a ela um GUID e devolve apenas o GUID; a senha não é devolvida na criação.
- RF-02: a senha tem exatamente 16 caracteres, inclui pelo menos 1 caractere especial, 1 letra maiúscula, 1 letra minúscula e 1 número, e não inclui espaços em branco. Os caracteres especiais permitidos são: `! @ # $ % ^ & * ( ) - _ = + [ ] { }`.
- RF-03: o visitante com um guid, informado no header `X-Guid` da requisição, pode ver a senha forte associada a ele. A consulta é ilimitada e repetível.
- RF-04: os GUIDs e as senhas continuam válidos após reinício da aplicação.
- RF-05: ao aceitar a criação de uma senha, é gerado um GUID único.
- RF-06: a consulta recebe o GUID apenas pelo header `X-Guid`. Header ausente, vazio ou com valor que não seja um GUID no formato padrão com hífens (GUID malformado) é erro de validação e retorna 400 com ProblemDetails, distinto da resposta para GUID inexistente.
- RF-07: GUID bem formado que não existe no sistema retorna 404 com mensagem própria, informando que o GUID não existe.

## Regras de negócio
- RN-01: o GUID é único em todo o sistema.
- RN-02: a senha é única em todo o sistema.
- RN-03: cada criação aceita de senha cria um GUID novo. Criação que falha não cria nem reserva GUID. A única falha possível na criação é interna (ex.: indisponibilidade).
- RN-04: todo GUID existente está vinculado a exatamente uma senha; não existe GUID sem senha.

## Casos de borda
- GUID inexistente no acesso: responder 404 informando que o guid não existe.
- Header do GUID ausente, vazio ou GUID malformado (valor que não é um GUID no formato padrão): recusar a consulta com 400 ProblemDetails, distinto de GUID inexistente.

## Fora de escopo
- Interface web: o sistema é consumido apenas por API, e o visitante informa o GUID no header da requisição
- Expiração de senha ou de GUID: senhas e GUIDs não expiram
- Cadastro e login de usuários
- Domínio personalizado
- Relatório com origem geográfica do acesso
- Edição do destino de um guid já criado
- Desativar ou apagar um guid (pendência assumida: um guid criado por engano
  permanece ativo indefinidamente)
- Limite de uso ou proteção contra abuso (limite de criações, tentativas de consulta ou volume armazenado). Pendência assumida: a criação é anônima, ilimitada e permanente.

## Riscos aceitos
- O GUID é um segredo de portador: qualquer pessoa que o possua lê a senha, quantas vezes quiser, sem autenticação.
- A senha fica armazenada de forma legível e permanente.
- Colisão de senha ou GUID na geração não é tratada com nova tentativa: a unicidade é garantida pelo banco, e uma colisão (probabilidade desprezível com 16 caracteres aleatórios) faz a criação falhar com erro interno, sem criar nem reservar GUID.

## Critérios de aceite
- Criar uma senha devolve apenas o guid, sem enviar dados na requisição.
- Duas senhas não podem ter o mesmo guid
- Após reiniciar a aplicação, uma senha criada antes continua funcionando.
- Consulta com um guid existente devolve a senha vinculada, e pode ser repetida com o mesmo resultado.
- A senha devolvida tem 16 caracteres, com pelo menos 1 especial (da lista permitida), 1 maiúscula, 1 minúscula e 1 número, e sem espaços.
- Consulta com GUID bem formado e inexistente responde 404, distinto do erro de validação.
- Consulta sem o header do GUID, com header vazio ou com GUID malformado, responde 400 ProblemDetails.
- Senhas geradas são únicas em todo o sistema.
