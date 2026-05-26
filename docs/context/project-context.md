# Contexto do Projeto

## Identidade

- Projeto Unity: `TopDungeon`
- Versão do editor: Unity 2023.2.19f1
- Plataforma afetada no planejamento atual: Android
- Estilo: dungeon crawler 2D com tilemap, salas conectadas por portais, combate melee e HUD persistente.

## Fluxo de Cenas

A ordem em `ProjectSettings/EditorBuildSettings.asset` é:

1. `Assets/_Scenes/Inicio.unity`
2. `Assets/_Scenes/Dungeon_0.unity`
3. `Assets/_Scenes/Dungeon_1.unity`
4. `Assets/_Scenes/Dungeon_Boss.unity`
5. `Assets/_Scenes/Creditos.unity`

`Assets/_Scenes/Fim.unity` existe, mas está desabilitada no build.

## Objetos Persistentes

`GameManager` em `Assets/_Scripts/Manager/GameManager.cs` mantém:

- `GameManager`
- `UIManager`
- `SaveManager`

`Player` em `Assets/_Scripts/PlayerLogic/Player.cs` também chama `DontDestroyOnLoad`.

Quando uma cena é carregada, `GameManager.LoadState` carrega o save, reposiciona o player no objeto `SpawnPoint` da cena e atualiza a UI.

## Sistemas Principais

- Movimento: `Player` lê teclado ou `MobileInputManager`; `Mover` aplica movimento com `transform.Translate`.
- Colisões de interação: `Colliderable` chama `BoxCollider2D.Overlap(...)` em `Update`.
- Portais entre cenas: `Portal` usa `SceneTranslate.ChangeToScene(sceneName)`.
- Portais dentro da mesma cena/sala: `Portal_Door` move o player para `boxOUT`.
- UI mobile: `ControlesMobile` contém joystick PinePie, ataque e rage. O singleton é `MobileInputManager`.
- HUD/menu: `UIManager`, `CharacterHUD` e `CharacterMenu`.
- Save: `SaveManager` grava em `Application.persistentDataPath` para funcionar no Android.

## Configurações Relevantes

- `ProjectSettings/Physics2DSettings.asset`: `m_AutoSyncTransforms: 0`.
- `ProjectSettings/ProjectSettings.asset`: `androidRenderOutsideSafeArea: 0`.
- `ProjectSettings/ProjectSettings.asset`: `androidMaxAspectRatio: 2.4`.
- `Assets/_Scenes/Dungeon_0.unity`: `ControlesMobile` foi normalizado para tela cheia e safe area.
- `Assets/_Scripts/UI/UIManager.cs`: usa referência mobile `1280x720`, `matchWidthOrHeight = 0.5`, joystick no canto inferior esquerdo e baú/menu no canto superior direito.

## Riscos Locais

- Mudar `Colliderable` afeta muitos sistemas além de portais.
- Mudar cenas manualmente pode quebrar referências serializadas.
- A UI persistente vem de `Dungeon_0`; cenas posteriores podem depender dela já existir.
- O Android precisa ser validado em dispositivo real, porque safe area, aspect ratio, path de save e timing de física variam mais do que no Editor.
