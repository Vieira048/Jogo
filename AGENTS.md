# AGENTS.md

## Projeto

Este repositório é um projeto Unity 2023.2.19f1 chamado `TopDungeon`, baseado em um dungeon crawler 2D com tilemaps, combate, UI persistente e suporte mobile adicionado por scripts próprios.

## Estrutura Principal

- `Assets/_Scenes/`: cenas Unity. Ordem de build atual: `Inicio`, `Dungeon_0`, `Dungeon_1`, `Dungeon_Boss`, `Creditos`; `Fim` está desabilitada.
- `Assets/_Scripts/Manager/`: estado global, save e áudio. `GameManager` e objetos relacionados usam `DontDestroyOnLoad`.
- `Assets/_Scripts/PlayerLogic/`: movimento, combate, câmera e dados do player.
- `Assets/_Scripts/Scene/`: portais, troca de cenas e controle de salas/rotas.
- `Assets/_Scripts/UI/`: HUD, menu, texto flutuante e input mobile.
- `Assets/_Prefabs/Scene/`: prefabs de portais e elementos de cena.
- `ProjectSettings/`: configurações de build, Android, input, física e UI.

## Regras de Trabalho

- Preserve arquivos gerados pelo Unity e altere `.unity`, `.prefab` e `.asset` manualmente somente quando a mudança for pequena e bem localizada.
- Antes de mudar cenas ou prefabs, confirme o impacto com `rg` e mantenha os `fileID` e referências existentes.
- Não reverta alterações locais do usuário. No estado inicial analisado havia alterações em `Logs/` e `UserSettings/`.
- Para buscas, use `rg` ou `rg --files`.
- Para edições manuais, use `apply_patch`.
- Evite depender de caminhos absolutos dentro de assets Unity.

## Pontos Sensíveis

- `GameManager` persiste entre cenas e também persiste `UIManager` e `SaveManager`.
- `Player` também usa `DontDestroyOnLoad` e é reposicionado no `GameManager.LoadState`.
- A base `Colliderable` faz consultas com `BoxCollider2D.Overlap(...)` a cada `Update`, e vários sistemas herdam dela: portais, armas, portas, NPCs, fontes e coletáveis.
- O movimento do player usa `transform.Translate` em `FixedUpdate`, sem `Rigidbody2D`.
- `ProjectSettings/Physics2DSettings.asset` está com `m_AutoSyncTransforms: 0`; consultas de física após mudanças por `Transform` podem depender de sincronização explícita.
- A UI mobile fica em `ControlesMobile`, filho do HUD persistente em `Dungeon_0`.
- A UI foi normalizada para `CanvasScaler.ScaleWithScreenSize`, referência `1280x720`, safe area e controles ancorados nos cantos corretos.
- No Android, saves devem usar `Application.persistentDataPath`; `Application.dataPath` aponta para dentro do APK e não é gravável.

## Documentos de Contexto

- `docs/context/project-context.md`: mapa do projeto e fluxo de cenas.
- `docs/context/mobile-bugfix-plan.md`: diagnóstico e plano para os bugs Android.
- `docs/context/android-verification-checklist.md`: checklist de testes para validar APK e dispositivo.

## Comandos Úteis

```powershell
rg --files
rg -n "SceneManager|LoadScene|Portal|OnTrigger|Overlap|Screen|CanvasScaler|Mobile|safeArea" Assets ProjectSettings
git status --short --branch
```
