## Pacman Project

Jogo clássico **Pac-Man** reimplementado em **C# / .NET 9** usando a **Uno Platform**, com suporte a múltiplas plataformas (desktop, navegador via WebAssembly, Android e iOS).  
O foco é reproduzir as mecânicas originais (movimento em grid, IA dos fantasmas, power pellets, pontuação e vidas) com uma arquitetura de código limpa e bem organizada.

---

## Visão geral

- **Nome do projeto**: `PacmanGameProject`
- **Tecnologias principais**:
  - C# / .NET 9
  - Uno Platform (WinUI + hosting multiplataforma)
  - XAML para a camada de UI (`MenuPage`, `MainPage`, `ScorePage`)
- **Projeto principal**: [`PacmanGameProject/PacmanGameProject.csproj`](PacmanGameProject/PacmanGameProject.csproj)
- **Targets suportados** (via `TargetFrameworks`):
  - `net9.0-android`
  - `net9.0-ios`
  - `net9.0-browserwasm`
  - `net9.0-desktop`

> O arquivo [`PacmanGameProject/ReadMe.md`](PacmanGameProject/ReadMe.md) contém apenas instruções genéricas da Uno Platform.  
> Este `README.md` na raiz é a **documentação principal do jogo Pac-Man**.

---

## Stack e dependências

- **Linguagem / runtime**
  - C# com .NET 9 (`global.json` e `PacmanGameProject.csproj`).
- **Uno Platform**
  - Inicialização de app específica por plataforma:
    - Desktop: [`PacmanGameProject/Platforms/Desktop/Program.cs`](PacmanGameProject/Platforms/Desktop/Program.cs)
    - WebAssembly: [`PacmanGameProject/Platforms/WebAssembly/Program.cs`](PacmanGameProject/Platforms/WebAssembly/Program.cs)

- **Pacotes NuGet principais** (ver `<ItemGroup>` no `.csproj`):
  - `NAudio`
  - `NetCoreAudio`
  - `System.Windows.Extensions`

Assets gráficos e de áudio ficam principalmente em:

- Ícones e splash: `PacmanGameProject/Assets/Icons`, `PacmanGameProject/Assets/Splash`
- Outros assets (sons, tiles, etc.): subpastas dentro de `PacmanGameProject/Assets`

---

## Como executar o projeto

### Pré-requisitos

- **.NET 9 SDK** instalado na máquina.
- IDE recomendada:
  - Visual Studio, Rider ou VS Code com extensões para C#.

### Build da solução pelo terminal

Na raiz do repositório (`/home/sallein/pacmangame_project`):

```bash
dotnet build
```

### Executar versão Desktop

```bash
dotnet run -f net9.0-desktop --project PacmanGameProject
```

Isso inicia a aplicação usando o host desktop configurado em  
`PacmanGameProject/Platforms/Desktop/Program.cs` (Win32, X11, etc.).

### Executar versão WebAssembly (navegador)

```bash
dotnet run -f net9.0-browserwasm --project PacmanGameProject
```

O host WebAssembly definido em  
`PacmanGameProject/Platforms/WebAssembly/Program.cs` sobe um servidor local.  
Após o build, acesse o endereço indicado no terminal (normalmente algo como `http://localhost:XXXX`) no navegador.

### Android / iOS

Para rodar em **Android** ou **iOS**, é recomendado usar uma IDE com suporte a deploy móvel:

- Abra a solução na IDE
- Selecione o target `net9.0-android` ou `net9.0-ios`
- Configure emulador/dispositivo e execute o projeto

Detalhes de publicação em loja não estão cobertos neste README.

---

## Gameplay e mecânicas

### Entidades principais

- **Pacman**
  - Implementado em [`PacmanGameProject/Game/Entities/Pacman.cs`](PacmanGameProject/Game/Entities/Pacman.cs)
  - Move-se em um **grid de tiles** (cada tile = 8px) e trabalha com:
    - `CurrentDirection`: direção atual
    - `DesiredDirection`: direção desejada (vinda do input)
  - Exposição da posição em grid via `GridPosition`:
    - Útil para IA dos fantasmas e lógica de colisão.
  - Teletransporte pelo túnel:
    - Se sair pela esquerda, reaparece à direita e vice-versa.

