# Gerador de Senhas Fortes

## Problema
Geração de senhas fortes exigem várias regras difíceis de serem atingidas.

## Objetivo
Permitir que uma pessoa gere senhas fortes seguindo um padrão e outro que dado um GUID, permite consultar uma senha associada.

## Usuários
- Criador da senha: gera senha forte e recebe um guid vinculado a ela
- Visitante: com o guid, permite consultar uma senha associada

## Histórias
- Como criador, quero gerar uma senha forte e receber um guid de retorno
- Como visitante, quero colocar o guid e receber uma senha forte

## Requisitos funcionais
- RF-01: o sistema gera uma senha forte e devolve um GUID quando a solicitação é aceita.
- RF-02: a senha deve ter pelo menos 16 caracteres, incluir caracteres especiais, letras maiúsculas, letras minúsculas e números, e não pode incluir espaços em branco.
- RF-03: o visitante com um guid, pode ver a senha forte associada a ele
- RF-06: o criador pode definir, opcionalmente, uma data de expiração para a senha; sem data, o GUID e a senha não expiram. A data inclui horário e fuso.
- RF-07: os GUIDs e as senhas continuam válidos após reinício da aplicação.
- RF-08: ao aceitar a criação de uma senha, é gerado um GUID único.
- RF-09: um GUID deve estar vinculado a uma senha; caso contrário, será inválido e sua consulta deve retornar uma resposta distinta das respostas para GUID inexistente ou expirado.

## Regras de negócio
- RN-01: o GUID é único em todo o sistema.
- RN-02: a senha é única em todo o sistema, inclusive após expirar.
- RN-03: GUID expirado não deve mostrar a senha; a consulta deve responder "senha expirada".
- RN-04: o GUID expirado continua ocupado, assim como sua senha, e ambos não devem ser reaproveitados.
- RN-05: cada criação aceita de senha cria um GUID novo. Solicitações recusadas não criam nem reservam GUID.
- RN-06: GUID não vinculado a uma senha é inválido e sua consulta deve retornar uma resposta distinta das respostas para GUID inexistente ou expirado.
- RN-07: datas de expiração e o instante atual devem ser comparados como instantes em uma referência comum, considerando o fuso informado.
- RN-08: o GUID expira no instante exato da data e horário definidos.

## Casos de borda
- Guid inexistente no acesso: responder que o guid não existe
- Data de expiração no passado: recusar a criação sem gerar ou reservar GUID. A validação compara instantes após considerar o fuso informado.
- GUID expirado: responder "senha expirada" e não mostrar a senha.
- GUID existente sem senha vinculada: responder como GUID inválido, com mensagem distinta de GUID inexistente e de GUID expirado.

## Fora de escopo
- Cadastro e login de usuários
- Domínio personalizado
- Relatório com origem geográfica do acesso
- Edição do destino de um guid já criado
- Desativar ou apagar um guid (pendência assumida: um guid criado por engano
  permanece ativo até expirar)

## Critérios de aceite
- Criar uma senha e mostrar o guid
- Duas senhas não podem ter o mesmo guid
- Solicitação com data de expiração no passado responde erro e não gera nem reserva GUID.
- Após reiniciar a aplicação, uma senha criada antes continua funcionando, inclusive quando não foi definida data de expiração.
- Senha expirada não é exibida e a consulta responde "senha expirada".
- GUID inexistente, GUID inválido (sem senha vinculada) e GUID expirado recebem respostas distintas.
- Senhas geradas são únicas em todo o sistema, inclusive em relação às senhas expiradas.