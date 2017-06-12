using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class vTutorialTextTrigger : MonoBehaviour
{
    [TextAreaAttribute(5, 3000), Multiline]    
    public string text;
    public Text _textUI;
    public GameObject painel;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag.Equals("Player"))
        {
            painel.SetActive(true);
            _textUI.gameObject.SetActive(true);
            _textUI.text = text;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {            
            painel.SetActive(false);
            _textUI.gameObject.SetActive(false);
            _textUI.text = " ";
        }
    }
}
