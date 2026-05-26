# DeathtrapDungeon

- [Protótipo do jogo](#protótipo-do-jogo)
- [Demonstração](#demonstração)
- [Recursos gráficos](#recursos-gráficos)
- [Implementação](#implementação)
- [Discussão técnica](#discussão-técnica)
- [Observações](#observações)
- [Referências](#referências)

## Protótipo do Jogo

**DeathtrapDungeon** é um jogo de aventura em masmorra no estilo **2D roguelike**. O jogador explora salas conectadas, enfrenta monstros com uma espada e tenta sobreviver dentro de uma masmorra perigosa. Os inimigos também são ameaçadores, então avançar exige atenção, boas escolhas e uso cuidadoso dos recursos.

- Blog original: [post do projeto DeathtrapDungeon](https://www.cnblogs.com/SouthBegonia/p/11604918.html)
- Repositório original: [DeathtrapDungeon - SouthBegonia](https://github.com/SouthBegonia/DeathtrapDungeon)
- Download jogável original: [DeathtrapDounge, código `wekp`](https://pan.baidu.com/s/1YhGINK1zqLKmD6bp1C29tA)

## Demonstração

![](https://img2018.cnblogs.com/blog/1688704/201910/1688704-20191014092449918-1863025223.gif)

![](https://img2018.cnblogs.com/blog/1688704/201910/1688704-20191014092511727-133877714.gif)

![](https://img2018.cnblogs.com/blog/1688704/201910/1688704-20191014092530510-377580805.gif)

![](https://img2018.cnblogs.com/blog/1688704/201910/1688704-20191014092616483-819960666.gif)

![](https://img2018.cnblogs.com/blog/1688704/201910/1688704-20191014092640210-874694545.gif)

![](https://img2018.cnblogs.com/blog/1688704/201910/1688704-20191014092708669-1567758134.gif)

![](https://img2018.cnblogs.com/blog/1688704/201910/1688704-20191022220131532-786062296.gif)

## Recursos Gráficos

- Os principais recursos de arte vêm de [Dungeon Tileset - itch.io](https://0x72.itch.io/16x16-dungeon-tileset).

## Implementação

**Sistema central**

- `GameManager.cs`: usa padrão singleton e centraliza o gerenciamento das principais instâncias do jogo.

**Classes de base**

- `Fighter.cs`: vida, dano e lógica de combate.
- `Mover.cs`: movimentação.
- `Collectable.cs`: interação com colisores do jogador.

**Jogador**

- `Player.cs`: sistema de fúria, troca de visual, morte e renascimento.
- `Weapon.cs`: ataque com arma e habilidade de fúria.

**Inimigos**

- `EnemyHitBox.cs`: repassa dano ao jogador.
- `Enemy.cs`: classe base da maioria dos inimigos, com experiência, perseguição, ataque, morte e reaparecimento.
- `Enemy_Chest.cs`: baú inimigo.
- `Trap.cs`: armadilhas.
- `Boss0.cs`: chefe final.

**Interface**

- `UIManager.cs`: gerenciamento da interface.
- `CharacterMenu.cs`: menu do personagem.
- `CharacterHUD.cs`: vida, experiência, fúria e tela de morte.
- `SCUI.cs`: tela de carregamento para troca assíncrona de cena.
- `FloatingTextManager.cs` e `FloatingText.cs`: sistema de textos flutuantes.

**Cenas**

- `CameraFollow.cs`: câmera seguindo o jogador.
- `SceneTranslate.cs`: troca assíncrona de cenas.
- `Portal.cs`: portal entre cenas diferentes.
- `Portal_Door.cs`: portal dentro da mesma cena ou sala.

**Objetos Interativos**

- `NPCTextPerson.cs`: interação com NPCs.
- `Chest.cs`: baús.
- `Crate.cs`: objetos destrutíveis.
- `HealingFountain.cs`: fonte de cura.
- `Door.cs`: portas.

![](https://img2018.cnblogs.com/blog/1688704/201910/1688704-20191022220208959-2106713466.png)
![](https://img2018.cnblogs.com/blog/1688704/201910/1688704-20191022220216548-386181557.png)
![](https://img2018.cnblogs.com/blog/1688704/201910/1688704-20191022220224478-570747477.png)
![](https://img2018.cnblogs.com/blog/1688704/201910/1688704-20191022220238304-1366744285.png)

## Discussão Técnica

### Animação de Ataque com Arma

1. A animação de balanço da arma deve durar mais de `0,3s`; se for curta demais, o golpe até acerta o inimigo, mas pode não afastá-lo o suficiente para evitar contra-ataques.
2. O movimento de corte até a posição horizontal deve acontecer na primeira metade da animação, porque esse trecho concentra a colisão de dano da arma.
3. A animação tenta simular um corte realista em quatro partes:
   - `p1`: a arma sobe e recua; velocidade moderada, collider desativado.
   - `p2`: corte principal de `90°` até `0°`; começo rápido, final ainda mais rápido, collider ativado.
   - `p3`: corte final de `0°` até `-20°`; foco em manter alguns frames de contato.
   - `p4`: recolhimento rápido da arma, collider desativado.

![](https://img2018.cnblogs.com/blog/1688704/201909/1688704-20190928211647918-820959590.gif)

### Desenho do Tilemap

1. O ideal é definir a escala em pixels e o estilo visual antes de montar os mapas, para evitar que recursos adicionados depois destoem do cenário.
2. Também é importante escolher cedo os tipos de tile: tiles comuns com objetos animados, `RuleTile` ou uma combinação controlada.
3. A ordem de renderização dos tiles depende das camadas do `Tilemap`, como parede superior, parede inferior e camada que pode ou não encobrir o jogador.
4. Em objetos que não são tiles, a ordem visual é definida por `SpriteRenderer`, `Sorting Layer` e `Order in Layer`.

### Colisão 2D

- Sempre que possível, mantenha os objetos no mesmo plano `z = 0`; diferenças no eixo `z` podem causar falhas difíceis de diagnosticar em colisões e consultas 2D.

### Otimização de Draw Calls

- Mapa em `Tilemap`: pode consumir várias draw calls, principalmente nas camadas de parede. Usar recursos visuais consistentes e marcar objetos estáticos como `Static` ajuda a reduzir custo.
- Menu de equipamento: tende a consumir mais draw calls porque mistura sprites de jogador, arma, botões e textos.
- HUD: também usa múltiplos sprites e textos; a margem de otimização é menor sem redesenhar os elementos.
- Objetos com `Animator`: quando possível, configurar `Culling Mode` como `Cull Completely` evita animações fora da tela.

### Otimização de GC

- Use o Profiler para localizar `GC Alloc` em scripts e reduzir alocações em loops, criação repetida de objetos e uso desnecessário de `foreach`.
- Objetos temporariamente fora de uso podem ser desativados para reduzir trabalho de atualização.

### Otimização de Memória

- Por ser um jogo 2D, a otimização mais direta aplicada ao projeto original foi trocar o `Load Type` da música de fundo para `Streaming`.
- Outras opções possíveis incluem `AssetBundle`, redução de textura e uso adequado de `MipMap`.

## Observações

Os sistemas centrais de combate, troca de cena e interação já estão estruturados. O `Tilemap` inclui chão, paredes superiores e inferiores, objetos de cenário, camada de colisão, `AnimatedTile` para elementos como fontes e lava, além de pincéis para posicionamento de baús.

Para criar fases próprias, é possível reaproveitar a base atual e ajustar tilemaps, valores de jogo, posicionamento de objetos, falas de NPCs e conexões de portais.

O site [itch.io](https://itch.io/game-assets) possui muitos recursos gráficos 2D que podem ser usados conforme suas respectivas licenças.

![](https://img2018.cnblogs.com/blog/1688704/201909/1688704-20190928211702664-1369000020.gif)

## Referências

- [Fluxo de trabalho para criar RPG com Unity e C#](https://www.bilibili.com/video/av45071686/?p=1)
- [16x16 Dungeon Tileset - itch.io](https://0x72.itch.io/16x16-dungeon-tileset)
- [2d-extras - Unity Technologies](https://github.com/Unity-Technologies/2d-extras)
- [Configuração de recursos gráficos no Unity - ww38362087](https://blog.csdn.net/ww386362087/article/details/81365595)
- [Redução de Draw Calls no Unity - linuxheik](https://blog.csdn.net/linuxheik/article/details/80688109)
