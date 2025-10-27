
using TMPro;

public class NoticeCanvas : Interactable
{
    public TMP_Text noticeText;
    private string[] _content;
    private int _index = 0;
    public override void TriggerOnClick()
    {
        if (_index < _content.Length)
        {
            noticeText.text = _content[_index];
            _index++;
        }
        else
        {
            _index = 0;
            gameObject.SetActive(false);
        }
        
    }

    public void SetContent(string[] content)
    {
        _content = content;
        noticeText.text = _content[0];
        _index = 1;
    }
}
