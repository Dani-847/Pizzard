/// <summary>
/// Fases del flujo de juego.
/// El ciclo típico es: MainMenu -> IntroDialog -> ShopBeforeBoss -> PreBossDialog -> BossFight -> 
/// PostBossDialog -> ShopAfterBoss -> PreNextBossDialog -> (siguiente boss o fin).
/// </summary>
public enum GamePhase
{
    None,
    /// <summary>Menú principal inicial del juego.</summary>
    MainMenu,
    /// <summary>Diálogo introductorio al entrar en la escena del boss.</summary>
    IntroDialog,
    /// <summary>Tienda antes de enfrentar al boss.</summary>
    ShopBeforeBoss,
    /// <summary>Diálogo justo antes del combate.</summary>
    PreBossDialog,
    /// <summary>Combate activo contra el boss.</summary>
    BossFight,
    /// <summary>Diálogo tras derrotar al boss.</summary>
    PostBossDialog,
    /// <summary>Tienda después del boss (preparación para el siguiente).</summary>
    ShopAfterBoss,
    /// <summary>Diálogo antes de pasar al siguiente boss.</summary>
    PreNextBossDialog,
    /// <summary>Juego pausado.</summary>
    Paused,
    /// <summary>Pantalla de muerte del jugador.</summary>
    PlayerDeath
}