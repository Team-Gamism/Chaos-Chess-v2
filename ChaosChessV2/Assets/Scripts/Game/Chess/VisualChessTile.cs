using UnityEngine;
using UnityEngine.Events;

public class VisualChessTile : MonoBehaviour
{
    private Vector2 coordinate;
    
    private UnityEvent OnHighlighted = new UnityEvent();
    private UnityEvent OnAttackable = new UnityEvent();
    private UnityEvent OnSelected = new UnityEvent();


}
