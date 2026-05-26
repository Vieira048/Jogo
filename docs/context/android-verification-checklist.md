# Checklist de Verificação Android

## Última Verificação Executada

Executada em 2026-05-26 no aparelho `SM_G780G` conectado por ADB:

- Build Android development gerado em `Builds/Android/TopDungeon.apk`.
- Resultado do build salvo em `TestResults/AndroidBuildResults.txt`.
- Pacote instalado e em primeiro plano: `com.southbegonia.topdungeon`.
- UI validada no aparelho:
  - joystick no canto inferior esquerdo;
  - baú/menu no canto superior direito;
  - ataque e rage no canto inferior direito;
  - HUD dentro da área visível.
- Passagem caminhando para o norte validada de `Dungeon_0` para `Dungeon_1`.
- Capturas principais:
  - `TestResults/android-hud-finalbuild.png`
  - `TestResults/android-north-finalbuild-centered.png`
- Save validado no path persistente do Android:
  - `/sdcard/Android/data/com.southbegonia.topdungeon/files/SaveData.json`

## Antes do Build

- Confirmar Unity 2023.2.19f1.
- Confirmar cenas habilitadas em `ProjectSettings/EditorBuildSettings.asset`.
- Confirmar que `Dungeon_0` ainda contém `GameManager`, `UIManager`, `SaveManager`, `Player` e `SpawnPoint`.
- Confirmar que `ControlesMobile` tem referências para joystick, ataque, rage e imagem de rage.
- Confirmar que os portais têm destino válido:
  - `Portal.sceneName`
  - `Portal_Door.boxOUT`
- Confirmar que `SaveManager` usa `Application.persistentDataPath`.

## Build de Diagnóstico

- Usar Development Build.
- Ativar Script Debugging quando for investigar logs.
- Rodar em Android real, não apenas em Game view.
- Coletar logs com:

```powershell
adb logcat -s Unity
```

## Testes de Portal

- `Inicio` -> `Dungeon_0` pelo botão de jogo.
- `Dungeon_0` -> `Dungeon_1` caminhando para o norte até o portal de cena.
- Portais internos em `Dungeon_1`:
  - cima;
  - baixo;
  - esquerda;
  - direita.
- Portal para boss/final, se existir no fluxo jogável.

Resultado esperado:

- O player é detectado pelo portal.
- O portal não dispara repetidamente em loop.
- A troca de cena carrega até o fim.
- O player aparece na posição correta.
- O collider do portal volta a aceitar entrada quando isso for esperado.
- Não há `DirectoryNotFoundException` relacionada a `SaveData.json`.

## Testes de UI Mobile

- Testar em pelo menos um aparelho widescreen.
- Testar com navegação por gestos ativa.
- Confirmar que estes elementos ficam dentro da área visível:
  - barra de vida;
  - barra de XP;
  - barra de rage;
  - joystick;
  - botão de ataque;
  - botão de rage;
  - menu e botão de voltar/sair, se visíveis.
- Confirmar que botões continuam clicáveis após aplicar safe area.
- Confirmar que controles não cobrem informações críticas do HUD.

## Testes de Regressão no Editor

- Runner rígido:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\2023.2.19f1\Editor\Unity.exe' -batchmode -projectPath 'C:\Users\Leo_s\Documents\Jogo' -executeMethod ComputerRegressionRunner.RunAll -logFile 'C:\Users\Leo_s\Documents\Jogo\Logs\ComputerRegressions.log'
```

- Suíte EditMode:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\2023.2.19f1\Editor\Unity.exe' -batchmode -projectPath 'C:\Users\Leo_s\Documents\Jogo' -runTests -testPlatform EditMode -testResults 'C:\Users\Leo_s\Documents\Jogo\TestResults\EditModeResults.xml' -logFile 'C:\Users\Leo_s\Documents\Jogo\Logs\EditModeTests.log'
```

- Play Mode em `Inicio`.
- Play Mode direto em `Dungeon_0`.
- Game view em aspect ratios:
  - `4:3`;
  - `16:9`;
  - `20:9`;
  - resolução próxima do aparelho Android usado.

## Sinais de Problema

- `NullReferenceException` em `SceneTranslate`, `GameManager.LoadState` ou `MobileInputManager`.
- `DirectoryNotFoundException` ao salvar `SaveData.json`.
- `SpawnPoint` ausente em alguma cena carregada.
- `boxOUT` nulo em algum `Portal_Door`.
- UI visível no Editor, mas fora da tela no Android.
- Player preso no portal por retrigger imediato.
