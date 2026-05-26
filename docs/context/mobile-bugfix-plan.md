# Plano de Correção dos Bugs Mobile

## Escopo

Bug 1: portais para passar de uma sala/cena para outra funcionavam no computador, mas não no Android rodando diretamente no aparelho.

Bug 2: a resolução/layout no celular estava incorreta e alguns itens de UI ficavam escondidos.

Este arquivo registra o diagnóstico inicial, o plano recomendado e o status da implementação aplicada.

## Status da Implementação

Aplicado em 2026-05-26:

- Portais agora usam detecção por componente `Player`, não por nome do objeto.
- Portais sincronizam `Physics2D.SyncTransforms()` antes das consultas de overlap.
- Colliders de portais são configurados como trigger em runtime.
- `Portal` também faz varredura ativa por overlap, o que reduz dependência de eventos de trigger em Android.
- `Portal_Door` ganhou cooldown contra retrigger imediato e calcula a saída por `boxOUT.transform.up * exitOffset`.
- UI mobile passou a usar `CanvasScaler.ScaleWithScreenSize` com referência `1280x720` e match `0.5`.
- `ControlesMobile` e HUD recebem `SafeAreaFitter` para respeitar `Screen.safeArea`.
- Joystick fica no canto inferior esquerdo; baú/menu fica no canto superior direito.
- `Dungeon_0` foi normalizada para canvas/controles em tela cheia.
- Android foi ajustado para `androidRenderOutsideSafeArea: 0` e `androidMaxAspectRatio: 2.4`.
- Save no Android foi corrigido para `Application.persistentDataPath`; `Application.dataPath` apontava para dentro do APK.
- Regressão batch de computador passou com 9 invariantes em `TestResults/ComputerRegressionResults.txt`.
- Suíte EditMode passou com 11/11 testes em `TestResults/EditModeResults.xml`.
- APK Android foi gerado e executado no aparelho conectado em `com.southbegonia.topdungeon`.
- A passagem caminhando para o norte de `Dungeon_0` para `Dungeon_1` foi validada no aparelho `SM_G780G`.

Atualizado em 2026-05-26:

- Corrigido bug em que o player não voltava a andar após morrer, voltar ao menu e iniciar novo jogo.
- `Player` agora possui reset explícito de gameplay, cancelando respawn pendente, reativando `isAlive`, collider, rotação e vida.
- `GameManager` ignora cenas não jogáveis em `LoadState`, prepara corretamente a volta ao menu e usa reset seguro no respawn.
- `SaveManager.NewGame()` também reseta o player persistente e a fúria para uma nova run.
- Regressão batch de computador passou com 10 invariantes.
- Suíte EditMode passou com 13/13 testes.
- Build Android foi gerado e validado com novo jogo e movimento por joystick no aparelho `SM_G780G`.

## Diagnóstico Inicial

### Portais

Arquivos principais:

- `Assets/_Scripts/InteractableItems/Colliderable.cs`
- `Assets/_Scripts/Scene/Portal.cs`
- `Assets/_Scripts/Scene/Portal_Door.cs`
- `Assets/_Scripts/Scene/SceneTranslate.cs`
- `Assets/_Scripts/PlayerLogic/Mover.cs`
- `ProjectSettings/Physics2DSettings.asset`

Achados:

- `Portal` e `Portal_Door` herdam de `Colliderable`.
- `Colliderable` usa `boxCollider.Overlap(filter, hits)` dentro de `Update`.
- O player se move com `transform.Translate(...)` em `FixedUpdate`, sem `Rigidbody2D`.
- A física 2D está com `m_AutoSyncTransforms: 0`.
- Os colliders dos portais nos prefabs não estavam como trigger (`m_IsTrigger: 0`); eles dependiam da consulta manual de overlap.
- Os portais comparavam `coll.name == "Player"`, o que é frágil se o objeto tiver nome diferente por instância/cena.
- O collider do portal de cena em `Dungeon_0` era muito pequeno para a entrada prática pelo corredor no Android.

Hipótese principal:

No Android, a combinação de movimento por `Transform`, consultas de física em `Update`, `AutoSyncTransforms` desligado e colliders pequenos fazia o overlap ficar fora de sincronia ou perder a detecção em alguns frames.

### UI e Resolução

Arquivos principais:

- `Assets/_Scenes/Dungeon_0.unity`
- `Assets/_Scripts/UI/UIManager.cs`
- `Assets/_Scripts/UI/MobileInputManager.cs`
- `ProjectSettings/ProjectSettings.asset`

Achados:

- `ControlesMobile` usava `CanvasScaler` em `Constant Pixel Size`.
- O canvas principal usava referência `800x600` com match em largura (`m_MatchWidthOrHeight: 0`), ruim para celulares widescreen.
- `androidRenderOutsideSafeArea: 1` permitia renderizar fora da área segura do aparelho.
- `androidMaxAspectRatio: 2.1` podia ser baixo para aparelhos 20:9 ou mais largos.
- Os controles estavam ancorados perto dos cantos e podiam cair em notch, barra de gesto ou borda.

Hipótese principal:

A UI mobile não estava sendo escalada nem compensada pela safe area. Em resoluções modernas, parte do HUD/controles podia ficar fora da área visível ou muito próxima das bordas.

### Save no Android

Durante a validação real no aparelho, o portal norte começou a disparar, mas apareceu `DirectoryNotFoundException` em `SaveManager.SaveGame()`.

Causa:

