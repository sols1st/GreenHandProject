public class GetCardPoint : Interactable
{
    public string CardName;
    public override void TriggerOnClick()
    {
        CardManager.Instance.GetNewCard(CardName);
        isInteractable = false;
    }
}