- **Fantasmas**
  - Implementados em [`PacmanGameProject/Game/Entities/Ghost.cs`](PacmanGameProject/Game/Entities/Ghost.cs)
  - Cada fantasma tem:
    - `Type` (`GhostType`): Blinky, Pinky, Inky, Clyde
    - `CurrentState` (`GhostState`): `Scatter`, `Chase`, `Frightened`, `Eaten`, `InHouse`
    - `GridPosition` para tomada de decisão em tiles
  - A lógica de movimento usa:
    - `GhostMover`, `GhostExitHandler`, `GhostDirectionService`
    - `GhostWaveService` para alternar ondas Scatter/Chase.

- **Pellets**
  - Entidade em [`PacmanGameProject/Game/Entities/Pellet.cs`](PacmanGameProject/Game/Entities/Pellet.cs)
  - Criados a partir do mapa em `MapRenderer`:
    - IDs específicos (`40`, `46`) indicam pellet normal ou power pellet.

### Estados dos fantasmas

Enumerados em [`PacmanGameProject/Game/Enums/GhostState.cs`](PacmanGameProject/Game/Enums/GhostState.cs):

- **Scatter**: cada fantasma mira um canto específico do mapa.
- **Chase**: os comportamentos de IA perseguem o Pacman de formas diferentes:
  - Exemplo de comportamento (`BlinkyBehavior`):  
    [`PacmanGameProject/Game/AI/BlinkyBehavior.cs`](PacmanGameProject/Game/AI/BlinkyBehavior.cs) mira diretamente o tile onde o Pacman está.
- **Frightened**: ativado por power pellets; fantasmas ficam mais lentos e podem ser comidos.
- **Eaten**: fantasmas retornam rapidamente à casa para respawn.
- **InHouse**: fantasmas dentro da casa aguardando liberação.

### Pontuação, vidas e fim de jogo

- **Serviço de estado de jogo**: [`PacmanGameProject/Game/Services/GameStateService.cs`](PacmanGameProject/Game/Services/GameStateService.cs)
  - `Lives`: vidas restantes (inicialmente 3)
  - `Score`: pontuação atual
  - Eventos importantes:
    - `OnGameOver`
    - `OnLifeChanged`
    - `OnScoreChanged`
    - `OnGameWon`
  - `PacmanDied()` decrementa vidas e dispara `OnGameOver` quando chega a 0.
  - `AddScore(int points)` soma pontos e notifica UI.

---

## Arquitetura interna

### Engine de jogo

- **GameLoop**
  - Arquivo: [`PacmanGameProject/Game/Engine/GameLoop.cs`](PacmanGameProject/Game/Engine/GameLoop.cs)
  - Responsável por:
    - Configurar um `DispatcherTimer` com intervalo de ~16ms (**60 FPS**)
    - Atualizar a cada tick:
      - `GhostWaveService` (ondas global de Scatter/Chase)
      - Input e movimento do Pacman
      - Movimento e IA dos fantasmas
    - Disparar o evento `OnUpdate` para que a tela redesenhe o frame.
  - Define constantes de tamanho do mapa (colunas e linhas) e lida com lógica de colisão com paredes.

- **MapData**
  - Arquivo: [`PacmanGameProject/Game/Engine/MapData.cs`](PacmanGameProject/Game/Engine/MapData.cs)
  - Representa o layout do mapa como uma matriz de inteiros (`int[,] Layout`).
  - Também fornece métodos auxiliares como `IsWall(...)` para identificar paredes.

### Entidades e colisão

- **Interface de colisão**
  - Arquivo: [`PacmanGameProject/Game/Entities/Interfaces/ICollidable.cs`](PacmanGameProject/Game/Entities/Interfaces/ICollidable.cs)
  - Usada por entidades como `Pacman`, `Ghost` e `Pellet` para padronizar checagem de colisão.

- **Serviços de colisão**
  - `CollisionService`: detecção geral de colisão com paredes.
  - `GhostCollisionService`: colisões entre Pacman e fantasmas.
  - `PelletService`: gerenciamento de coleta de pellets e power pellets.
  - Interfaces relacionadas em `Game/Services/interfaces`.

### IA dos fantasmas

- **Interface de comportamento**
  - Arquivo: [`PacmanGameProject/Game/AI/IGhostBehavior.cs`](PacmanGameProject/Game/AI/IGhostBehavior.cs)
  - Define `GetTargetTile(...)` que retorna o tile alvo para cada fantasma.

- **Implementações concretas**
  - `BlinkyBehavior`, `PinkyBehavior`, `InkyBehavior`, `ClydeBehavior` em `Game/AI`.
  - Cada uma decide o tile alvo com uma estratégia diferente, reproduzindo o estilo do jogo original.

