# Guía de Estructuración de Escenas de Boss

Este documento explica cómo está organizada la escena del boss (Pblob) y cómo crear nuevas escenas de boss siguiendo la misma estructura.

## 🔧 RECONSTRUIR ESCENA1 DESDE CERO

Sigue estos pasos en orden para crear una escena limpia y funcional:

### Paso 1: Crear Nueva Escena Vacía
1. En Unity: `File > New Scene > Empty`
2. Guardar como `Escena1_Nueva.unity`

### Paso 2: Crear Estructura de GameObjects
Crea estos GameObjects vacíos como contenedores (usar Create Empty):

```
📦 Escena1
├── --- MANAGERS ---          (Empty GameObject - contenedor)
│   ├── GameFlowManager       (+ componente GameFlowManager.cs)
│   ├── SoundManager          (+ componente SoundManager.cs + AudioSource)
│   ├── UIManager             (+ componente UIManager.cs)
│   ├── LocalizationManager   (+ componente LocalizationManager.cs)
│   └── ElementsRegister      (+ componente ElementsRegister.cs)
│
├── --- PLAYER ---            (Empty GameObject - contenedor)
│   └── MainCharacter         (Instanciar Prefab: Assets/Prefabs/MainCharacter.prefab)
│
├── --- BOSS ---              (Empty GameObject - contenedor)
│   ├── Pblob                 (+ componente PblobController.cs)
│   │   └── MoustachePoints   (+ puntos MP1-MP6 para proyectiles)
│   ├── PblobArena            (Tilemap con paredes, tag: Wall)
│   └── PblobRhythmManager    (+ componente PblobRhythmManager.cs)
│
├── --- ENVIRONMENT ---       (Empty GameObject - contenedor)
│   └── (Tilemaps decorativos, luces, etc.)
│
├── --- UI ---                (Empty GameObject - contenedor)
│   ├── Canvas                (+ Canvas + CanvasScaler + GraphicRaycaster)
│   │   ├── MenuUI            (+ componente MenuUI.cs)
│   │   │   ├── ButtonPlay    (Button + TextMeshPro)
│   │   │   ├── ButtonOptions (Button + TextMeshPro)
│   │   │   ├── ButtonExit    (Button + TextMeshPro)
│   │   │   ├── ButtonResume  (Button - oculto por defecto)
│   │   │   └── ButtonBackToMenu (Button - oculto por defecto)
│   │   │
│   │   ├── DialogUI          (+ componente DialogUI.cs)
│   │   │   └── DialogPanel   (Panel con texto y botón)
│   │   │       ├── DialogText    (TextMeshPro)
│   │   │       └── ButtonNext    (Button para continuar)
│   │   │
│   │   ├── ShopUI            (+ componente ShopUI.cs)
│   │   │   ├── ElementSelection  (Selección de elementos)
│   │   │   ├── EquipSelector     (Selector de equipamiento)
│   │   │   └── ButtonExitShop    (Button para salir)
│   │   │
│   │   ├── DeathUI           (+ componente DeathUI.cs)
│   │   │   ├── DeathPanel    (Panel principal - oculto por defecto)
│   │   │   ├── ButtonRetry   (Button para reintentar)
│   │   │   └── ButtonExitToMenu (Button para volver al menú)
│   │   │
│   │   ├── OptionsUI         (+ componente OptionsUI.cs)
│   │   │   └── (Controles de volumen, idioma, etc.)
│   │   │
│   │   ├── HealthUI          (Corazones del jugador)
│   │   ├── PotionUI          (UI de pociones)
│   │   ├── PblobUI           (+ componente PblobUI.cs - barra de vida del boss)
│   │   ├── ElementsUI        (UI de elementos equipados)
│   │   └── CombinationsUI    (UI de combinaciones)
│   │
│   └── EventSystem           (+ EventSystem + InputSystemUIInputModule)
│
└── Main Camera               (+ Camera + AudioListener + CameraFollow.cs)
```

### Paso 3: Configurar Referencias en Inspector

**GameFlowManager** necesita referencias a:
- `uiManager`: → UIManager
- `pblobController`: → Pblob (PblobController)
- `playerController`: → MainCharacter (PlayerController)
- `playerHPController`: → MainCharacter (PlayerHPController)
- `faseInicial`: MainMenu

**UIManager** necesita referencias a:
- `menuUI`: → MenuUI
- `optionsUI`: → OptionsUI
- `tiendaUI`: → ShopUI
- `dialogUI`: → DialogUI
- `deathUI`: → DeathUI