- `SaveManager` gravava em `Application.dataPath + "/SaveData.json"`.
- No Android, `Application.dataPath` aponta para o APK/base do aplicativo, que não é gravável.

Correção:

- `SaveManager` passou a usar `Application.persistentDataPath`.
- Foi mantida migração simples para ler um save legado em `Application.dataPath` quando existir no Editor.
- O portal passou a continuar a troca de cena mesmo se o save falhar, registrando aviso em vez de bloquear a transição.

### Morte e Novo Jogo

Após a morte, `Player.Death()` marcava `isAlive = false` e iniciava uma corrotina de respawn. Como o `Player` é persistente entre cenas, voltar ao menu antes de concluir esse fluxo podia deixar o mesmo objeto de player morto. Ao clicar em “Novo Jogo”, a cena era carregada, mas a movimentação continuava bloqueada porque `FixedUpdate()` só processa input quando `isAlive` é verdadeiro.

Correção:

- `Player.ResetForGameplay(...)` centraliza o reset do estado jogável.
- `Player.CancelPendingRespawn()` cancela corrotina de respawn ao voltar ao menu ou iniciar nova run.
- `GameManager.LoadState(...)` só roda em cenas jogáveis e restaura o player quando ele entra morto na cena.
- `SaveManager.NewGame()` chama reset completo de player e fúria.

## Plano Recomendado

### Fase 1: Instrumentar e Reproduzir

1. Adicionar logs controlados por `Debug.isDebugBuild` em `Portal`, `Portal_Door` e sistemas relacionados para confirmar se o Android detecta overlap com o player.
2. Gerar um APK development build com script debugging.
3. Validar em dispositivo:
   - `Inicio` -> `Dungeon_0`.
   - `Dungeon_0` -> `Dungeon_1`.
   - Portais internos de `Dungeon_1`.
   - `Dungeon_1` -> boss, se aplicável.
4. Coletar `adb logcat` filtrando por `Unity`.

### Fase 2: Corrigir Portais

Opção preferida:

1. Criar uma detecção dedicada para portais usando `OnTriggerEnter2D`/`OnTriggerStay2D`.
2. Marcar colliders dos portais como `isTrigger`.
3. Trocar `coll.name == "Player"` por verificação por componente `Player`.
4. Colocar um pequeno cooldown/debounce no portal para impedir retrigger imediato.
5. Em `Portal_Door`, calcular a saída pelo `boxOUT.transform.up` em vez de comparar quaternions exatos:
   - `var exit = boxOUT.transform.position + boxOUT.transform.up * exitOffset;`
   - fixar `z = 0`.
6. Em portais de cena, manter uma varredura ativa por overlap como fallback para Android.

Alternativa de menor alteração:

1. Mover a consulta de `Colliderable` para `FixedUpdate`.
2. Chamar `Physics2D.SyncTransforms()` antes das consultas que dependem do player.
3. Ainda trocar a checagem por nome para componente/tag.

A opção preferida é mais segura para portais, porque reduz dependência de timing e evita alterar todos os herdeiros de `Colliderable`.

### Fase 3: Corrigir UI Mobile

1. Ajustar `ControlesMobile` para `Scale With Screen Size`.
2. Usar referência landscape `1280x720` e `Match Width Or Height` em `0.5`.
3. Criar `SafeAreaFitter` para aplicar `Screen.safeArea` em um `RectTransform` raiz do HUD/controles.
4. Ancorar joystick no canto inferior esquerdo e botões no canto inferior direito com margens dentro da safe area.
5. Ancorar o baú/menu no canto superior direito.
6. Desligar renderização fora da safe area para Android.
7. Revisar `androidMaxAspectRatio` para suportar aparelhos widescreen comuns.

### Fase 4: Validar Regressão

1. Testar no Editor com resoluções:
   - `800x600`
   - `1280x720`
   - `1920x1080`
   - `2400x1080`
2. Testar Android real em landscape.
3. Confirmar que:
   - portais disparam uma vez por entrada;
   - transição de cena conclui;
   - player aparece no `SpawnPoint` ou `boxOUT` correto;
   - joystick, ataque, rage, HUD e menu ficam visíveis;
   - nada fica abaixo de notch/barra de gestos;
   - save é criado no path persistente do Android.

## Arquivos Alterados ou Criados na Implementação

- `Assets/_Scripts/Manager/SaveManager.cs`
- `Assets/_Scripts/Scene/Portal.cs`
- `Assets/_Scripts/Scene/Portal_Door.cs`
- `Assets/_Scripts/Scene/PortalFinal.cs`
- `Assets/_Scripts/Scene/PortalPlayerDetector.cs`
- `Assets/_Scripts/UI/SafeAreaFitter.cs`
- `Assets/_Scripts/UI/UIManager.cs`
- `Assets/_Scenes/Dungeon_0.unity`
- `ProjectSettings/ProjectSettings.asset`
- `Assets/Tests/EditMode/Editor/PortalRegressionTests.cs`
- `Assets/Tests/EditMode/Editor/MobileLayoutRegressionTests.cs`
- `Assets/Tests/EditMode/Editor/SaveManagerRegressionTests.cs`
- `Assets/Editor/ComputerRegressionRunner.cs`
- `Assets/Editor/AndroidBuildRunner.cs`

## Ordem de Implementação Sugerida

1. Corrigir portais em código com mudança pequena e testável.
2. Ajustar prefabs/cenas de portal.
3. Corrigir scaler e safe area da UI.
4. Ajustar `ProjectSettings` do Android.
5. Corrigir save para path gravável no Android.
6. Fazer build Android development e validar no aparelho.