- **GhostWaveService**
  - Arquivo: [`PacmanGameProject/Game/Services/GhostWaveService.cs`](PacmanGameProject/Game/Services/GhostWaveService.cs)
  - Gerencia a linha do tempo de ondas **Scatter/Chase** e atualiza o estado global dos fantasmas.

### Rendering (mapa e sprites)

- **MapRenderer**
  - Arquivo: [`PacmanGameProject/Game/Rendering/MapRenderer.cs`](PacmanGameProject/Game/Rendering/MapRenderer.cs)
  - Responsável por:
    - Desenhar o mapa base (`Tiles`) em um `Canvas`.
    - Identificar tiles que representam pellets e criar:
      - Entidades `Pellet`
      - Sprites (`Image`) correspondentes, armazenados em `PelletSprites`.

- **SpriteRenderer**
  - Arquivo: [`PacmanGameProject/Game/Rendering/SpriteRenderer.cs`](PacmanGameProject/Game/Rendering/SpriteRenderer.cs)
  - Cuida da atualização visual de Pacman e fantasmas com base nas posições de suas entidades.

- **Assets**
  - Tiles do mapa: `PacmanGameProject/Assets/Tiles/*.png`
  - Sons (por exemplo, chomping do Pacman): `PacmanGameProject/Assets/sounds/...`

### Input e Views (UI)

- **InputManager**
  - Arquivo: [`PacmanGameProject/Game/Input/InputManager.cs`](PacmanGameProject/Game/Input/InputManager.cs)
  - Traduz eventos de teclado em `DesiredDirection` para o Pacman.

- **Páginas XAML**
  - `MenuPage`:
    - Arquivo XAML: [`PacmanGameProject/Game/Views/MenuPage.xaml`](PacmanGameProject/Game/Views/MenuPage.xaml)
    - Code-behind: `MenuPage.xaml.cs`
    - Exibe:
      - Título principal do jogo
      - Botão para iniciar jogo
      - Botão para quadro de pontuação
      - Botão para sair
      - Botão de mute de áudio
  - `MainPage`:
    - Arquivo XAML: [`PacmanGameProject/Game/Views/MainPage.xaml`](PacmanGameProject/Game/Views/MainPage.xaml)
    - Code-behind: `MainPage.xaml.cs`
    - Contém:
      - HUD (tempo, score, vidas)
      - `MapCanvas` e `SpriteCanvas` dentro de um `Viewbox` para manter proporção
      - Camada de debug opcional
      - Overlays de Game Over e vitória
  - `ScorePage`:
    - Arquivo XAML: [`PacmanGameProject/Game/Views/ScorePage.xaml`](PacmanGameProject/Game/Views/ScorePage.xaml)
    - Code-behind: `ScorePage.xaml.cs`
    - Mostra o quadro de pontuação usando `ListView` vinculado a `ScoreEntry`.

### Serviços de domínio

Serviços principais em `PacmanGameProject/Game/Services`:

- `GameStateService`: vidas, pontuação, eventos de game over e vitória.
- `ScoreService`: gerenciamento de `ScoreEntry` e persistência / carga de pontuações.
- `GameAudioService`: reprodução de sons (chomping, morte, etc.).
- `EntitySpawnService`: spawn inicial de Pacman, fantasmas e pellets.
- `FrightenedModeService`: duração e controle do modo frightened após power pellet.
- `GameInitializerService`: orquestra criação de mapa, entidades e serviços ao iniciar partida.

---

## Estrutura de pastas (resumo)

Dentro de `PacmanGameProject/`:

- **`Game/Engine`**: loop principal do jogo (`GameLoop`), definição e dados do mapa (`MapData`).
- **`Game/Entities`**: entidades de domínio (`Pacman`, `Ghost`, `Pellet`, interfaces de colisão).
- **`Game/AI`**: comportamentos de IA dos fantasmas e auxiliares.
- **`Game/Rendering`**: renderização de mapa e sprites (`MapRenderer`, `SpriteRenderer`, `TileSet`).
- **`Game/Services`**: serviços de negócio (estado de jogo, score, áudio, spawn, frightened, colisões).
- **`Game/Views`**: páginas XAML (`MenuPage`, `MainPage`, `ScorePage`) e respectivos `*.xaml.cs`.
- **`Game/Input`**: tratamento de input (teclado, etc.).
- **`Platforms/*`**: bootstraps específicos de cada plataforma (Desktop, WebAssembly, Android, iOS).
- **`Assets/*`**: imagens, ícones, tiles, sons, splash screen.