**MenuUI** necesita referencias a:
- `botonJugar`: → ButtonPlay
- `botonAjustes`: → ButtonOptions
- `botonSalir`: → ButtonExit
- `botonReanudar`: → ButtonResume
- `botonVolverMenu`: → ButtonBackToMenu

**DialogUI** necesita referencias a:
- `dialogPanel`: → DialogPanel
- `dialogText`: → DialogText
- `continueButton`: → ButtonNext

**DeathUI** necesita referencias a:
- `pantallaMuerte`: → DeathPanel
- `botonReintentar`: → ButtonRetry
- `botonSalirAlMenu`: → ButtonExitToMenu

**ShopUI** necesita referencias a:
- `btnShopExit`: → ButtonExitShop
- `elementSelectionUI`: → ElementSelection
- `equipSelectorUI`: → EquipSelector
- `playerEquip`: → MainCharacter (PlayerEquip)

**PblobController** necesita referencias a:
- `attackPatterns`: Array con patrones de ataque
- `phase2Door`: (opcional) Puerta de fase 2

### Paso 4: Configurar Tags y Layers

Verificar que estos Tags existan en Project Settings:
- `Player` - Para MainCharacter y DmgReciever
- `Boss` - Para Pblob
- `Wall` - Para paredes del arena
- `Enemy` - Para enemigos
- `Projectile`, `CharacterProjectile`, `EnemyProjectile`
- `CastPoint`, `Weapon`, `FireTrail`

### Paso 5: Estado Inicial de UIs

Al iniciar, solo MenuUI debe estar visible:
- MenuUI: **Activo**
- DialogUI: Inactivo (DialogPanel inactivo)
- ShopUI: Inactivo
- DeathUI: Inactivo (DeathPanel inactivo)
- OptionsUI: Inactivo

### Paso 6: Probar el Flujo

1. Iniciar juego → Debe aparecer MenuUI
2. Click en Play → Debe ir a IntroDialog (DialogUI)
3. Click en Continuar → Debe ir a ShopBeforeBoss (ShopUI)
4. Click en Salir Tienda → Debe ir a PreBossDialog (DialogUI)
5. Click en Continuar → Debe iniciar BossFight (Pblob activo)
6. Si muere → DeathUI aparece
7. Si gana → PostBossDialog → ShopAfterBoss → etc.

## Estructura de la Escena (Referencia)

Una escena de boss debe tener la siguiente jerarquía organizada:

```
📁 --- MANAGERS ---
    ├── GameFlowManager      # Orquesta el flujo del juego
    ├── SoundManager         # Gestiona audio y música
    ├── UIManager            # Gestiona todas las UIs
    └── ElementsRegister     # Registro de elementos del jugador

📁 --- BOSS ---
    ├── BossController       # Controlador del boss (PblobController, etc.)
    ├── BossArena            # Arena/escenario del boss
    │   └── (Tilemaps, paredes, etc.)
    ├── AttackPatterns       # Contenedor de patrones de ataque
    │   ├── Pattern1
    │   ├── Pattern2
    │   └── ...
    └── RhythmManager        # (Opcional) Sistema de ritmo

📁 --- PLAYER ---
    ├── MainCharacter        # Prefab del personaje principal
    └── DamageReciever       # Colisionador para recibir daño

📁 --- UI ---
    ├── Canvas               # Canvas principal
    │   ├── MenuUI           # Menú principal/pausa
    │   ├── DialogUI         # Diálogos
    │   ├── ShopUI           # Tienda
    │   ├── DeathUI          # Pantalla de muerte
    │   ├── OptionsUI        # Opciones
    │   ├── HealthUI         # Barra de vida del jugador
    │   ├── BossHealthUI     # Barra de vida del boss
    │   └── ElementsUI       # UI de elementos
    └── EventSystem          # Sistema de eventos de Unity
```

## Flujo de Inicialización

El orden de ejecución es importante. Así es como funciona:

### 1. Awake (Inicialización de datos)
- Los componentes inicializan sus datos internos
- NO se buscan referencias a otros objetos aquí
- NO se inician comportamientos activos

### 2. Start (Configuración de referencias)
- Los componentes buscan referencias usando `FindObjectOfType`
- Se suscriben a eventos de otros componentes
- Se configura el estado inicial (desactivado por defecto para bosses)

