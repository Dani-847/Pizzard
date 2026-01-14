using UnityEngine;

// Gestiona los contextos y todas las UI
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public MenuUI menuUI;
    public OptionsUI optionsUI;
    public ShopUI tiendaUI;
    public DialogUI dialogUI;

    private UIContext lastContext = UIContext.None;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OpenOptions(UIContext context)
    {
        lastContext = context;
        optionsUI.Show();
    }

    public void CloseOptions()
    {
        optionsUI.Hide();

        switch (lastContext)
        {
            case UIContext.Menu:
                menuUI.Show();
                break;
            case UIContext.Tienda:
                //tiendaUI.Show();
                break;
            case UIContext.Dialogo:
                //dialogUI.Show();
                break;
            case UIContext.BossFight:
                break;
        }

        lastContext = UIContext.None;
    }
}