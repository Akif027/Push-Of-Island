using UnityEngine;

public class TokenManager : MonoBehaviour
{
    public static TokenManager Instance;

    [SerializeField]
    private LayerMask tokenLayerMask; // LayerMask for token detection
    [SerializeField] ForceSlider forceSlider;
    private Token selectedToken;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (GameManager.Instance.getCurrentPhase() != GamePhase.GamePlay) return;

        // Check if the mouse is over a UI element; ignore raycast if true
        if (IsPointerOverUIElement()) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Ensure Z-plane consistency

        if (Input.GetMouseButtonDown(0)) // On left mouse button press
        {
            SelectTokenAtMousePosition(mousePosition);
        }

        if (Input.GetMouseButton(0) && selectedToken != null) // While holding left mouse button
        {
            selectedToken.DragToRotate(mousePosition);
        }

        if (Input.GetMouseButtonUp(0) && selectedToken != null) // On left mouse button release
        {
            selectedToken.StopDragging();
        }
    }
    private bool IsPointerOverUIElement()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }


    private void SelectTokenAtMousePosition(Vector3 mousePosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, tokenLayerMask);

        if (hit.collider != null) // Token is hit
        {
            Token token = hit.collider.GetComponent<Token>();
            if (token != null)
            {
                SetSelectedToken(token);
                token.StartDragging(mousePosition);
            }
        }
        else // Clicked on empty space
        {
            DeselectToken();
        }
    }
    private void DeselectToken()
    {

        if (selectedToken != null)
        {
            MapScroll.Instance.EnableScroll();
            selectedToken.OnTokenDeselected();
            selectedToken = null; // Clear the selected token reference
            Debug.Log("Token deselected. ");
        }
    }
    public void SetSelectedToken(Token token)
    {
        if (selectedToken != null && selectedToken != token)
        {
            selectedToken.OnTokenDeselected();
        }
        MapScroll.Instance.DisableScroll();
        selectedToken = token;
        selectedToken.OnTokenSelected();
    }

    public void AttackButtonClicked()
    {
        if (selectedToken != null)
        {
            selectedToken.SetForce(forceSlider.getForce());
            selectedToken.Launch();
            Debug.Log($"Launched Token: {selectedToken.name}");
        }
        else
        {
            Debug.LogWarning("No token is selected to launch!");
        }
    }
}
