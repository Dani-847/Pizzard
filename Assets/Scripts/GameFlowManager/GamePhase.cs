/// <summary>
/// Fases del flujo de juego por cada boss.
/// El ciclo típico es: IntroDialog -> ShopBeforeBoss -> PreBossDialog -> BossFight -> 
/// PostBossDialog -> ShopAfterBoss -> PreNextBossDialog -> (siguiente boss o fin).
/// </summary>
public enum GamePhase
{
    None,
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
    Paused
}