### 3. GameFlowManager controla el flujo
- **MainMenu**: Se muestra el menú principal
- **IntroDialog**: Se muestra el diálogo introductorio
- **ShopBeforeBoss**: Se muestra la tienda
- **PreBossDialog**: Se muestra el diálogo pre-combate
- **BossFight**: SE INICIA EL COMBATE (aquí se llama `StartBossBattle()`)
- **PostBossDialog**: Diálogo post-victoria
- **ShopAfterBoss**: Tienda después del boss
- **PreNextBossDialog**: Diálogo antes del siguiente boss

## Puntos Importantes

### ✅ Buenas Prácticas

1. **No iniciar el boss automáticamente**
   ```csharp
   // CORRECTO - Esperar a que GameFlowManager inicie la batalla
   void Start()
   {
       // Solo inicializar datos, NO iniciar batalla
   }
   ```

2. **Verificar null antes de suscribirse a eventos**
   ```csharp
   if (bossController != null && bossController.OnHealthChanged != null)
   {
       bossController.OnHealthChanged.AddListener(UpdateHealthBar);
   }
   ```

3. **Desuscribirse de eventos en OnDestroy**
   ```csharp
   void OnDestroy()
   {
       if (bossController != null && bossController.OnHealthChanged != null)
       {
           bossController.OnHealthChanged.RemoveListener(UpdateHealthBar);
       }
   }
   ```

4. **Usar referencias serializadas cuando sea posible**
   ```csharp
   [SerializeField] private PblobController bossController;
   // Mejor que: bossController = FindObjectOfType<PblobController>();
   ```

### ❌ Errores Comunes

1. **Iniciar comportamientos en Start()**
   ```csharp
   // INCORRECTO
   void Start()
   {
       StartBossBattle(); // ¡El boss empieza antes de tiempo!
   }
   ```

2. **No verificar null en referencias**
   ```csharp
   // INCORRECTO - Puede causar NullReferenceException
   SoundManager.Instance.PlayBossMusic();
   
   // CORRECTO
   if (SoundManager.Instance != null)
   {
       SoundManager.Instance.PlayBossMusic();
   }
   ```

3. **Olvidar desuscribirse de eventos**
   ```csharp
   // Causa memory leaks si no se hace
   void OnDestroy()
   {
       bossController.OnHealthChanged.RemoveListener(UpdateHealthBar);
   }
   ```

## Crear un Nuevo Boss

Para crear un nuevo boss:

1. **Duplica la escena `Escena1.unity`** como base
   - Renómbrala según el boss (ej: `Escena2_Boss2.unity`)
   - Elimina los objetos específicos del boss anterior

2. **Crea el controlador del boss**
   - Hereda de `BossController` si quieres compatibilidad con `GameFlowManager`
   - O implementa tus propios eventos

3. **Implementa los patrones de ataque**
   - Hereda de `PblobAttackPattern` o crea tu propia clase base
   - Implementa `StartPattern()` y `StopPattern()`

4. **Configura las referencias en el Inspector**
   - Asigna el nuevo boss controller a `GameFlowManager.bossController`
   - Configura las referencias de UI

5. **Configura la transición desde la escena anterior**
   - En la escena anterior, configura `siguienteBossEscena` en `GameFlowManager`

## Scripts del Boss Pblob

### PblobController.cs
Controlador principal del boss. Gestiona:
- Vida y fases
- Estados vulnerable/invulnerable
- Eventos (OnBossDefeated, OnPhaseChanged, etc.)

### PblobRhythmManager.cs
Sistema de ritmo sincronizado con la música:
- Detecta beats de la música
- Dispara eventos en cada beat
- Los patrones de ataque se sincronizan con estos eventos

### PblobAttackPattern.cs (Base)
Clase base para patrones de ataque:
- `StartPattern()`: Inicia el patrón
- `StopPattern()`: Detiene el patrón

### PblobAttackPattern1.cs
Patrón de hairballs:
- Dispara proyectiles desde puntos del bigote
- Sincronizado con el ritmo
- Aumenta dificultad por fase

### PblobAttackPattern2.cs
Patrón de círculos de movimiento:
- Placeholder para fase 2
- El jugador debe seguir círculos en pantalla

### PblobUI.cs
UI específica del boss:
- Barra de vida con delay visual
- Texto de HP
- Nombre del boss

### Phase2Door.cs
Puerta de transición entre fases:
- Se desbloquea cuando el boss completa fase 1
- Inicia fase 2 cuando el jugador entra

## Notas de Debugging

Todos los scripts tienen modo debug con teclas de acceso rápido:
- `T`: Aplicar daño al boss
- `V`: Toggle vulnerabilidad
- `P`: Avanzar fase
- `2`: Desbloquear fase 2

Estos solo funcionan cuando `debugMode = true` en el Inspector.
